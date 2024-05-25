using UnityEngine;

public class CatSpirit : MonoBehaviour
{
    [SerializeField] private float speed = 50.0f;
    [SerializeField] private float duration = 2.0f;

    Animator    animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        transform.position += Vector3.up * speed * Time.deltaTime;

        duration -= Time.deltaTime;
        if (duration <= 0.0f)
        {
            animator.SetTrigger("FadeOut");
        }
    }

    public void OnAnimationEnd()
    {
        Destroy(gameObject);
    }
}
