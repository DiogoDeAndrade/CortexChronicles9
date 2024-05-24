using UnityEngine;
using UnityEngine.UI;

public class ProbDisplay : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private Image  fill;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.identity;

        float p = player.probHit;

        fill.transform.localScale = new Vector3(p, 1, 1);
    }
}
