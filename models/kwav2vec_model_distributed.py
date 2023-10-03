import os
import torch
from torch import nn

import soundfile as sf
from tqdm import tqdm

from transformers import Wav2Vec2Processor, Wav2Vec2Model

class Kwav2vec_feature_extractor():
    def __init__(self, config):
        self.args = config
        self.file_path = self.args.path
        self.max_len = self.args.max_length
        self.processor = Wav2Vec2Processor.from_pretrained("kresnik/wav2vec2-large-xlsr-korean")
        self.encoder = Wav2Vec2Model.from_pretrained("kresnik/wav2vec2-large-xlsr-korean")
        self.feature_extractor = self.encoder.feature_extractor
        self.feature_projection = self.encoder.feature_projection
        #self.mask_hidden_states = self.encoder._mask_hidden_states
        self.len_ = len(os.listdir(self.args.path))

        del self.encoder
        self.feature_extractor = self.feature_extractor.to(self.args.cuda)
        self.feature_projection = self.feature_projection.to(self.args.cuda)

    def readfile(self,path):
        wav, _ = sf.read(path)
        if len(wav.shape) > 1:
            wav = wav.reshape((1,-1)).squeeze()
        return wav

    def __call__(self,data):
        wav = self.readfile(data['wav'])

        processor_output = self.processor(wav, sampling_rate=16000, return_attention_mask=True, return_tensors='pt')
        processor_output = processor_output.to(self.args.cuda)
        processor_output = processor_output.input_values

        feature_extractor_output = self.feature_extractor(processor_output)
        feature_extractor_output = feature_extractor_output.transpose(1, 2)
        #print("feature_extractor_output", feature_extractor_output.shape)
        
        hidden_states, _ = self.feature_projection(feature_extractor_output)
        #print(hidden_states.shape)
        #hidden_states = self._mask_hidden_states(hidden_states)
        torch.cuda.empty_cache()
        return hidden_states
    
class Kwav2vec_encoder():
    def __init__(self, config):
        self.args = config
        self.file_path = self.args.path
        self.max_len = self.args.max_length
        self.base_encoder = Wav2Vec2Model.from_pretrained("kresnik/wav2vec2-large-xlsr-korean")
        self.encoder = self.base_encoder.encoder
        self.len_ = len(os.listdir(self.args.path))

        del self.base_encoder
        self.encoder = self.encoder.to(self.args.cuda)
        
    def __call__(self,hidden_states):
        torch.cuda.empty_cache()
        encoder_outputs = self.encoder(
            hidden_states,
            output_hidden_states=False
        )
        return encoder_outputs[0]

class Kwav2vec_classfier(nn.Module):
    def __init__(self, audio_config, classifier_config):
        super().__init__()

        self.audio_args = audio_config
        self.args = classifier_config

        self.feature_extractor = Kwav2vec_feature_extractor(self.audio_args)
        self.audio_encoder = Kwav2vec_encoder(self.audio_args)

        self.num_heads = self.args.num_heads
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
        predicted = torch.Tensor().to(self.args.cuda)

        for data in batch:
            # audio encoding
            hidden_states = self.feature_extractor(data)
            audio_out = self.audio_encoder(hidden_states)

            # 음성 데이터만 사용하는 음성 교사 모델 훈련시 forward
            audio_out = self._conv1d(audio_out)
            audio_out = audio_out.transpose(1, 2)
            #print(audio_out.shape) #Torch.size([16, 768, 512])
            
            audio_out = self.avgpool(audio_out)
            audio_out = torch.squeeze(audio_out)
            #print(audio_out.shape) #Torch.size([16, 768])
            predicted = torch.cat([predicted,self.classifier(audio_out)],dim=0)

        return predicted