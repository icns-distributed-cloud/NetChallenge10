using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlramManager : MonoBehaviour
{
    public GameObject popupObj;

    public void onPopup()
    {
        StartCoroutine("startAction");
    }

    private void OnDestroy()
    {
        StopCoroutine("startAction");
    }

    IEnumerator startAction()
    {
        popupObj.SetActive(true);

        while (true)
        {
            yield return new WaitForSeconds(1);
            Handheld.Vibrate();
        }
    }
}
