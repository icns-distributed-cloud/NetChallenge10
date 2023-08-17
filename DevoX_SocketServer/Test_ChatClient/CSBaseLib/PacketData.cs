using MessagePack; //https://github.com/neuecc/MessagePack-CSharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSBaseLib
{
    public class PacketDef
    {
        public const UInt16 PACKET_HEADER_SIZE = 5;
        public const int MAX_USER_ID_BYTE_LENGTH = 16;
        public const int MAX_USER_PW_BYTE_LENGTH = 16;

        public const int INVALID_ROOM_NUMBER = -1;
    }

    public class PacketToBytes
    {
        public static byte[] Make(PACKETID packetID, byte[] bodyData)
        {
            byte type = 0;
            var pktID = (UInt16)packetID;
            UInt16 bodyDataSize = 0;
            if (bodyData != null)
            {
                bodyDataSize = (UInt16)bodyData.Length;
            }
            var packetSize = (UInt16)(bodyDataSize + PacketDef.PACKET_HEADER_SIZE);

            var dataSource = new byte[packetSize];
            Buffer.BlockCopy(BitConverter.GetBytes(packetSize), 0, dataSource, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(pktID), 0, dataSource, 2, 2);
            dataSource[4] = type;

            if (bodyData != null)
            {
                Buffer.BlockCopy(bodyData, 0, dataSource, 5, bodyDataSize);
            }

            return dataSource;
        }

        public static Tuple<int, byte[]> ClientReceiveData(int recvLength, byte[] recvData)
        {
            var packetSize = BitConverter.ToUInt16(recvData, 0);
            var packetID = BitConverter.ToUInt16(recvData, 2);
            var bodySize = packetSize - PacketDef.PACKET_HEADER_SIZE;

            var packetBody = new byte[bodySize];
            Buffer.BlockCopy(recvData, PacketDef.PACKET_HEADER_SIZE, packetBody, 0, bodySize);

            return new Tuple<int, byte[]>(packetID, packetBody);
        }
    }


    // Sun Packet


    [MessagePackObject]
    public class PKTStudentMike
    {
        [Key(0)]
        public string UserID;
        [Key(1)]
        public bool MikeFlag;
    }


    [MessagePackObject]
    public class PKTAllData_Student_List
    {
        [Key(0)]
        public List<PKTAllData_Student> Student_All_Data_List;
    }

    [MessagePackObject]
    public class PKTAllData_Teacher_List
    {
        [Key(0)]
        public List<PKTAllData_Teacher> Teacher_All_Data_List;
    }

    [MessagePackObject]
    public class PKTAllData_Student
    {
        [Key(0)]
        public string UserID;
        [Key(1)]
        public int UserMode;
        [Key(2)]
        public bool LeftEyes;
        [Key(3)]
        public bool RightEyes;
        [Key(4)]
        public bool Mouse;
        [Key(5)]
        public float HeadPose_X;
        [Key(6)]
        public float HeadPose_Y;
        [Key(7)]
        public float HeadPose_Z;
    }

    [MessagePackObject]
    public class PKTAllData_Teacher
    {
        [Key(0)]
        public string UserID;
        [Key(1)]
        public int UserMode;
        [Key(2)]
        public byte[] Screen_Byte;
        [Key(3)]
        public bool LeftEyes;
        [Key(4)]
        public bool RightEyes;
        [Key(5)]
        public bool Mouse;
        [Key(6)]
        public float HeadPose_X;
        [Key(7)]
        public float HeadPose_Y;
        [Key(8)]
        public float HeadPose_Z;
    }

    [MessagePackObject]
    public class PKTAudioData
    {
        [Key(0)]
        public string UserID;
        [Key(1)]
        public byte[] Audio_Data;
    }


    [MessagePackObject]
    public class PKTPosition
    {
        [Key(0)]
        public string UserID;
        [Key(1)]
        public int UserMode;
        [Key(2)]
        public float X;
        [Key(3)]
        public float Y;
        [Key(4)]
        public float Z;
    }

    //////


    // 로그인 요청
    [MessagePackObject]
    public class PKTReqLogin
    {
        [Key(0)]
        public string UserID;
        [Key(1)]
        public string AuthToken;
    }

    [MessagePackObject]
    public class PKTResLogin
    {
        [Key(0)]
        public UInt16 Result;
    }

    [MessagePackObject]
    public class PKNtfMustClose
    {
        [Key(0)]
        public UInt16 Result;
    }

    [MessagePackObject]
    public class PKTReqRoomEnter
    {
        [Key(0)]
        public int RoomNumber;
        [Key(1)]
        public string UserID;
        [Key(2)]
        public string UserName;
        [Key(3)]
        public int UserMode;
        [Key(4)]
        public int UserAvatarType;
    }

    [MessagePackObject]
    public class PKTResRoomEnter
    {
        [Key(0)]
        public UInt16 Result;
        [Key(1)]
        public string UserID;
        [Key(2)]
        public string UserName;
        [Key(3)]
        public int UserMode;
        [Key(4)]
        public int UserAvatarType;
    }

    [MessagePackObject]
    public class PKTNtfRoomUserList
    {
        [Key(0)]
        public List<string> UserIDList = new List<string>();
        [Key(1)]
        public List<string> UserNameList = new List<string>();
        [Key(2)]
        public List<int> UserModeList = new List<int>();
        [Key(3)]
        public List<int> UserAvatarTypeList = new List<int>();
        [Key(4)]
        public List<int> UserIndexList = new List<int>();
    }

    [MessagePackObject]
    public class PKTNtfRoomNewUser
    {
        [Key(0)]
        public string UserID;
        [Key(1)]
        public string UserName;
        [Key(2)]
        public int UserMode;
        [Key(3)]
        public int UserAvatarType;
        [Key(4)]
        public int UserIndex;
    }


    ////////


    [MessagePackObject]
    public class PKTReqRoomLeave
    {
    }

    [MessagePackObject]
    public class PKTResRoomLeave
    {
        [Key(0)]
        public UInt16 Result;
    }

    [MessagePackObject]
    public class PKTNtfRoomLeaveUser
    {
        [Key(0)]
        public string UserID;
    }


    [MessagePackObject]
    public class PKTReqRoomChat
    {
        [Key(0)]
        public string ChatMessage;
    }


    [MessagePackObject]
    public class PKTNtfRoomChat
    {
        [Key(0)]
        public string UserID;

        [Key(1)]
        public string ChatMessage;
    }
}
