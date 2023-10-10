using CSBaseLib;
using CSCore;
using CSCore.Codecs.WAV;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

//In metaverse room ecah user voice synchronization with this script
public class SynMike : MonoBehaviour
{
    private AudioSource mRecordVoiceMike_Source;
   // private AudioSource mOuputSoundCard_Source;
   // private AudioSource mOutputVoiceMike_Source;

    public bool isMine = false;

    public bool masterMikeFlag = true; // 선생님이 설정한것, 기본은 false, 선생님이 말을 할 수 있도록 설정하면 true

    private Queue<PKTAudioData> mPacket_AudioData_Mike = new Queue<PKTAudioData>(); // 가공이 끝난 패킷 데이터

    private GameObject mOnceAudio_Obj;

    private List<float> mReciveStudentAudioBuffer = new List<float>();
    private List<float> mReciveTeacherAudioBuffer = new List<float>();
    private List<float> mSendMikeAudioBuffer = new List<float>();

    private int mMikeRecordTime = 1;

    private int mCurrentMikeOffest;

    private int mReviceCount_StudentMike = 0;
    private int mReciveCount_TeacherMike = 0;
    private int mMaxReciveCount_TeacherMike = 2;
    private int mMaxReviceCound_StudentMike = 5;

    private int mReciveCount_SoundCard = 0;
    private int mMaxReciveCount_SoundCard = 5;

    private int mCurrentSendMike_Count = 0;
    private int mMaxSendAudio_Count = 0;

    private int mlastRecordVoicePos_Mike;
    private int mCurrentRecordVoicePos;


    public void SetMine()
    {
        isMine = true;
    }

    private void Start()
    {
        Init();
    }

    public void Init()
    {
        if (Microphone.devices.Length == 0)
        {
            return;
        }

        mCurrentMikeOffest = 0;

        //  mOuputSoundCard_Source = gameObject.transform.Find("SoundCardAudio").GetComponent<AudioSource>();
        //  mOutputVoiceMike_Source = gameObject.transform.Find("VoiceMikeAudio").GetComponent<AudioSource>();

        //  mOuputSoundCard_Source.clip = AudioClip.Create("ClipName", 44100, 2, 44100, false);

        mRecordVoiceMike_Source = gameObject.AddComponent<AudioSource>();
       // mRecordVoiceMike_Source = GameObject.Find("Voice").GetComponent<AudioSource>();

        StartCoroutine("RecordMike");

        while (Microphone.GetPosition(null) < 0) { }


        //------- 음성 출력 디폴트 설정값 -------//
        mRecordVoiceMike_Source.spatialBlend = 0.8f; 
     //   mOutputVoiceMike_Source.spatialBlend = 0.8f;
     //   mOuputSoundCard_Source.spatialBlend = 0.8f;

        mRecordVoiceMike_Source.volume = 0.97f;
      //  mOuputSoundCard_Source.volume = 0.97f;
     //   mOutputVoiceMike_Source.volume = 0.97f;
        //---------------------------------------//

       // mOnceAudio_Obj = Resources.Load("Prefab/Once_Audio") as GameObject;
    }

    private void OnDisable()
    {
        StopCoroutine("RecordMike");
    }

    public string GetCurrentMikeName()
    {
        return Microphone.devices[mCurrentMikeOffest].ToString();
    }

    public void ChangeMike()
    {
        mCurrentMikeOffest++;

        if (mCurrentMikeOffest >= Microphone.devices.Length)
        {
            mCurrentMikeOffest = 0;
        }

        if (mCurrentMikeOffest != 0)
        {
            if (Microphone.devices[mCurrentMikeOffest].ToString()[0].Equals("a") && Microphone.devices[mCurrentMikeOffest].ToString()[0].Equals("n")
                && Microphone.devices[mCurrentMikeOffest].ToString()[0].Equals("d") && Microphone.devices[mCurrentMikeOffest].ToString()[0].Equals("r"))
            {
                mRecordVoiceMike_Source.clip = Microphone.Start(Microphone.devices[0].ToString(), true, mMikeRecordTime, 8000);
            }
            else
            {
                mRecordVoiceMike_Source.clip = Microphone.Start(Microphone.devices[mCurrentMikeOffest].ToString(), true, mMikeRecordTime, 8000);
            }
        }
    }

    private IEnumerator RecordMike()
    {
        WaitForSeconds waitTime = new WaitForSeconds(mMikeRecordTime);

        mRecordVoiceMike_Source.clip = Microphone.Start(Microphone.devices[mCurrentMikeOffest].ToString(), true, mMikeRecordTime, 8000);


        while (true)
        {
            if ((mCurrentRecordVoicePos = Microphone.GetPosition(null)) > 0)
            {

                if (mlastRecordVoicePos_Mike > mCurrentRecordVoicePos) mlastRecordVoicePos_Mike = 0;

                if (mCurrentRecordVoicePos - mlastRecordVoicePos_Mike > 0)
                {
                    // Allocate the space for the sample.
                    float[] recordMikeData = new float[(mCurrentRecordVoicePos - mlastRecordVoicePos_Mike) * mRecordVoiceMike_Source.clip.channels];

                    // Get the data from microphone.
                    mRecordVoiceMike_Source.clip.GetData(recordMikeData, mlastRecordVoicePos_Mike);

                    float sum = 0;
                    for (int i = 0; i < recordMikeData.Length; i++)
                    {
                        sum += recordMikeData[i] * recordMikeData[i];
                    }

                    float value = 0;
                    value = Mathf.Sqrt(sum / recordMikeData.Length);

                    mlastRecordVoicePos_Mike = mCurrentRecordVoicePos;

                    if (value >= 0) // 해당 데시벨 넘어야지만 소리 전송, 현재는 무조건 출력
                    {
                       // if (GameManager.instance.userData.userSound.flagMike == true && Microphone.devices.Length != 0 && GameManager.instance.userData.isSoundCard == false) // 사운드 카드 동시에 안되게
                        //if (GameManager.Instance.userData.userSound.flagMike == true && Microphone.devices.Length != 0) // 사운드카드 동시에 되게
                        {
                            if (true) //  무조건 MasterMikeFlag True함
                            {
                                mCurrentSendMike_Count++;
                                for (int i = 0; i < recordMikeData.Length; i++)
                                {
                                    mSendMikeAudioBuffer.Add(recordMikeData[i]);
                                }

                                mMaxSendAudio_Count = 15;

                                //if (mCurrentSendMike_Count >= mMaxSendAudio_Count)
                                yield return new WaitForSeconds(1);
                                {
                                    mCurrentSendMike_Count = 0;
                                    byte[] sendBytes = ToByteArray(mSendMikeAudioBuffer.ToArray());

                                 

                                    if (GameManager.instance.userData.userId.Equals("홍길동")) // 수정 필요
                                    {
                                        var request = new CSBaseLib.PKTAudioData()
                                        {
                                            Audio_Data = SavWav.Save(mRecordVoiceMike_Source.clip),
                                            UserID = GameManager.instance.userData.userId,
                                            AudioType = 0,
                                            Channels = mRecordVoiceMike_Source.clip.channels,
                                            SampleRate = mRecordVoiceMike_Source.clip.samples,
                                            Frequnecy = mRecordVoiceMike_Source.clip.frequency,
                                        };
                                        GameManager.instance.tcpMainServerManager.SendAudioData_Mike(request);

                                    }
                                    else
                                    {
                                        var request_ = new CSBaseLib.PKTAudioData_Recive()
                                        {
                                            Audio_Data = SavWav.Save(mRecordVoiceMike_Source.clip),
                                            UserID = GameManager.instance.userData.userId,
                                            AudioType = 0,
                                            Channels = mRecordVoiceMike_Source.clip.channels,
                                            SampleRate = mRecordVoiceMike_Source.clip.samples,
                                            Frequnecy = mRecordVoiceMike_Source.clip.frequency,
                                        };
                                        GameManager.instance.tcpMainServerManager.SendAudioData_Recive_Mike(request_);
                                    }
                                  

                                    mSendMikeAudioBuffer.Clear();
                                }
                            }
                        }
                    }
                }
            }

            yield return null;
        }
        yield break;
    }


    public byte[] ToByteArray(float[] floatArray)
    {
        int len = floatArray.Length * 4;
        byte[] byteArray = new byte[len];
        int pos = 0;
        foreach (float f in floatArray)
        {
            byte[] data = System.BitConverter.GetBytes(f);
            System.Array.Copy(data, 0, byteArray, pos, 4);
            pos += 4;
        }
        if (byteArray.Length >= GameManager.instance.buildOption.maxPacketSize)
        {
            return null;
        }
        return byteArray;
    }

    public float[] ToFloatArray(byte[] byteArray)
    {
        int len = byteArray.Length / 4;
        float[] floatArray = new float[len];
        for (int i = 0; i < byteArray.Length; i += 4)
        {
            floatArray[i / 4] = System.BitConverter.ToSingle(byteArray, i);
        }
        return floatArray;
    }
}
