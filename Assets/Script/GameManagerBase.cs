using UnityEngine;

public class GameManagerBase : MonoBehaviour
{
    protected bool isInitialized = false;

    protected virtual void Initialize()
    {
        isInitialized = true;
    }

    protected void SafeLog(string message)
    {
        if (Application.isEditor)
        {
            Debug.Log(message);
        }
    }
}
