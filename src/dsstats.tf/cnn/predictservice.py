import numpy as np
import dataservice

def Predict(model, data, commander_to_index):

    # Define the input data for the new example
    new_cmdrs = np.zeros((1, 34))
    new_ratings = np.zeros((1, 6))

    # Set the appropriate indices in the input data to 1
    new_cmdrs[0, [3, 4, 5]] = 1
    new_cmdrs[0, 31] = 1
    new_ratings[0, :] = [957.94, 1283.86, 929.5, 1012.53, 667.26, 1102.45]

    # Reshape the input data to fit the model
    new_cmdrs = new_cmdrs.reshape(1, 34, 1)
    new_ratings = new_ratings.reshape(1, 6, 1)

    # Make a prediction on the new example
    prediction = model.predict([new_cmdrs, new_ratings])[0][0]

    # Print the expectation to win
    print('Expectation to win: {:.2f}%'.format(prediction*100))
