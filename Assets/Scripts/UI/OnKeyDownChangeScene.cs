using UnityEngine;
using NaughtyAttributes;

public class OnKeyDownChangeScene : MonoBehaviour
{
    [SerializeField, Scene]
    private string sceneName;
    [SerializeField]
    private bool escapeExits = true;

    // Update is called once per frame
    void Update()
    {
        if (Input.anyKeyDown)
        {
            if (escapeExits)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    Application.Quit();
                    return;
                }
            }
            GameManager.Instance.GotoScene(sceneName);
            Destroy(this);
        }
    }
}
