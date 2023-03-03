using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManaPoolManager : MonoBehaviour
{
    [SerializeField]
    private int m_MaxMana = 6;

    [SerializeField]
    private int m_InitialMana = 3;

    [SerializeField]
    private float m_ManaRegenerationTime = 4f;

    [SerializeField]
    private int m_RegeneratedManaAmount = 1;

    public int GetMaxMana { get { return m_MaxMana; } }
    public int GetInitialMana { get { return m_InitialMana; } }
    public float GetTimeRegeneration { get { return m_ManaRegenerationTime; } }
    public int GetRegenerationAmount { get { return m_RegeneratedManaAmount; } }

    public Action<CharacterType, int> OnManaUpdate;

    public void NotifyManaUpdate(CharacterType type, int mana)
    {
        OnManaUpdate?.Invoke(type, mana);
    }
}
