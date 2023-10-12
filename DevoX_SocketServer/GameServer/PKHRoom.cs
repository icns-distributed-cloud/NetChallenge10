using System;
using System.Collections.Generic;
using MessagePack;
using CSBaseLib;
using System.IO;

using System.Timers;
//Server Room connect socket handler. Handle metaverse room packet and recive, process that.
//Join room, Leave room, Chat message, voice chat (...etc).

namespace GameServer
{
    public class PKHRoom : PKHandler
    {
        List<Room> RoomList = null;
        GameTcpaterManager GameUpdateMgrRef;

        private Timer _timer;

        public void SetObject(List<Room> roomList, GameTcpaterManager gameUpdateMgr)
        {
            RoomList = roomList;

            GameUpdateMgrRef = gameUpdateMgr;

            _timer = new Timer(500); // 1000ms = 1s
            _timer.Elapsed += OnTimedEvent;
            _timer.AutoReset = true; // If you want to run it repeatedly. Set to false if you want to run it only once.
            _timer.Start();

        }
        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            string filePath = "/home/ubuntu/NetChallenge10/Audio/Result/Result.txt";
           // string filePath = "/home/icns/NetChallenge10/Audio/Result/Result.txt";

            try
            {
                if (File.Exists(filePath))
                {
                    using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string content = reader.ReadToEnd().Trim();

                        if (string.IsNullOrEmpty(content))
                        {
                            // Console.WriteLine("The file is empty.");
                        }
                        else
                        {
                            Console.WriteLine($"The file contains the following text: \"{content}\"");
                            Warning(content);
                        }
                    }
                }
                else
                {
                    //Console.WriteLine($"The file {filePath} does not exist.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }


        }

        public void RegistPacketHandler(Dictionary<UInt16, Action<ServerPacketData>> packetHandlerMap)
        {
            packetHandlerMap.Add((UInt16)PACKETID.REQ_ROOM_ENTER, RequestRoomEnter);
            packetHandlerMap.Add((UInt16)PACKETID.REQ_ROOM_LEAVE, RequestLeave);
            packetHandlerMap.Add((UInt16)PACKETID.NTF_IN_ROOM_LEAVE, NotifyLeaveInternal);
            packetHandlerMap.Add((UInt16)PACKETID.REQ_ROOM_CHAT, RequestChat);

            packetHandlerMap.Add((UInt16)PACKETID.REQ_ROOM_DEV_ALL_ROOM_START_GAME, RequestDevAllRoomStartGame);
            packetHandlerMap.Add((UInt16)PACKETID.REQ_ROOM_DEV_ALL_ROOM_END_GAME, RequestDevAllRoomStopGame);

            packetHandlerMap.Add((UInt16)PACKETID.All_Data_Student, RequestAllData_Student);
            packetHandlerMap.Add((UInt16)PACKETID.All_Data_Teacher, RequestAllData_Teacher);

            packetHandlerMap.Add((UInt16)PACKETID.Audio_Data, RequestAudioData_Teacher);
            packetHandlerMap.Add((UInt16)PACKETID.Audio_Data_Recive, RequestAudioData_Recive);
            packetHandlerMap.Add((UInt16)PACKETID.Audio_Data_SoundCard, RequestAudioData_SoundCard_Teacher);

            packetHandlerMap.Add((UInt16)PACKETID.Position, RequestPosition);
            packetHandlerMap.Add((UInt16)PACKETID.Student_Mike, RequestStudentMike); 
            packetHandlerMap.Add((UInt16)PACKETID.CloseClassRoom, RequesCloseRoom);

            packetHandlerMap.Add((UInt16)PACKETID.Teacher_Screen, RequestScreenData_Teacher);
            packetHandlerMap.Add((UInt16)PACKETID.CheckUser, CheckUser);



            packetHandlerMap.Add((UInt16)PACKETID.Call, Call);
            packetHandlerMap.Add((UInt16)PACKETID.AgreeCall, AgreeCall);
        }

        private Room GetRoom(int lectureID)
        {
            bool checkClearFind = false;

            int index = -1;
            int count = 0;

            foreach (var room in RoomList)
            {
                if (lectureID == room.LectureID)
                {
                    index = count;
                    checkClearFind = true;
                    break;
                }
                count++;
            }
            
            if (checkClearFind == false)
            {
                index = -1;
                count = 0;

                foreach (var room in RoomList)
                {
                    if (lectureID != room.LectureID && room.LectureID == -1)
                    {
                        index = count;
                        break;
                    }
                    count++;
                }
            }
      
            if (index < 0)
            {
                return null;
            }

            return RoomList[index];
        }

        (bool, Room, RoomUser) CheckRoomAndRoomUser(int userNetSessionIndex)
        {
            var user = UserMgr.GetUser(userNetSessionIndex);
            if (user == null)
            {
                return (false, null, null);
            }

            var room = GetRoom(user.LectureID);

            if (room == null)
            {
                return (false, null, null);
            }

            var roomUser = room.GetUser(userNetSessionIndex);

            if (roomUser == null)
            {
                return (false, room, null);
            }

            return (true, room, roomUser);
        }

        public void RequestRoomEnter(ServerPacketData packetData)
        {
            var sessionID = packetData.SessionID;
            var sessionIndex = packetData.SessionIndex;
            MainServer.MainLogger.Debug("RequestRoomEnter");

            try
            {
                var user = UserMgr.GetUser(sessionIndex);

                if (user == null)
                {
                    ResponseEnterRoomToClient(ERROR_CODE.ROOM_ENTER_INVALID_USER, sessionID);
                    return;
                }

                if (user.IsConfirm(sessionID) == false)
                {
                    ResponseEnterRoomToClient(ERROR_CODE.ROOM_ENTER_INVALID_USER, sessionID);
                    return;
                }

                if (user.IsStateRoom())
                {
                    ResponseEnterRoomToClient(ERROR_CODE.ROOM_ENTER_INVALID_STATE, sessionID);
                    return;
                }

                var reqData = MessagePackSerializer.Deserialize<PKTReqRoomEnter>(packetData.BodyData);

                var room = GetRoom(reqData.LectureID);

                if (room == null)
                {
                    ResponseEnterRoomToClient(ERROR_CODE.ROOM_ENTER_INVALID_ROOM_NUMBER, sessionID);
                    return;
                }

                if (room.AddUser(user.ID(), reqData.UserName, reqData.UserMode, reqData.UserAvatarType, sessionIndex, sessionID, reqData.LectureID) == false)
                {
                    ResponseEnterRoomToClient(ERROR_CODE.ROOM_ENTER_INVALID_ROOM_NUMBER, sessionID);
                    return;
                }

                user.EnteredRoom(reqData.RoomNumber);

                room.NotifyPacketUserList(sessionID);

                user.SaveConnectedTime();

                int newUserIndex = 0;
                room.UserIndex.TryGetValue(user.ID(), out newUserIndex);

                room.NofifyPacketNewUser(sessionIndex, user.ID(), reqData.UserName, reqData.UserMode, reqData.UserAvatarType, newUserIndex);

                ResponseEnterRoomToClient(ERROR_CODE.NONE, sessionID);

                MainServer.MainLogger.Debug("RequestEnterInternal - Success");
            }
            catch (Exception ex)
            {
                MainServer.MainLogger.Error(ex.ToString());
            }
        }

        void ResponseEnterRoomToClient(ERROR_CODE errorCode, string sessionID)
        {
            var resRoomEnter = new PKTResRoomEnter()
            {
                Result = (UInt16)errorCode
            };

            var bodyData = MessagePackSerializer.Serialize(resRoomEnter);
            var sendData = PacketToBytes.Make(PACKETID.RES_ROOM_ENTER, bodyData);

            ServerNetwork.SendData(sessionID, sendData);
        }
        int SessionIndex_ = 0; 
        /// <summary>
        public void Call(ServerPacketData packetData)
        {
            var sessionID = packetData.SessionID;
            SessionIndex_ = packetData.SessionIndex;
            var user = UserMgr.GetUser(packetData.SessionIndex);

            if (user == null)
            {
                return;
            }

            var room = GetRoom(user.LectureID);


            var PKT = new PKTReqCall()
            {
                ReviceID = user.UserID
            };

            var bodyData = MessagePackSerializer.Serialize(PKT);
            var sendData = PacketToBytes.Make(PACKETID.Call, bodyData);

            if (room == null)
            {
                return;
            }

            var roomObject = CheckRoomAndRoomUser(packetData.SessionIndex);

            if (roomObject.Item1 == false)
            {
                return;
            }

            roomObject.Item2.Broadcast(-1, sendData);
        }

        public void AgreeCall(ServerPacketData packetData)
        {
            var sessionID = packetData.SessionID;

            var user = UserMgr.GetUser(packetData.SessionIndex);

            if (user == null)
            {
                return;
            }

            var room = GetRoom(user.LectureID);

            var PKT = new PKTReqAgreeCall()
            {
                
            };

            var bodyData = MessagePackSerializer.Serialize(PKT);
            var sendData = PacketToBytes.Make(PACKETID.AgreeCall, bodyData);

            if (room == null)
            {
                return;
            }

            var roomObject = CheckRoomAndRoomUser(packetData.SessionIndex);

            if (roomObject.Item1 == false)
            {
                return;
            }

            roomObject.Item2.Broadcast(-1, sendData);
        }

        public void Warning(string id)
        {
            var user = UserMgr.GetUser(SessionIndex_);

            if (user == null)
            {
                return;
            }

            var room = GetRoom(user.LectureID);

            var PKT = new PKTWarning()
            {
                ID = id
            };
            var bodyData = MessagePackSerializer.Serialize(PKT);
            var sendData = PacketToBytes.Make(PACKETID.Warning, bodyData);

            if (room == null)
            {
                return;
            }

            var roomObject = CheckRoomAndRoomUser(user.SessionIndex);

            if (roomObject.Item1 == false)
            {
                return;
            }

            roomObject.Item2.Broadcast(-1, sendData);
        }
        /// <summary>

        public void CheckUser(ServerPacketData packetData)
        {
            var sessionID = packetData.SessionID;

            var user = UserMgr.GetUser(packetData.SessionIndex);

            if (user == null)
            {
                return;
            }

            var room = GetRoom(user.LectureID);

            var sendData = PacketToBytes.Make(PACKETID.CheckUser, packetData.BodyData);

            if (room == null)
            {
                return;
            }

            var roomObject = CheckRoomAndRoomUser(packetData.SessionIndex);

            if (roomObject.Item1 == false)
            {
                return;
            }
            user.SaveConnectedTime();
        }

        public void RequestAllData_Student(ServerPacketData packetData)
        {
            var sessionID = packetData.SessionID;
            // MainServer.MainLogger.Debug("Room_Request_AllData_Student");

            var user = UserMgr.GetUser(packetData.SessionIndex);

            if (user == null)
            {
                return;
            }

            var room = GetRoom(user.LectureID);

            var reqData = MessagePackSerializer.Deserialize<PKTAllData_Student>(packetData.BodyData);

            user.SaveConnectedTime();

            if (room == null)
            {
                return;
            }
            if (room.AddSendData_Student_List(reqData) == true)
            {
                var roomObject = CheckRoomAndRoomUser(packetData.SessionIndex);

                if (roomObject.Item1 == false)
                {
                    return;
                }
                user.SaveConnectedTime();
                roomObject.Item2.Broadcast(-1, room.GetSendData_Student_List());
            }
        }
        public void RequestAudioData_SoundCard_Teacher(ServerPacketData packetData)
        {
            var sessionID = packetData.SessionID;

            var user = UserMgr.GetUser(packetData.SessionIndex);

            if (user == null)
            {
                return;
            }

            var room = GetRoom(user.LectureID);

            var reqData = MessagePackSerializer.Deserialize<PKTAudioData_SoundCard>(packetData.BodyData);

            var sendData = PacketToBytes.Make(PACKETID.Audio_Data_SoundCard, packetData.BodyData);

            if (room == null)
            {
                return;
            }

            var roomObject = CheckRoomAndRoomUser(packetData.SessionIndex);

            if (roomObject.Item1 == false)
            {
                return;
            }
            user.SaveConnectedTime();
            roomObject.Item2.Broadcast(-1, sendData, reqData.UserID);
        }
        int count = 0;
        int count_Recive = 0;
        public void RequestAudioData_Teacher(ServerPacketData packetData)
        {
            var reqData = MessagePackSerializer.Deserialize<PKTAudioData>(packetData.BodyData);

            // File.WriteAllBytes("/home/icns/NetChallenge/AudioData/" + reqData.UserID + "_" + count + ".wav", reqData.Audio_Data);

            if (reqData.UserID.Equals("홍길동"))
            {
                File.WriteAllBytes("/home/ubuntu/NetChallenge10/Audio/send/" + "Daejeon_Hong_" + count + ".wav", reqData.Audio_Data);
               // File.WriteAllBytes("/home/icns/NetChallenge10/Audio/send/" + "Suwon_Hong_" + count + ".wav", reqData.Audio_Data);
                count++;
            }
            else
            {
                File.WriteAllBytes("/home/ubuntu/NetChallenge10/Audio/receive/" + "Daejeon_Kim_" + count_Recive + ".wav", reqData.Audio_Data);
               // File.WriteAllBytes("/home/icns/NetChallenge10/Audio/receive/" + "Suwon_Kim_" + count_Recive + ".wav", reqData.Audio_Data);
                count_Recive++;
            }

            Console.WriteLine(reqData.Audio_Data.Length + "   길이");
           
            /*var sessionID = packetData.SessionID;

            var user = UserMgr.GetUser(packetData.SessionIndex);

            if (user == null)
            {
                return;
            }

            var room = GetRoom(user.LectureID);

            var reqData = MessagePackSerializer.Deserialize<PKTAudioData>(packetData.BodyData);

            var sendData = PacketToBytes.Make(PACKETID.Audio_Data, packetData.BodyData);

            if (room == null)
            {
                return;
            }

            var roomObject = CheckRoomAndRoomUser(packetData.SessionIndex);

            if (roomObject.Item1 == false)
            {
                return;
            }
            user.SaveConnectedTime();
            roomObject.Item2.Broadcast(-1, sendData, reqData.UserID);*/
        }
        public void RequestAudioData_Recive(ServerPacketData packetData)
        {
            var reqData = MessagePackSerializer.Deserialize<PKTAudioData_Recive>(packetData.BodyData);


            // File.WriteAllBytes("/home/icns/NetChallenge/AudioData/" + reqData.UserID + "_" + count + ".wav", reqData.Audio_Data);

            if (reqData.UserID.Equals("홍길동"))
            {
               File.WriteAllBytes("/home/ubuntu/NetChallenge10/Audio/send/" + "Daejeon_Hong_" + count + ".wav", reqData.Audio_Data);
               // File.WriteAllBytes("/home/icns/NetChallenge10/Audio/send/" + "Suwon_Hong_" + count + ".wav", reqData.Audio_Data);
                count++; 
            }
            else
            {
               File.WriteAllBytes("/home/ubuntu/NetChallenge10/Audio/receive/" + "Daejeon_Kim_" + count_Recive + ".wav", reqData.Audio_Data);
              // File.WriteAllBytes("/home/icns/NetChallenge10/Audio/receive/" + "Suwon_Kim_" + count_Recive + ".wav", reqData.Audio_Data);
                count_Recive++;
            }



            Console.WriteLine(reqData.Audio_Data.Length + "   길이");
           
            /*var sessionID = packetData.SessionID;

            var user = UserMgr.GetUser(packetData.SessionIndex);

            if (user == null)
            {
                return;
            }

            var room = GetRoom(user.LectureID);

            var reqData = MessagePackSerializer.Deserialize<PKTAudioData>(packetData.BodyData);

            var sendData = PacketToBytes.Make(PACKETID.Audio_Data, packetData.BodyData);

            if (room == null)
            {
                return;
            }

            var roomObject = CheckRoomAndRoomUser(packetData.SessionIndex);

            if (roomObject.Item1 == false)
            {
                return;
            }
            user.SaveConnectedTime();
            roomObject.Item2.Broadcast(-1, sendData, reqData.UserID);*/
        }

        public void RequestScreenData_Teacher(ServerPacketData packetData)
        {
            var sessionID = packetData.SessionID;

            var user = UserMgr.GetUser(packetData.SessionIndex);

            if (user == null)
            {
                return;
            }

            var room = GetRoom(user.LectureID);

            var reqData = MessagePackSerializer.Deserialize<PKTTeacherScreen>(packetData.BodyData);

            var sendData = PacketToBytes.Make(PACKETID.Teacher_Screen, packetData.BodyData);

            if (room == null)
            {
                return;
            }

            var roomObject = CheckRoomAndRoomUser(packetData.SessionIndex);

            if (roomObject.Item1 == false)
            {
                return;
            }
            user.SaveConnectedTime();
            roomObject.Item2.Broadcast(-1, sendData, reqData.UserID);
        }

        public void RequestAllData_Teacher(ServerPacketData packetData)
        {
            var sessionID = packetData.SessionID;
            // MainServer.MainLogger.Debug("Room_RequestAllData_Teacher");

            var user = UserMgr.GetUser(packetData.SessionIndex);

            if (user == null)
            {
                return;
            }

            var room = GetRoom(user.LectureID);

            var reqData = MessagePackSerializer.Deserialize<PKTAllData_Teacher>(packetData.BodyData);

            user.SaveConnectedTime();

            if (room == null)
            {
                return;
            }

            if (room.AddSendData_Teacher_List(reqData) == true)
            {
                var roomObject = CheckRoomAndRoomUser(packetData.SessionIndex);

                if (roomObject.Item1 == false)
                {
                    return;
                }
                user.SaveConnectedTime();
                roomObject.Item2.Broadcast(-1, room.GetSendData_Teacher_List());
            }
        }

        public void RequestPosition(ServerPacketData packetData)
        {
            var sessionID = packetData.SessionID;
            //  MainServer.MainLogger.Debug("Room_RequestPosition");

            var sendData = PacketToBytes.Make(PACKETID.Position, packetData.BodyData);

            var roomObject = CheckRoomAndRoomUser(packetData.SessionIndex);

            if (roomObject.Item1 == false)
            {
                return;
            }

            roomObject.Item2.Broadcast(-1, sendData);
        }

        public void RequestStudentMike(ServerPacketData packetData)
        {
            var sessionID = packetData.SessionID;
            //MainServer.MainLogger.Debug("Room_RequestMike");

            var sendData = PacketToBytes.Make(PACKETID.Student_Mike, packetData.BodyData);

            var roomObject = CheckRoomAndRoomUser(packetData.SessionIndex);

            if (roomObject.Item1 == false)
            {
                return;
            }

            roomObject.Item2.Broadcast(-1, sendData);
        }

        public void RequesCloseRoom(ServerPacketData packetData)
        {
            var sessionID = packetData.SessionID;
            //MainServer.MainLogger.Debug("Room_RequestCloseRoom");

            var sendData = PacketToBytes.Make(PACKETID.CloseClassRoom, packetData.BodyData);

            var roomObject = CheckRoomAndRoomUser(packetData.SessionIndex);

            if (roomObject.Item1 == false)
            {
                return;
            }

            roomObject.Item2.Broadcast(-1, sendData);
        }

        public void RequestLeave(ServerPacketData packetData)
        {
            var sessionID = packetData.SessionID;
            var sessionIndex = packetData.SessionIndex;
            MainServer.MainLogger.Debug("퇴장 요청 받음");

            try
            {
                var user = UserMgr.GetUser(sessionIndex);
                if (user == null)
                {
                    return;
                }

                if (LeaveRoomUser(sessionIndex, user.LectureID) == false)
                {
                    return;
                }

                user.LeaveRoom();

                ResponseLeaveRoomToClient(sessionID);

                MainServer.MainLogger.Debug("Room RequestLeave - Success");
            }
            catch (Exception ex)
            {
                MainServer.MainLogger.Error(ex.ToString());
            }
        }

        bool LeaveRoomUser(int sessionIndex, int lectureID)
        {
            MainServer.MainLogger.Debug($"LeaveRoomUser. SessionIndex:{sessionIndex}");

            var room = GetRoom(lectureID);
            if (room == null)
            {
                return false;
            }

            var roomUser = room.GetUser(sessionIndex);
            if (roomUser == null)
            {
                return false;
            }

            var userID = roomUser.UserID;
            room.RemoveUser(roomUser);
            if (room.CurrentUserCount() == 0)
            {
                room.Clear();
            }

            room.NotifyPacketLeaveUser(userID);
            return true;
        }

        void ResponseLeaveRoomToClient(string sessionID)
        {
            var resRoomLeave = new PKTResRoomLeave()
            {
                Result = (UInt16)ERROR_CODE.NONE
            };

            var bodyData = MessagePackSerializer.Serialize(resRoomLeave);
            var sendData = PacketToBytes.Make(PACKETID.RES_ROOM_LEAVE, bodyData);

            ServerNetwork.SendData(sessionID, sendData);
        }

        public void NotifyLeaveInternal(ServerPacketData packetData)
        {
            var sessionIndex = packetData.SessionIndex;
            MainServer.MainLogger.Debug($"NotifyLeaveInternal. SessionIndex: {sessionIndex}");

            for (int i = 0; i < RoomList.Count; i++)
            {
                Room room = RoomList[i];

                for (int j = 0; j < room.UserList.Count; j++)
                {
                    if (room.UserList[j].NetSessionIndex == sessionIndex)
                    {
                        var reqData = MessagePackSerializer.Deserialize<PKTInternalNtfRoomLeave>(packetData.BodyData);
                        LeaveRoomUser(sessionIndex, room.LectureID);
                        break;
                    }    

                }
            }
        }

        public void RequestChat(ServerPacketData packetData)
        {
            var sessionID = packetData.SessionID;
            var sessionIndex = packetData.SessionIndex;
            MainServer.MainLogger.Debug("Room RequestChat");

            try
            {
                var roomObject = CheckRoomAndRoomUser(sessionIndex);

                if (roomObject.Item1 == false)
                {
                    return;
                }

                var reqData = MessagePackSerializer.Deserialize<PKTReqRoomChat>(packetData.BodyData);

                var notifyPacket = new PKTNtfRoomChat()
                {
                    UserName = roomObject.Item3.UserName,
                    ChatMessage = reqData.ChatMessage
                };

                var Body = MessagePackSerializer.Serialize(notifyPacket);
                var sendData = PacketToBytes.Make(PACKETID.NTF_ROOM_CHAT, Body);

                roomObject.Item2.Broadcast(-1, sendData);

                MainServer.MainLogger.Debug("Room RequestChat - Success");
            }
            catch (Exception ex)
            {
                MainServer.MainLogger.Error(ex.ToString());
            }
        }

        public void RequestDevAllRoomStartGame(ServerPacketData packetData)
        {
            var sessionID = packetData.SessionID;
            var sessionIndex = packetData.SessionIndex;
            MainServer.MainLogger.Debug("Room RequestDevAllRoomStartGame");

            try
            {
                foreach (var room in RoomList)
                {
                    GameUpdateMgrRef.NewStartGame(room.MoGameObj);
                }
            }
            catch (Exception ex)
            {
                MainServer.MainLogger.Error(ex.ToString());
            }
        }

        public void RequestDevAllRoomStopGame(ServerPacketData packetData)
        {
            var sessionID = packetData.SessionID;
            var sessionIndex = packetData.SessionIndex;
            MainServer.MainLogger.Debug("Room RequestDevAllRoomStopGame");

            try
            {
                GameUpdateMgrRef.AllStop();
            }
            catch (Exception ex)
            {
                MainServer.MainLogger.Error(ex.ToString());
            }
        }
    }
}
