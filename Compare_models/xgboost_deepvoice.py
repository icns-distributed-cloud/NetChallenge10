import librosa
import json
import math
import numpy as np
from tqdm import tqdm
import xgboost as xgb

from sklearn.model_selection import train_test_split
from tensorflow.keras.utils import to_categorical, Sequence
from sklearn.metrics import accuracy_score
from skimage.transform import resize


class Dataloader(Sequence):
    def __init__(self, Audios, labels, batch_size):
        self.Audios = Audios
        self.labels = labels
        self.batch_size = batch_size
        self.num_classes = len(set(self.labels))
        self.indices = np.arange(len(self.labels))

    def __len__(self):
        return math.ceil(len(self.labels)/self.batch_size)
    
    def __getitem__(self, idx):
        indices = self.indices[idx*self.batch_size : (idx+1)*self.batch_size]
        batch_x = [self.Audios[i] for i in indices]
        batch_audios = self.get_Audios(batch_x)
        batch_y = [self.labels[i] for i in indices]
        # 라벨을 원-핫 인코딩
        batch_y = to_categorical(batch_y, num_classes=self.num_classes)
        return np.array(batch_audios), np.array(batch_y)
    
    def get_Audios(self, path_list):
        # 오디오 데이터 로딩 및 전처리
        spectrograms = []
        for file_path in path_list:
            audio, sample_rate = librosa.load(file_path, res_type='kaiser_fast')
            mfccs = librosa.feature.mfcc(y=audio, sr=sample_rate, n_mfcc=40)
            mfccsscaled = np.mean(mfccs.T,axis=0)
            
            spectrograms.append(mfccsscaled) 

        return np.array(spectrograms)



# JSON 파일 로딩 및 데이터 및 라벨 생성
with open('train_dataset.json', 'r') as f:
    folder_label_mapping = json.load(f)

file_paths = []
labels = []

# 각 폴더 및 라벨에 대해
for file_path, label in folder_label_mapping.items():
    # 파일 확장자 확인하여 wav 파일만 처리
    if file_path.lower().endswith('.wav'):
        # 파일 경로 및 라벨 저장
        file_paths.append(file_path)
        labels.append(label)

# 라벨을 정수로 변환 (만약 문자열 라벨을 사용하고 있다면)
unique_labels = sorted(set(labels))
label_to_int = {label: i for i, label in enumerate(unique_labels)}
labels = [label_to_int[label] for label in labels]

# 라벨의 종류 수 계산
num_classes = len(set(labels))

# 라벨 배열이 비어 있지 않은지 확인
if len(labels) == 0:
    raise ValueError("Labels array is empty. Check your data loading logic.") 


Train_dataloader = Dataloader(file_paths, labels, 16)


epochs = 30

# 3. XGBoost 모델 훈련
model = xgb.XGBClassifier(tree_method='hist', device='cuda:0')

for _ in range(epochs):
    for i in tqdm(range(Train_dataloader.__len__())):
        x, y = Train_dataloader.__getitem__(i)
        model.fit(x, y)

model.save('xgboost_{}epochs_model.h5'.format(epochs))

# 4. 예측 및 정확도 계산
#predictions = model.predict(X_test)
#accuracy = accuracy_score(y_test, predictions)
#print("Accuracy: %.2f%%" % (accuracy * 100.0))
