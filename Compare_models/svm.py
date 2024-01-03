from tensorflow.keras.utils import to_categorical, Sequence
from sklearn.model_selection import train_test_split
from sklearn.metrics import classification_report
from sklearn.preprocessing import StandardScaler
from sklearn.svm import LinearSVC
from tqdm import tqdm
import numpy as np
import librosa
import joblib
import math
import os
import json

class Dataloader(Sequence):
    def __init__(self, Audios, labels, batch_size):
        self.Audios = Audios
        self.labels = labels
        self.batch_size = batch_size
        self.num_classes = len(set(self.labels))
        self.indices = np.arange(len(self.labels))
        self.scaler = StandardScaler()


    def __len__(self):
        return math.ceil(len(self.labels)/self.batch_size)
    
    def __getitem__(self, idx):
        indices = self.indices[idx*self.batch_size : (idx+1)*self.batch_size]
        batch_x = [self.Audios[i] for i in indices]
        batch_audios = self.get_Audios(batch_x)
        batch_y = [self.labels[i] for i in indices]
        # 라벨을 원-핫 인코딩
        #batch_y = to_categorical(batch_y, num_classes=self.num_classes)
        return np.array(batch_audios), np.array(batch_y)
    
    # 스펙트로그램 패딩 함수
    def extract_features(self, audio_file):
        y, sr = librosa.load(audio_file)

        mfccs = librosa.feature.mfcc(y=y, sr=sr)

        zero_crossing = librosa.feature.zero_crossing_rate(y)

        spectral_rolloff = librosa.feature.spectral_rolloff(y=y, sr=sr)

        chroma_stft = librosa.feature.chroma_stft(y=y, sr=sr)

        spectral_contrast = librosa.feature.spectral_contrast(y=y, sr=sr)

        features = np.concatenate((np.mean(mfccs, axis=1), [np.mean(zero_crossing), np.mean(spectral_rolloff), np.mean(chroma_stft), np.mean(spectral_contrast)]))
        
        return features
    

    def get_Audios(self, path_list):
        # 오디오 데이터 로딩 및 전처리
        features_list = []
        for file_path in path_list:
            features = self.extract_features(file_path)  # 패딩
            features_list.append(features)
        features_list = np.array(features_list)
        return self.scaler.fit_transform(features_list)


epochs = 50

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


Train_dataloader = Dataloader(file_paths, labels, 16)

svm_model = LinearSVC(max_iter=epochs)

# 학습 시작 메시지 출력
for i in tqdm(range(Train_dataloader.__len__())):
    x, y = Train_dataloader.__getitem__(i)
    svm_model.fit(x, y)

#svm_model.fit(Train_dataloader)
# 학습 완료 메시지 출력
joblib.dump(svm_model, 'svm_{}epochs_model.json'.format(epochs))
#print(classification_report(y_test, y_pred))