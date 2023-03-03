using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Show character select page
public class ShowCharacter : MonoBehaviour
{
    public List<ChampionSelectItem> championSelectItems;
    public GameObject m_DropDown;

    //04/11/2022
    public GameObject m_MenuObj;
    //

    private void Start()
    {
        Init();
    }

    public void Init()
    {
        for(int i = 0; i < championSelectItems.Count; i++)
        {
            championSelectItems[i].gameObject.GetComponent<Image>().color = championSelectItems[i].m_DeselectColor;
        }

        if (Global.playmode == 0) //PVE : load saved charcter id
        {
            m_DropDown.SetActive(true);
            championSelectItems[PlayerPrefs.GetInt("characterid1", 0)].OnClick();
        }
        else if (Global.playmode == 1) //PVP : load saved charcter id
        {
            m_DropDown.SetActive(false);
            championSelectItems[PlayerPrefs.GetInt("characterid2", 0)].OnClick();
        }
        else if (Global.playmode == 2) //Tournament
        {
            m_DropDown.SetActive(false);
            if (ServerManager.instance.bExist) //load saved charcter id
            {
                championSelectItems[ServerManager.instance.m_Mine.characterid].OnClick();
            }
        }
    }

    public void BackButtonClick()
    {
        m_MenuObj.SetActive(true);

        //25/10/2022
        gameObject.transform.localScale = Vector3.zero;
        //
    }
}
