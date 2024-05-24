using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float probabilityDecay = 0.1f;
    [SerializeField] private float probabilityDecayByDistance = 0.005f;

    float currentHitProbability = 0.0f;
    Vector3 prevPos;

    public float probHit => currentHitProbability;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        prevPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentHitProbability > 0)
        {
            float distance = Vector3.Distance(prevPos, transform.position) * probabilityDecayByDistance;

            currentHitProbability = Mathf.Clamp01(currentHitProbability - probabilityDecay * Time.deltaTime - probabilityDecayByDistance * distance);
        }
        prevPos = transform.position;
    }

    public void ProbabilityHit(float prob)
    {
        if (currentHitProbability > 0.3f)
        {
            if (Random.Range(0.0f, 1.0f) < currentHitProbability)
            {
                Destroy(gameObject);
            }
            else
            {
                currentHitProbability = Mathf.Clamp01(currentHitProbability + prob);
            }
        }
        else
        {
            currentHitProbability = Mathf.Clamp01(currentHitProbability + prob);
        }
    }
}
