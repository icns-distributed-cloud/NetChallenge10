import os
import json

def Save_To_dataset(ori_data):
    txt_to_label = {
        'real' : 0, 
        'fake' : 1
    }
    
    dic = {}

    for item in ori_data['data']:
        dic[item['wav']] = txt_to_label[item['label']]
    
    return dic

with open('../NetChallenge10/data/test_preprocessed_data.json', 'r') as f:
    ori_test_data = json.load(f)
with open('../NetChallenge10/data/train_preprocessed_data.json', 'r') as f:
    ori_train_data = json.load(f)


train_dataset = Save_To_dataset(ori_train_data)
test_dataset = Save_To_dataset(ori_test_data)

with open('./train_dataset.json', 'w') as outfile:
    json.dump(train_dataset, outfile)

with open('./test_dataset.json', 'w') as outfile:
    json.dump(test_dataset, outfile)