import librosa
import numpy as np
import math
import json
import os
from tensorflow.keras import layers, models
from sklearn.model_selection import train_test_split
from tensorflow.keras.utils import to_categorical, Sequence


class Dataloader(Sequence):
    def __init__(self, Audios, labels, batch_size, target_shape):
        self.Audios = Audios
        self.labels = labels
        self.target_shape = target_shape
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
    
    # 스펙트로그램 패딩 함수
    def pad_spectrogram(self, spectrogram):
        """
        주어진 스펙트로그램을 target_shape으로 패딩하거나 자릅니다.
        spectrogram: 원본 스펙트로그램
        target_shape: 목표 형태 (빈도 수, 시간 프레임)
        """
        freq_bins, time_frames = spectrogram.shape
        target_freq_bins, target_time_frames = self.target_shape
        padded_spectrogram = np.zeros(self.target_shape)

        # 패딩 또는 자르기 시작 인덱스 계산
        freq_start = (target_freq_bins - freq_bins) // 2
        time_start = (target_time_frames - time_frames) // 2

        if time_frames > target_time_frames:  # 원본이 더 큰 경우, 자르기
            crop_start = (time_frames - target_time_frames) // 2
            spectrogram = spectrogram[:, crop_start:crop_start+target_time_frames]
        elif time_frames < target_time_frames:  # 목표가 더 큰 경우, 패딩
            padded_spectrogram[:, time_start:time_start+time_frames] = spectrogram
            return padded_spectrogram

        if freq_bins > target_freq_bins:  # 원본이 더 큰 경우, 자르기
            crop_start = (freq_bins - target_freq_bins) // 2
            spectrogram = spectrogram[crop_start:crop_start+target_freq_bins, :]
        elif freq_bins < target_freq_bins:  # 목표가 더 큰 경우, 패딩
            padded_spectrogram[freq_start:freq_start+freq_bins, :] = spectrogram
            return padded_spectrogram

        return spectrogram


    def get_Audios(self, path_list):
        # 오디오 데이터 로딩 및 전처리
        spectrograms = []
        for file_path in path_list:
            audio, sr = librosa.load(file_path, sr=None)
            spectrogram = librosa.feature.melspectrogram(y=audio, sr=sr)
            log_spectrogram = librosa.power_to_db(spectrogram)
            padded_spectrogram = self.pad_spectrogram(log_spectrogram)  # 패딩
            spectrograms.append(padded_spectrogram)
            #spectrograms.append(log_spectrogram)
            labels.append(label)

        return np.array(spectrograms)[..., np.newaxis]  # 채널 차원 추가

# 2. 모델 구축 함수 정의
def build_model(input_shape):
    """
    Xception 스타일의 모델을 구축합니다.
    input_shape: 입력 데이터의 형태 (빈도 수, 시간 프레임, 채널 수)
    """
    model = models.Sequential()

    # Add the Xception base model
    model.add(layers.Input(shape=input_shape))
    model.add(layers.Conv2D(32, (3, 3), strides=(2, 2), padding="same"))
    model.add(layers.BatchNormalization())
    model.add(layers.Activation("relu"))

    # Add more layers as per Xception architecture...

    # Add classification head
    model.add(layers.GlobalAveragePooling2D())
    model.add(layers.Dense(1, activation='sigmoid'))  # Binary classification

    return model


epochs = 30
target_shape = (128, 300) # 예시 형태, 조절 필요

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

# 데이터를 훈련 세트와 테스트 세트로 분할합니다.
X_train, X_test, y_train, y_test = train_test_split(file_paths, labels, test_size=0.2, random_state=123)

Train_dataloader = Dataloader(X_train, y_train, 16, target_shape)
Test_dataloader = Dataloader(X_test, y_test, 16, target_shape)

# 데이터 전처리



# 모델 구축 및 컴파일
input_shape = (target_shape[0], target_shape[1], 1)
model = build_model(input_shape)
model.compile(optimizer='adam', loss='binary_crossentropy', metrics=['accuracy'])

# 모델 학습
model.fit(Train_dataloader, validation_data=Test_dataloader, epochs=epochs, batch_size=16)

#모델 저장
model.save('xception_{}epochs_model.h5'.format(epochs))