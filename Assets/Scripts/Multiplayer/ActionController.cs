using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionController : MonoBehaviour
{
    [SerializeField] ChargeMeter _charge;
    [SerializeField] GameObject _chipSlots;
    [SerializeField] AttackButton _attack;
    [SerializeField] GameObject _noRemain;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void OnGameStart()
    {
        _charge.Reshuffle(true);
        _noRemain.SetActive(false);
    }

    public void DisableCharge()
    {
        _charge.gameObject.SetActive(false);
        _noRemain.SetActive(true);
    }
}
