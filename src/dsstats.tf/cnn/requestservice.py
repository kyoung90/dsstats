import requests
import json
import numpy as np

def NormalizeRatings(ratings, min_rating=0, max_rating=3000):
    return [(float(r) - min_rating) / (max_rating - min_rating) for r in ratings]

def GetData(possiblecmdrs, cdmrs1, cmdrs2, ratings):
    # Convert cmdrs1 and cmdrs2 to lists of integers
    cmdrs1_list = [int(c) for c in cmdrs1.split('|') if c]
    cmdrs2_list = [int(c) for c in cmdrs2.split('|') if c]

    # Initialize the cmdrs_input tensor with all zeros
    cmdrs_input = np.zeros((1, 34, 1))

    # Set the values of the cmdrs_input tensor to 1 for the commanders in cmdrs1_list and cmdrs2_list
    for c in cmdrs1_list + cmdrs2_list:
        if c in possiblecmdrs:
            cmdrs_input[0, possiblecmdrs.index(c), 0] = 1

    # Convert the ratings string to a list of floats
    ratings_list = [float(r) for r in ratings.split('|')]
    ratings_list = NormalizeRatings(ratings_list)

    # Reshape the ratings_list into a 2D array
    ratings_input = np.array(ratings_list).reshape(1, -1, 1).tolist()

    # Define the data to send to the server
    data = {
        'signature_name': 'serving_default',
        'inputs': {
            'cmdrs_input': cmdrs_input.tolist(),
            'ratings_input': ratings_input
        }
    }
    return data

# Define the URL for your TensorFlow Serving instance
url = 'http://localhost:8501/v1/models/dsstatsModel:predict'

possiblecmdrs = [0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 110, 120, 130, 140, 150, 160, 170]
cmdrs1 = "|20|30|40|"
cmdrs2 = "|100|110|120|"
ratings = '957.94|1283.86|929.5|1012.53|667.26|1102.45'

data = GetData(possiblecmdrs, cmdrs1, cmdrs2, ratings)

# Define the headers for the request
headers = {'Content-Type': 'application/json'}

# Send the request to the server
response = requests.post(url, json=data, headers=headers)

# Extract the predictions from the response
predictions = np.array(response.json()['outputs'])

# Print the predictions
print(predictions[0])