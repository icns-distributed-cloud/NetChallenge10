using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIButtonManager : MonoBehaviour
{
    public void LoginButton(InputField input)
    {
        GameManager.instance.userData.userId = input.text;
        GameManager.instance.userData.userName = input.text;

        SceneManager.LoadScene("MainScene");
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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
