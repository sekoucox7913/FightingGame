using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System;
using Random = UnityEngine.Random;

public class ChargeMeter : MonoBehaviour, IPointerDownHandler
{
    [SerializeField]
    private Slider chargeSlider = null;
    [SerializeField]
    private TMP_Text text = null;
    [SerializeField]
    private GameObject chipSlots;

    [SerializeField]
    private GameObject m_GraveyardSlot;

    private bool startedClickOnCharge = false;

    public void OnGameStart()
    {
        Reshuffle(true);
    }

    public void Update()
    {
        if (!Global.IsGameStarted)
            return;

        if (EventSystem.current.currentSelectedGameObject == gameObject)
        {
            if (Input.GetButtonDown("Fire1"))
            {
                startedClickOnCharge = chargeSlider.value < chargeSlider.maxValue;
                //CharacterController.Player.PlayAnim("Charge");
                CharacterController.MyChar.PlayAnim("Charge");
            }

            if (Input.GetButton("Fire1"))
            {
                chargeSlider.value += (Time.deltaTime / 5.0f);
                //CharacterController.Player.transform.GetChild(0).gameObject.SetActive(true);
                CharacterController.MyChar.transform.GetChild(0).gameObject.SetActive(true);
            }

            if (Input.GetButtonUp("Fire1"))
            {
				//CharacterController.Player.transform.GetChild(0).GetComponent<ParticleSystem>().Stop();
				//StartCoroutine(DisableAfterSeconds(CharacterController.Player.transform.GetChild(0), 0.5f));
				//CharacterController.Player.PlayAnim("Idle");
				CharacterController.MyChar.transform.GetChild(0).GetComponent<ParticleSystem>().Stop();
				StartCoroutine(DisableAfterSeconds(CharacterController.MyChar.transform.GetChild(0), 0.5f));
				CharacterController.MyChar.PlayAnim("Idle");
			}
        }

        if (/*CharacterController.Player*/CharacterController.MyChar.deck.Sum(x => x.qty) == 0)
            GetComponent<Button>().interactable = false;
        else
            GetComponent<Button>().interactable = true;

        if (chargeSlider.value < chargeSlider.maxValue)
            chargeSlider.value += (Time.deltaTime / (10.0f));

    }

    private IEnumerator DisableAfterSeconds(Transform t, float time)
    {
        yield return new WaitForSeconds(time);
        t.gameObject.SetActive(false);
    }

    public void OnClick()
    {
        if (chargeSlider.value >= chargeSlider.maxValue && !startedClickOnCharge)
            Reshuffle();

        GameController.ShakeScreen(50);
    }

    public void Reshuffle(bool replaceAllChips = false)
    {
        //CharacterController.Player.mambaActive = false;
        CharacterController.MyChar.mambaActive = false;
        chargeSlider.value = chargeSlider.minValue;
        int drawCount = 0;

        List<Chip> chipToDestroy = new List<Chip>();

        // Todo : check the graveyard instead
        foreach(var chip in m_GraveyardSlot.GetComponentsInChildren<Chip>())
        {
            if (chip.IsGraveyard)
            {
                chipToDestroy.Add(chip);
                drawCount++;
            }
            else if (replaceAllChips)
            {
                chipToDestroy.Add(chip);
            }
        }

        /*
        foreach (var chip in chipSlots.GetComponentsInChildren<Chip>())
        {
            if (chip.IsEmpty)
            {
                chipToDestroy.Add(chip);
                drawCount++;
            }
            else if (replaceAllChips)
                chipToDestroy.Add(chip);
        }
        */

        foreach (var chip in chipToDestroy)
            Destroy(chip.gameObject);

        chipToDestroy.Clear();

        if (replaceAllChips)
            drawCount = 5;

        for (int i = 0; i < drawCount; i++)
        {
            try
            {
				//int randIdx = Random.Range(0, CharacterController.Player.deck.Count);
				//Instantiate(CharacterController.Player.deck[randIdx].chip, chipSlots.transform);
				//CharacterController.Player.deck[randIdx].qty--;

				//if (CharacterController.Player.deck[randIdx].qty <= 0)
				//{
				//    var go = CharacterController.Player.deck[randIdx];
				//    CharacterController.Player.deck.RemoveAt(randIdx);
				//    Destroy(go.gameObject);
				//}
				int randIdx = Random.Range(0, CharacterController.MyChar.deck.Count);
				Instantiate(CharacterController.MyChar.deck[randIdx].chip, chipSlots.transform);
				CharacterController.MyChar.deck[randIdx].qty--;

				if (CharacterController.MyChar.deck[randIdx].qty <= 0)
				{
					var go = CharacterController.MyChar.deck[randIdx];
					CharacterController.MyChar.deck.RemoveAt(randIdx);
					//Destroy(go.gameObject);
				}
			}
            catch (ArgumentOutOfRangeException) { }
        }

        if(CharacterController.MyChar.deck.Count == 0)
        {
            transform.GetComponentInParent<ActionController>().DisableCharge();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        GameController.ShakeScreen(50);
    }
}
