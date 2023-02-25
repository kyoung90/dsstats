import json
import mysql.connector
import numpy as np
import tensorflow as tf
import os

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

# Load the data
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

def GetCNNModel():
    model = tf.keras.Sequential([
        tf.keras.layers.Conv2D(32, kernel_size=(3, 3), activation='relu', input_shape=(3, 2, 1)),
        tf.keras.layers.MaxPooling2D(pool_size=(2, 2)),
        tf.keras.layers.Conv2D(64, kernel_size=(3, 3), activation='relu'),
        tf.keras.layers.MaxPooling2D(pool_size=(2, 2)),
        tf.keras.layers.Flatten(),
        tf.keras.layers.Dense(64, activation='relu'),
        tf.keras.layers.Dropout(0.5),
        tf.keras.layers.Dense(1, activation='sigmoid')
    ]) 
    # Compile the model
    model.compile(optimizer='adam', loss='binary_crossentropy', metrics=['accuracy'])
    return model;

data = GetReplayDataWithRatings('2022-01-01', '2023-01-01')

# Preprocess the data
input_data = []
output_data = []
for i in range(len(data)):
    # get the data for this game
    game = data[i]
    commanders_team1 = game['CommandersTeam1']
    commanders_team2 = game['CommandersTeam2']
    ratings = game['ratings']
    
    # concatenate the data and reshape to (3, 2, 1)
    input_data.append(np.concatenate([commanders_team1.reshape(-1, 1), commanders_team2.reshape(-1, 1), ratings.reshape(-1, 1)], axis=-1).reshape(3, 2, 1))
    output_data.append(game['WinnerTeam'] == 1)

# Compile the model
model = GetCNNModel()

# Train the model
model.fit(input_data, output_data, epochs=10)

# Evaluate the model
loss, accuracy = model.evaluate(input_data, output_data)
print('Accuracy:', accuracy)
