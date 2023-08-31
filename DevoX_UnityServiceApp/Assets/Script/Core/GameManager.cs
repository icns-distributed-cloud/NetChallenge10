using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

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

    void init()
    {
        mInstance = this;

        mUserData = new UserData();

        mBuildOption = new BuildOption();

        mtcpMainServerManager = gameObject.AddComponent<TcpServerManager>();

        mUserData = new UserData();
        mJoinRoomManager = gameObject.AddComponent<JoinUserManager>();
        SceneManager.LoadScene("LobbyScene");

        DontDestroyOnLoad(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
