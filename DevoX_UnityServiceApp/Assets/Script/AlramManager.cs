using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlramManager : MonoBehaviour
{
    public GameObject popupObj;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(startAction());
    }
    void onPopup()
    {
        
    }

    IEnumerator startAction()
    {
        yield return new WaitForSeconds(15);

        popupObj.SetActive(true);

        yield return new WaitForSeconds(1);
        Handheld.Vibrate();

        yield return new WaitForSeconds(1);
        Handheld.Vibrate();

        yield return new WaitForSeconds(1);
        Handheld.Vibrate();

        yield return null;
    }
}
