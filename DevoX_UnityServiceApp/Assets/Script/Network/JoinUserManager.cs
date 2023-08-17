using CSBaseLib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This script is manage all character in the metaverse room
public class JoinUserManager : MonoBehaviour
{
    public List<GameObject> userObejctList;

    public void ResetUserData()
    {
        userObejctList.Clear();
    }

    private void Init()
    {
        userObejctList = new List<GameObject>();
    }

    private IEnumerator SetTeacherRotation(GameObject obj)
    {
        yield return new WaitForSeconds(3.5f);
        obj.transform.Find("Character_Reference").transform.localRotation = Quaternion.Euler(0, 180, 0);
    }

    public void JoinNewUser(string userId_, string userName_, int userMode_, int userAvatar_, int userIndex)
    {
        GameObject obj = new GameObject();
        //obj.transform.SetParent(GameManager.instance.classData.student_Group_Obj.transform);
        
        obj.AddComponent<JoinUser>().Init(userId_, userIndex, userName_, userMode_);

        if (userId_.Equals(GameManager.instance.userData.userId)) // 새로운 유저가 나 자신이라면
        {
            GameManager.instance.userData.userObject = obj;
            obj.GetComponent<JoinUser>().SetMine();
        }

        userObejctList.Add(obj);

    }

 

    public void ExitUser(string userId) // 다른 유저가 나갔을 때
    {
        for (int i = 0; i < userObejctList.Count; i++)
        {
            if (userObejctList[i].GetComponent<JoinUser>().userId.Equals(userId))
            {
                Destroy(userObejctList[i]);
                userObejctList.RemoveAt(i);

                break;
            }
        }
    }

    public void SetAllData_Student(PKTAllData_Student data) // 받은 데이터를 사용, 선생님이 학생의 데이터를 받거나, 학생이 학생의 데이터를 받음
    {
        if (data.UserID.Equals(GameManager.instance.userData.userId)) // 자기자신의 데이터라면
        {
            return;
        }

        for (int i = 0; i < userObejctList.Count; i++)
        {
            if (userObejctList[i] == null)
            {
                userObejctList.RemoveAt(i);
            }
            else
            {
                if (userObejctList[i].GetComponent<JoinUser>().userId.Equals(data.UserID))
                {
                  //  userObejctList[i].GetComponent<JoinUser>().synUserInfo.SynAll_StudentData(data);
                    break;
                }
            }
        }
    }
    public void SetAudioData(PKTAudioData data) // 음성 데이터 받은걸 사용, 학생이 선생님의 데이터를 받거나, 선생님이 선생님의 데이터를 받음
    {
        for (int i = 0; i < userObejctList.Count; i++)
        {
            if (userObejctList[i].GetComponent<JoinUser>().userId.Equals(data.UserID))
            {
               // userObejctList[i].GetComponent<JoinUser>().synUserInfo.synCharacter.OnSpeakingMark();
                break;
            }
        }
        GameManager.instance.userData.userObject.GetComponent<JoinUser>().synUserInfo.Syn_AudioData_Mike(data);
    }

    public void SetAudioData_SoundCard(PKTAudioData_SoundCard data) // 음성 데이터 받은걸 사용, 학생이 선생님의 데이터를 받거나, 선생님이 선생님의 데이터를 받음
    {
        for (int i = 0; i < userObejctList.Count; i++)
        {
            if (userObejctList[i].GetComponent<JoinUser>().userId.Equals(data.UserID))
            {
              //  userObejctList[i].GetComponent<JoinUser>().synUserInfo.synCharacter.OnSpeakingMark();
                break;
            }
        }
    }


    private void Start()
    {
        Init();
    }
}
