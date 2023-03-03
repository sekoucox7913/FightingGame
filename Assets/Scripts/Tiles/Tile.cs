using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Tile : MonoBehaviour
{
    public Vector2Int coordinate;

    [SerializeField]
    public Character owner = null;
    public Boulder boulder = null; //boulder-in game object

    [SerializeField]
    private Image image = null;

    public bool walkable;
    public bool damaged;
    private bool highlighted = false;
    private Character originalOwner = null;

    [SerializeField]
    private Sprite m_initialImage;

    [SerializeField]
    private RectTransform m_RectTransform;

    private readonly Color playerTileColor = new Color(1.0f, 0.5f, 0.5f, 0.4f);
    private readonly Color enemyTileColor = new Color(0.3f, 0.5f, 1.0f, 0.4f);
    private readonly Color defaultTileColor = new Color(1.0f, 1.0f, 1.0f, 0.4f);
    private readonly Color highlightTileColor = new Color(1.0f, 1.0f, 1.0f, 0.4f);

    //03/12/2022
    private bool highalert = false;
    public GameObject m_alert;
    //

    private void Awake()
    {
        originalOwner = owner;

        m_initialImage = GetComponent<Image>().sprite;

        highalert = false;

        //05/12/2022
        m_alert.SetActive(false);
        //
    }

    // ADD SCRIPT
    Button BTN_Tile;
	private void Start()
	{
        BTN_Tile = GetComponent<Button>();
        BTN_Tile.onClick.AddListener(() => {
            Select(Global.MyCT == CharacterType.Player ? CharacterController.Player : CharacterController.Enemy);
        });

        m_RectTransform = GetComponent<RectTransform>();

    }
    //

	public void Select(Character c)
    {
		if (Global.IsPausePlay)
		{
            return;
		}

        if (GameController.instance.bEnd)
            return;

        //02/11/2022
        //if this tile includes boulder, the pplayer can't move to this tile.
        if (TileController.Info(coordinate).boulder != null)
            return;

        if (!TileController.Info(coordinate).walkable)
            return;
        //

        //if the player is getting damage, the player can't move.
        if (c.Stun > 0)
        {
            if(c.Mode == 1 || c.Mode == 2)
            {
                return;
            }
        }

        var currentStateType = c.CurrentStateType;
        if (currentStateType == StateType.Idle || currentStateType == StateType.Move)
        {
            c.SetState(new SetTarget(c, coordinate));
        }
        else if(currentStateType == StateType.Attack)
        {
            c.currentState.ForceSetTarget(c, coordinate);
        }
    }

    public CharacterType OwnerType => owner.characterType;

    private void Update()
    {
        SetColor();
    }

    private void SetColor()
    {
        if (owner == null)
            return;

        switch(owner.characterType)
        {
            case CharacterType.Player:
                image.color = playerTileColor;
                break;
            case CharacterType.Enemy:
                image.color = enemyTileColor;
                break;
            default:
                image.color = defaultTileColor;
                break;
        }

        if (highlighted)
            image.color = new Color(1.0f,1.0f,1.0f,0.4f);
    }

    public void Highlight(float cooldown, bool showHighlight = false)
    {
        if (!showHighlight) return;

        StartCoroutine(HighlightRoutine(cooldown));
    }

    private IEnumerator HighlightRoutine(float cooldown)
    {
        highlighted = true;
        yield return new WaitForSeconds(cooldown);
        highlighted = false;
    }

    public void HighAlert(float cooldown, bool highalert = false)
    {
        if (!highalert) return;

        StartCoroutine(HighAlertRoutine(cooldown));
    }

    private IEnumerator HighAlertRoutine(float cooldown)
    {
        highalert = true;
        m_alert.SetActive(true);
        yield return new WaitForSeconds(cooldown);
        m_alert.SetActive(false);
        highalert = false;
    }


    public void RevertOwnership(float delay)
    {
        StartCoroutine(RevertRoutine(delay));
    }

    private IEnumerator RevertRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (originalOwner == CharacterController.Player)
        {
            if (coordinate.x > 0)
            {
                while (TileController.Info(coordinate - Vector2Int.right).owner == CharacterController.Enemy)
                    yield return null;
            }
            while (CharacterController.Enemy.currentPos.x <= coordinate.x)
                yield return null;
        }
        else
        {
            if (coordinate.x < TileController.BoardSize.x - 1)
            {
                while (TileController.Info(coordinate + Vector2Int.right).owner == CharacterController.Player)
                    yield return null;
            }
            while (CharacterController.Player.currentPos.x >= coordinate.x)
                yield return null;
        }

        owner = originalOwner;
        yield break;
    }

    public Vector2 GetSize()
    {
        return m_RectTransform.sizeDelta;
    }
}
