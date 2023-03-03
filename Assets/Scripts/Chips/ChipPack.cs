    using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public enum Player { None,Yasuke,Draco,Marbelle};
public class ChipPack : MonoBehaviour
{
    public int qty;
    public AtkType atkType;
    public Chip chip;
    public Player limitToPlayer;    

    [SerializeField]
    private GameObject DeckContainer;
    [SerializeField]
    private TMP_Text Title;
    [SerializeField]
    private TMP_Text Description;
    [SerializeField]
    private TMP_Text Level;
    [SerializeField]
    private TMP_Text Cost;
    [SerializeField]
    private TMP_Text Damage;
    [SerializeField]
    private Image Preview;
    [SerializeField]
    private ChipLibrary chipLibrary;

    public GameObject m_SelectedImg;

    public int m_index = 0;
    public void AddToDeck()
    {
        var go = Instantiate(gameObject, DeckContainer.transform);
        CharacterController.Player.deck.Add(go.GetComponent<ChipPack>());

        Select();
        go.GetComponent<ChipPack>().Select();
    }

    public void RemoveFromDeck()
    {
        var pack = CharacterController.Player.deck.Where(x => x.atkType == atkType).FirstOrDefault();
        if(pack != null)
        {
            CharacterController.Player.deck.Remove(pack);
            Destroy(pack.gameObject);
        }
    }

    public void Select()
    {
        Title.text = chip.displayName;
        Description.text = chip.description;
        Level.text = "Mana  " + chip.Mana;
        Cost.text = "Cost  " + chip.Cost + "MB";
        Damage.text = "Damage  " + chip.Damage + "HP";
        Preview.sprite = GetComponent<Image>().sprite;

        chipLibrary.selectedPack = this;

        if(gameObject.name.Contains("Clone"))
        {
            for (int i = 0; i < chipLibrary.m_Dock.childCount; i++)
            {
                chipLibrary.m_Dock.GetChild(i).gameObject.GetComponent<ChipPack>().m_SelectedImg.SetActive(false);
            }
        }
        else
        {
            for (int i = 0; i < chipLibrary.m_ChipContainer.transform.childCount; i++)
            {
                chipLibrary.m_ChipContainer.transform.GetChild(i).gameObject.GetComponent<ChipPack>().m_SelectedImg.SetActive(false);
            }
        }

        m_SelectedImg.SetActive(true);
    }
}