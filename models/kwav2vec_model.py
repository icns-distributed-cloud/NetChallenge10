import os
import torch
from torch import nn

import soundfile as sf
from tqdm import tqdm

from transformers import Wav2Vec2Processor, Wav2Vec2Model

'''
class SpeechExtractorForCrossAttention():
    def __init__(self, config):
        self.args = config
        self.file_path = self.args.path
        self.max_len = self.args.max_length

        # pretrained
        self.processor = Wav2Vec2Processor.from_pretrained("kresnik/wav2vec2-large-xlsr-korean")
        self.encoder = Wav2Vec2Model.from_pretrained("kresnik/wav2vec2-large-xlsr-korean")
  
        # 음성파일데이터를 모두 미리 인코딩하여 데이터셋 생성
        # 한번 인코딩해두면, 이후 재 학습할 때 빠르게 학습할 수 
        #if 'hidden_states' not in os.listdir(self.args.path):
        print("Wav Embedding Save")
        os.makedirs(self.args.path + 'hidden_states', exist_ok=True)
        embed_path = self.args.path + 'hidden_states/'
        self.encoder.to(self.args.cuda)
        len_ = len(os.listdir(self.args.path))
        # if 'hidden_state.json' not in embed_files or 'extract_feature.json' not in embed_files:
        #self.encoder.eval()
        with torch.no_grad():
            for idx, i in enumerate(os.listdir(self.args.path)):
                print('{}/{}'.format(idx + 1, len_))
                name, ext = os.path.splitext(i)
                if ext == '.wav' and not (os.path.isfile(embed_path+name+'.pt')):
'''
'''
                        for j in tqdm(os.listdir(self.args.path)):
                            if os.path.splitext(j)[-1] == '.wav':
                                wav = self.readfile(j)
                                encoded = self._encoding(wav, output_hidden_state=False)
                                pooled_hidden = encoded.last_hidden_state
                                torch.save(pooled_hidden, embed_path + j[:-4] + '.pt')
                                torch.cuda.empty_cache()
'''
'''
                    wav = self.readfile(i)
                    encoded = self._encoding(wav, output_hidden_state=False)
                    pooled_hidden = encoded.last_hidden_state
                    torch.save(pooled_hidden, embed_path + i[:-4] + '.pt')
                    torch.cuda.empty_cache()

            print("Wav Embedding Save finished")

    def readfile(self,file_name):
        if file_name[0] in ['M', 'F']:
            path = self.args.path + 'emotiondialogue/' + file_name
        else:
            path = self.args.path + file_name
        wav, _ = sf.read(path)
        return wav

    def _encoding(self,raw_wav,output_hidden_state=False):
        extract_feature = encoding(raw_wavs=raw_wav,
                                   cuda=self.args.cuda,
                                   encoder=self.encoder,
                                   processor=self.processor,
                                   return_hidden_state=output_hidden_state)

        return extract_feature

    def __call__(self,batch):
        hidden_batch = torch.Tensor().to(self.args.cuda)
        file_name = [data['wav'][:-4]+'.pt' for data in batch]

        for data in file_name:
            # 미리 인코딩한 데이터셋 
            hidden = torch.load(self.file_path+'hidden_states/'+data,map_location=self.args.cuda)
            seq = hidden.size()[1]
            if seq > self.max_len:
                # truncation
                hidden = hidden[:,:self.max_len,:].to(self.args.cuda)
            elif seq < self.max_len:
                
                # padding
                pad = torch.Tensor([[[0]*1024]*(self.max_len-seq)]).to(self.args.cuda)
                hidden = torch.cat([hidden,pad], dim=1)
            try:
                hidden_batch = torch.cat([hidden_batch,hidden],dim=0)
            except:
                continue
        return hidden_batch
'''
def encoding(raw_wavs,cuda, processor=None, encoder=None, return_hidden_state=False):
    assert bool(processor) == bool(encoder)

    # Audio reshape
    if len(raw_wavs.shape) > 1:
        raw_wavs = raw_wavs.reshape((1,-1)).squeeze()
    inputs = processor(raw_wavs,
                       sampling_rate=16000,
                       return_attention_mask=True,
                       return_tensors="pt")
    inputs = inputs.to(cuda)
    encoder = encoder.to(cuda)
    outputs = encoder(output_hidden_states=return_hidden_state, **inputs)
    torch.cuda.empty_cache()
    return outputs

class Kwav2vec():
    def __init__(self, config):
        self.args = config
        self.file_path = self.args.path
        self.max_len = self.args.max_length
        self.processor = Wav2Vec2Processor.from_pretrained("kresnik/wav2vec2-large-xlsr-korean")
        self.encoder = Wav2Vec2Model.from_pretrained("kresnik/wav2vec2-large-xlsr-korean")
        self.len_ = len(os.listdir(self.args.path))

    def encoding_one_data(self, data):
        os.makedirs(self.args.path + 'hidden_states', exist_ok=True)
        embed_path = self.args.path + 'hidden_states/'
        self.encoder.to(self.args.cuda)
        
        with torch.no_grad():
            wav = self.readfile(data['wav'])
            encoded = self._encoding(wav, output_hidden_state=False)
            pooled_hidden = encoded.last_hidden_state

        return pooled_hidden

    def readfile(self,path):
        wav, _ = sf.read(path)
        return wav

    def _encoding(self,raw_wav,output_hidden_state=False):
        extract_feature = encoding(raw_wavs=raw_wav,
                                   cuda=self.args.cuda,
                                   encoder=self.encoder,
                                   processor=self.processor,
                                   return_hidden_state=output_hidden_state)

        return extract_feature

    def __call__(self,batch):

        hidden_batch = torch.Tensor().to(self.args.cuda)

        for data in batch:
            hidden = self.encoding_one_data(data)
            seq = hidden.size()[1]

            if seq > self.max_len:
                # truncation
                hidden = hidden[:,:self.max_len,:].to(self.args.cuda)
            elif seq < self.max_len:
                # padding
                pad = torch.Tensor([[[0]*1024]*(self.max_len-seq)]).to(self.args.cuda)
                hidden = torch.cat([hidden,pad], dim=1)

            hidden_batch = torch.cat([hidden_batch,hidden],dim=0)

        return hidden_batch



class Kwav2vec_classfier(nn.Module):
    def __init__(self, audio_config, multi_modal_config):
        super().__init__()

        self.audio_args = audio_config
        self.args = multi_modal_config

        self.audio_encoder = Kwav2vec(self.audio_args)

        self.num_heads = self.args.num_heads
        self.layers = self.args.layers
        self.attn_dropout = self.args.attn_dropout
        self.relu_dropout = self.args.relu_dropout
        self.res_dropout = self.args.res_dropout
        self.embed_dropout = self.args.embed_dropout

        input_dim = self.args.projection_dim

        self.classifier = nn.Sequential(
            nn.Dropout(self.args.dropout),
            nn.Linear(input_dim, self.args.output_dim),
            nn.GELU(),
            nn.Dropout(self.args.dropout),
            nn.Linear(self.args.output_dim, self.args.num_labels)
        ).to(self.args.cuda)

        self.projection = nn.Conv1d(1024, self.args.projection_dim, kernel_size=1, padding=0, bias=False).to(self.args.cuda)
        self.avgpool = nn.AdaptiveAvgPool1d(1)
        self.flatten = nn.Flatten()


    def _conv1d(self, input_features):
        hidden_states = input_features.transpose(1,2).contiguous()  # -> B x (D x L)
        hidden_states = self.projection(hidden_states)
        out = hidden_states.transpose(1, 2).contiguous()            # -> B x (L x D)
        return out


    def forward(self, batch):
        """
        text, audio should have dimension [batch_size, seq_len, n_features]
        """            
        # 음성 데이터만 사용하는 음성 교사 모델 훈련시 forward
        audio_out = self.audio_encoder(batch)
        audio_out = self._conv1d(audio_out)
        
        return self.classifier(audio_out)