using CSBaseLib;
using UnityEngine;

//In metaverse room ecah user have synchronization data. And this script is that manager script.
public class SynUserInfo : MonoBehaviour
{
    private SynMike mSynMike;
    public bool IsMine;

    public SynMike synMike
    {
        get
        {
            if (mSynMike == null)
            {
                mSynMike = gameObject.AddComponent<SynMike>();

            }
            return mSynMike;
        }
        set
        {
            mSynMike = value;
        }
    }
   

    public void SetMine()
    {
        IsMine = true;

        if (synMike == null)
        {
            gameObject.GetComponent<SynMike>().SetMine();
        }

    }

    public void Init(int index, string userName, int UserMode) // 다른 캐릭터도 기본으로 호출 되는 것들, 자기 포함
    {
        IsMine = false;
       
    }

    public void Syn_AudioData_Mike(PKTAudioData data) // 선생님,학생의 음성데이터를 받을 때
    {
        if (data.Audio_Data != null)
        {
            if (data.Audio_Data.Length != 0)
            {
            }
        }
    }

}
