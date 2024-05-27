using UnityEngine;

public class DeathZone : MonoBehaviour
{
    void Start()
    {
        
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        var player = collider.GetComponent<Player>();
        if (player == null) player = collider.GetComponentInParent<Player>();
        if (player != null)
        {
            // Instant death
            player.ProbabilityHit(2.0f);
        }
    }
}
