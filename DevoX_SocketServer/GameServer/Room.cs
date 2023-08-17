using CSBaseLib;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;

//Metaverse room data script. Can control room and room users.
namespace GameServer
{
    enum UserMode
    {
        Teacher,
        Student
    }

    public class Room
    {
        public int Index { get; private set; }
        public int Number { get; private set; }

        private int MaxUserCount = 0;

        public Dictionary<string, int> UserIndex = new Dictionary<string, int>(); // 유저의 자리 위치

        public  List<RoomUser> UserList = new List<RoomUser>();

        public static Func<string, byte[], bool> NetSendFunc;

        public GameLogic MoGameObj {get; private set; } = new GameLogic();

        List<PKTAllData_Student> SendAllData_Student_List = new List<PKTAllData_Student>();
        List<PKTAllData_Teacher> SendAllData_Teacher_List = new List<PKTAllData_Teacher>();

        public int LectureID { get; set; } // DB에 저장된 강의실 마스터 아이디

        public void Init(int index, int number, int maxUserCount)
        {
            Index = index;
            Number = number;
            LectureID = -1;
            MaxUserCount = maxUserCount;

            var interval = (UInt16)16;
            MoGameObj.Init((UInt32)index, interval);
        }

        public void Clear()
        {
            if (UserIndex.Count != 0)
            {
                UserIndex.Clear();
            }
            if (SendAllData_Student_List.Count != 0)
            {
                SendAllData_Student_List.Clear();
            }
            if (SendAllData_Teacher_List.Count != 0)
            {
                SendAllData_Teacher_List.Clear();
            }
            LectureID = -1;
        }

        public byte[] GetSendData_Student_List()
        {
            var resRoomLeave = new PKTAllData_Student_List()
            {
                Student_All_Data_List = SendAllData_Student_List
            };

            var bodyData = MessagePackSerializer.Serialize(resRoomLeave);
            var sendData = PacketToBytes.Make(PACKETID.All_Data_Student_List, bodyData);

            SendAllData_Student_List.Clear();
            return sendData;
        }

        public bool AddSendData_Student_List(PKTAllData_Student data)
        {
            if (SendAllData_Student_List.Count >= UserList.Count)
            {
                return true;
            }
            SendAllData_Student_List.Add(data);

            return false;
        }

        public byte[] GetSendData_Teacher_List()
        {
            var resRoomLeave = new PKTAllData_Teacher_List()
            {
                Teacher_All_Data_List = SendAllData_Teacher_List
            };

            var bodyData = MessagePackSerializer.Serialize(resRoomLeave);
            var sendData = PacketToBytes.Make(PACKETID.All_Data_Teacher_List, bodyData);

            SendAllData_Teacher_List.Clear();
            return sendData;
        }

        public bool AddSendData_Teacher_List(PKTAllData_Teacher data)
        {
            SendAllData_Teacher_List.Add(data);
            if (SendAllData_Teacher_List.Count >= 1)
            {
                return true;
            }

            return false;
        }

        public bool AddUser(string userID, string userName, int userMode, int userAvatarType, int netSessionIndex, string netSessionID, int lectureId)
        {
            if(GetUser(userID) != null)
            {
                return false;
            }

            if (UserIndex.ContainsKey(userID) == false && userMode == (int)UserMode.Student)
            {
                int count = 0;
                foreach (var item in UserIndex)
                {
                    int idx = item.Value;
                    if (count != idx)
                    {
                        break;
                    }
                    count++;
                }

                UserIndex.Add(userID, count);
            }

            var roomUser = new RoomUser();

            int index = 0;
            if (userMode == (int)UserMode.Student)
            {
                UserIndex.TryGetValue(userID, out index);
            }
            else
            {
                index = 0;
            }

            LectureID = lectureId;

            roomUser.Set(userID, userName, userMode, userAvatarType, index, netSessionIndex, netSessionID);
            Console.WriteLine("추가완료!!!");
            UserList.Add(roomUser);

            return true;
        }

        public void RemoveUser(int netSessionIndex)
        {
            var index = UserList.FindIndex(x => x.NetSessionIndex == netSessionIndex);
            UserList.RemoveAt(index);
            UserIndex.Remove(GetUser(netSessionIndex).UserID);
        }

        public bool RemoveUser(RoomUser user)
        {
            UserIndex.Remove(user.UserID);
            return UserList.Remove(user);
        }

        public RoomUser GetUser_SessionID(string sessionID)
        {
            return UserList.Find(x => x.NetSessionID == sessionID);
        }

        public RoomUser GetUser(string userID)
        {
            return UserList.Find(x => x.UserID == userID);
        }

        public RoomUser GetUser(int netSessionIndex)
        {
            return UserList.Find(x => x.NetSessionIndex == netSessionIndex);
        }

        public int CurrentUserCount()
        {
            return UserList.Count();
        }

        public void NotifyPacketUserList(string userNetSessionID)
        {
            var packet = new CSBaseLib.PKTNtfRoomUserList();
            foreach (var user in UserList)
            {
                packet.UserIDList.Add(user.UserID);
                packet.UserNameList.Add(user.UserName);
                packet.UserModeList.Add(user.UserMode);
                packet.UserAvatarTypeList.Add(user.UserAvatarType); 
                packet.UserIndexList.Add(user.UserIndex); 
            }

            var bodyData = MessagePackSerializer.Serialize(packet);
            var sendPacket = PacketToBytes.Make(PACKETID.NTF_ROOM_USER_LIST, bodyData);

            NetSendFunc(userNetSessionID, sendPacket);
        }

        public void NofifyPacketNewUser(int newUserNetSessionIndex, string newUserID, string newUserName, int newUserMode, int newUserAvatarType, int newUserIndex)
        {
            var packet = new PKTNtfRoomNewUser();
            packet.UserID = newUserID;
            packet.UserName = newUserName;
            packet.UserMode = newUserMode;
            packet.UserAvatarType = newUserAvatarType;
            packet.UserIndex = newUserIndex;
            

            var bodyData = MessagePackSerializer.Serialize(packet);
            var sendPacket = PacketToBytes.Make(PACKETID.NTF_ROOM_NEW_USER, bodyData);

            Broadcast(newUserNetSessionIndex, sendPacket);
        }

        public void NotifyPacketLeaveUser(string userID)
        {
            if(CurrentUserCount() == 0)
            {
                return;
            }

            var packet = new PKTNtfRoomLeaveUser();
            packet.UserID = userID;
            
            var bodyData = MessagePackSerializer.Serialize(packet);
            var sendPacket = PacketToBytes.Make(PACKETID.NTF_ROOM_LEAVE_USER, bodyData);

            Broadcast(-1, sendPacket);
        }

        public void Broadcast(int excludeNetSessionIndex, byte[] sendPacket)
        {
            foreach(var user in UserList)
            {
                if(user.NetSessionIndex == excludeNetSessionIndex)
                {
                    continue;
                }

                NetSendFunc(user.NetSessionID, sendPacket);
            }
        }

        public void Broadcast(int excludeNetSessionIndex, byte[] sendPacket, string sameUserId)
        {
            foreach (var user in UserList)
            {
                if (user.NetSessionIndex == excludeNetSessionIndex)
                {
                    continue;
                }
                if (user.UserID.Equals(sameUserId))
                {
                    continue;
                }

                Console.WriteLine(user.UserName);
                NetSendFunc(user.NetSessionID, sendPacket);
            }
        }
    }

    public class RoomUser
    {
        public string UserID { get; private set; }
        public string UserName { get; private set; }
        public int UserMode { get; private set; }
        public int UserAvatarType { get; private set; }
        public int UserIndex { get; private set; }

        public int NetSessionIndex { get; private set; }
        public string NetSessionID { get; private set; }

        public void Set(string userID, string userName, int userMode, int userAvatarType, int userIndex, int netSessionIndex, string netSessionID)
        {
            UserID = userID;
            UserName = userName;
            UserMode = userMode;
            UserAvatarType = userAvatarType;
            UserIndex = userIndex;

            NetSessionIndex = netSessionIndex;
            NetSessionID = netSessionID;
        }
    }
}
