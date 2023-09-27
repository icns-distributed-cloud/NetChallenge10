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
        PhotonNetwork.ConnectUsingSettings(); // ���� ���ἳ��     //������ ����
    }

    public GameObject character;

    public byte userNum = 5;

    private bool connect = false;




    //���� �Ǹ� ȣ��
    public override void OnConnectedToMaster()
    {
        Debug.Log("�������ӿϷ�");
        string nickName = PhotonNetwork.LocalPlayer.NickName;
        Debug.Log("����� �̸��� " + nickName + " �Դϴ�.");
        connect = true;

        JoinRoom();
    }

    //���� ����
    public void Disconnect() => PhotonNetwork.Disconnect();
    //���� ������ �� ȣ��
    public override void OnDisconnected(DisconnectCause cause) => print("�������");

    //�� ����
    public void JoinRoom()
    {
        if (connect)
        {
            PhotonNetwork.JoinRandomRoom();
        }
    }

    //���� �� ���忡 �����ϸ� ���ο� �� ���� (master �� ����)
    public override void OnJoinRandomFailed(short returnCode, string message) =>
    PhotonNetwork.CreateRoom("1", new RoomOptions { MaxPlayers = userNum });

    //�濡 ���� ���� �� ȣ�� 
    public override void OnJoinedRoom()
    {
        character = PhotonNetwork.Instantiate("Cube", Vector3.zero, Quaternion.identity, 0);
    }

}
