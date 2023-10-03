using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIButtonManager : MonoBehaviour
{
    public InputField LoginID_Input;
    public InputField LoginPassWord_Input;

    public InputField RegisterID_Input;
    public InputField RegisterPassWord_Input;

    public Toggle IsAgreeToggle;
    public Toggle IsAutoToggle;

    public void LoginButton()
    {
        if (GameManager.instance.userData.userId.Equals(LoginID_Input.text) &&
            GameManager.instance.userData.userPassWord.Equals(LoginPassWord_Input.text))
        {
            if (IsAutoToggle.isOn == true)
            {
                GameManager.instance.userData.isAutoLogin = true;
            }
            else
            {
                GameManager.instance.userData.isAutoLogin = true;
            }

            GameManager.instance.dataManager.SaveData();

            SceneManager.LoadScene("UserScene");
        }
    }

    public void RegisterButton(GameObject obj)
    {
        if (RegisterID_Input.text.Length != 0 && LoginPassWord_Input.text.Length != 0)
        {
            if (IsAgreeToggle.isOn == true)
            {
                GameManager.instance.userData.isAgree = true;

                GameManager.instance.userData.userId = RegisterID_Input.text;
                GameManager.instance.userData.userPassWord = LoginPassWord_Input.text;

                GameManager.instance.dataManager.SaveData();

                obj.gameObject.SetActive(false);
            }
        }
    }

    public void StartButton()
    {
        SceneManager.LoadScene("LogginScene");
    }


    public void OffObject(GameObject obj)
    {
        if (obj.activeSelf == true)
        {
            obj.SetActive(false);
        }
    }
    public void OnObject(GameObject obj)
    {
        if (obj.activeSelf == false)
        {
            obj.SetActive(true);
        }
    }
}
