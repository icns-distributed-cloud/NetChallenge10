import json

# JSON 파일 로딩
with open('your_modified_file.json', 'r') as f:
    data = json.load(f)

# 문자열 대체
modified_data = {}
for key, value in data.items():
    new_key = key.replace('/root/\uc0c8 \ud3f4\ub354/Dataset/voice_output -test', '/content/data/voice_output -test')  # 키에서 문자열 대체
    new_value = value  # 값을 변경하려면 이 부분을 수정하세요.
    modified_data[new_key] = new_value

# 결과를 새 JSON 파일로 저장
with open('your_modified_file2.json', 'w') as f:
    json.dump(modified_data, f, indent=4)
