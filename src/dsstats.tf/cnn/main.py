import dataservice
import modelservice
import saveservice
import tensorflow as tf

data = dataservice.GetReplayDataWithRatings('2020-01-01', '2023-01-01')

commanders = dataservice.GetCommanders()
commander_to_index = {cmdr: i for i, cmdr in enumerate(sorted(commanders))}

cmdrs, ratings, labels = dataservice.PreprocessData(data, commander_to_index)

print('shape of cmdrs: ', cmdrs.shape, cmdrs.min(), cmdrs.max())
print('shape of ratings: ', ratings.shape, ratings.min(), ratings.max())
print('shape of labels: ', labels.shape, labels.min(), labels.max())

model = modelservice.GetCNNModelTest(len(commanders))

# Reshape the input data to fit the model
cmdrs = cmdrs.reshape(cmdrs.shape[0], cmdrs.shape[1], 1)
ratings = ratings.reshape(ratings.shape[0], ratings.shape[1], 1)

# convert to tensor
cmdrs_tensor = tf.constant(cmdrs, dtype=tf.float32)
ratings_tensor = tf.constant(ratings, dtype=tf.float32)

# Train the model on the input data and labels
model.fit([cmdrs_tensor, ratings_tensor], labels, epochs=30, batch_size=32, validation_split=0.2)

saveservice.SaveModel(model, 1)

# Evaluate
testdata = dataservice.GetReplayDataWithRatings('2023-01-01', '2023-03-01')
testcmdrs, testratings, testlabels = dataservice.PreprocessData(testdata, commander_to_index)

# Reshape the input data to fit the model
testcmdrs = testcmdrs.reshape(testcmdrs.shape[0], testcmdrs.shape[1], 1)
testratings = testratings.reshape(testratings.shape[0], testratings.shape[1], 1)

# convert to tensor
testcmdrs_tensor = tf.constant(testcmdrs, dtype=tf.float32)
testratings_tensor = tf.constant(testratings, dtype=tf.float32)

# Evaluate the model on the test set
loss, accuracy = model.evaluate([testcmdrs_tensor, testratings_tensor], testlabels)
print('Test loss:', loss)
print('Test accuracy:', accuracy)

