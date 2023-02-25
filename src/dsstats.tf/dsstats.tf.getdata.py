import json
import mysql.connector
import numpy as np
import tensorflow as tf
import os

# os.environ["CUDA_VISIBLE_DEVICES"] = "-1"

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
    query = ("select r.WinnerTeam, r.CommandersTeam1, r.CommandersTeam2, GROUP_Concat(rpr.Rating order by rpr.GamePos separator '|') as ratings"
           + "from Replays as r" +
           + "inner join ReplayPlayers as rp on rp.ReplayId = r.ReplayId" +
           + "inner join RepPlayerRatings as rpr on rpr.ReplayPlayerId = rp.ReplayPlayerId" +
           + "inner join ReplayRatings as rr on rr.ReplayId = r.ReplayId" +
           + "where r.GameTime >= '" + fromDate + "' and r.GameTime < '" + toDate + "' and rr.RatingType = 1 and r.MaxLeaver < 90" +
           + "group by r.ReplayId, r.GameTime, r.CommandersTeam1, r.CommandersTeam2, r.WinnerTeam" +
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

def GetCNNModel(num_commanders):
    model = tf.keras.Sequential([
        tf.keras.layers.Reshape((num_commanders, 2, 1), input_shape=(num_commanders*2,)),
        tf.keras.layers.Conv2D(32, (3, 1), activation='relu'),
        tf.keras.layers.MaxPooling2D((2, 1)),
        tf.keras.layers.Conv2D(64, (3, 1), activation='relu'),
        tf.keras.layers.MaxPooling2D((2, 1)),
        tf.keras.layers.Flatten(),
        tf.keras.layers.Dense(64, activation='relu'),
        tf.keras.layers.Dense(1, activation='sigmoid')
    ])
    model.compile(optimizer='adam', loss='binary_crossentropy', metrics=['binary_accuracy'])
    return model

commanders = GetCommanders()
commander_to_index = {cmdr: i for i, cmdr in enumerate(sorted(commanders))}

trainData = GetReplayData('2020-01-01', '2022-06-01')
trainWinLoss = [row['WinnerTeam'] for row in trainData]
trainWinRate = sum(trainWinLoss) / len(trainWinLoss)

testData = GetReplayData('2022-06-01', '2023-01-01')
testWinLoss = [row['WinnerTeam'] for row in testData]
testWinRate = sum(testWinLoss) / len(testWinLoss)

evalData = GetReplayData('2023-02-01', '2023-03-01')
evalWinLoss = [row['WinnerTeam'] for row in evalData]
evalWinRate = sum(evalWinLoss) / len(evalWinLoss)

X_train = np.concatenate([GetTeamData(row, commander_to_index) for row in trainData])
X_test = np.concatenate([GetTeamData(row, commander_to_index) for row in testData])
X_eval = np.concatenate([GetTeamData(row, commander_to_index) for row in evalData])

# convert numpy arrays to tensorflow tensors
X_train_tensor = tf.convert_to_tensor(X_train, dtype=tf.float32)
Y_train_tensor = tf.convert_to_tensor(trainWinLoss, dtype=tf.float32)
X_test_tensor = tf.convert_to_tensor(X_test, dtype=tf.float32)
Y_test_tensor = tf.convert_to_tensor(testWinLoss, dtype=tf.float32)

model = GetCNNModel(len(commanders))

# train the model
model.fit(X_train_tensor, Y_train_tensor, epochs=20, batch_size=32)

# evaluate the model on test data
model.evaluate(X_test_tensor, Y_test_tensor, batch_size=32)

model.save('/data/ai/dsstatsModel_CNNv1.h5')
