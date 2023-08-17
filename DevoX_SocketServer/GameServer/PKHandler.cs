//Packet handle script.
namespace GameServer
{
    public class PKHandler
    {
        protected MainServer ServerNetwork;
        protected UserManager UserMgr = null;

        public void Init(MainServer serverNetwork, UserManager userMgr)
        {
            ServerNetwork = serverNetwork;
            UserMgr = userMgr;
        }
    }
}
