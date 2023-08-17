using System;
using System.Collections.Concurrent;
using System.Threading;

//Game server udpate class. Check game logic and control update process.
namespace GameServer
{
    class GameUpdater
    {
        private bool IsThreadRunning = false;
        private Thread ProcessThread = null;

        private UInt16 MaxGameCount = 0;
        private GameLogic[] GameLogics = null;

        private ConcurrentQueue<InOutGameElement> NewGameQueue = new ConcurrentQueue<InOutGameElement>();

        public void Init(UInt16 maxGameCount)
        {
            MaxGameCount = maxGameCount;
            GameLogics = new GameLogic[maxGameCount];

            IsThreadRunning = true;
            ProcessThread = new Thread(this.Process);
            ProcessThread.Start();
        }

        public void Stop()
        {
            if (IsThreadRunning == false)
            {
                return;
            }

            IsThreadRunning = false;
            ProcessThread.Join();
        }

        public void NewGame(UInt16 index, GameLogic game)
        {
            NewGameQueue.Enqueue(new InOutGameElement { Index = index, GameObj = game });
        }

        private void Process()
        {
            while (IsThreadRunning)
            {
                if (NewGameQueue.TryDequeue(out var newGame))
                {
                    GameLogics[newGame.Index] = newGame.GameObj;
                }

                for (var i = 0; i < MaxGameCount; ++i)
                {
                    if (GameLogics[i] == null)
                    {
                        continue;
                    }

                    if (GameLogics[i].IsStop)
                    {
                        GameLogics[i] = null;
                    }

                    GameLogics[i].Update();
                }

                Thread.Sleep(1);
            }
        }
    }

    class InOutGameElement
    {
        public UInt16 Index;
        public GameLogic GameObj;
    }
}
