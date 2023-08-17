//Game server user data. it's singleton class.

namespace GameServer
{
    class CurrentServerData
    {
        public bool isCheckConnectionFlag;
        public CurrentServerData()
        {
            isCheckConnectionFlag = false;
        }
    }

    class GameManager
    {
        private static GameManager mInstance;

        private GameManager()
        {
            mCurrentServerData = new CurrentServerData();
        }

        private CurrentServerData mCurrentServerData;
        public CurrentServerData userData
        {
            get
            {
                return mCurrentServerData;
            }
        }

        public static GameManager instance
        {
            get
            {
                if (mInstance == null)
                {
                    mInstance = new GameManager();
                }
                return mInstance;
            }
        }
    }
}
