using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Graveyard : MonoBehaviour
{
    [SerializeField]
    private List<Chip> m_Chips = new List<Chip>();

    public void Push(Chip chip)
    {
        var t = chip.GetTransform();
        t.parent = transform;

        m_Chips.Add(chip);
    }

    public void Pull()
    {
        // todo: remove from list 
    }
}
