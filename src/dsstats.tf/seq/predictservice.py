import dataservice
import numpy as np
import tensorflow as tf

# data: {'WinnerTeam': 2, 'CommandersTeam1': '|90|130|90|', 'CommandersTeam2': '|50|60|20|', 'ratings': '957.94|1283.86|929.5|1012.53|667.26|1102.45'}
def Predict(model, data, commander_to_index):

    testcmdrs, testratings, testlabels = dataservice.PreprocessData([data], commander_to_index)

    # Reshape the input data to fit the model
    testcmdrs = testcmdrs.reshape(testcmdrs.shape[0], testcmdrs.shape[1], 1)
    testratings = testratings.reshape(testratings.shape[0], testratings.shape[1], 1)

    # convert to tensor
    testcmdrs_tensor = tf.constant(testcmdrs, dtype=tf.float32)
    testratings_tensor = tf.constant(testratings, dtype=tf.float32)
    
    # Make a prediction on the new example
    prediction = model.predict([testcmdrs_tensor, testratings_tensor])[0][0]

    # Print the expectation to win
    print('Expectation to win: {:.2f}%'.format(prediction*100))
