using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    void Start()
    {
        init();
    }

    private void init()
    {
        PhotonNetwork.LocalPlayer.NickName = GameManager.instance.userData.userName;
        PhotonNetwork.GameVersion = "1";
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.ConnectUsingSettings(); // 포톤 연결설정     //서버에 접속
    }

    public GameObject character;

    public byte userNum = 5;

    private bool connect = false;




    //연결 되면 호출
    public override void OnConnectedToMaster()
    {
        Debug.Log("서버접속완료");
        string nickName = PhotonNetwork.LocalPlayer.NickName;
        Debug.Log("당신의 이름은 " + nickName + " 입니다.");
        connect = true;

        JoinRoom();
    }

    //연결 끊기
    public void Disconnect() => PhotonNetwork.Disconnect();
    //연결 끊겼을 때 호출
    public override void OnDisconnected(DisconnectCause cause) => print("연결끊김");

    //방 입장
    public void JoinRoom()
    {
        if (connect)
        {
            PhotonNetwork.JoinRandomRoom();
        }
    }

    //랜덤 룸 입장에 실패하면 새로운 방 생성 (master 방 생성)
    public override void OnJoinRandomFailed(short returnCode, string message) =>
    PhotonNetwork.CreateRoom("1", new RoomOptions { MaxPlayers = userNum });

    //방에 입장 했을 때 호출 
    public override void OnJoinedRoom()
    {
        character = PhotonNetwork.Instantiate("Cube", Vector3.zero, Quaternion.identity, 0);
    }

}
