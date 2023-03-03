using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChampionSelectItem : MonoBehaviour
{
    public TextMeshProUGUI m_RSVPText;
    [SerializeField]
    private int index;
    [SerializeField]
    private Image image = null;
    public void Initialise(Sprite sprite) => image.sprite = sprite;
    public Color m_SelectColor;
    public Color m_DeselectColor;


    public void OnClick()
    {
        GameController.SelectChampionSprite(index);
        GetComponent<Image>().color = m_SelectColor;
        CharacterController.Player.characterName = (CharacterName)index;
        foreach (var sib in transform.parent.GetComponentsInChildren<Transform>()
            .Where(x => x.transform != transform && x.transform.parent == transform.parent))
            sib.GetComponent<Image>().color = m_DeselectColor;

        if (Global.playmode == 0) //PVE : save selected character id
        {
            PlayerPrefs.SetInt("characterid1", index);
        }
        else if (Global.playmode == 1) //PVP : save selected character id
        {
            PlayerPrefs.SetInt("characterid2", index);
        }
        else if (Global.playmode == 2) //Tournament : save selected character id
        {
            if(m_RSVPText.text == "Lobby")
            {
                if(PlayerPrefs.GetInt("characterid3") != index)
                {
                    m_RSVPText.text = "Update";
                }
            }

            PlayerPrefs.SetInt("characterid3", index);
        }
    }
}
