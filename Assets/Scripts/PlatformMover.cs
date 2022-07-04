using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformMover : MonoBehaviour
{
    public float distance = 5;
    private Vector3 originalPosition;

    // Start is called before the first frame update
    void Start()
    {
        originalPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        var now = Time.realtimeSinceStartup;
        var offset = Mathf.Sin(now);
        var newPosition = originalPosition;
        newPosition.x += offset * distance;
        transform.position = newPosition;
    }
}
