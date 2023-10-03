using CSBaseLib;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MessagePack;
using System.Linq;
using System.Threading;

struct PacketData
{
    public UInt16 DataSize;
    public UInt16 PacketID;
    public SByte Type;
    public byte[] BodyData;
}

enum CLIENT_STATE
{
    NONE = 0,
    CONNECTED = 1,
    LOGIN = 2,
    ROOM = 3
}

public enum LOG_LEVEL
{
    TRACE,
    DEBUG,
    INFO,
    WARN,
    ERROR,
    DISABLE
}

//Tcp Socker Manager. This script is very important, because In this script, control all packet data and use taht packet.
//Send and recive packet data. And this script is main socket thread.(Metaverse room server socket)
public class TcpServerManager : MonoBehaviour
{
    private CLIENT_STATE mClientState = CLIENT_STATE.NONE;

    public int clientStage
    {
        get
        {
            return (int)mClientState;
        }
    }

    public ConnectTcpServer connet_Server;

    private bool mIsNetworkThreadRunning = false;
    private bool mIsJoinRoom = false;

    private string mTcpServerIp;

    private int mTcpServerPort;
    private int mCheckConnetServerCount = 0;
    private int IsDisConnectedCount = 0; // ������ �ȵǴ� Ƚ��, ���� Ƚ�� ������ Ŭ�󿡼� ������ ������ ������ �������

    private System.Threading.Thread mNetworkReadThread = null;
    private System.Threading.Thread mNetworkSendThread = null;
    private System.Threading.Thread mPacketCheckThread = null;

    private PacketBufferManager mPacketBuffer = new PacketBufferManager();
    private Queue<PacketData> mRecvPacketQueue = new Queue<PacketData>();

    //---------------------������ ���� ��Ŷ ������---------------------//

    private Queue<PKTAllData_Student_List> mPacket_StudentData = new Queue<PKTAllData_Student_List>(); 
    private Queue<PKTAllData_Teacher_List> mPacket_TeacherData = new Queue<PKTAllData_Teacher_List>(); 

    private Queue<PKTNtfRoomUserList> mPacket_Room_UserList = new Queue<PKTNtfRoomUserList>(); 
    private Queue<PKTNtfRoomNewUser> mPacket_Room_NewUser = new Queue<PKTNtfRoomNewUser>();
    private Queue<PKTNtfRoomLeaveUser> mPacket_Room_UserLeave = new Queue<PKTNtfRoomLeaveUser>();
    private Queue<PKTResRoomLeave> mPacket_Room_SelfLeave = new Queue<PKTResRoomLeave>();
    private Queue<PKTNtfRoomChat> mPacket_Room_Chat = new Queue<PKTNtfRoomChat>();
    private Queue<PKTCloseClassRoom> mPacket_CloseClassRoom = new Queue<PKTCloseClassRoom>();

    private Queue<PKTPosition> mPacket_UserPosition = new Queue<PKTPosition>();

    private Queue<PKTStudentMike> mPacket_Mike = new Queue<PKTStudentMike>();
    private Queue<PKTAudioData> mPacket_AudioData_Mike = new Queue<PKTAudioData>(); 
    private Queue<PKTAudioData_SoundCard> mPacket_AudioData_SoundCard = new Queue<PKTAudioData_SoundCard>();
  
    private Queue<PKTTeacherScreen> mPacket_TeacherScreen = new Queue<PKTTeacherScreen>();

    //---------------------������ ���� ��Ŷ ������---------------------//

    private Queue<byte[]> mSendPacketList = new Queue<byte[]>(); // ������ ���� ��Ŷ ������

    private void Start_ConnectTcpServer()
    {
        while (true)
        {
            BackGroundProcess();
        }
    }

    private void SetSendAllData()
    {
        GameManager.instance.userData.isSendAllData = false;
    }

    private IEnumerator CheckUsePacket()
    {
        while (true)
        {
            lock (((System.Collections.ICollection)mPacket_Room_UserList).SyncRoot) // ���� ���� ������ ��, ������ ���� �������� ����
            {
                if (mPacket_Room_UserList.Count() > 0)
                {
                    GameManager.instance.userData.isSendAllData = true;
                    try
                    {
                        CancelInvoke("SetSendAllData");
                    }
                    catch
                    {
                    }
                    Invoke("SetSendAllData", 10);

                    PKTNtfRoomUserList data = mPacket_Room_UserList.Dequeue();

                    
                }
            }

            lock (((System.Collections.ICollection)mPacket_Room_NewUser).SyncRoot) // ���� ������ ����, ���ο� ������ ���� ����
            {
                if (mPacket_Room_NewUser.Count() > 0)
                {
                    GameManager.instance.userData.isSendAllData = true;
                    try
                    {
                        CancelInvoke("SetSendAllData");
                    }
                    catch
                    {
                    }
                    Invoke("SetSendAllData", 10);

                    PKTNtfRoomNewUser data = mPacket_Room_NewUser.Dequeue();

                    GameManager.instance.joinRoomManager.JoinNewUser(data.UserID, data.UserName, data.UserMode, data.UserAvatarType, data.UserIndex);
                }
            }

           

            lock (((System.Collections.ICollection)mPacket_AudioData_Mike).SyncRoot) // ����, �л��� ����� �����͸� ���� ��
            {
                if (mPacket_AudioData_Mike.Count() > 0)
                {
                    PKTAudioData data = mPacket_AudioData_Mike.Dequeue();
                    if (mPacket_AudioData_Mike.Count >= 10)
                    {
                        mPacket_AudioData_Mike.Clear();
                    }
                    GameManager.instance.joinRoomManager.SetAudioData(data);
                }
            }


            lock (((System.Collections.ICollection)mPacket_Room_SelfLeave).SyncRoot) // ������ ������ �泪���⸦ ������, �װ��� ����� ������
            {
                if (mPacket_Room_SelfLeave.Count() > 0)
                {
                    PKTResRoomLeave data = mPacket_Room_SelfLeave.Dequeue();

                    EndConnectSever();

                }
            }


            lock (((System.Collections.ICollection)mPacket_Room_UserLeave).SyncRoot) // ������ ���� ��
            {
                if (mPacket_Room_UserLeave.Count() > 0)
                {
                    PKTNtfRoomLeaveUser data = mPacket_Room_UserLeave.Dequeue();

                    GameManager.instance.joinRoomManager.ExitUser(data.UserID);
                }
            }


            yield return null;
        }

        yield break;
    }

    private void StudentEndClassRoomByTeacher() //�������� ���� ���Ḧ ���� �� ��, Invoke
    {
        EndConnectSever();
    }

    private void CheckConnectServer()
    {
        if (mIsJoinRoom == true)
        {
            return;
        }

        if (mCheckConnetServerCount >= 3)
        {
            EndConnectSever();
            return;
        }
        if (connet_Server.isConented == true) // ���� ���� ���� �Ǹ�
        {
            if (ConnetServer() == true) // ������ ���� ��ٴ� ���·� ����
            {
                RequestLogin(GameManager.instance.userData.userId, GameManager.instance.userData.userPassWord); // ���ӵǸ� �ٷ� ������ �α���
            }
            else
            {
                EndConnectSever();
            }
        }
        Invoke("CheckConnectServer", 3);
        mCheckConnetServerCount++;


        return;
    }

    public void Init(string tcpServerIp, int tcpServerPort)
    {
        mTcpServerIp = tcpServerIp;
        mTcpServerPort = tcpServerPort;

        connet_Server = new ConnectTcpServer();

        mPacketBuffer.Init((GameManager.instance.buildOption.maxPacketSize * 2), CSBaseLib.PacketDef.PACKET_HEADER_SIZE, GameManager.instance.buildOption.maxPacketSize);

        Debug.Log($"������ ���� �õ�: ip:{mTcpServerIp}, port:{mTcpServerPort}" + LOG_LEVEL.INFO);

        mIsNetworkThreadRunning = true;
        mNetworkReadThread = new System.Threading.Thread(this.NetworkReadProcess);
        mNetworkReadThread.Start();
        mNetworkSendThread = new System.Threading.Thread(this.NetworkSendProcess);
        mNetworkSendThread.Start();

        connet_Server.Connect(mTcpServerIp, mTcpServerPort);

        if (connet_Server.isConented == true) // ���� ���� ���� �Ǹ�
        {
            if (ConnetServer() == true) // ������ ���� ��ٴ� ���·� ����
            {
                RequestLogin(GameManager.instance.userData.userId, GameManager.instance.userData.userPassWord); // ���ӵǸ� �ٷ� ������ �α���
            }
            else
            {
                EndConnectSever();
            }

            mPacketCheckThread = new Thread(Start_ConnectTcpServer);
            mPacketCheckThread.Start();

            StartCoroutine("CheckUsePacket");
        }
        else
        {
            EndConnectSever();
        }

        Invoke("CheckConnectServer", 5);

    }

    private void OnDisable()
    {
        try
        {
            Colse();
            mPacketCheckThread.Abort();
            StopCoroutine("CheckUsePacket");
        }
        catch
        {
        }
    }

    public bool ConnetServer() // 1. ���� ����
    {
        string address = mTcpServerIp;

        int port = mTcpServerPort;

        if (connet_Server.isConented)
        {
            Debug.Log(string.Format("{0}. ������ ���� ��", DateTime.Now));
            mClientState = CLIENT_STATE.CONNECTED;

            return true;
        }
        else
        {
            Debug.Log(string.Format("{0}. ������ ���� ����", DateTime.Now));
        }

        return false;
    }

    private void RequestLogin(string userID, string authToken) // 2. �α���
    {
        if (GameManager.instance.userData.isExit == true)
        {
            return;
        }

        if (mClientState == CLIENT_STATE.CONNECTED)
        {
            Debug.Log("������ �α��� ��û" + LOG_LEVEL.INFO);

            var reqLogin = new CSBaseLib.PKTReqLogin() { UserID = userID, AuthToken = authToken, LectureID = GameManager.instance.userData.roomID };

            var Body = MessagePackSerializer.Serialize(reqLogin);
            var sendData = CSBaseLib.PacketToBytes.Make(CSBaseLib.PACKETID.REQ_LOGIN, Body);
            PostSendPacket(sendData);
        }
    }

    public void JoinRoom() // 3. �� ����
    {
        if (GameManager.instance.userData.isExit == true)
        {
            return;
        }

        Debug.Log("������ �� ���� ��û" + LOG_LEVEL.INFO);

        var request = new CSBaseLib.PKTReqRoomEnter()
        {
            RoomNumber = GameManager.instance.userData.roomID,
            UserID = GameManager.instance.userData.userId,
            UserName = GameManager.instance.userData.userName,
            LectureID = GameManager.instance.userData.roomID,
        };

        var Body = MessagePackSerializer.Serialize(request);
        var sendData = CSBaseLib.PacketToBytes.Make(CSBaseLib.PACKETID.REQ_ROOM_ENTER, Body);
        PostSendPacket(sendData);
    }


    public void SendAudioData_Mike(PKTAudioData request)
    {
        if (GameManager.instance.userData.isExit == true)
        {
            return;
        }

        if (request.Audio_Data == null)
        {
            return;
        }

        var Body = MessagePackSerializer.Serialize(request);
        var sendData = CSBaseLib.PacketToBytes.Make(CSBaseLib.PACKETID.Audio_Data, Body);
        PostSendPacket(sendData);
    }


    public void ExitRoom() // �� ������
    {
        GameManager.instance.userData.isExit = true;

        Debug.Log("������ �� ������ ��û" + LOG_LEVEL.INFO);

        var sendData = CSBaseLib.PacketToBytes.Make(CSBaseLib.PACKETID.REQ_ROOM_LEAVE, null);
        PostSendPacket(sendData);

    }

    private void Colse() // ���α׷��� ������ ��
    {
        EndConnectSever();

        mIsNetworkThreadRunning = false;

        if (mNetworkReadThread.IsAlive)
        {
            mNetworkReadThread.Join();
        }

        if (mNetworkSendThread.IsAlive)
        {
            mNetworkSendThread.Join();
        }

        if (mPacketCheckThread.IsAlive)
        {
            mPacketCheckThread.Join();
        }
    }

    private void NetworkSendProcess()
    {
        while (mIsNetworkThreadRunning)
        {
            System.Threading.Thread.Sleep(1);

            if (connet_Server.IsConnected() == false)
            {
                continue;
            }

            lock (((System.Collections.ICollection)mSendPacketList).SyncRoot)
            {
                if (mSendPacketList.Count > 0)
                {
                    var packet = mSendPacketList.Dequeue();
                    connet_Server.Send(packet);
                }
            }
        }
    }

    private void BackGroundProcess()
    {
        try
        {
            var packet = new PacketData();

            lock (((System.Collections.ICollection)mRecvPacketQueue).SyncRoot)
            {
                if (mRecvPacketQueue.Count() > 0)
                {
                    packet = mRecvPacketQueue.Dequeue();
                }
            }

            if (packet.PacketID != 0)
            {
                PacketProcess(packet);
            }
        }
        catch (Exception ex)
        {
            Debug.Log(string.Format("ReadPacketQueueProcess. error:{0}", ex.Message));
        }
    }

    private void NetworkReadProcess()
    {
        const UInt16 PacketHeaderSize = CSBaseLib.PacketDef.PACKET_HEADER_SIZE;

        while (mIsNetworkThreadRunning)
        {
            if (connet_Server.IsConnected() == false)
            {
                System.Threading.Thread.Sleep(1);
                continue;
            }

            var recvData = connet_Server.Receive();

            if (recvData != null)
            {
                mPacketBuffer.Write(recvData.Item2, 0, recvData.Item1);

                while (true)
                {
                    var data = mPacketBuffer.Read();
                    if (data.Count < 1)
                    {
                        break;
                    }

                    var packet = new PacketData();
                    packet.DataSize = (UInt16)(data.Count - PacketHeaderSize);
                    packet.PacketID = BitConverter.ToUInt16(data.Array, data.Offset + 2);
                    packet.Type = (SByte)data.Array[(data.Offset + 4)];
                    packet.BodyData = new byte[packet.DataSize];
                    Buffer.BlockCopy(data.Array, (data.Offset + PacketHeaderSize), packet.BodyData, 0, (data.Count - PacketHeaderSize));
                    lock (((System.Collections.ICollection)mRecvPacketQueue).SyncRoot)
                    {
                        mRecvPacketQueue.Enqueue(packet);
                    }
                }
                //DevLog.Write($"���� ������: {recvData.Item2}", LOG_LEVEL.INFO);
            }
            else
            {
                mIsNetworkThreadRunning = false;
                connet_Server.Close();
                SetDisconnectd();
                Debug.Log("������ ���� ���� !!!" + LOG_LEVEL.INFO);
            }
        }
    }

    public void PostSendPacket(byte[] sendData)
    {
        if (sendData.Length >= GameManager.instance.buildOption.maxPacketSize)
        {
            Debug.Log("�����Ͱ� ũ�� ������ ���� �� ����");
            return;
        }

        if (connet_Server.IsConnected() == false)
        {
            IsDisConnectedCount++;
            if (IsDisConnectedCount >= 20)
            {
                IsDisConnectedCount = -10000;
                EndConnectSever();
            }

            Debug.Log("���� ������ �Ǿ� ���� �ʽ��ϴ�" + LOG_LEVEL.ERROR);
            return;
        }
        IsDisConnectedCount = 0;
        mSendPacketList.Enqueue(sendData);
    }

    public void EndConnectSever()// ���� ���� ����, �������� ����� �泪�����ٴ� �޼����� ������ �ش� �Լ� ȣ��
    {
        Debug.Log($"���� ���� ����" + LOG_LEVEL.INFO);

        mClientState = CLIENT_STATE.NONE;

        StopCoroutine("CheckUsePacket");
        StopCoroutine("CheckSend");
        StopAllCoroutines();
        mPacketCheckThread.Abort();
        mNetworkSendThread.Abort();
        mNetworkReadThread.Abort();

        SetDisconnectd();
        connet_Server.Close();
    }

    public void SetDisconnectd()
    {
        mClientState = CLIENT_STATE.NONE;

        mSendPacketList.Clear();
    }

    private void PacketProcess(PacketData packet)
    {

        switch ((PACKETID)packet.PacketID)
        {
            case PACKETID.REQ_RES_TEST_ECHO:
                {
                    Debug.Log($"Echo ����: {packet.BodyData.Length}" + LOG_LEVEL.INFO);
                    break;
                }
            case PACKETID.RES_LOGIN: // �����κ��� �α��� �������θ� ����
                {
                    var resData = MessagePackSerializer.Deserialize<PKTResLogin>(packet.BodyData);

                    if (resData.Result == (UInt16)ERROR_CODE.NONE)
                    {
                        mClientState = CLIENT_STATE.LOGIN;
                        Debug.Log("�α��� ����" + LOG_LEVEL.INFO);

                        JoinRoom();
                    }
                    else
                    {
                        Debug.Log(string.Format("�α��� ����: {0} {1}", resData.Result, ((ERROR_CODE)resData.Result).ToString()) + LOG_LEVEL.ERROR);
                    }
                }
                break;

            case PACKETID.RES_ROOM_ENTER: // ���� �� ���� ������ ���� ������Ʈ
                {
                    var resData = MessagePackSerializer.Deserialize<PKTResRoomEnter>(packet.BodyData);

                    if (resData.Result == (UInt16)ERROR_CODE.NONE)
                    {
                        mClientState = CLIENT_STATE.ROOM;
                        Debug.Log("�� ���� ����" + LOG_LEVEL.INFO);
                        mIsJoinRoom = true;
                        //  GameManager.Instance.joinUserManager.JoinNewUser(resData.UserID, resData.UserName, resData.UserMode, resData.UserAvatarType);
                        // GameManager.Instance.userData.UserObject.GetComponent<JoinUser>().synUserInfo.synFace.SetCharacterPosition();
                        //GameManager.Instance.tcpServerManager.sendPosition();
                    }
                    else if (resData.Result == (UInt16)ERROR_CODE.EnterRoomAgain)
                    {
                        RequestLogin(GameManager.instance.userData.userId, GameManager.instance.userData.userPassWord);
                    }
                    else // EROOR_CODE �� ������ ���� (������ ���� �ȵ��ְų�, �̹� �濡 ���� �ְų�)
                    {
                        Debug.Log(string.Format("������ ����: {0} {1}", resData.Result, ((ERROR_CODE)resData.Result).ToString()) + LOG_LEVEL.INFO);
                    }
                }
                break;
            case PACKETID.NTF_ROOM_USER_LIST: // ������ ����, ���� ������ �ִٸ� ����Ʈ�� �޾ƿ�
                {
                    var ntfData = MessagePackSerializer.Deserialize<PKTNtfRoomUserList>(packet.BodyData);

                    lock (((System.Collections.ICollection)mPacket_Room_UserList).SyncRoot)
                    {
                        mPacket_Room_UserList.Enqueue(ntfData);
                    }
                }
                break;
            case PACKETID.NTF_ROOM_NEW_USER: // �� �̹� �����ְ�, ���ο� ������ ��� ���� ��
                {
                    var ntfData = MessagePackSerializer.Deserialize<PKTNtfRoomNewUser>(packet.BodyData);

                    lock (((System.Collections.ICollection)mPacket_Room_NewUser).SyncRoot)
                    {
                        mPacket_Room_NewUser.Enqueue(ntfData);
                    }
                }
                break;

            case PACKETID.CloseClassRoom: // �������� ���� ���Ḧ ���� �� ��
                {
                    var ntfData = MessagePackSerializer.Deserialize<PKTCloseClassRoom>(packet.BodyData);

                    mClientState = CLIENT_STATE.LOGIN;
                    Debug.Log("�� ������ ����" + LOG_LEVEL.INFO);

                    lock (((System.Collections.ICollection)mPacket_CloseClassRoom).SyncRoot)
                    {
                        mPacket_CloseClassRoom.Enqueue(ntfData);
                    }
                }
                break;

            case PACKETID.Position: // �� ������ �ƹ�Ÿ ��ġ ������
                {
                    var ntfData = MessagePackSerializer.Deserialize<PKTPosition>(packet.BodyData);

                    if (ntfData.UserID.Equals(GameManager.instance.userData.userId))
                    {
                        return;
                    }

                    lock (((System.Collections.ICollection)mPacket_UserPosition).SyncRoot)
                    {
                        mPacket_UserPosition.Enqueue(ntfData);
                    }
                }
                break;

            case PACKETID.All_Data_Student_List: // �л��� ��� �����͸� ���� ��, ���ļ� ����
                {
                    var ntfData = MessagePackSerializer.Deserialize<PKTAllData_Student_List>(packet.BodyData);

                    lock (((System.Collections.ICollection)mPacket_StudentData).SyncRoot)
                    {
                        mPacket_StudentData.Enqueue(ntfData);
                    }
                }
                break;

            case PACKETID.All_Data_Teacher_List: // ������ ��� �����͸� ���� ��, ���ļ� ����
                {
                    var ntfData = MessagePackSerializer.Deserialize<PKTAllData_Teacher_List>(packet.BodyData);

                    lock (((System.Collections.ICollection)mPacket_TeacherData).SyncRoot)
                    {
                        mPacket_TeacherData.Enqueue(ntfData);
                    }
                }
                break;

            case PACKETID.Audio_Data:
                {
                    if (packet.BodyData == null)
                    {
                        return;
                    }
                    var ntfData = MessagePackSerializer.Deserialize<PKTAudioData>(packet.BodyData);

                    if (ntfData.UserID.Equals(GameManager.instance.userData.userId))
                    {
                        return;
                    }
                    lock (((System.Collections.ICollection)mPacket_AudioData_Mike).SyncRoot)
                    {
                        mPacket_AudioData_Mike.Enqueue(ntfData);
                    }
                }
                break;


            case PACKETID.Audio_Data_SoundCard:
                {
                    if (packet.BodyData == null)
                    {
                        return;
                    }
                    var ntfData = MessagePackSerializer.Deserialize<PKTAudioData_SoundCard>(packet.BodyData);

                    if (ntfData.UserID.Equals(GameManager.instance.userData.userId))
                    {
                        return;
                    }
                    lock (((System.Collections.ICollection)mPacket_AudioData_Mike).SyncRoot)
                    {
                        mPacket_AudioData_SoundCard.Enqueue(ntfData);
                    }
                }
                break;

            case PACKETID.Student_Mike: // ����ũ On / Off ������
                {
                    var ntfData = MessagePackSerializer.Deserialize<PKTStudentMike>(packet.BodyData);

                    lock (((System.Collections.ICollection)mPacket_Mike).SyncRoot)
                    {
                        mPacket_Mike.Enqueue(ntfData);
                    }
                }
                break;

            case PACKETID.RES_ROOM_LEAVE:
                {
                    var resData = MessagePackSerializer.Deserialize<PKTResRoomLeave>(packet.BodyData);

                    if (resData.Result == (short)ERROR_CODE.NONE)
                    {
                        mClientState = CLIENT_STATE.LOGIN;
                        Debug.Log("�� ������ ����" + LOG_LEVEL.INFO);

                        lock (((System.Collections.ICollection)mPacket_Room_SelfLeave).SyncRoot)
                        {
                            mPacket_Room_SelfLeave.Enqueue(resData);
                        }
                    }
                    else
                    {
                        Debug.Log(string.Format("�� ������ ����: {0} {1}", resData.Result, ((ERROR_CODE)resData.Result).ToString()) + LOG_LEVEL.ERROR);
                    }
                }
                break;
            case PACKETID.NTF_ROOM_LEAVE_USER: // ���� ������ ��
                {
                    var ntfData = MessagePackSerializer.Deserialize<PKTNtfRoomLeaveUser>(packet.BodyData);

                    lock (((System.Collections.ICollection)mPacket_Room_UserLeave).SyncRoot)
                    {
                        mPacket_Room_UserLeave.Enqueue(ntfData);
                    }
                }
                break;

            case PACKETID.NTF_ROOM_CHAT: // ä�� �޼���
                {
                    var ntfData = MessagePackSerializer.Deserialize<PKTNtfRoomChat>(packet.BodyData);


                    lock (((System.Collections.ICollection)mPacket_Room_Chat).SyncRoot)
                    {
                        mPacket_Room_Chat.Enqueue(ntfData);
                    }
                }
                break;

            case PACKETID.Teacher_Screen: // ��ũ��
                {
                    var ntfData = MessagePackSerializer.Deserialize<PKTTeacherScreen>(packet.BodyData);


                    lock (((System.Collections.ICollection)mPacket_TeacherScreen).SyncRoot)
                    {
                        mPacket_TeacherScreen.Enqueue(ntfData);
                    }
                }
                break;
        }
    }
}