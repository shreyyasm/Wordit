using UnityEngine;

public class PostToReddit : MonoBehaviour
{
    public void OnPostButtonClicked()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        Application.ExternalCall("CallPostAPI");
#else
        Debug.Log("Post button clicked (not WebGL build)");
#endif
    }
}
