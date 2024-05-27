using UnityEngine;
using NaughtyAttributes;

public class OnKeyDownChangeScene : MonoBehaviour
{
    [SerializeField, Scene]
    private string sceneName;

    // Update is called once per frame
    void Update()
    {
        if (Input.anyKeyDown)
        {
            GameManager.Instance.GotoScene(sceneName);
            Destroy(this);
        }
    }
}
