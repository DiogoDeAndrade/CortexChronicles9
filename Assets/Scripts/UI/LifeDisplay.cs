using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeDisplay : MonoBehaviour
{
    [SerializeField]
    private GameObject[] displayObjects;

    void Update()
    {
        int lives = GameManager.Instance.nLives;

        for (int i = 0; i < displayObjects.Length; i++)
        {
            displayObjects[i].SetActive(i < lives);
        }
    }
}
