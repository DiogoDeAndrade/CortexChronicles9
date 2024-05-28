using UnityEngine;

public class Cheats : MonoBehaviour
{
    [SerializeField] private Checkpoint[] checkpoints;

    void Update()
    {
        for (int i = 0; i < checkpoints.Length; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                Player player = FindAnyObjectByType<Player>();
                if (player)
                {
                    player.transform.position = checkpoints[i].transform.position;
                }
            }
        }
    }
}
