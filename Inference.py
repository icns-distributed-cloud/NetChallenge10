import argparse
from tqdm import tqdm

import pandas as pd
import numpy as np
import torch
from torch.utils.data.dataloader import DataLoader
from sklearn.metrics import accuracy_score, recall_score, precision_score, f1_score, confusion_matrix

from merdataset import *
from config import *
from utils import *

import time

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
        default=test_config['cuda'],
        help='cuda'
    )

    parser.add_argument(
        '--model_name',
        type=str,
        help='checkpoint name to load'
    )

    parser.add_argument(
        '--one_data',
        type=str,
        help='one_data path for inference '
    )
    '''
    parser.add_argument(
        '--do_clf',
        action='store_true',
    )
    '''
    args = parser.parse_args()
    return args


args = parse_args()
if args.cuda != 'cuda:0':
    test_config['cuda'] = args.cuda


def test(model, test_data):
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
    test_data = {
        'wav' : args.one_data,
    }
    # 모델 불러오기
    model = torch.load('./ckpt/{}.pt'.format(args.model_name))
    result = test(model, test_data)
    print("실제 음성일 확률:", round(result[0].item()*100, 2), "%")
    print("합성 음성일 확률:", round(result[1].item()*100, 2), "%")

    if result[0] >= result[1]:
        print("정상 음성입니다.")
    else:
        print("Deep Voice 입니다.")
    

if __name__ == '__main__':
    import os
    os.environ['CUDA_LAUNCH_BLOCKING'] = "0"
    main()
