import argparse
import random
from tqdm import tqdm

import torch
import numpy as np
import pandas as pd
from torch.utils.data.dataloader import DataLoader

#from models.kwav2vec_model import *
from models.kwav2vec_model_distributed import *
from merdataset import *
from config import *
from utils import *
import time

args = None

def parse_args():
    parser = argparse.ArgumentParser(description='get arguments')
    parser.add_argument(
        '--epochs',
        default=train_config['epochs'],
        type=int,
        required=False,
        help='epochs'
    )
    parser.add_argument(
        '--batch',
        default=train_config['batch_size'],
        type=int,
        required=False,
        help='batch size'
    )
    parser.add_argument(
        '--shuffle',
        default=False,
        required=False,
        help='shuffle'
    )
    parser.add_argument(
        '--lr',
        default=train_config['lr'],
        type=float,
        required=False,
        help='learning rate'
    )
    parser.add_argument(
        '--cuda',
        default='cuda:0',
        help='class weight'
    )

    parser.add_argument(
        '--save',
        default=True,
        action='store_true',
        help='save checkpoint'
    )

    parser.add_argument(
        '--model_name',
        type=str,
        default='test',
        help='checkpoint name to load or save'
    )

    args = parser.parse_args()
    return args

args = parse_args()
if args.cuda != 'cuda:0':
    audio_config['cuda'] = args.cuda
    classifier_config['cuda'] = args.cuda
    train_config['cuda'] = args.cuda


def train(model, feature_extractor, optimizer, dataloader):
    model.train()
    
    # 각 발화 및 스크립트 별 평가자들의 평가결과를 Softmax로 사용, MSEloss를 이용해 학습
    loss_func = torch.nn.CrossEntropyLoss(ignore_index=-1).to(train_config['cuda'])

    tqdm_train = tqdm(total=len(dataloader), position=1)
    accumulation_steps = train_config['accumulation_steps']
    loss_list = []
    
    for batch_id, batch in enumerate(dataloader):
        batch_x, batch_y = batch[0], batch[1]
        hidden_state_batch = [feature_extractor(data) for data in batch_x]
        outputs = model(hidden_state_batch)
        loss = loss_func(outputs.to(torch.float32).to(train_config['cuda']), batch_y.to(torch.float32).to(train_config['cuda']))
        loss_list.append(loss.item())
        
        tqdm_train.set_description('loss is {:.2f}'.format(loss.item()))
        tqdm_train.update()
        loss = loss / accumulation_steps
        loss.backward()
        if batch_id % accumulation_steps == 0:
            optimizer.step()
            optimizer.zero_grad()
    optimizer.zero_grad()
    tqdm_train.close()
    print("Train Loss: {:.5f}".format(sum(loss_list)/len(loss_list)))

def main():
    audio_conf = pd.Series(audio_config)
    classifier_conf = pd.Series(classifier_config)

    print(audio_conf)
    print(classifier_conf)
    print(train_config)

    #audio_conf['path'] = './TOTAL/Extracted_Dataset/'

    # 데이터셋 불러오기
    dataset = MERGEDataset(data_option='train', path='./data/')

    # 모델 생성
    feature_extractor = Kwav2vec_feature_extractor(audio_conf)
    model = Kwav2vec_classfier(audio_conf, classifier_conf)

    device = args.cuda
    print('---------------------',device)

    feature_extractor = feature_extractor
    model = model.to(device)
    optimizer = torch.optim.AdamW(params=model.parameters(), lr=args.lr)

    if 'ckpt' not in os.listdir():
        os.mkdir('ckpt')

    print(model)
    get_params(model)

    if args.save:
        print("checkpoint will be saved every 5epochs!")

    for epoch in range(args.epochs):
        print(epoch, "epoch start!")
        dataloader = DataLoader(dataset, batch_size=args.batch, shuffle=args.shuffle,
                                    collate_fn=lambda x: (x, torch.FloatTensor([i['label'] for i in x])))
        train(model, feature_extractor, optimizer, dataloader)
        
        # 5의 배수 epoch마다 모델 저장
        if (epoch+1) % 5 == 0:
            if args.save:
                torch.save(model,'./ckpt/{}_epoch{}.pt'.format(args.model_name,epoch))



if __name__ == '__main__':
    import os
    os.environ['CUDA_LAUNCH_BLOCKING'] = "0"
    start_time = time.time()
    main()
    end_time = time.time()
    print("Total Training time is : ", end_time-start_time)
