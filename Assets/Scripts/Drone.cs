using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Drone : MonoBehaviour
{
    [SerializeField] private float          speed;
    [SerializeField] private Transform[]    patrolWaypoints;
    [SerializeField] private float          pauseBetweenWaypoints = 2.0f;
    [SerializeField] private Light2D        scanLight;
    [SerializeField] private Color          scanColor = Color.yellow;
    [SerializeField] private Color          targetColor = Color.red;
    [SerializeField] private LayerMask      environmentMask;
    [SerializeField] private float          scanRange = 45.0f;
    [SerializeField] private float          scanSpeed = 15.0f;
    [SerializeField] private float          minScanRange = 100.0f;
    [SerializeField] private float          pauseBetweenScans = 1.0f;
    [SerializeField] private float          shotCooldown = 2;
    [SerializeField] private float          intraVolleyDelay = 0.1f;
    [SerializeField] private int            volleyCount = 6;
    [SerializeField] private Projectile     shotPrefab;

    private int         patrolIndex = 0;
    private Animator    animator;
    private float       originalAngle;
    private Coroutine   patrolCR;
    private Coroutine   chaseCR;
    private Coroutine   scanCR;
    private Coroutine   targetCR;
    private float       playerLastSeenTimer;
    private float       shotTimer;
    private Player      player;
    private Transform   playerTransform;

    void Awake()
    {
        animator = GetComponent<Animator>();

        originalAngle = scanLight.transform.rotation.eulerAngles.z;

        RunPatrol();
        RunScan();
    }

    private void Update()
    {
        if (patrolCR != null)
        {
            if (SearchPlayer())
            {
                RunChase();
                RunTarget();
                playerLastSeenTimer = 0;
            }
        }
        else if (targetCR != null)
        {
            if (!SearchPlayer())
            {
                playerLastSeenTimer += Time.deltaTime;
                if (playerLastSeenTimer > 4)
                {
                    RunPatrol();
                    RunScan();
                }
            }
            else
            {
                playerLastSeenTimer = 0;
            }
        }

        if (shotTimer > 0)
        {
            shotTimer -= Time.deltaTime;
        }
    }

    bool SearchPlayer()
    {
        if (player == null)
        {
            player = FindAnyObjectByType<Player>();
            if (player) playerTransform = player.targetPos;
        }
        if ((player != null) && (!player.isDead))
        {
            // Raycast to player to see if it can detect him
            Vector2 toPlayer = playerTransform.position - scanLight.transform.position;
            float distance = toPlayer.magnitude;
            if ((distance > 0) && (distance < scanLight.pointLightOuterRadius))
            {
                // Check angle
                toPlayer /= distance;

                float angle = Vector2.Angle(scanLight.transform.up, toPlayer);
                // Check if angle is within cone, or if the cat is very close, just in front
                if ((angle < scanLight.pointLightInnerAngle * 0.5f) || ((distance < minScanRange) && (angle < 180.0f)))
                {
                    // Raycast
                    var hit = Physics2D.Raycast(scanLight.transform.position, toPlayer, distance * 0.95f, environmentMask);

                    return (hit.collider == null);
                }
            }
        }
        return false;
    }

    IEnumerator PatrolCR()
    {
        while (true)
        {
            animator.SetTrigger("Move");
            yield return GotoPointCR(patrolWaypoints[patrolIndex]);

            patrolIndex = (patrolIndex + 1) % patrolWaypoints.Length;

            animator.SetTrigger("Scan");
            yield return new WaitForSeconds(pauseBetweenWaypoints);
        }
    }

    IEnumerator GotoPointCR(Transform waypoint)
    {
        Quaternion lightRotation = scanLight.transform.rotation;
        if (waypoint.position.x > transform.position.x)
        {
            transform.rotation = Quaternion.identity;
            scanLight.transform.rotation = lightRotation;
        }
        else 
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
            scanLight.transform.rotation = lightRotation;
        }

        while (Vector3.Distance(waypoint.position, transform.position) > 1)
        {
            transform.position = Vector3.MoveTowards(transform.position, waypoint.position, speed * Time.deltaTime);

            yield return null;
        }        
    }

    IEnumerator ChaseCR()
    {
        while (true)
        {
            yield return null;
        }
    }

    void RunPatrol()
    {
        if (chaseCR != null)
        {
            StopCoroutine(chaseCR);
            chaseCR = null;
        }
        if (patrolCR == null)
        {
            patrolCR = StartCoroutine(PatrolCR());
        }
    }
    void RunChase()
    {
        if (patrolCR != null)
        {
            StopCoroutine(patrolCR);
            patrolCR = null;
        }
        if (chaseCR == null)
        {
            chaseCR = StartCoroutine(ChaseCR());
        }
    }

    void RunScan()
    {
        if (targetCR != null)
        {
            StopCoroutine(targetCR);
            targetCR = null;
        }
        if (scanCR == null)
        {
            scanCR = StartCoroutine(ScanCR());
        }
    }
    void RunTarget()
    {
        if (scanCR != null)
        {
            StopCoroutine(scanCR);
            scanCR = null;
        }
        if (targetCR == null)
        {
            targetCR = StartCoroutine(TargetCR());
        }
    }

    IEnumerator ScanCR()
    {
        scanLight.color = scanColor;
        while (true)
        {
            yield return RotateScanCR(-scanRange);
            yield return new WaitForSeconds(pauseBetweenScans);
            yield return RotateScanCR(0);
            yield return new WaitForSeconds(pauseBetweenScans);
            yield return RotateScanCR(scanRange);
            yield return new WaitForSeconds(pauseBetweenScans);
        }
    }

    IEnumerator RotateScanCR(float offsetAngle)
    {
        float targetAngle = originalAngle + offsetAngle;
        float currentAngle = scanLight.transform.rotation.eulerAngles.z;

        while (Mathf.Abs(scanLight.transform.rotation.eulerAngles.z - targetAngle) > 1)
        {
            currentAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, scanSpeed * Time.deltaTime);
            scanLight.transform.rotation = Quaternion.Euler(0, 0, currentAngle);
            yield return null;
        }
    }
    IEnumerator TargetCR()
    {
        scanLight.color = targetColor;
        while (true)
        {
            // Search for closest position to player that has line of sight
            Vector3 closestPos = FindClosestPositionWithLOS();
            transform.position = Vector3.MoveTowards(transform.position, closestPos, speed * Time.deltaTime);

            if (SearchPlayer())
            {                
                var toPlayer = (playerTransform.position - transform.position).normalized;

                scanLight.transform.rotation = Quaternion.RotateTowards(scanLight.transform.rotation, Quaternion.LookRotation(Vector3.forward, toPlayer), 360.0f * Time.deltaTime);

                float angle = Vector3.Angle(scanLight.transform.up, toPlayer);
                if ((angle < 5.0f) && (shotTimer <= 0))
                {
                    StartCoroutine(ShootCR());
                }
            }

            yield return null;
        }
    }

    IEnumerator ShootCR()
    {
        shotTimer = shotCooldown;

        for (int i = 0; i < volleyCount; i++)
        {
            Shoot();
            yield return new WaitForSeconds(intraVolleyDelay);
        }
    }

    void Shoot()
    {
        Vector3 direction = scanLight.transform.up;
        direction = ChangeAngle(direction, Random.Range(-4.0f, 4.0f));
        Quaternion rotation = Quaternion.LookRotation(Vector3.forward, direction);
        Instantiate(shotPrefab, scanLight.transform.position, rotation);
    }

    Vector3 ChangeAngle(Vector3 baseDir, float angle)
    {
        Vector3 ret = baseDir;
        float   a = Mathf.Deg2Rad * angle;
        float   c = Mathf.Cos(a);
        float   s = Mathf.Sin(a);
        ret.x = ret.x * c - ret.y * s;
        ret.y = ret.x * s + ret.y * c;

        return ret;
    }

    private Vector3 FindClosestPositionWithLOS()
    {
        Vector3 ret = transform.position;
        if (player == null)
        {
            player = FindAnyObjectByType<Player>();
            if (player) playerTransform = player.targetPos;
        }
        if ((player != null) && (!player.isDead))
        {
            Vector3 playerPos = playerTransform.position;
            float   minDist = Vector3.Distance(ret, playerPos);

            for (int i = 0; i < patrolWaypoints.Length; i++)
            {
                Vector3 p1 = patrolWaypoints[i].position;
                Vector3 p2 = patrolWaypoints[(i + 1) % patrolWaypoints.Length].position;

                Vector3 p = FindClosestPointOnLineSegment(p1, p2, playerPos);
                float   d = Vector3.Distance(p, playerPos);
                if ((d > 0) && (d < minDist))
                {
                    // Raycast
                    var toPlayer = (playerPos - p) / d;
                    var hit = Physics2D.Raycast(p, toPlayer, d * 0.95f, environmentMask);

                    if (hit.collider == null)
                    {
                        ret = p;
                        minDist = d;
                    }
                }
            }
        }

        return ret;
    }

    public static Vector3 FindClosestPointOnLineSegment(Vector3 A, Vector3 B, Vector3 P)
    {
        Vector3 AB = B - A;
        Vector3 AP = P - A;

        // Calculate the projection scalar t
        float magnitudeAB = AB.sqrMagnitude; // The square of the length of AB
        float ABAPproduct = Vector3.Dot(AP, AB); // The dot product of AP and AB
        float t = ABAPproduct / magnitudeAB;

        // Clamp t to be in the range [0, 1]
        t = Mathf.Clamp01(t);

        // Calculate the closest point
        Vector3 closestPoint = A + AB * t;

        return closestPoint;
    }

    private void OnDrawGizmosSelected()
    {
        if (patrolWaypoints != null)
        {
            Gizmos.color = Color.yellow;
            for (int i = 0; i < patrolWaypoints.Length; i++)
            {
                Gizmos.DrawLine(patrolWaypoints[i].position, patrolWaypoints[(i + 1) % patrolWaypoints.Length].position);
            }
        }
    }
}
