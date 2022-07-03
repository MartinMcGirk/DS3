using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class timer : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Gate");
        Debug.Log(Time.realtimeSinceStartup);
    }
}
