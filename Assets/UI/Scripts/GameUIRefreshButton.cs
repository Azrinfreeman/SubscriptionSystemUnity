using UnityEngine;

public class GameUIRefreshButton : MonoBehaviour
{
    public void Refresh()
    {
        if (PlayerManager.instance != null)
            PlayerManager.instance.RequestRefresh();
    }
}
