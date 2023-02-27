import tensorflow as tf
import os

def SaveModel(model, version):
    MODEL_DIR = '/data/ai/dsstatsModelSeq'
    export_path = os.path.join(MODEL_DIR, str(version))
    print('export_path = {}\n'.format(export_path))

    tf.keras.models.save_model(
        model,
        export_path,
        overwrite=True,
        include_optimizer=True,
        save_format=None,
        signatures=None,
        options=None
    )