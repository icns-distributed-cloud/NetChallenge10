from sklearn.svm import LinearSVC
import os
import json
import numpy as np
import librosa
from sklearn.model_selection import train_test_split
from sklearn.metrics import classification_report
from sklearn.preprocessing import StandardScaler

# 데이터 로딩 및 특징 추출 함수
def extract_features(audio_file):
    try:
        y, sr = librosa.load(audio_file)
        
        # 1. MFCC
        mfccs = librosa.feature.mfcc(y=y, sr=sr)
        
        # 2. Zero-Crossing Rate
        zero_crossings = librosa.feature.zero_crossing_rate(y)
        
        # 3. Spectral Roll-off
        spectral_rolloff = librosa.feature.spectral_rolloff(y=y, sr=sr)
        
        # 4. Chroma Feature
        chroma_stft = librosa.feature.chroma_stft(y=y, sr=sr)
        
        # 5. Spectral Contrast
        spectral_contrast = librosa.feature.spectral_contrast(y=y, sr=sr)
        
        # 특징들을 하나의 배열로 합치기
        features = np.concatenate((np.mean(mfccs, axis=1), [np.mean(zero_crossings), np.mean(spectral_rolloff), np.mean(chroma_stft), np.mean(spectral_contrast)]))
        return features
    except Exception as e:
        print(f"Error processing {audio_file}: {str(e)}")    
        return None

# JSON 파일 로딩 및 데이터 및 라벨 생성
with open('train_dataset.json', 'r') as f:
    folder_label_mapping = json.load(f)

file_paths = []
labels = []

# 각 폴더 및 라벨에 대해
for folder_path, label in folder_label_mapping.items():
    # 폴더 내의 모든 파일에 대해
    for filename in os.listdir(folder_path):
        # 파일 확장자 확인하여 오디오 파일만 처리
        if filename.lower().endswith('.wav'):
            # 파일 경로 및 라벨 저장
            file_paths.append(os.path.join(folder_path, filename))
            labels.append(label)

X_and_y = [(extract_features(file), label) for file, label in zip(file_paths, labels)]

# None 값을 필터링합니다.
X_and_y = [(x, y) for x, y in X_and_y if x is not None]

print("필터링 완료")

# X와 y를 분리합니다.
X, y = zip(*X_and_y)
X = np.array(X)
y = np.array(y)

print("분리 완료")
scaler = StandardScaler()
X_scaled = scaler.fit_transform(X)
print("변환 완료")

X_train, X_test, y_train, y_test = train_test_split(X_scaled, y, test_size=0.2, random_state=42)

X_train = X_train.astype(np.float32)
y_train = y_train.astype(np.float32)
print("분할 완료")

svm_model = LinearSVC()
# 학습 시작 메시지 출력
print("Training the model...")
svm_model.fit(X_train, y_train)

# 학습 완료 메시지 출력
print("Training completed!")

#y_pred = svm_model.predict(X_test)
#print(classification_report(y_test, y_pred))