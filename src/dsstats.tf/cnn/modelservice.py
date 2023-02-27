import tensorflow as tf

def GetCNNModel(cmdrlength):
    # Define the model architecture
    cmdrs_input = tf.keras.layers.Input(shape=(cmdrlength*2, 1), name='cmdrs_input')
    ratings_input = tf.keras.layers.Input(shape=(6, 1), name='ratings_input')

    cmdrs_conv = tf.keras.layers.Conv1D(filters=32, kernel_size=3, activation='relu')(cmdrs_input)
    cmdrs_pool = tf.keras.layers.MaxPooling1D(pool_size=2)(cmdrs_conv)
    cmdrs_flat = tf.keras.layers.Flatten()(cmdrs_pool)

    ratings_conv = tf.keras.layers.Conv1D(filters=16, kernel_size=3, activation='relu')(ratings_input)
    ratings_pool = tf.keras.layers.MaxPooling1D(pool_size=2)(ratings_conv)
    ratings_flat = tf.keras.layers.Flatten()(ratings_pool)

    concat = tf.keras.layers.Concatenate()([cmdrs_flat, ratings_flat])
    dense1 = tf.keras.layers.Dense(64, activation='relu')(concat)
    output = tf.keras.layers.Dense(1, activation='sigmoid')(dense1)

    model = tf.keras.models.Model(inputs=[cmdrs_input, ratings_input], outputs=output)

    # Compile the model with binary cross-entropy loss and Adam optimizer
    model.compile(loss='binary_crossentropy', optimizer='adam', metrics=['accuracy'])

    return model

def GetCNNModelTest(cmdrlength):

    # trained versions:
    # v3 => trained on elo ratings (without confidence and consistency) without cmdr info (only None for leavers) => very good result
    # v4 => trained on elo ratings (without confidence and consistency) with cmdr info => bad result

    # Define the model architecture
    cmdrs_input = tf.keras.layers.Input(shape=(cmdrlength*2, 1), name='cmdrs_input')
    ratings_input = tf.keras.layers.Input(shape=(6, 1), name='ratings_input')

    cmdrs_conv = tf.keras.layers.Conv1D(filters=32, kernel_size=3, activation='relu')(cmdrs_input)
    cmdrs_pool = tf.keras.layers.MaxPooling1D(pool_size=2)(cmdrs_conv)
    cmdrs_flat = tf.keras.layers.Flatten()(cmdrs_pool)

    ratings_conv = tf.keras.layers.Conv1D(filters=16, kernel_size=3, activation='relu')(ratings_input)
    ratings_pool = tf.keras.layers.MaxPooling1D(pool_size=2)(ratings_conv)
    ratings_flat = tf.keras.layers.Flatten()(ratings_pool)

    concat = tf.keras.layers.Concatenate()([cmdrs_flat, ratings_flat])
    dense1 = tf.keras.layers.Dense(128, activation='relu')(concat)
    output = tf.keras.layers.Dense(1, activation='sigmoid')(dense1)

    model = tf.keras.models.Model(inputs=[cmdrs_input, ratings_input], outputs=output)

    # Compile the model with binary cross-entropy loss and Adam optimizer
    model.compile(loss='binary_crossentropy', optimizer='adam', metrics=['accuracy'])

    return model


def GetMLPModelTest(cmdrlength):
    
    # trained versions:
    # v5 trained on elo ratings (without confidence and consistency) => biniary output only

    # Define the model architecture
    cmdrs_input = tf.keras.layers.Input(shape=(cmdrlength*2, 1), name='cmdrs_input')
    ratings_input = tf.keras.layers.Input(shape=(6, 1), name='ratings_input')

    # Flatten the inputs and concatenate them
    cmdrs_flat = tf.keras.layers.Flatten()(cmdrs_input)
    ratings_flat = tf.keras.layers.Flatten()(ratings_input)
    concat = tf.keras.layers.Concatenate()([cmdrs_flat, ratings_flat])

    # Define the hidden layers and output layer
    dense1 = tf.keras.layers.Dense(128, activation='relu')(concat)
    dense2 = tf.keras.layers.Dense(64, activation='relu')(dense1)
    output = tf.keras.layers.Dense(1, activation='sigmoid')(dense2)

    # Define the model with binary cross-entropy loss and Adam optimizer
    model = tf.keras.models.Model(inputs=[cmdrs_input, ratings_input], outputs=output)
    model.compile(loss='binary_crossentropy', optimizer='adam', metrics=['accuracy'])

    return model

def GetRNNModelTest(cmdrlength):
    # Define the model architecture
    cmdrs_input = tf.keras.layers.Input(shape=(None, cmdrlength*2), name='cmdrs_input')
    ratings_input = tf.keras.layers.Input(shape=(None, 6), name='ratings_input')

    # Define the LSTM layer for cmdrs_input
    cmdrs_lstm = tf.keras.layers.LSTM(units=32, return_sequences=False)(cmdrs_input)

    # Define the LSTM layer for ratings_input
    ratings_lstm = tf.keras.layers.LSTM(units=16, return_sequences=False)(ratings_input)

    # Concatenate the outputs of the two LSTMs
    concat = tf.keras.layers.Concatenate()([cmdrs_lstm, ratings_lstm])

    # Define the output layer
    output = tf.keras.layers.Dense(units=1, activation='sigmoid')(concat)

    # Define the model with binary cross-entropy loss and Adam optimizer
    model = tf.keras.models.Model(inputs=[cmdrs_input, ratings_input], outputs=output)
    model.compile(loss='binary_crossentropy', optimizer='adam', metrics=['accuracy'])

    return model