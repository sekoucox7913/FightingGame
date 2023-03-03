using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterMana : MonoBehaviour
{
    [SerializeField]
    private CharacterType m_CharacterType;

    [SerializeField]
    private Slider m_ManaGauge;

    public int m_MaxMana;
    public int m_ManaRemaining;
    public int m_RegenerationAmount;
    public float m_TimeRegeneration;

    public int GetMana { get { return m_ManaRemaining; } }

    private Coroutine m_Coroutine;
    private ManaPoolManager m_Manager;

    // only for debug purpose right now
    private ManaDebug m_ManaDebug;

    private void Awake()
    {
        m_Manager = FindObjectOfType<ManaPoolManager>();
        if(m_Manager)
        {
            m_ManaRemaining = m_Manager.GetInitialMana;
            m_MaxMana = m_Manager.GetMaxMana;

            m_TimeRegeneration = m_Manager.GetTimeRegeneration;
            m_RegenerationAmount = m_Manager.GetRegenerationAmount;
        }
        else
        {
            Debug.LogError("Could not find ManaPoolManager object");
        }

        m_ManaDebug = FindObjectOfType<ManaDebug>();
        UpdateManaDisplay();
        //DebugMana();
    }

    private void UpdateManaDisplay()
    {
        m_ManaGauge.value = m_ManaRemaining;
    }

    private void OnEnable()
    {
        m_Manager.OnManaUpdate += OnManaUpdated;
    }

    private void OnDisable()
    {
        m_Manager.OnManaUpdate -= OnManaUpdated;

        if(m_Coroutine != null)
        {
            StopCoroutine(m_Coroutine);
        }
    }

    public void NotifyManaUsed(CharacterType type, int mana)
    {
        //m_Manager.NotifyManaUpdate(type, mana);
        DecreaseMana(mana);
    }

    public bool IsManaSufficient(int mana)
    {
        return m_ManaRemaining >= mana;
    }

    private void OnManaUpdated(CharacterType type, int mana)
    {
        if(type == m_CharacterType)
        {
            DecreaseMana(mana);
        }
        else
        {
            IncreaseMana(mana);
        }

        Debug.Log("Mana Update for character " + m_CharacterType.ToString() + " remaining : " + m_ManaRemaining);
        //UpdateManaDisplay();
        //DebugMana();
    }

    private void DecreaseMana(int amount)
    {
        //m_ManaRemaining -= amount;
        // just hack for now since some value came strange
        amount = Mathf.Clamp(amount, 0, 6);
        m_ManaRemaining = Mathf.Clamp(m_ManaRemaining - amount, 0, m_MaxMana);

        // start the coroutine as soon we get under our initial amount ( considering we can't get over our max amount )
        //if (m_Coroutine == null)
        //{
        //    m_Coroutine = StartCoroutine(ManaRegeneration());
        //}
        UpdateManaDisplay();
    }

    private void IncreaseMana(int amount)
    {
        if (m_ManaRemaining == m_MaxMana) return;

        // Todo : do we increase up to the max initial mana ?
        amount = Mathf.Clamp(amount, 0, 6);
        m_ManaRemaining = Mathf.Clamp(m_ManaRemaining + amount, 0, m_MaxMana);
        UpdateManaDisplay();
    }

    public void SetMana(int amount)
    {
        m_ManaRemaining = Mathf.Clamp(amount, 0, m_MaxMana);
        UpdateManaDisplay();
    }

    private IEnumerator ManaRegeneration()
    {
        while (m_ManaRemaining <= m_MaxMana)
        {
            yield return new WaitForSeconds(m_TimeRegeneration);
            IncreaseMana(m_RegenerationAmount);
            //DebugMana();
        }
    }

    private void DebugMana()
    {
        if (m_ManaDebug != null)
        {
            if (m_CharacterType == CharacterType.Player)
            {
                m_ManaDebug.UpdatePlayerMana(m_ManaRemaining);
            }
            else
            {
                m_ManaDebug.UpdateBotMana(m_ManaRemaining);
            }
        }
    }

    float fTmpTime = 0;
    private void Update()
    {
        if(!Global.IsGameStarted)
        {
            if (fTmpTime > 0)
            {
                fTmpTime = 0;
            }
            return;
        }
        if(m_ManaRemaining == m_MaxMana)
        {
            return;
        }
        if(fTmpTime >= m_TimeRegeneration)
        {
            fTmpTime = 0;
            IncreaseMana(m_RegenerationAmount);
        }
        else
        {
            fTmpTime += Time.deltaTime;
        }
    }
}
