import json
import mysql.connector
import numpy as np
import tensorflow as tf
import os
from sklearn.preprocessing import MinMaxScaler
from sklearn.model_selection import train_test_split

os.environ["CUDA_VISIBLE_DEVICES"] = "-1"

config_file = "/data/localserverconfig.json"

def GetDbConnection(config_file):
    # Load the configuration from the JSON file
    with open(config_file, 'r') as f:
        config = json.load(f)

    # Get the database connection details from the configuration
    db_config = config['ServerConfig']['PythonConnection']

    # connectioninfo = dict(item.split('=') for item in db_config.split(';'))

    # Connect to the database
    cnx = mysql.connector.connect(**dict(item.split('=') for item in db_config.split(';')))

    return cnx

def GetReplayData(fromDate, toDate):
    cnx = GetDbConnection(config_file)

    cursor = cnx.cursor()
    query = ("SELECT r.GameTime, r.WinnerTeam, r.CommandersTeam1, r.CommandersTeam2 " 
    + "FROM Replays as r " 
    + "WHERE r.GameTime >= '" + fromDate + "' and r.GameTime < '" + toDate + "' and r.GameMode = 3 and r.Duration >= 320 and r.Maxleaver < 90 and r.PlayerCount = 6")
    cursor.execute(query)

    # Retrieve the results and store them in a list of dictionaries
    results = []
    for row in cursor:
        results.append({'GameTime': row[0], 'WinnerTeam': row[1], 'CommandersTeam1': row[2], 'CommandersTeam2': row[3]})

    # Close the cursor and the connection
    cursor.close()
    cnx.close()
    return results

def GetReplayDataWithRatings(fromDate, toDate):
    cnx = GetDbConnection(config_file)

    cursor = cnx.cursor()
    query = ("select r.WinnerTeam, r.CommandersTeam1, r.CommandersTeam2, GROUP_Concat(rpr.Rating order by rpr.GamePos separator '|') as ratings "
           + "from Replays as r "
           + "inner join ReplayPlayers as rp on rp.ReplayId = r.ReplayId "
           + "inner join RepPlayerRatings as rpr on rpr.ReplayPlayerId = rp.ReplayPlayerId "
           + "inner join ReplayRatings as rr on rr.ReplayId = r.ReplayId "
           + "where r.GameTime >= '" + fromDate + "' and r.GameTime < '" + toDate + "' and rr.RatingType = 1 and r.MaxLeaver < 90 "
           + "group by r.ReplayId, r.GameTime, r.CommandersTeam1, r.CommandersTeam2, r.WinnerTeam "
           + "order by r.GameTime")
    cursor.execute(query)

    # Retrieve the results and store them in a list of dictionaries
    results = []
    for row in cursor:
        results.append({'WinnerTeam': row[0], 'CommandersTeam1': row[1], 'CommandersTeam2': row[2], 'ratings': row[3]})

    # Close the cursor and the connection
    cursor.close()
    cnx.close()
    return results

def GetCommanders():
    return { 10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 110, 120, 130, 140, 150, 160, 170 }

def GetTeamData(row, commander_to_index):
    team1 = np.zeros(len(commander_to_index))
    team2 = np.zeros(len(commander_to_index))
    for cmdr in row['CommandersTeam1'].split('|'):
        if cmdr != '':
            team1[commander_to_index[int(cmdr)]] = 1
    for cmdr in row['CommandersTeam2'].split('|'):
        if cmdr != '':
            team2[commander_to_index[int(cmdr)]] = 1
    return np.concatenate((team1, team2)).reshape(1, -1)

def GetRatingData(row):
    return np.array(list(map(float, row['ratings'].split('|'))))
    # return np.array(list(map(float, row['ratings'].split('|')))).reshape(1, -1)

def GetCNNModel(num_commanders):
    # Define input shapes
    commander_shape = (num_commanders*2,)
    rating_shape = (6,)
    winloss_shape = (1,)

    # Define input layers for each type of data
    commander_input = tf.keras.layers.Input(shape=commander_shape, name='commander_input')
    rating_input = tf.keras.layers.Input(shape=rating_shape, name='rating_input')
    winloss_input = tf.keras.layers.Input(shape=winloss_shape, name='winloss_input')

    # Define embedding layer for commander data
    commander_emb = tf.keras.layers.Embedding(input_dim=num_commanders*2, output_dim=64)(commander_input)
    commander_emb = tf.keras.layers.Flatten()(commander_emb)

    # Define dense layer for rating data
    rating_dense = tf.keras.layers.Dense(64, activation='relu')(rating_input)

    # Merge inputs using concatenation layer
    merged = tf.keras.layers.concatenate([commander_emb, rating_dense, winloss_input])

    # Define remaining layers of the model
    x = tf.keras.layers.Dense(128, activation='relu')(merged)
    x = tf.keras.layers.Dropout(0.5)(x)
    x = tf.keras.layers.Dense(64, activation='relu')(x)
    x = tf.keras.layers.Dropout(0.5)(x)
    output = tf.keras.layers.Dense(1, activation='sigmoid')(x)

    # Define model with multiple inputs
    model = tf.keras.Model(inputs=[commander_input, rating_input, winloss_input], outputs=output)

    # Compile model
    model.compile(optimizer=tf.keras.optimizers.Adam(lr=0.001), loss='binary_crossentropy', metrics=['binary_accuracy'])

    return model

def GetCNNModelv2(num_commanders):
    commander_input = tf.keras.layers.Input(shape=(num_commanders*2,), name='commander_input')
    rating_input = tf.keras.layers.Input(shape=(1,), name='rating_input')
    winloss_input = tf.keras.layers.Input(shape=(1,), name='winloss_input')

    # Concatenate the commander and rating inputs
    x = tf.keras.layers.concatenate([commander_input, rating_input, winloss_input])

    # Add a dense layer with 64 units
    x = tf.keras.layers.Dense(64, activation='relu')(x)

    # Add the output layer with a sigmoid activation function
    output = tf.keras.layers.Dense(1, activation='sigmoid')(x)

    # Create the model
    model = tf.keras.Model(inputs=[commander_input, rating_input, winloss_input], outputs=output)

    # Compile the model
    model.compile(optimizer='adam', loss='binary_crossentropy', metrics=['accuracy'])
    return model

def GetModelData(fromDate, toDate, test_size=0.5):
    data = GetReplayDataWithRatings(fromDate, toDate)
    print(data[0])
    winloss = [row['WinnerTeam'] for row in data]
    cmdrs = np.concatenate([GetTeamData(row, commander_to_index) for row in data])
    ratings = np.array([GetRatingData(row) for row in data])
    labels = np.array([int(winner == 2) for winner in winloss])

    # num_matches = ratings.shape[0]
    # num_players_per_team = ratings.shape[1] // 2
    # ratings = ratings.reshape((num_matches, num_players_per_team, 2))

    # Split the data into train and test sets
    cmdrs_train, cmdrs_test, ratings_train, ratings_test, winloss_train, winloss_test, labels_train, labels_test = train_test_split(
        cmdrs, ratings, winloss, labels, test_size=test_size, random_state=42)

    # Scale the ratings using MinMaxScaler
    scaler = MinMaxScaler()
    ratings_scaled_train = scaler.fit_transform(ratings_train.reshape((-1, 1)))
    #ratings_scaled_train = ratings_scaled_train.reshape(ratings_train.shape)

    ratings_scaled_test = scaler.fit_transform(ratings_test.reshape((-1, 1)))
    #ratings_scaled_test = ratings_scaled_test.reshape(ratings_test.shape)

    # convert to tnesor    
    winloss_tensor_train = tf.constant(winloss_train, dtype=tf.float32)
    cmdrs_tensor_train = tf.constant(cmdrs_train, dtype=tf.float32)
    ratings_scaled_tensor_train = tf.constant(ratings_scaled_train, dtype=tf.float32)

    winloss_tensor_test = tf.constant(winloss_test, dtype=tf.float32)
    cmdrs_tensor_test = tf.constant(cmdrs_test, dtype=tf.float32)
    ratings_scaled_tensor_test = tf.constant(ratings_scaled_test, dtype=tf.float32)

    return (cmdrs_tensor_train, ratings_scaled_tensor_train, winloss_tensor_train, labels_train, cmdrs_tensor_test, ratings_scaled_tensor_test, winloss_tensor_test, labels_test)


commanders = GetCommanders()
commander_to_index = {cmdr: i for i, cmdr in enumerate(sorted(commanders))}

cmdrs_train, ratings_train, winloss_train, labels_train, cmdrs_test, ratings_test, winloss_test, labels_test = GetModelData('2022-01-01', '2023-01-01')

model = GetCNNModelv2(len(commanders))

# train the model
model.fit({'commander_input': cmdrs_train, 'rating_input': ratings_train, 'winloss_input': winloss_train}, labels_train, epochs=10, batch_size=32, validation_split=0.2)

# evaluate the model on test data
loss, accuracy = model.evaluate([cmdrs_test, ratings_test, winloss_test], labels_test)
print('Test loss:', loss)
print('Test accuracy:', accuracy)

model.save('/data/ai/dsstatsModel_CNNv2.h5')
