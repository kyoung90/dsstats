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