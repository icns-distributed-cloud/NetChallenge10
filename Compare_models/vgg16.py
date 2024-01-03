import os
import json
import math
import numpy as np
import librosa
from sklearn.model_selection import train_test_split

from tensorflow.keras.utils import to_categorical, Sequence
from tensorflow.keras.applications.vgg16 import VGG16
from tensorflow.keras.models import Sequential
from tensorflow.keras.layers import Dense, Flatten
from tensorflow.keras.preprocessing.image import ImageDataGenerator
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
            # 오디오 파일 로딩
            y, sr = librosa.load(file_path, sr=None)
            # 오디오 파일을 스펙트로그램으로 변환
            S = librosa.feature.melspectrogram(y=y, sr=sr)
            log_S = librosa.power_to_db(S, ref=np.max)
            
            # 스펙트로그램 이미지의 크기를 VGG16 입력 크기에 맞춤 (224, 224)
            log_S_resized = resize(log_S, (224, 224))
            
            # 채널 차원 추가 (VGG16은 RGB 이미지를 입력으로 받기 때문에 3차원이 필요)
            log_S_resized = np.stack([log_S_resized] * 3, axis=-1)
            
            spectrograms.append(log_S_resized) 

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

# 데이터를 훈련 세트와 테스트 세트로 분할합니다.
X_train, X_test, y_train, y_test = train_test_split(file_paths, labels, test_size=0.2, random_state=123)

Train_dataloader = Dataloader(X_train, y_train, 16)
Test_dataloader = Dataloader(X_test, y_test, 16)


# VGG16 모델 불러오기
base_model = VGG16(weights='imagenet', include_top=False, input_shape=(224, 224, 3))

# 모델 커스터마이징
model = Sequential()
model.add(base_model)
model.add(Flatten())
model.add(Dense(256, activation='relu'))
model.add(Dense(128, activation='relu'))
model.add(Dense(num_classes, activation='softmax'))

# 모델 컴파일
model.compile(optimizer='adam', loss='categorical_crossentropy', metrics=['accuracy'])

# 데이터 증강
datagen = ImageDataGenerator()

epochs = 50

# 모델 훈련
model.fit(
    Train_dataloader,
    validation_data=Test_dataloader,
    epochs=epochs
)

model.save('vgg16_{}epochs_model.h5'.format(epochs))

# 추론을 위한 함수
def predict_audio(file_path):
    # 오디오 파일 로딩 및 전처리
    y, sr = librosa.load(file_path, sr=None)
    S = librosa.feature.melspectrogram(y=y, sr=sr)
    log_S = librosa.power_to_db(S, ref=np.max)
    log_S_resized = resize(log_S, (224, 224))
    log_S_resized = np.stack([log_S_resized] * 3, axis=-1)
    
    # 모델 예측
    prediction = model.predict(np.array([log_S_resized]))
    predicted_label = np.argmax(prediction)
    
    # 예측된 라벨 반환
    return predicted_label

def predict_folder(folder_path):
    predictions = {}
    for filename in os.listdir(folder_path):
        if filename.lower().endswith('.wav'):
            file_path = os.path.join(folder_path, filename)
            predicted_label = predict_audio(file_path)
            predictions[filename] = predicted_label
            print(f"Predicted label for {file_path}: {predicted_label}")
    return predictions

## 예제 폴더로 추론 테스트
#test_folder = "/root/data/AIhub/test"
#predictions = predict_folder(test_folder)



# 269*4 개 샘플로 학습 572s 2s/step - loss: 0.2106 - accuracy: 0.9388 - val_loss: 0.0118 - val_accuracy: 0.9926
