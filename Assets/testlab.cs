using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testlab : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var test = MathF.Sin(Time.realtimeSinceStartup * 0.2f);
        Debug.Log(Mathf.Abs(test));
    }
}
