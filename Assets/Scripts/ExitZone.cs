using UnityEngine;
using NaughtyAttributes;

public class ExitZone : MonoBehaviour
{
    [SerializeField, Scene]
    private string nextScene;

    void Start()
    {
        
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        var player = collider.GetComponent<Player>();
        if (player == null) player = collider.GetComponentInParent<Player>();
        if (player != null)
        {
            GameManager.Instance.GotoScene(nextScene);            
        }
    }
}
