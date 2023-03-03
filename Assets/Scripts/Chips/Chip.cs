using System.Linq;
using UnityEngine;
using UnityEngine.UI;


public enum AtkType
{
    None,
    AreaAdvance,
    Blaster,
    ForcePalm,
    ForcePulse,
    K9,
    MambaMentality,
    MGun,
    NanoSword,
    Orbitar,
    ThorsFury,
    Basic,
    FireBlast,
    Charge,
    Entei,
    Boulder,
    BubbleShield,
    Earthquake,
    PeaceMaker,
    PopFire,
    RadiantBurst,
    Recovery50,
    ShadowVeil,
    SlapOff
};

public class Chip : MonoBehaviour
{
    [SerializeField]
    private Sprite emptySprite;

    [SerializeField]
    private Image image;
    [SerializeField]
    private Sprite graveyardSprite;

    public string displayName;
    public string description;
    public int Cost;
    public int level;
    public int Damage;
    public int Mana;
    public Sprite sprite;
    public AtkType atkType;
    public float cooldown;
    public float damageDelay;
    // public Mover owner;
    // ADD SCRIPT
    Button BTN_Chip;


    public Transform GetTransform()
    {
        return transform;
    }

    //[SerializeField] CharacterType CT;
	private void Start()
	{
        BTN_Chip = GetComponent<Button>();
        BTN_Chip.onClick.RemoveAllListeners();

        BTN_Chip.onClick.AddListener(() => {
            if (Global.IsPausePlay)
            {
                return;
            }

            Use(CharacterController.MyChar);
        });
	}
	//

	public void Use() => Use(/*CharacterController.Player*/CharacterController.MyChar);

    public void Use(Character character)
    {
		if (Global.IsPausePlay)
		{
            return;
		}

        //if the player is stunned, can't use card
        if (character.waiting)
            return;

        //18/10/2022
        if(character.m_CurrentChip == AtkType.ThorsFury && atkType != AtkType.Basic)
        {
            return;
        }
        if (character.m_CurrentChip == AtkType.Basic)
        {
            return;
        }

        //if the player status is shadow or protection, ignore same card
        if (character.m_ProtecttionLevel > 0 || character.m_IsIntangible)
        {
            if (atkType == AtkType.BubbleShield || atkType == AtkType.ShadowVeil)
            {
                return;
            }
        }

        if (character.CurrentStateType == StateType.Idle && image.sprite != emptySprite)
        {
            // Todo : update mana from here, also check that the character does have enough mana to pursue attack.
            Debug.Log(character.characterType.ToString() + " will attack with a chip mana of : " + Mana);

            if (character.CanUseChip(Mana))
            {
                //18/10/2022
                character.m_CurrentChip = atkType;
                //

                if (character.characterType == CharacterType.Player)
                {
                    character.UpdateMana(-Mana);
                    CharacterController.Enemy.SetMana(CharacterController.Enemy.mana + Mana);
                }
                //else
                //{
                //    //CharacterController.Player.UpdateMana(Mana);
                //    CharacterController.Player.SetMana(CharacterController.Player.mana + Mana);
                //}

                Attack(character);

                if (GetComponent<ChipAnimation>())
                {
                    GetComponent<ChipAnimation>().DoAnimation(() => {
                        SetEmpty();
                        
                        character.PushToGraveyard(this);
                        SetGraveyard();

                        //18/10/2022
                        character.m_CurrentChip = AtkType.None;
                        //
                    });
                }

                //image.sprite = emptySprite;
            }
            else
            {
                Debug.Log("#### Not enough mana to use this chip");
            }
        }
    }

    private void Update()
    {
        if(transform.parent.gameObject.name != "ChipSlots")
        {
            return;
        }

        if (!CharacterController.MyChar.CanUseChip(Mana))
        {
            image.sprite = graveyardSprite;
            image.color = Color.white;
        }
        else
        {
            image.sprite = sprite;
        }
    }

    public void SetEmpty()
    {
        image.sprite = emptySprite;
    }

    public void SetGraveyard()
    {
        image.sprite = graveyardSprite;
        image.color = Color.white;

        Destroy(GetComponent<Button>());
    }

    public void Attack(Character character)
    {
        //18/10/2022
        //if(character.CurrentStateType == StateType.Idle)
        {
            character.SetState(new Attack(character, this));
        }
    }

    public void ContinueAttack(Character character)
    {
        character.SetState(new Attack(character, this));
    }


    public bool IsEmpty => image.sprite == emptySprite;
    public bool IsGraveyard => image.sprite == graveyardSprite;
}