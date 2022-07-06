using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DebugWidget : MonoBehaviour
{
    public bool isVisible = false;
    private MeshRenderer mesh;

    void Start()
    {
        mesh = GetComponent<MeshRenderer>();
        mesh.enabled = isVisible;
    }

    private void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard.pageDownKey.wasPressedThisFrame)
        {
            ToggleVisible();
        }
    }

    public void ToggleVisible()
    {
        isVisible = !isVisible;
        mesh.enabled = isVisible;
    }
}
