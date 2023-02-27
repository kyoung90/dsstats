import dataservice
import modelservice
import tensorflow as tf

data = dataservice.GetReplayDataWithRatings('2023-02-01', '2023-03-01')

commanders = dataservice.GetCommanders()
commander_to_index = {cmdr: i for i, cmdr in enumerate(sorted(commanders))}

cmdrs, ratings, labels = dataservice.PreprocessData(data, commander_to_index)

model = modelservice.GetCNNModelTest(len(commanders))

# Reshape the input data to fit the model
cmdrs = cmdrs.reshape(cmdrs.shape[0], cmdrs.shape[1], 1)
ratings = ratings.reshape(ratings.shape[0], ratings.shape[1], 1)

# convert to tensor
cmdrs_tensor = tf.constant(cmdrs, dtype=tf.float32)
ratings_tensor = tf.constant(ratings, dtype=tf.float32)

model = modelservice.GetCNNModelTest(len(commanders))

# Evaluate
loss, acc = model.evaluate([cmdrs_tensor, ratings_tensor], labels)
print("Untrained model, accuracy: {:5.2f}%".format(100 * acc))

# Loads the weights
model.load_weights('/data/ai/dsstatsModel_CNNv4.h5')

# Re-evaluate the model
loss, acc = model.evaluate([cmdrs_tensor, ratings_tensor], labels)
print("Restored model, accuracy: {:5.2f}%".format(100 * acc))