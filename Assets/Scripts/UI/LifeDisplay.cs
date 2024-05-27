using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LifeDisplay : MonoBehaviour
{
    [SerializeField]
    private GameObject[]    displayObjects;
    [SerializeField]
    private TextMeshProUGUI displayText;

    void Update()
    {
        int lives = GameManager.Instance.nLives;

        for (int i = 0; i < displayObjects.Length; i++)
        {
            displayObjects[i].SetActive(i < lives);
        }
        if (displayText)
        {
            displayText.text = $"x{lives}";
        }
    }
}
