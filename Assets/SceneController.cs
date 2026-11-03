using UnityEngine;

public class SceneController : MonoBehaviour
{
    public void Logout(string scenename)
    {
        if (PlayerManager.instance != null)
            PlayerManager.instance.Logout(scenename);
    }
}
