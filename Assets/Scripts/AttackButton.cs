using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class AttackButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField]
    private Chip chip;
    float startPress;
    bool pointerDown = false;
    Coroutine blastRoutine = null;

    //18/10/2022
    private Color m_color;
    //

    public void OnPointerDown(PointerEventData data)
    {
        if (Global.IsPausePlay)
        {
            return;
        }

        //18/10/2022
        if (CharacterController.MyChar.m_CurrentChip == AtkType.Basic)
        {
            return;
        }
        if (CharacterController.MyChar.Stun > 0)
        {
            return;
        }
        //

        startPress = Time.time;
        pointerDown = true;

        if (blastRoutine != null)
        {
            StopCoroutine(blastRoutine);
            blastRoutine = null;
        }

        //if(!CharacterController.Player.GetComponent<Animator>().GetBool("ContinueBlasting"))
        //    CharacterController.Player.GetComponent<Animator>().SetBool("FirstBlast", true);
        //else
        //    CharacterController.Player.GetComponent<Animator>().SetBool("FirstBlast", false);
        //CharacterController.Player.GetComponent<Animator>().SetBool("ContinueBlasting", true);
        if (!CharacterController.MyChar.GetComponent<Animator>().GetBool("ContinueBlasting"))
            CharacterController.MyChar.GetComponent<Animator>().SetBool("FirstBlast", true);
        else
            CharacterController.MyChar.GetComponent<Animator>().SetBool("FirstBlast", false);
        CharacterController.MyChar.GetComponent<Animator>().SetBool("ContinueBlasting", true);
    }

    private void Update()
    {
        if (pointerDown && Time.time >= startPress + 3.0f)
            //CharacterController.Player.transform.GetChild(0).gameObject.SetActive(true);
            CharacterController.MyChar.transform.GetChild(0).gameObject.SetActive(true);
    }

    public void OnPointerUp(PointerEventData data)
    {
		if (Global.IsPausePlay)
		{
            return;
		}

        //18/10/2022
        if(CharacterController.MyChar.m_CurrentChip == AtkType.Basic)
        {
            return;
        }
        if(CharacterController.MyChar.Stun > 0)
        {
            return;
        }
        //

        //CharacterController.Player.transform.GetChild(0).gameObject.SetActive(false);
        CharacterController.MyChar.transform.GetChild(0).gameObject.SetActive(false);
        pointerDown = false;
        if(Time.time >= startPress + 3.0f)
        {
            //chip.atkType = CharacterController.Player.characterName == CharacterName.Marbelle ? AtkType.Charge : AtkType.Basic;
            //chip.Damage = 20;
            //chip.Attack(CharacterController.Player);
            chip.atkType = CharacterController.MyChar.characterName == CharacterName.Marbelle ? AtkType.Charge : AtkType.Basic;
            chip.Damage = 20;
            chip.Attack(CharacterController.MyChar);
        }
        else
        {
            chip.atkType = AtkType.Basic;
            chip.Damage = 10;
            if (/*CharacterController.Player*/CharacterController.MyChar.GetComponent<Animator>().GetBool("FirstBlast"))
                chip.Attack(/*CharacterController.Player*/CharacterController.MyChar);
            else
                chip.ContinueAttack(/*CharacterController.Player*/CharacterController.MyChar);
        }

        //var beam = CharacterController.Player.transform.GetChild(2);
        var beam = CharacterController.MyChar.transform.GetChild(2);
        if (/*CharacterController.Player*/CharacterController.MyChar.characterName == CharacterName.Draco)
        {
            beam.gameObject.SetActive(true);
            beam.GetComponent<ParticleSystem>().Play();
            
        }

        blastRoutine = StartCoroutine(EndBlasting(beam));

        //18/10/2022
        CharacterController.MyChar.m_CurrentChip = chip.atkType;
        m_color = gameObject.transform.GetChild(0).gameObject.GetComponent<UnityEngine.UI.Image>().color;
        gameObject.transform.GetChild(0).gameObject.GetComponent<UnityEngine.UI.Image>().color = Color.red;
        StartCoroutine(EndAttack());
        //
    }

    private IEnumerator DisableAfterSeconds(Transform t, float time)
    {
        yield return new WaitForSeconds(time);
        t.gameObject.SetActive(false);
    }

    private IEnumerator EndBlasting(Transform t)
    {
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(DisableAfterSeconds(t, 0.5f));
        //CharacterController.Player.GetComponent<Animator>().SetBool("ContinueBlasting", false);
        CharacterController.MyChar.GetComponent<Animator>().SetBool("ContinueBlasting", false);
    }

    private IEnumerator EndAttack()
    {
        yield return new WaitForSeconds(1.2f);
        CharacterController.MyChar.m_CurrentChip = AtkType.None;
        gameObject.transform.GetChild(0).gameObject.GetComponent<UnityEngine.UI.Image>().color = m_color;
    }
}
