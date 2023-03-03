using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//display spike sprite effect when the player got damaage
public class StandardDamage : MonoBehaviour
{
    public float fShowTime;
    public void ShowEffect()
    {
        transform.localScale = Vector3.zero;
        LeanTween.scale(gameObject, Vector3.one, fShowTime);
        Invoke("HideEffect", fShowTime);
    }
    public void HideEffect()
    {
        transform.localScale = Vector3.one;
        LeanTween.scale(gameObject, Vector3.zero, fShowTime);
    }
}
