using UnityEngine;

//This script is public Utility about Unity object. (Turn on/off all unity object)
public class UnityObject : MonoBehaviour
{
    public static void OnActive(GameObject obj)
    {
        if(obj.activeSelf == false)
        {
            obj.SetActive(true);
        }
    }

    public static void OffActive(GameObject obj)
    {
        if (obj.activeSelf == true)
        {
            obj.SetActive(false);
        }
    }
}
