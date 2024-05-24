using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class CameraFollow2d : MonoBehaviour
{
    public enum TagMode { Closest = 0, Furthest = 1, Average = 2 };

    [SerializeField] Hypertag targetTag;
    [SerializeField] TagMode tagMode = TagMode.Closest;
    [SerializeField] Transform targetObject;
    [SerializeField] Rect rect = new Rect(-100.0f, -100.0f, 200.0f, 200.0f);
    [SerializeField] BoxCollider2D cameraLimits;

    private new Camera camera;
    private Bounds allObjectsBound;

    void Start()
    {
        camera = GetComponent<Camera>();

        float currentZ = transform.position.z;
        Vector3 targetPos = GetTargetPos();
        transform.position = new Vector3(targetPos.x, targetPos.y, currentZ);

        CheckBounds();
    }

    void FixedUpdate()
    {
        FixedUpdate_Box();
    }

    void FixedUpdate_Box()
    {
        float currentZ = transform.position.z;
        Vector3 targetPos = GetTargetPos();
        Vector2 delta = transform.position;
        Rect r = rect;
        r.position += delta;

        if (targetPos.x > r.xMax) r.position += new Vector2(targetPos.x - r.xMax, 0);
        if (targetPos.x < r.xMin) r.position += new Vector2(targetPos.x - r.xMin, 0);
        if (targetPos.y < r.yMin) r.position += new Vector2(0, targetPos.y - r.yMin);
        if (targetPos.y > r.yMax) r.position += new Vector2(0, targetPos.y - r.yMax);

        transform.position = new Vector3(r.center.x, r.center.y, currentZ);

        CheckBounds();
    }

    void CheckBounds()
    {
        if (cameraLimits == null) return;

        Bounds r = cameraLimits.bounds;

        float halfHeight = camera.orthographicSize;
        float halfWidth = camera.aspect * halfHeight;

        float xMin = transform.position.x - halfWidth;
        float xMax = transform.position.x + halfWidth;
        float yMin = transform.position.y - halfHeight;
        float yMax = transform.position.y + halfHeight;

        Vector3 position = transform.position;

        if (xMin <= r.min.x) position.x = r.min.x + halfWidth;
        else if (xMax >= r.max.x) position.x = r.max.x - halfWidth;
        if (yMin <= r.min.y) position.y = r.min.y + halfHeight;
        else if (yMax >= r.max.y) position.y = r.max.y - halfHeight;

        transform.position = position;
    }

    public Vector3 GetTargetPos()
    {
        if (targetObject != null) return targetObject.transform.position;
        else if (targetTag)
        {
            Vector3 selectedPosition = transform.position;
            var potentialObjects = gameObject.FindObjectsOfTypeWithHypertag<Transform>(targetTag);
            if (tagMode == TagMode.Closest)
            {
                var minDist = float.MaxValue;
                foreach (var obj in potentialObjects)
                {
                    var d = Vector3.Distance(obj.position, transform.position);
                    if (d < minDist)
                    {
                        minDist = d;
                        selectedPosition = obj.position;
                    }
                }
            }
            else if (tagMode == TagMode.Furthest)
            {
                var maxDist = 0.0f;
                foreach (var obj in potentialObjects)
                {
                    var d = Vector3.Distance(obj.position, transform.position);
                    if (d > maxDist)
                    {
                        maxDist = d;
                        selectedPosition = obj.position;
                    }
                }
            }
            else if (tagMode == TagMode.Average)
            {
                if (potentialObjects.Length > 0)
                {
                    allObjectsBound = new Bounds(potentialObjects[0].position, Vector3.zero);
                    selectedPosition = Vector3.zero;
                    foreach (var obj in potentialObjects)
                    {
                        var d = Vector3.Distance(obj.position, transform.position);
                        selectedPosition += obj.position;
                        allObjectsBound.Encapsulate(obj.position);
                    }
                    selectedPosition /= potentialObjects.Length;
                }
            }

            return selectedPosition;
        }

        return transform.position;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(GetTargetPos(), 0.5f);

        Vector2 delta = transform.position;
        Rect r = rect;
        r.position += delta;

        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(new Vector2(r.xMin, r.yMin), new Vector2(r.xMax, r.yMin));
        Gizmos.DrawLine(new Vector2(r.xMax, r.yMin), new Vector2(r.xMax, r.yMax));
        Gizmos.DrawLine(new Vector2(r.xMax, r.yMax), new Vector2(r.xMin, r.yMax));
        Gizmos.DrawLine(new Vector2(r.xMin, r.yMax), new Vector2(r.xMin, r.yMin));

        if (cameraLimits)
        {
            Bounds b = cameraLimits.bounds;

            Gizmos.color = Color.green;
            Gizmos.DrawLine(new Vector2(b.min.x, b.min.y), new Vector2(b.max.x, b.min.y));
            Gizmos.DrawLine(new Vector2(b.max.x, b.min.y), new Vector2(b.max.x, b.max.y));
            Gizmos.DrawLine(new Vector2(b.max.x, b.max.y), new Vector2(b.min.x, b.max.y));
            Gizmos.DrawLine(new Vector2(b.min.x, b.max.y), new Vector2(b.min.x, b.min.y));
        }
    }
}
