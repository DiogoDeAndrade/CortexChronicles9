using UnityEngine;

public class Parallax : MonoBehaviour
{
    [SerializeField] private Vector2 parallaxSpeed = Vector2.zero;

    Vector3 thisOrigin;
    Vector3 parentOrigin;

    void Start()
    {
        thisOrigin = transform.position;
        parentOrigin = transform.parent.position;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 delta = transform.parent.position - parentOrigin;

        transform.position = thisOrigin + new Vector3(delta.x * parallaxSpeed.x, delta.y * parallaxSpeed.y, 0);
    }
}
