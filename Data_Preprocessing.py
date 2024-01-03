from sklearn.model_selection import train_test_split
from config import *
import json
import os

def search_wav(label, path):
    temp = []

    for file in os.listdir(path):
        file_path = os.path.join(path, file)

        if(os.path.isdir(file_path)):
            temp += search_wav(label, file_path)
        elif(os.path.splitext(file_path)[-1] == '.wav'):
            temp.append([label, file_path])
    
    return temp


def Generate_Balanced_Dataset(larger_dataset, smaller_dataset):
    import random
    return random.sample(larger_dataset, len(smaller_dataset))
    
def Save_As_Json(PATH, data_list, file_name):
    base_json = {"data":[]}

    for label, path in data_list:
        data = {
            "label" : label,
            "wav" : path
        }
        base_json['data'].append(data)

    # save preprocessed data
    with open(os.path.join(PATH, file_name),'w') as j:
        json.dump(base_json,j,ensure_ascii=False, indent=4)



if __name__ == '__main__':
    args = dataset_config

    # check folders
    print('real_dataset_folders:', args['real_dataset_folders'])
    real = args['real_dataset_folders']

    print('fake_dataset_folders:', args['fake_dataset_folders'])
    generated = args['fake_dataset_folders']

    # Set PATH
    PATH = os.path.join(os.getcwd(), 'dataset')
    print('dataset path:', PATH)
    os.makedirs(PATH, exist_ok=True)

    # real_dataset_list
    label = 'real'
    real_dataset_list = []
    for folder in real:
        real_dataset_list += search_wav(label, os.path.join(PATH, folder))

    # fake_dataset_list
    label = 'fake'
    fake_dataset_list = []
    for folder in generated:
        fake_dataset_list += search_wav(label, os.path.join(PATH, folder))

    print('length of real:', len(real_dataset_list))
    print('length of fake:', len(fake_dataset_list))

    if len(real_dataset_list) > len(fake_dataset_list):
        real_dataset_list = Generate_Balanced_Dataset(real_dataset_list, fake_dataset_list)
    else:
        fake_dataset_list = Generate_Balanced_Dataset(fake_dataset_list, real_dataset_list)

    print('length of balanced:', len(fake_dataset_list), len(real_dataset_list))

    PATH = os.path.join(os.getcwd(), 'data')
    print('save dataset path:', PATH)
    os.makedirs(PATH, exist_ok=True)

    # Save dataset
    Save_As_Json(PATH, real_dataset_list+fake_dataset_list, 'balanced_total_data.json')

    #Save Split dataset
    train_data, test_data = train_test_split(real_dataset_list+fake_dataset_list, train_size=args['train_size'], test_size=args['test_size'], random_state=args['random_state'], shuffle=args['shuffle'])
    Save_As_Json(PATH, train_data, 'train_preprocessed_data.json')
    print('Trainset saved')
    Save_As_Json(PATH, test_data, 'test_preprocessed_data.json')
    print('Testset saved')