using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
public enum CharacterType { Player, Enemy };
public enum CharacterName { Yasuke, Draco, Marbelle };
public class Character : MonoBehaviour
{
    public ParticleSystem particleSystem;
    public State currentState;
    public Vector2Int currentPos;
    public float moveSpeed;
    public bool waiting;
    public bool cancelMove;
    public bool mambaActive;
    public float Stun = 0.0f;
    public float StunDelay = 0.0f;

    public float NoDamageStun = 0.0f;
    public List<ChipPack> deck;
    public int health;
    public int mana;
    public CharacterName characterName;

    public StateType CurrentStateType => currentState.stateType;

    public CharacterType characterType;
    [SerializeField]
    public Animator animator;
    [SerializeField]
    private Image image;

    private CharacterMana m_CharacterMana;

    [SerializeField]
    private Graveyard m_CharacterGraveyard;

    // ### extra parameters from new cards

    // bubble shield
    public int m_ProtecttionLevel = 0;
    private GameObject m_ShieldObject;

    // shadow veil
    public bool m_IsIntangible;

    //18/10/2022
    public int Mode;

    public AtkType m_CurrentChip;
    public bool bRed = false;
    public bool bYellow = false;
    float fTmpTimeRed;
    float fTmpTimeYellow;
    float fFlashTime = 0.05f;
    public GameObject m_StandardDamage;
    //
    //02/11/2022
    float fTmpTime = 0;
    public int m_MaxMana;
    public int m_RegenerationAmount;
    public float m_TimeRegeneration;
    //

    public void PlayAnim(string animName)
    {
        if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Dead") && !animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Die"))
            animator.Play(animName);
    }

    private void Awake() => currentState = new Idle(this);

	private void Start()
	{
        TileController.Info(currentPos).Select(this);
        m_CharacterMana = GetComponent<CharacterMana>();
        //18/10/2022
        m_CurrentChip = AtkType.None;
        bRed = false;
        bYellow = false;
        fTmpTimeRed = 0;
        fTmpTimeYellow = 0;
        fFlashTime = 0.05f;
        //02/11/2022
        fTmpTime = 0;
        mana = 3;
        //
}

	private void FixedUpdate()
    {
        if (currentState != null)
            currentState.Tick();

        //if (characterType == CharacterType.Enemy)
        //    return;
    }

    private void Update()
    {
        if (Input.GetButtonDown("Fire1"))
            StartDrag();

        if (Input.GetButtonUp("Fire1"))
            CheckDrag();

        //19/10/2022
        //show damaged status(blink red)
        if (bRed)
        {
            if (fTmpTimeRed > fFlashTime)
            {
                var col = image.color;
                if (col.b == 1f)
                {
                    col.r = 1f;
                    col.g = 0.7f;
                    col.b = 0.7f;
                }
                else
                {
                    col.r = 1f;
                    col.g = 1f;
                    col.b = 1f;
                }
                image.color = col;
                fTmpTimeRed = 0;
            }
            else
            {
                fTmpTimeRed += Time.deltaTime;
            }
        }

        //show stunned status(blink yellow)
        if (bYellow)
        {
            if (fTmpTimeYellow > fFlashTime)
            {
                var col = image.color;
                if (col.b == 1f)
                {
                    col.r = 1f;
                    col.g = 1f;
                    col.b = 0.7f;
                }
                else
                {
                    col.r = 1f;
                    col.g = 1f;
                    col.b = 1f;
                }
                image.color = col;
                fTmpTimeYellow = 0;
            }
            else
            {
                fTmpTimeYellow += Time.deltaTime;
            }
        }
        //

        //02/11/2022
        if (!Global.IsGameStarted)
        {
            if (fTmpTime > 0)
            {
                fTmpTime = 0;
            }
            return;
        }
        if (mana == m_MaxMana)
        {
            return;
        }
        if (fTmpTime >= m_TimeRegeneration)
        {
            fTmpTime = 0;
            GenerateMana(m_RegenerationAmount);
        }
        else
        {
            fTmpTime += Time.deltaTime;
        }
        //
    }

    Vector2 dragStart;
    private void StartDrag()
    {
        dragStart = Input.mousePosition;
    }

    private void CheckDrag()
    {
        if (characterType != Global.MyCT /*CharacterType.Player*/)
            return;

        //18/10/2022
        if (m_CurrentChip == AtkType.Basic)
        {
            return;
        }
        if(Stun > 0)
        {
            return;
        }
        //


        if (Vector2.Distance(Input.mousePosition, dragStart) > 100)
        {
            if(Mathf.Abs(Input.mousePosition.x - dragStart.x) > Mathf.Abs(Input.mousePosition.y - dragStart.y))
            {
                if (Input.mousePosition.x > dragStart.x)
                {
                    if (currentPos.x < TileController.BoardSize.x - 1 && 
                        TileController.Info(currentPos + Vector2Int.right).boulder == null &&
                        TileController.Info(currentPos + Vector2Int.right).walkable)
                        TileController.Info(currentPos + Vector2Int.right).Select(this);
                }
                else
                {
                    if (currentPos.x > 0 && 
                        TileController.Info(currentPos - Vector2Int.right).boulder == null &&
                        TileController.Info(currentPos - Vector2Int.right).walkable)
                        TileController.Info(currentPos - Vector2Int.right).Select(this);
                }
            }
            else
            {
                if (Input.mousePosition.y > dragStart.y)
                {
                    if (currentPos.y < TileController.BoardSize.y - 1 && 
                        TileController.Info(currentPos + Vector2Int.up).boulder == null &&
                        TileController.Info(currentPos + Vector2Int.up).walkable)
                        TileController.Info(currentPos + Vector2Int.up).Select(this);
                }
                else
                {
                    if (currentPos.y > 0 && 
                        TileController.Info(currentPos - Vector2Int.up).boulder == null &&
                        TileController.Info(currentPos - Vector2Int.up).walkable)
                        TileController.Info(currentPos - Vector2Int.up).Select(this);
                }
            }
        }
    }

    public void SetState(State state)
    {
        if (currentState != null)
            currentState.OnStateExit();

        currentState = state;

        if (currentState != null)
            currentState.OnStateEnter();
    }

    public void StunOnNextIdle(float duration, float delay, int mode) => StartCoroutine(StunRoutine(duration, delay, mode));
    
    public void NoDamageStunOnNextIdle(float duration) => StartCoroutine(NoDamageStunRoutine(duration));

    private IEnumerator NoDamageStunRoutine(float duration)
    {
        if(health <= 0)
        {
            yield break;
        }

        waiting = true;
        yield return new WaitForSeconds(duration);
        waiting = false;
        NoDamageStun = 0.0f;
        var col = image.color;
        col.r = 1f;
        col.g = 1f;
        col.b = 1f;
        image.color = col;
    }

    //process damage effect
    private IEnumerator StunRoutine(float duration, float delay, int mode)
    {
        if (health <= 0)
        {
            yield break;
        }

        //Debug.LogError("Damaged");

        yield return new WaitForSeconds(delay);
        waiting = true;

        //19/10/2022
        if(mode == 0)
        {
            bRed = false;
            bYellow = false;

            PlayAnim("Hurt");
        }
        else if(mode == 1) //heavy hit
        {
            bYellow = false;

            fTmpTimeRed = 0;
            bRed = true;
            PlayAnim("Hurt");
        }
        else if(mode == 2) //stun Force palm
        {
            bRed = false;

            fTmpTimeYellow = 0;
            bYellow = true;
            PlayAnim("Stun");
        }
        else if (mode == 3) //standard hit
        {
            bRed = false;
            bYellow = false;

            m_StandardDamage.GetComponent<StandardDamage>().ShowEffect();
            PlayAnim("Hurt");
        }
        else if (mode == 4) //force pulse
        {
            bYellow = false;

            fTmpTimeRed = 0;
            bRed = true;
            PlayAnim("Hurt");
        }
        else
        {
            bRed = false;
            bYellow = false;

            PlayAnim("Hurt");
        }
        //
        yield return new WaitForSeconds(duration);
        waiting = false;
        Stun = 0.0f;

        //19/10/2022
        if (mode == 0)
        {

        }
        else if (mode == 1)
        {
            bRed = false;
            fTmpTimeRed = 0;
        }
        else if (mode == 2)
        {
            bYellow = false;
            fTmpTimeYellow = 0;
        }
        else if (mode == 3)
        {

        }
        else if (mode == 4)
        {
            bRed = false;
            fTmpTimeRed = 0;
        }
        else
        {

        }
        var col = image.color;
        col.r = 1f;
        col.g = 1f;
        col.b = 1f;
        image.color = col;
        //
    }

    public void TakeDamage(int amount)
    {
        if (health <= 0)
        {
            return;
        }

        if (waiting && Mode != 2)
            return;

        // check for any shield protection
        if (IsShieldProtected() || m_IsIntangible)
        {
            return;
        }

        if (Global.playmode > 0 && Global.MyCT != characterType)
		{
            return;
		}

        int hl = health - amount;
        if(hl < 0)
        {
            hl = 0;
        }
        health = hl;
        if (Global.MyCT == characterType)
        {
            if(Global.playmode != 0) //02/11/2022
            {
                PunManager._Instance.SendHealth(health);
            }
        }
        GameController.ShakeScreen(amount);

        if(health == 0)
        {
            animator.SetInteger("Die", 1);
            GameController.instance.EndGame();
        }
    }

    public void SetHealth(int amount)
	{
        this.health = amount;
        if (health <= 0)
        {
            animator.SetInteger("Die", 1);
            GameController.instance.EndGame();
        }
    }

    public void SetMana(int amount)
    {
        this.mana = Mathf.Clamp(amount, 0, m_MaxMana);
    }

    public void GenerateMana(int amount)
    {
        this.mana = Mathf.Clamp(mana + amount, 0, m_MaxMana);
    }

    public void UpdateMana(int amount)
    {
        if (Global.playmode > 0 && Global.MyCT != characterType)
        {
            return;
        }

        mana = Mathf.Clamp(mana + amount, 0, m_MaxMana);

        if (Global.MyCT == characterType)
        {
            if (Global.playmode != 0) //02/11/2022
            {
                PunManager._Instance.SendMana(amount);
            }
        }
    }


    public void Heal(int amount)
    {
        if (Global.playmode > 0 && Global.MyCT != characterType)
        {
            return;
        }

        int hl = health + amount;
        if (hl < 0)
        {
            hl = 0;
        }
        health = hl;
        if (Global.MyCT == characterType)
        {
            if (Global.playmode != 0) //02/11/2022
            { 
                PunManager._Instance.SendHealth(health);
            }
        }
    }

    public bool CanUseChip(int amount)
    {
        return IsManaSufficient(amount);
    }

    //02/11/2022
    public bool IsManaSufficient(int amount)
    {
        return mana >= amount;
    }
    //

    public void SetProtection(int protectionLevel, GameObject shieldObject)
    {
        m_ProtecttionLevel = protectionLevel;
        m_ShieldObject = shieldObject;
    }

    // for bubble shield chip
    private bool IsShieldProtected()
    {
        if(m_ProtecttionLevel > 0)
        {
            m_ProtecttionLevel -= 1;

            if(m_ProtecttionLevel <= 0)
            {
                // destroy the prefab
                m_ShieldObject.GetComponent<AnimatorStateEvent>().SetBool("Breaking", true, true, 0.3f);
            }

            return true;
        }
        return false;
    }

    /// <summary>
    /// set charatcer alpha and also the Intangible flag from a shadow veil used card.
    /// </summary>
    /// <param name="alphaValue">character will be intangible for value less than 1f</param>
    public void SetCharacterTransparency(float alphaValue)
    {
        m_IsIntangible = alphaValue < 1 ? true : false;

        var imageColor = image.color;
        imageColor.a = alphaValue;

        image.color = imageColor;
    }

    public void PushToGraveyard(Chip chip)
    {
        if(m_CharacterGraveyard != null)
        {
            m_CharacterGraveyard.Push(chip);
        }
    }

    public void PullFromGraveyard()
    {
        if (m_CharacterGraveyard != null)
        {

        }
    }

    public int GetChipCount()
    {
        int res = (from x in deck select x.m_index).Distinct().Count();
        return res;
    }
}
