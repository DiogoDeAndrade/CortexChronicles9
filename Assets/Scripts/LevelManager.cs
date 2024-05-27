using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private Transform  initialSpawnPos;
    [SerializeField] private Player     playerPrefab;
    [SerializeField] private Checkpoint activeCheckpoint;

    bool        detectedPlayer = false;
    Player      player;
    Vector3     spawnPos;
    Quaternion  spawnRotation;

    static LevelManager _Instance = null;
    public static LevelManager Instance => _Instance;

    private void Awake()
    {
        if (_Instance == null)
        {
            _Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        if (activeCheckpoint != null)
        {
            activeCheckpoint.EnableCheckpoint(true);
            spawnPos = activeCheckpoint.transform.position;
            spawnRotation = activeCheckpoint.transform.rotation;
        }
        else if (initialSpawnPos != null)
        {
            spawnPos = initialSpawnPos.position;
            spawnRotation = initialSpawnPos.rotation;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null)
        {
            player = FindAnyObjectByType<Player>();
            if (player == null)
            {
                // Check if we have remaining lives
                if (GameManager.Instance.hasLives)
                {
                    CatSpirit cs = FindAnyObjectByType<CatSpirit>();
                    if (cs == null)
                    {
                        if (detectedPlayer)
                        {
                            GameManager.Instance.RemoveLives(1);
                        }

                        player = Instantiate(playerPrefab, spawnPos, spawnRotation);
                    }
                }
            }
            else
            {
                detectedPlayer = true;
            }
        }
        else
        {
            detectedPlayer = true;
        }
    }

    public void SetCheckpoint(Checkpoint checkpoint)
    {
        if (activeCheckpoint != null)
        {
            activeCheckpoint.EnableCheckpoint(false);
        }
        activeCheckpoint = checkpoint;
        activeCheckpoint.EnableCheckpoint(true);

        spawnPos = activeCheckpoint.transform.position;
        spawnRotation = activeCheckpoint.transform.rotation;
    }
}