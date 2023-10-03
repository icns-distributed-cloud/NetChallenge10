using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CheckLogin : MonoBehaviour
{
    void Start()
    {
        if (GameManager.instance.userData.isAutoLogin == true)
        {
            SceneManager.LoadScene("UserScene");
        }
    }
}
