using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

//*****This script is very important.*****
//In this sicrpt, when join Metaverse space, all server conenction and communication process in this scirpt.
//This scirpt control about Connect Metaverse, Screen Data(Socket), Audio Data(Socekt).
//This script check class start&end time. and use send user's data(Quiz score, lecture participation score, etc...) at Metaverse DB.
public class MainServerProc : MonoBehaviour
{
    private bool mParticipationLevel_Flag;

    // Start is called before the first frame update
    private void Start()
    {
        Init();
    }

    private void Init()
    {
        mParticipationLevel_Flag = false;
        StartCoroutine("Start_MainProc");
    }

    private void OnDisable() // When this compoent off, then have kill this process.
    {
        try
        {
            StopAllCoroutines();
        }
        catch
        {
        }
    }

    private void OnDestroy() // When this compoent deleted, then have kill this process.
    {
        try
        {
            StopAllCoroutines();
        }
        catch
        {
        }
    }

    public void Save_ParticipationLevel_Button() // Save lecture participation rate button
    {
        if (mParticipationLevel_Flag == true)
        {
            return;
        }
        StartCoroutine("API_ParticipationLevel");
    }

    private IEnumerator Start_MainProc()
    {
       // UnityObject.OnActive(GameManager.instance.uIManager.popUpGroup.transform.Find("LoaddingPopup").gameObject);

       // GameManager.instance.userData.lecture_Data_Res.mapMaxUser = 50; // test
        GameManager.instance.userData.tcpServerIp = "45.248.75.170"; // test
       // GameManager.instance.userData.tcpServerIp = "127.0.0.1"; // test

        GameManager.instance.tcpMainServerManager.Init(GameManager.instance.userData.tcpServerIp, GameManager.instance.buildOption.tcpServerPort); // 먼저 TCP 서버 접속 후
      
        while (true) // check user join room in socket server
        {
            if (GameManager.instance.tcpMainServerManager.clientStage == (int)CLIENT_STATE.ROOM) // if clear join room in socket, then break this while loop.
            {
                break;
            }
            yield return null;
        }

        yield return new WaitForSeconds(3);

      //  UnityObject.OffActive(GameManager.instance.uIManager.popUpGroup.transform.Find("LoaddingPopup").gameObject);
        StartCoroutine("SendAllData");

      
        yield break;
    }


    IEnumerator SendAllData()
    {
        while (true)
        {
            yield return null;
        }
        yield break;
    }
}
