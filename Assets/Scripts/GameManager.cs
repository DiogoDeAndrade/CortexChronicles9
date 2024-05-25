using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    const int maxLives = 3;

    Player  player;
    int     _nLives = maxLives;

    static GameManager _Instance = null;
    public static GameManager Instance => _Instance;

    public bool hasLives => _nLives > 1;
    public int nLives => _nLives;

    private void Awake()
    {
        if (_Instance == null)
        {
            _Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void Init()
    {
        _nLives = maxLives;
    }

    void Start()
    {

    }

    void Update()
    {
    }

    public void RemoveLives(int l)
    {
        _nLives = Mathf.Clamp(_nLives - l, 0, maxLives);
    }

    public void GotoScene(int sceneId)
    {
        SceneManager.LoadScene(sceneId);
    }
}
