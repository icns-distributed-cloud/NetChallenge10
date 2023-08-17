using System;
using System.Collections.Generic;
using System.Linq;
using CSBaseLib;

//This script is controll All user. check All user connection, and add, remove, check user information.
namespace GameServer
{
    public class UserManager
    {
        int MaxUserCount;
        UInt64 UserSequenceNumber = 0;

        public Dictionary<int, User> UserMap = new Dictionary<int, User>();

        public void Init(int maxUserCount)
        {
            MaxUserCount = maxUserCount;
        }

        public ERROR_CODE AddUser(string userID, string sessionID, int sessionIndex, int lectureID)
        {
            if (IsFullUserCount())
            {
                return ERROR_CODE.LOGIN_FULL_USER_COUNT;
            }

            if (UserMap.ContainsKey(sessionIndex))
            {
                return ERROR_CODE.ADD_USER_DUPLICATION;
            }


            ++UserSequenceNumber;

            var user = new User();
            user.Set(UserSequenceNumber, sessionID, sessionIndex, userID, lectureID);
            UserMap.Add(sessionIndex, user);

            return ERROR_CODE.NONE;
        }

        public ERROR_CODE RemoveUser(int sessionIndex)
        {
            if (UserMap.Remove(sessionIndex) == false)
            {
                return ERROR_CODE.REMOVE_USER_SEARCH_FAILURE_USER_ID;
            }

            return ERROR_CODE.NONE;
        }

        public User GetUser(int sessionIndex)
        {
            User user = null;
            UserMap.TryGetValue(sessionIndex, out user);
            return user;
        }

        bool IsFullUserCount()
        {
            return MaxUserCount <= UserMap.Count();
        }

        public int GetUserMapCount()
        {
            return UserMap.Count;
        }
    }

    public class User
    {
        UInt64 SequenceNumber = 0;
        public string SessionID;
        public int SessionIndex = -1;
        public int RoomNumber = -1;
        public string UserID;
        public int LectureID = -1;

        public DateTime UserLastConnectedTime;
        public bool IsFirstRequestReceive = false; // 클라로부터 최초로 리시브 받을때, 정확히는 현재 시점상 최초로 방 들어 올때

        public void Set(UInt64 sequence, string sessionID, int sessionIndex, string userID, int lectureID)
        {
            SequenceNumber = sequence;
            SessionID = sessionID;
            SessionIndex = sessionIndex;
            UserID = userID;
            LectureID = lectureID;
        }

        public string GetUserSeesionID
        {
            get
            {
                return SessionID;
            }
        }
        public bool IsConfirm(string netSessionID)
        {
            return SessionID == netSessionID;
        }

        public string ID()
        {
            return UserID;
        }

        public void SaveConnectedTime()
        {
            UserLastConnectedTime = DateTime.Now;
            IsFirstRequestReceive = true;
        }

        public void EnteredRoom(int roomNumber)
        {
            RoomNumber = roomNumber;
        }

        public void LeaveRoom()
        {
            RoomNumber = -1;
        }

        public bool IsStateLogin() { return SessionIndex != -1; }
        public bool IsStateRoom() { return RoomNumber != -1; }
    }
}
