using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shake : MonoBehaviour
{
    private Vector3 originalPosition;
    private List<Vector3> shakeDelta = new List<Vector3>();
    public void ShakeUIElement(Vector2 direction)
    {
        var dir = new Vector3(direction.x, direction.y);
        transform.position += dir;
        shakeDelta.Add(dir);
    }
           
    public void ResetPosition()
    {
        foreach(var d in shakeDelta)
            transform.position -= d;

        shakeDelta.Clear();
    }
}
