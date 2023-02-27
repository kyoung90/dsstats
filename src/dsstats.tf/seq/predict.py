import predictservice
import dataservice
import modelservice

commanders = dataservice.GetCommanders()
commander_to_index = {cmdr: i for i, cmdr in enumerate(sorted(commanders))}

model = modelservice.GetCNNModelTest(len(commanders))
model.load_weights('/data/ai/dsstatsModel_CNNv4.h5')

# data = {'WinnerTeam': 2, 'CommandersTeam1': '|90|130|90|', 'CommandersTeam2': '|50|60|20|', 'ratings': '957.94|1283.86|929.5|1012.53|667.26|1102.45'}
# data = {'WinnerTeam': 2, 'CommandersTeam1': '|90|130|90|', 'CommandersTeam2': '|50|60|20|', 'ratings': '2000|2000|2000|1000|1000|1000'}
# data = {'WinnerTeam': 2, 'CommandersTeam1': '|90|130|90|', 'CommandersTeam2': '|50|60|20|', 'ratings': '1000|1000|1000|2000|2000|2000'}
data = {'WinnerTeam': 2, 'CommandersTeam1': '|80|10|10|', 'CommandersTeam2': '|60|160|120|', 'ratings': '1812.35|1341.33|1283.76|1161.74|1479.96|1035.93'}


predictservice.Predict(model, data, commander_to_index)