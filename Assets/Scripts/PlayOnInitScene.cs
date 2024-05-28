using UnityEngine;

public class PlayOnInitScene : MonoBehaviour
{
    [SerializeField] private AudioClip music;

    void Start()
    {
        GameManager.Instance.SwitchMusic(music);
    }
}
