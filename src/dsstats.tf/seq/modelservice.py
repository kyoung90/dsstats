import tensorflow as tf

def GetTfModel():
    # define the input layers
    cmdrs_input = tf.keras.layers.Input(shape=(6, 18))
    ratings_input = tf.keras.layers.Input(shape=(6,))

    # flatten the inputs
    cmdrs_flat = tf.keras.layers.Flatten()(cmdrs_input)
    ratings_flat = tf.keras.layers.Flatten()(ratings_input)

    # concatenate the flattened inputs
    concatenated = tf.keras.layers.Concatenate()([cmdrs_flat, ratings_flat])

    # add some dense layers with dropout to prevent overfitting
    x = tf.keras.layers.Dense(256, activation='relu')(concatenated)
    x = tf.keras.layers.Dropout(0.2)(x)
    x = tf.keras.layers.Dense(128, activation='relu')(x)
    x = tf.keras.layers.Dropout(0.2)(x)
    x = tf.keras.layers.Dense(64, activation='relu')(x)
    x = tf.keras.layers.Dropout(0.2)(x)

    # output layer
    output = tf.keras.layers.Dense(1, activation='sigmoid')(x)

    # create the model
    model = tf.keras.models.Model(inputs=[cmdrs_input, ratings_input], outputs=output)

    # Compile the model with binary cross-entropy loss and Adam optimizer
    model.compile(loss='binary_crossentropy', optimizer='adam', metrics=['accuracy'])

    return model    