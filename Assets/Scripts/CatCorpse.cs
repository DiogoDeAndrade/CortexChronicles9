using UnityEngine;

public class CatCorpse : MonoBehaviour
{
    [SerializeField] private LayerMask environmentMask;

    Vector2 targetPos = Vector3.zero;
    Vector2 velocity = Vector3.zero;

    void Start()
    {
        var hit = Physics2D.Raycast(transform.position + Vector3.up * 5.0f, Vector3.down, float.MaxValue, environmentMask);
        if (hit.collider == null)
        {
            targetPos = transform.position + Vector3.down * 1000.0f;
        }
        else
        {
            targetPos = hit.point;
        }
    }

    void FixedUpdate()
    {
        if (targetPos.y < transform.position.y)
        {
            velocity = velocity + Physics2D.gravity * Time.fixedDeltaTime;
            transform.position += new Vector3(velocity.x, velocity.y, 0.0f) * Time.fixedDeltaTime;
        }
    }
}
