using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private Transform  _catTarget;
    [SerializeField] private float      probabilityDecay = 0.1f;
    [SerializeField] private float      probabilityDecayByDistance = 0.005f;
    [SerializeField] private GameObject deadCatPrefab;
    [SerializeField] private CatSpirit  catSpiritPrefab;
    [SerializeField] private AudioClip  deadCatSound;

    float       currentHitProbability = 0.0f;
    Vector3     prevPos;
    bool        dead = false;

    public float probHit => currentHitProbability;
    public bool  isDead => dead;

    public Transform targetPos => (_catTarget != null) ? (_catTarget) : (transform);

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
        if (isDead) return;

        if ((currentHitProbability > 0.3f) || (prob >= 1.0f))
        {
            if ((Random.Range(0.0f, 1.0f) < currentHitProbability) || (prob >= 1.0f))
            {
                dead = true;
                
                if (deadCatPrefab)
                {
                    var deadCat= Instantiate(deadCatPrefab, transform.position, transform.rotation);
                    Destroy(gameObject);
                }
                if (catSpiritPrefab)
                {
                    Instantiate(catSpiritPrefab, transform.position, transform.rotation);
                }
                if (deadCatSound)
                {
                    SoundManager.PlaySound(deadCatSound, 0.5f, 1.0f);
                }
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
