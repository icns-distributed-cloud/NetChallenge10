using UnityEngine;

//This script is manage each 1 character in the metaverse room
public class JoinUser : MonoBehaviour
{
    public SynUserInfo synUserInfo;

    public bool isMine;

    public string userId;

    public int userIndex;

    public string userName;

    public void SetMine()
    {
        isMine = true;
        synUserInfo.SetMine();
    }

    public void Init(string userId_, int userIndex_, string userName_, int UserMode_)
    {
        isMine = false;
        userId = userId_;
        userName = userName_;
        userIndex = userIndex_;

        synUserInfo = gameObject.AddComponent<SynUserInfo>();

        synUserInfo.Init(userIndex_, userName_, UserMode_);
    }
}
