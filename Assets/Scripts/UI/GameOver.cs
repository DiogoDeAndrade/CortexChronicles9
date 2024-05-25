using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;

public class GameOver : MonoBehaviour
{
    [SerializeField, Scene]
    private int menuScene;

    Image   image;
    float   timer = 1.0f;
    Player  player;

    void Start()
    {
        image = GetComponent<Image>();
        if (image != null)
        {
            image.enabled = false;
        }
    }

    void Update()
    {
        if (GameManager.Instance.nLives == 0)
        {
            // Check if player is dead
            if (player == null)
            {
                player = FindAnyObjectByType<Player>();
            }
            if (player == null)
            {
                image.enabled = true;
                timer -= Time.deltaTime;

                if ((Input.anyKeyDown) && (timer <= 0))
                {
                    // Go back to main menu
                    GameManager.Instance.GotoScene(menuScene);
                }
            }
        }
    }
}
