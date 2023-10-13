﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks.Dataflow;

//Send & Recive pacekt process.
namespace GameServer
{
    class PacketProcessor
    {
        private bool IsThreadRunning = false;
        System.Threading.Thread ProcessThread = null;

        //receive쪽에서 처리하지 않아도 Post에서 블럭킹 되지 않는다. 
        //BufferBlock<T>(DataflowBlockOptions) 에서 DataflowBlockOptions의 BoundedCapacity로 버퍼 가능 수 지정. BoundedCapacity 보다 크게 쌓이면 블럭킹 된다
        private BufferBlock<ServerPacketData> MsgBuffer = new BufferBlock<ServerPacketData>();

        private UserManager UserMgr = new UserManager();

        private GameTcpaterManager GameUpdateMgr = new GameTcpaterManager();

        private Tuple<int, int> RoomNumberRange = new Tuple<int, int>(-1, -1);
        private List<Room> RoomList = new List<Room>();

        private Dictionary<UInt16, Action<ServerPacketData>> PacketHandlerMap = new Dictionary<UInt16, Action<ServerPacketData>>();
        private PKHCommon CommonPacketHandler = new PKHCommon();
        private PKHRoom RoomPacketHandler = new PKHRoom();


        //TODO MainServer를 인자로 주지말고, func을 인자로 넘겨주는 것이 좋다
        public void CreateAndStart(List<Room> roomList, MainServer mainServer)
        {
            var maxRoomCount = MainServer.ServerOption.RoomMaxCount;
            var maxUserCount = maxRoomCount * MainServer.ServerOption.RoomMaxUserCount;
            UserMgr.Init(maxUserCount);

            GameUpdateMgr.Init(4, (UInt16)(maxRoomCount / 4));

            RoomList = roomList;
            var minRoomNum = RoomList[0].Number;
            var maxRoomNum = RoomList[0].Number + RoomList.Count() - 1;
            RoomNumberRange = new Tuple<int, int>(minRoomNum, maxRoomNum);

            RegistPacketHandler(mainServer);

            IsThreadRunning = true;
            ProcessThread = new System.Threading.Thread(this.Process);
            ProcessThread.Start();
        }

        public void Destory()
        {
            GameUpdateMgr.AllStop();

            IsThreadRunning = false;
            MsgBuffer.Complete();

            MainServer.MainLogger.Info("[PacketProcessor.Destory] End");
        }

        public void InsertPacket(ServerPacketData data)
        {
            MsgBuffer.Post(data);
        }

        private void RegistPacketHandler(MainServer serverNetwork)
        {
            CommonPacketHandler.Init(serverNetwork, UserMgr);
            CommonPacketHandler.RegistPacketHandler(PacketHandlerMap);

            RoomPacketHandler.Init(serverNetwork, UserMgr);
            RoomPacketHandler.SetObject(RoomList, GameUpdateMgr);
            RoomPacketHandler.RegistPacketHandler(PacketHandlerMap);
        }

        private void Process()
        {
            while (IsThreadRunning)
            {
                //System.Threading.Thread.Sleep(64); //테스트 용
                try
                {
                    var packet = MsgBuffer.Receive();

                    if (PacketHandlerMap.ContainsKey(packet.PacketID))
                    {
                        try
                        {
                            GameManager.instance.userData.isCheckConnectionFlag = true;
                            PacketHandlerMap[packet.PacketID](packet);
                        }
                        catch (Exception e)
                        {
                            GameManager.instance.userData.isCheckConnectionFlag = false;
                            Console.WriteLine("패킷 에러!" + e.ToString()); 
                        }
                    }
                    else
                    {
                        try
                        {
                            if (packet.BodyData == null)
                            {
                                PacketHandlerMap.Remove(packet.PacketID);
                            }
                            //System.Diagnostics.Debug.WriteLine("세션 번호 {0}, PacketID {1}, 받은 데이터 크기: {2}", packet.SessionID, packet.PacketID, packet.BodyData.Length);
                        }
                        catch
                        {
                            PacketHandlerMap.Remove(packet.PacketID);
                        }
                    }
                }
                catch (Exception ex)
                {
                    IsThreadRunning.IfTrue(() => MainServer.MainLogger.Error(ex.ToString()));
                }
            }
        }
    }
}
