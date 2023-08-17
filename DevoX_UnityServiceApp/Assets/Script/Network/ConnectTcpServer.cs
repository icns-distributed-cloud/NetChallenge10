using System;
using System.Net.Sockets;
using System.Net;

//Connet tcp socket server script.
public class ConnectTcpServer
{
    public Socket sock = null;
    public string latestErrorMsg;

    public bool isConented = false;
     
    public bool Connect(string ip, int port)
    {
        try
        {
            IPAddress serverIP = IPAddress.Parse(ip);
            int serverPort = port;

            sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sock.SendBufferSize = GameManager.instance.buildOption.maxPacketSize;
            sock.Connect(new IPEndPoint(serverIP, serverPort));

            if (sock == null || sock.Connected == false)
            {
                return false;
            }

            isConented = true;
            return true;
        }
        catch (Exception ex)
        {
            latestErrorMsg = ex.Message;
            return false;
        }
    }

    public Tuple<int, byte[]> Receive()
    {
        try
        {
            byte[] ReadBuffer = new byte[GameManager.instance.buildOption.maxPacketSize];
            var nRecv = sock.Receive(ReadBuffer, 0, ReadBuffer.Length, SocketFlags.None);

            if (nRecv == 0)
            {
                return null;
            }
           // UnityEngine.Debug.Log("수신됨!!");
            return Tuple.Create(nRecv, ReadBuffer);
        }
        catch (SocketException se)
        {
            latestErrorMsg = se.Message;
        }

        return null;
    }

    //Write stream
    public void Send(byte[] sendData)
    {
        try
        {
            if (sock != null && sock.Connected) //Check socket connection
            {
                //UnityEngine.Debug.Log("보냄!");
                sock.Send(sendData, 0, sendData.Length, SocketFlags.None);
            }
            else
            {
                latestErrorMsg = "먼저 채팅서버에 접속하세요!";
            }
        }
        catch (SocketException se)
        {
            latestErrorMsg = se.Message;
        }
    }

    //close socekt and stream
    public void Close()
    {
        if (sock != null && sock.Connected)
        {
            sock.Close();
        }
    }

    public bool IsConnected() { return (sock != null && sock.Connected) ? true : false; }
}
