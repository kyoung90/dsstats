
import json
import mysql.connector
import numpy as np
from sklearn.preprocessing import MinMaxScaler

config_file = "/data/localserverconfig.json"

def GetDbConnection(config_file):
    # Load the configuration from the JSON file
    with open(config_file, 'r') as f:
        config = json.load(f)

    # Get the database connection details from the configuration
    db_config = config['ServerConfig']['PythonConnection']

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

def PreprocessData(results, commander_to_index):
    winloss = [row['WinnerTeam'] for row in results]
    cmdrs = np.concatenate([GetTeamData(row, commander_to_index) for row in results])
    ratings = np.array([GetRatingData(row) for row in results])
    labels = np.array([int(winner == 1) for winner in winloss])

    # Scale the ratings using MinMaxScaler
    scaler = MinMaxScaler()
    ratings_scaled = scaler.fit_transform(ratings.reshape((-1, 1)))
    ratings_scaled = ratings_scaled.reshape(ratings.shape)

    return (cmdrs, ratings_scaled, labels)