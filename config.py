dataset_config = {
    'train_size' : 0.8,
    'test_size' : 0.2,
    'shuffle' : True,
    'random_state' : 123,
    #'real_dataset_folders': ['LJSpeech-1.1', 'data'],
    #'fake_dataset_folders': ['generated_audio']
    #'real_dataset_folders': ['data'],
    #'fake_dataset_folders': ['voice_output', 'voice_output_2']
    'real_dataset_folders': ['LJSpeech-1.1', 'data'],
    'fake_dataset_folders': ['generated_audio', 'voice_output', 'voice_output_2']
}
    
audio_config = {
    #'K': 1,
    #'output_dim': 256,
    #'use': 'hidden_state',
    #'num_label': 2,
    'path': './data/',
    'cuda': 'cuda:0',
    # about 10s of wav files
    'max_length' : 512
}

train_config = {
    'epochs': 100,
    #'epochs': 30,
    'batch_size': 16,
    'lr': 5e-5,
    'accumulation_steps': 8,
    'cuda': 'cuda:0'
}

test_config = {
    'batch_size': 16,
    'cuda': 'cuda:0'
}


classifier_config = {
    'projection_dim': 768,
    'output_dim': 512,
    'num_labels': 2,
    'dropout': 0.1,
    'cuda': 'cuda:0',
    'num_heads': 8,
    #'layers': 3,
    'attn_dropout': 0,
    'relu_dropout': 0,
    'res_dropout': 0,
    'embed_dropout': 0
}


