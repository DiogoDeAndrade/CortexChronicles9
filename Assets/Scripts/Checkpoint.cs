using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private Sprite activeSprite;
    [SerializeField] private Sprite inactiveSprite;
    [SerializeField] private ParticleSystem onPS;
    [SerializeField] private ParticleSystem offPS;
    [SerializeField] private AudioClip      onSound;

    bool active = false;
    SpriteRenderer sr;
    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        EnableCheckpoint(false, true);
    }

    public void EnableCheckpoint(bool b, bool silent = false)
    {
        if (((!active) && (b)) && (onSound) && (!silent))
        {
            SoundManager.PlaySound(onSound, 1.0f, 1.0f);
        }
        active = b;

        sr.sprite = (active) ? (activeSprite) : (inactiveSprite);

        if (onPS)
        {
            var emissionModule = onPS.emission;
            emissionModule.enabled = b;
        }
        if (offPS)
        {
            var emissionModule = offPS.emission;
            emissionModule.enabled = !b;
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        Player player = collider.GetComponent<Player>();
        if (player == null) player = collider.GetComponentInParent<Player>();
        if (player != null)
        {
            LevelManager.Instance.SetCheckpoint(this);
        }
    }
}
