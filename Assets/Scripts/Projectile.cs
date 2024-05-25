using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float      speed = 500.0f;
    [SerializeField] private LayerMask  environmentMask;
    [SerializeField] private LayerMask  playerMask;
    [SerializeField] private float      hitProbabilityInFlight = 0.1f;
    [SerializeField] private float      hitProbabilityInGround = 0.1f;
    [SerializeField] private float      groundHitRadius = 10.0f;
    [SerializeField] private GameObject hitFx;

    float   distance = 1000.0f;
    Vector3 targetPos;
    Vector3 normal;
    Vector3 prevPos;

    void Start()
    {
        // Let's determine where the shot will hit
        var hit = Physics2D.Raycast(transform.position, transform.up, float.MaxValue, environmentMask);
        if (hit.collider != null)
        {
            distance = hit.distance;
            targetPos = hit.point;
            normal = hit.normal;
        }
        Destroy(gameObject, distance / speed);

        prevPos = transform.position;
    }

    void Update()
    {
        transform.position = transform.position + transform.up * speed * Time.deltaTime;

        var hits = Physics2D.LinecastAll(prevPos, transform.position, playerMask);
        foreach (var hit in hits)
        {
            var player = hit.collider.GetComponent<Player>();
            if (player != null) 
            {
                player.ProbabilityHit(hitProbabilityInFlight);
            }
        }

        prevPos = transform.position;
    }

    private void OnDestroy()
    {
        if ((distance < 1000.0f) && (hitFx))
        {
            Quaternion rotation = Quaternion.LookRotation(Vector3.forward, normal);
            Instantiate(hitFx, targetPos, rotation);

            if (hitProbabilityInGround > 0)
            {
                // Detect if player is somewhere around
                var hits = Physics2D.OverlapCircleAll(targetPos, groundHitRadius, playerMask);
                foreach (var hit in hits)
                {
                    var player = hit.GetComponent<Player>();
                    if (player != null)
                    {
                        player.ProbabilityHit(hitProbabilityInGround);
                    }
                }
            }
        }
    }
}
