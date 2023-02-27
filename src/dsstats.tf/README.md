
# Tensorflow experiments

## Prerequisites

Tested on WSL 2 (Windows 11) on Ubuntu 20.04 LTS
* Tensorflow 2.11.0
* Cuda 12 [install](https://developer.nvidia.com/cuda-downloads?target_os=Linux&target_arch=x86_64&Distribution=WSL-Ubuntu&target_version=2.0&target_type=deb_network)
* `sudo apt-get install cuda-toolkit-12-0`

## Training

* Recalculate ratings with ELO and consistency/confidence off (setup in MmrOptions)
* Train the model (python3 main.py - increase model version in SaveModel)
* Recalculate ratings with Tf and consistency/confidence on

## Serving

[Setup](https://www.tensorflow.org/tfx/serving/setup)
```
tensorflow_model_server --rest_api_port=8501 --model_name=dsstatsModel --model_base_path=/data/ai/dsstatsModel
```