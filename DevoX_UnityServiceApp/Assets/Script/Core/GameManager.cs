using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private DataManager mDataManager;
    public DataManager dataManager
    {
        get
        {
            return mDataManager;
        }
    }
    public TcpServerManager tcpMainServerManager
    {
        get
        {
            return mtcpMainServerManager;
        }
    }    

    private TcpServerManager mtcpMainServerManager;

    private UserData mUserData;

    public UserData userData
    {
        get
        {
            return mUserData;
        }
    }
    private BuildOption mBuildOption;
    public BuildOption buildOption
    {
        get
        {
            return mBuildOption;
        }
    }

    private static GameManager mInstance = null;
    public static GameManager instance
    {
        get
        {
            return mInstance;
        }
    }
    // Start is called before the first frame update
    void Awake()
    {
        init();
    }
    private JoinUserManager mJoinRoomManager;
    public JoinUserManager joinRoomManager
    {
        get
        {
            return mJoinRoomManager;
        }
    }
    private MainServerProc mMainServerProc;

    public MainServerProc mainServerProc
    {
        get
        {
            return mMainServerProc;
        }
    }

    void init()
    {
        mInstance = this;

        mUserData = new UserData();

        mBuildOption = new BuildOption();

        mtcpMainServerManager = gameObject.AddComponent<TcpServerManager>();

        mUserData = new UserData();

        mDataManager = new DataManager();

        mJoinRoomManager = gameObject.AddComponent<JoinUserManager>();
        SceneManager.LoadScene("LobbyScene");

        mMainServerProc = gameObject.AddComponent<MainServerProc>();

        DontDestroyOnLoad(this);

        Screen.SetResolution(1080, 1920, true);
    }
}
