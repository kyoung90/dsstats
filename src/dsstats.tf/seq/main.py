import dataservice
import modelservice
import saveservice
import tensorflow as tf

data = dataservice.GetReplayDataWithRatings('2020-07-28', '2023-01-01')

commanders = dataservice.GetCommanders()
commander_to_index = {cmdr: i for i, cmdr in enumerate(sorted(commanders))}

cmdrs, ratings, labels = dataservice.PreprocessData(data, commander_to_index)

print('minRating: ', dataservice.minRating)
print('maxRating: ', dataservice.maxRating)
print('shape of cmdrs: ', cmdrs.shape, ' min: ', cmdrs.min(), ' max: ', cmdrs.max())
print('shape of ratings: ', ratings.shape, ' min: ', ratings.min(), ' max: ', ratings.max())
print('shape of labels: ', labels.shape, ' min: ', labels.min(), ' max: ', labels.max())

model = modelservice.GetTfModel()

# convert to tensor
cmdrs_tensor = tf.constant(cmdrs, dtype=tf.float32)
ratings_tensor = tf.constant(ratings, dtype=tf.float32)

# Train the model on the input data and labels

# model.fit(x=[cmdrs_tensor, ratings_tensor], y=labels, epochs=10, batch_size=32, validation_split=0.1)
model.fit(x=[cmdrs_tensor, ratings_tensor], y=labels, epochs=50, batch_size=128, validation_split=0.1)

saveservice.SaveModel(model, 2)

# Evaluate
testdata = dataservice.GetReplayDataWithRatings('2023-01-23', '2023-03-01')
testcmdrs, testratings, testlabels = dataservice.PreprocessData(testdata, commander_to_index)

# convert to tensor
testcmdrs_tensor = tf.constant(testcmdrs, dtype=tf.float32)
testratings_tensor = tf.constant(testratings, dtype=tf.float32)

# Evaluate the model on the test set
loss, accuracy = model.evaluate([testcmdrs_tensor, testratings_tensor], testlabels)
print('Test loss:', loss)
print('Test accuracy:', accuracy)

