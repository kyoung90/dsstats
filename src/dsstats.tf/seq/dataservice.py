
import json
import mysql.connector
import numpy as np
import sys

config_file = "/data/localserverconfig.json"

# this is a pointer to the module object instance itself.
this = sys.modules[__name__]

# we can explicitly make assignments on it 
this.minRating = 500
this.maxRating = 1500

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
    query = ("select r.WinnerTeam, r.CommandersTeam1, r.CommandersTeam2, GROUP_Concat(rpr.Rating order by rpr.GamePos separator '|') as ratings, group_concat((r.Duration - rp.Duration) order by rp.GamePos separator '|') as leavers"
           + " from Replays as r"
           + " inner join ReplayPlayers as rp on rp.ReplayId = r.ReplayId"
           + " inner join RepPlayerRatings as rpr on rpr.ReplayPlayerId = rp.ReplayPlayerId"
           + " inner join ReplayRatings as rr on rr.ReplayId = r.ReplayId"
           + " where r.GameTime >= '" + fromDate +"' and r.GameTime < '" + toDate +"' and rr.RatingType = 1"
           + " group by r.ReplayId, r.GameTime, r.CommandersTeam1, r.CommandersTeam2, r.WinnerTeam"
           + " order by r.GameTime")
    cursor.execute(query)

    # Retrieve the results and store them in a list of dictionaries
    results = []
    for row in cursor:
        results.append({'WinnerTeam': row[0], 'CommandersTeam1': row[1], 'CommandersTeam2': row[2], 'ratings': row[3], 'leavers': row[4]})

    # Close the cursor and the connection
    cursor.close()
    cnx.close()
    return results



def GetCommanders():
    return { 0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 110, 120, 130, 140, 150, 160, 170 }

def GetLeavers(row):
    leavers = np.zeros(6)
    for idx, leave in enumerate(map(int, row['leavers'].split('|'))):
        if leave > 89:
            leavers[idx] = 1
    return leavers

def GetTeamData(row, commander_to_index):
    leavers = GetLeavers(row)
    team1cmdrs = list(map(int, list(filter(None, row['CommandersTeam1'].split('|')))))
    team2cmdrs = list(map(int, list(filter(None, row['CommandersTeam2'].split('|')))))
    teamCmdrs = team1cmdrs + team2cmdrs
    cmdrArrays = []
    for idx, cmdr in enumerate(teamCmdrs):
        if leavers[idx] == 1 or cmdr <= 3:
            teamCmdrs[idx] = 0
            cmdr = 0
        cmdrArray = np.zeros(len(commander_to_index), int)
        cmdrIndex = commander_to_index[cmdr]
        cmdrArray[cmdrIndex] = 1
        cmdrArrays.append(cmdrArray)
    return cmdrArrays

def GetNormalizedRating(rating):
    if rating > this.maxRating:
        this.maxRating = rating
    elif rating < this.minRating:
        this.minRating = rating    
    nrating = (rating - this.minRating) / (this.maxRating - this.minRating)
    # if nrating < 0:
    #     nrating = 0
    # elif nrating > 1:
    #     nrating = 1
    return nrating

def GetRatingData(row):
    return np.array(list(map(GetNormalizedRating, map(float, row['ratings'].split('|')))))

def PreprocessData(results, commander_to_index):
    winloss = [row['WinnerTeam'] for row in results]
    cmdrs = np.array([GetTeamData(row, commander_to_index) for row in results])
    ratings = np.array([GetRatingData(row) for row in results])
    labels = np.array([int(winner == 1) for winner in winloss])

    return (cmdrs, ratings, labels)


    # ((2, (3, 18)), )

    # [
    #     {
    #         [ rating_scaled ], { 0, 1, ... }
    #         [ rating_scaled ], [ 18 ]
    #         [ rating_scaled ], [ 18 ]
    #     }
    #     {
    #         [ rating_scaled ], [ 18 ]
    #         [ rating_scaled ], [ 18 ]
    #         [ rating_scaled ], [ 18 ]
    #     }
    # ]