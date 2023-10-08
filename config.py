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


