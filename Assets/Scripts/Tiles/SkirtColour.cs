using UnityEngine;
using UnityEngine.UI;

public class SkirtColour : MonoBehaviour
{
    [SerializeField]
    private Image referenceTile;
    [SerializeField]
    private Image skirt;

    private void Update()
    {
        if (referenceTile.color != skirt.color)
            skirt.color = referenceTile.color;
    }
}