import argparse
import pandas as pd
import numpy as np
import torch
from torch.utils.data.dataloader import DataLoader
from sklearn.metrics import accuracy_score, recall_score, precision_score, f1_score, confusion_matrix
from config import *
from utils import *

import time
import os

def parse_args():
    parser = argparse.ArgumentParser(description='get arguments')
    parser.add_argument(
        '--batch',
        default=test_config['batch_size'],
        type=int,
        required=False,
        help='batch size'
    )

    parser.add_argument(
        '--cuda',
        default='cuda:0' if torch.cuda.is_available() else 'cpu',
        help='cuda'
    )

    parser.add_argument(
        '--model_name',
        type=str,
        default='DeepvoiceDetector_latest',
        help='checkpoint name to load'
    )

    parser.add_argument(
        '--data_path',
        required=False,
        default='./data',
        type=str,
        help='data path for inference '
    )

    parser.add_argument(
        '--result_path',
        type=str,
        default= './result.txt',
        help='the result path'
    )
    parser.add_argument(
        '--sleep_time',
        type=int,
        required=False,
        default=1,
        help='sleep time every round'
    )
    parser.add_argument(
        '--round_num',
        type=int,
        required=False,
        default=50,
        help='set round number'
    )
    parser.add_argument(
        '--fake_line',
        type=float,
        required=False,
        default=0.7,
        help='set round number'
    )
    args = parser.parse_args()
    return args


args = parse_args()
if args.cuda != 'cuda:0':
    test_config['cuda'] = args.cuda


def inference(model, test_data):
    start_time = time.time()
    model.eval()

    softmax = torch.nn.Softmax(dim=0)
    with torch.no_grad():
        batch_x = [test_data]
        outputs = model(batch_x)
        outputs = softmax(outputs)
        
    end_time = time.time()
    
    inference_time = end_time-start_time
    print("inference time : ", round(inference_time, 2), "초")
    return outputs

def main():
    count = 0
    while(True):
        if((args.round_num > 0) and (count > args.round_num)):
            break
        else:
            count += 1
        if not os.path.isfile('./ckpt/{}.pt'.format(args.model_name)):
            print('./ckpt/{}.pt'.format(args.model_name), 'have no model')
            break
        
        if count % 2 == 0:
            data_type = 'send'
        else:
            data_type = 'recive'

        data_path = os.path.join(args.data_path, data_type)
        file_name = str(len(os.listdir(data_path))) + '.wav'
        test_data = {
            'wav' : os.path.join(data_path, file_name),
        }

        # 모델 불러오기
        model = torch.load('./ckpt/{}.pt'.format(args.model_name))
        result = inference(model, test_data)
        
        # if it is fake voice
        if ((result[0] < result[1]) and (result[1] > args.fake_line)):
            with open(args.result_path, 'w') as f:
                data = data_type
                f.write(data)

        time.sleep(args.sleep_time)

        if count % 100 == 0:
            print('data_cleared')
            for data_type in ['send', 'recive']:
                data_path = os.path.joint(args.data_path, data_type)
                for wavs in os.listdir(data_path):
                    os.remove(os.path.join(data_path, wavs))
        
if __name__ == '__main__':
    import os
    os.environ['CUDA_LAUNCH_BLOCKING'] = "0"
    main()
