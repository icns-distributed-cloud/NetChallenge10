import argparse
import pandas as pd
import numpy as np
import torch
from torch.utils.data.dataloader import DataLoader
from sklearn.metrics import accuracy_score, recall_score, precision_score, f1_score, confusion_matrix
from config import *
from utils import *

from logging import handlers
import logging

from datetime import datetime
import ftplib
import shutil
import time
import os

def parse_args():
    parser = argparse.ArgumentParser(description='get arguments')
    parser.add_argument(
        '--batch',
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
        default='./Audio',
        type=str,
        help='data path for inference '
    )

    parser.add_argument(
        '--result_path',
        type=str,
        default= './Audio/Result',
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
        default=-1,
        help='set round number'
    )
    parser.add_argument(
        '--fake_line',
        type=float,
        required=False,
        default=0.7,
        help='set round number'
    )
    parser.add_argument(
        '--copy_path',
        type=str,
        required=False,
        default='./Audio/to_core',
        help='set round number'
    )
    args = parser.parse_args()
    return args


args = parse_args()
if args.cuda != 'cuda:0':
    audio_config['cuda'] = args.cuda

def get_today():
    return ('-').join(list(map(str, [datetime.today().year, datetime.today().month, datetime.today().day])))

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

def main(today_date):
    # Setting Variables
    count = 0
    present = get_today()

    # Set log
    ## log settings
    LogFormatter = logging.Formatter('%(asctime)s,%(message)s')

    ## handler settings
    os.makedirs('log', exist_ok=True)
    LogHandler = handlers.TimedRotatingFileHandler(filename='./log/Inferencing.log', when='midnight', interval=1, encoding='utf-8')
    LogHandler.setFormatter(LogFormatter)
    LogHandler.suffix = "%Y%m%d"

    ## logger set
    Logger = logging.getLogger()
    Logger.setLevel(logging.INFO)
    Logger.addHandler(LogHandler)

    # Get Time
    if (present > today_date):
        count = 0
        shutil.rmtree(os.path.join(args.copy_path, today_date))
    else:
        today_date = present

    
    # Model setting
    # 모델 불러오기
    model = torch.load('./ckpt/{}.pt'.format(args.model_name), map_location=torch.device(args.cuda))
    
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
        file_list = os.listdir(data_path)
        owner_name = file_list.pop().split('_')[0]
        file_name = owner_name + '_' + str(len(file_list)) + '.wav'
        test_data = {
            'wav' : os.path.join(data_path, file_name),
        }
        #use logger
        Logger.info("read {}".format(test_data['wav']))

        # inferencing
        result = inference(model, test_data)
        
        # if it is fake voice
        os.makedirs(os.path.join(args.copy_path, today_date), exist_ok=True)

        if ((result[0] < result[1]) and (result[1] > args.fake_line)):
            file_result = 'fake'
            os.makedirs(args.result_path, exist_ok=True)
            result_path = os.path.join(args.result_path, 'Result.txt')
            with open(result_path, 'w') as f:
                data = owner_name
                f.write(data)

        else:
            file_result = 'real'

        os.makedirs(os.path.join(args.copy_path, today_date, file_result), exist_ok=True)
        shutil.copy(test_data['wav'], os.path.join(args.copy_path, today_date, file_result, file_name))
        
        #use logger
        Logger.info("write {} as {} data".format(os.path.join(args.copy_path, today_date, file_result, file_name), file_result))
        time.sleep(args.sleep_time)
        
if __name__ == '__main__':
    import os
    os.environ['CUDA_LAUNCH_BLOCKING'] = "0"
    main(get_today())
