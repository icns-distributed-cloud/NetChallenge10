using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public void init()
    {
        LoadData();
    }

    public void SaveData()
    {
        if (GameManager.instance.userData.isAgree == false)
        {
            PlayerPrefs.SetInt("IsAgree", 0);
        }
        else
        {
            PlayerPrefs.SetInt("IsAgree", 1);
        }

        if (GameManager.instance.userData.isAutoLogin == false)
        {
            PlayerPrefs.SetInt("isAutoLogin", 0);
        }
        else
        {
            PlayerPrefs.SetInt("isAutoLogin", 1);
        }


        PlayerPrefs.SetString("userId", GameManager.instance.userData.userId);
        PlayerPrefs.SetString("userPassWord", GameManager.instance.userData.userPassWord);
        PlayerPrefs.Save();
    }

    private void LoadData()
    {
        if (PlayerPrefs.GetInt("IsAgree") == 0)
        {
            GameManager.instance.userData.isAgree = false;
        }
        else
        {
            GameManager.instance.userData.isAgree = true;
        }

        if (PlayerPrefs.GetInt("isAutoLogin") == 0)
        {
            GameManager.instance.userData.isAutoLogin = false;
        }
        else
        {
            GameManager.instance.userData.isAutoLogin = true;
        }

        GameManager.instance.userData.userId = PlayerPrefs.GetString("userId");
        GameManager.instance.userData.userPassWord = PlayerPrefs.GetString("userPassWord");
    }
}
