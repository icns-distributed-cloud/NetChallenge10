using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserData : MonoBehaviour
{
    public UserData()
    {
        userName = "";
        userId = "";
        userPassWord = "";
        tcpServerIp = "";
        isSendAllData = false;
        isExit = false;
        isAgree = false;
        isAutoLogin = false;
        roomID = 0;
    }
    public string userName;
    public string userId;
    public string userPassWord;
    public string tcpServerIp = "163.180.117.186";
    public bool isSendAllData;
    public bool isExit;

    public int roomID;

    public bool isAgree;
    public bool isAutoLogin;


    public GameObject userObject;
  
}
