using UnityEngine;
using TMPro;
using System.Linq;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class ChipLibrary : MonoBehaviour
{
    public TextMeshProUGUI m_RSVPText;

    public ChipPack selectedPack = null;

    [SerializeField]
    private float maxCapacity = 2000;
    [SerializeField]
    private TMP_Text capacity;

    public GameObject m_MenuObj;
    public Transform m_Dock;

    public List<ChipPack> m_chipPackList;
    public GameObject m_ChipContainer;

    private void Start()
    {
        Init();
    }

    public void Init()
    {
        foreach (var pack in m_ChipContainer.GetComponentsInChildren<ChipPack>(true))
        {
            if (pack.limitToPlayer == Player.None || (int)pack.limitToPlayer == (int)CharacterController.Player.characterName + 1)
                pack.gameObject.SetActive(true);
            else
                pack.gameObject.SetActive(false);
        }

        if (Global.playmode == 0) //PVE : load pre-used card list
        {
            CharacterController.Player.deck.Clear();
            for (int i = 0; i < m_Dock.childCount; i++)
            {
                Destroy(m_Dock.GetChild(i).gameObject);
            }

            if(PlayerPrefs.HasKey("decklist1"))
            {
                if(PlayerPrefs.GetString("decklist1") != "")
                {
                    string[] splitArray = PlayerPrefs.GetString("decklist1").Split(char.Parse(","));
                    for (int j = 0; j < splitArray.Length; j++)
                    {
                        int index = int.Parse(splitArray[j]);
                        selectedPack = m_chipPackList[index];
                        AddToDeck();
                    }
                }
                else
                {
                    selectedPack = m_chipPackList[0];
                    selectedPack.Select();
                }
            }
        }
        else if (Global.playmode == 1) //PVP : load pre-used card list
        {
            CharacterController.Player.deck.Clear();
            for (int i = 0; i < m_Dock.childCount; i++)
            {
                Destroy(m_Dock.GetChild(i).gameObject);
            }

            if (PlayerPrefs.HasKey("decklist2"))
            {
                if (PlayerPrefs.GetString("decklist2") != "")
                {
                    string[] splitArray = PlayerPrefs.GetString("decklist2").Split(char.Parse(","));
                    for (int j = 0; j < splitArray.Length; j++)
                    {
                        int index = int.Parse(splitArray[j]);
                        selectedPack = m_chipPackList[index];
                        AddToDeck();
                    }
                }
                else
                {
                    selectedPack = m_chipPackList[0];
                    selectedPack.Select();
                }
            }
        }
        else if (Global.playmode == 2 && ServerManager.instance.bExist) //Tournament : if the player enter lobby after the player win, load pre-used card list
        {
            CharacterController.Player.deck.Clear();
            for (int i = 0; i < m_Dock.childCount; i++)
            {
                Destroy(m_Dock.GetChild(i).gameObject);
            }

            if (PlayerPrefs.HasKey("decklist3"))
            {
                if (PlayerPrefs.GetString("decklist3") != "")
                {
                    string[] splitArray = ServerManager.instance.m_Mine.decklist.Split(char.Parse(","));
                    for (int j = 0; j < splitArray.Length; j++)
                    {
                        int index = int.Parse(splitArray[j]);
                        selectedPack = m_chipPackList[index];
                        AddToDeck();
                    }
                }
                else
                {
                    selectedPack = m_chipPackList[0];
                    selectedPack.Select();
                }
            }
        }

        //selectedPack = null;
    }

    public void AddToDeck()
    {
        if (selectedPack != null &&
            CharacterController.Player.deck.FindAll(x => x.atkType == selectedPack.atkType).Count < 3 &&
            CharacterController.Player.deck.Sum(x => x.chip.Cost) < maxCapacity - selectedPack.chip.Cost)
        {
            selectedPack.AddToDeck();
            capacity.text = "Deck Capacity:\n" + CharacterController.Player.deck.Sum(x => x.chip.Cost) + "/" + maxCapacity + "MB";
        }
        else
        {
            if(CharacterController.Player.deck.FindAll(x => x.atkType == selectedPack.atkType).Count >= 3)
            {
                GameController.DisplayMessage(ServerManager.instance.m_MessageList[19], 2f);
            }
            else if(CharacterController.Player.deck.Sum(x => x.chip.Cost) > maxCapacity - selectedPack.chip.Cost)
            {
                GameController.DisplayMessage(ServerManager.instance.m_MessageList[20], 2f);
            }
        }
    }

    public void RemoveFromDeck()
    {
        if (selectedPack != null)
        {
            selectedPack.RemoveFromDeck();
            capacity.text = "Deck Capacity:\n" + CharacterController.Player.deck.Sum(x => x.chip.Cost) + "/" + maxCapacity + "MB";
        }
    }

    public void SaveButtonClick()
    {
        string s = "";
        for (int i = 0; i < m_Dock.childCount; i++)
        {
            if (i == 0)
            {
                s = m_Dock.GetChild(i).gameObject.GetComponent<ChipPack>().m_index.ToString();
            }
            else
            {
                s += "," + m_Dock.GetChild(i).gameObject.GetComponent<ChipPack>().m_index.ToString();
            }
        }

        if (Global.playmode == 0) //PVE : save selected card list
        {
            PlayerPrefs.SetString("decklist1", s);
        }
        else if (Global.playmode == 1) //PVP : save selected card list
        {
            PlayerPrefs.SetString("decklist2", s);
        }
        else if (Global.playmode == 2) //Tournament : save selected card list
        {
            if (m_RSVPText.text == "Lobby")
            {
                if (PlayerPrefs.GetString("decklist3") != s)
                {
                    m_RSVPText.text = "Update";
                }
            }
            PlayerPrefs.SetString("decklist3", s);
        }

        GameController.DisplayMessage(ServerManager.instance.m_MessageList[18], 2f);
    }

    public void BackButtonClick()
    {
        //if (CharacterController.Player.GetChipCount() < 5)
        //{
        //    GameController.DisplayMessage("You must have at least 5 different chips to start playing", 2f);
        //    return;
        //}

        string s = "";
        for (int i = 0; i < m_Dock.childCount; i++)
        {
            if (i == 0)
            {
                s = m_Dock.GetChild(i).gameObject.GetComponent<ChipPack>().m_index.ToString();
            }
            else
            {
                s += "," + m_Dock.GetChild(i).gameObject.GetComponent<ChipPack>().m_index.ToString();
            }
        }

        if (Global.playmode == 0)
        {
            if(PlayerPrefs.GetString("decklist1", "") != s)
            {
                GameController.DisplayMessage(ServerManager.instance.m_MessageList[15], 2f);
                return;
            }
        }
        else if (Global.playmode == 1)
        {
            if (PlayerPrefs.GetString("decklist2", "") != s)
            {
                GameController.DisplayMessage(ServerManager.instance.m_MessageList[15], 2f);
                return;
            }
        }
        else if (Global.playmode == 2)
        {
            if (PlayerPrefs.GetString("decklist3", "") != s)
            {
                GameController.DisplayMessage(ServerManager.instance.m_MessageList[15], 2f);
                return;
            }
        }

        m_MenuObj.SetActive(true);

        //25/10/2022
        //gameObject.SetActive(false);
        gameObject.transform.localScale = Vector3.zero;
        //
    }
}