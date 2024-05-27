using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class GameManager : MonoBehaviour
{
    [SerializeField] private CanvasGroup fader;
    [SerializeField] private float fadeTime = 0.5f;

    const int maxLives = 9;

    Player  player;
    int     _nLives = maxLives;
    float   targetAlpha;
    Action  callback;

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

    private void OnEnable()
    {
        // Subscribe to the sceneLoaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // Unsubscribe from the sceneLoaded event
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode != LoadSceneMode.Additive)
        {
            targetAlpha = 0.0f;
        }
    }

    public void Init()
    {
        _nLives = maxLives;
    }

    void Start()
    {
        fader.alpha = 1.0f;
    }

    void Update()
    {
        if (targetAlpha < fader.alpha)
        {
            fader.alpha = Mathf.Clamp01(fader.alpha - Time.deltaTime / fadeTime);
            if ((targetAlpha == fader.alpha) && (callback != null))
            {
                callback();
            }
        }
        else if (targetAlpha > fader.alpha)
        {
            fader.alpha = Mathf.Clamp01(fader.alpha + Time.deltaTime / fadeTime);
            if ((targetAlpha == fader.alpha) && (callback != null))
            {
                callback();
            }
        }
    }

    public void RemoveLives(int l)
    {
        _nLives = Mathf.Clamp(_nLives - l, 0, maxLives);
    }

    void FadeOut(Action action)
    {
        targetAlpha = 1.0f;
        callback = action;
    }

    public void GotoScene(string sceneName)
    {
        FadeOut(() =>
        {
            SceneManager.LoadScene(sceneName);
        });
    }
}
