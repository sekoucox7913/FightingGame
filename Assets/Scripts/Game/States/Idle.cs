using System.Linq;
using UnityEngine;

public class Idle : State
{
    public Idle(Character character) : base(character)
    {
        stateType = StateType.Idle;
    }

    public override void Tick()
    {
        if (character.Stun > 0.0f && !character.waiting)
            character.StunOnNextIdle(character.Stun, character.StunDelay, character.Mode);

        if (character.NoDamageStun > 0.0f && !character.waiting)
            character.NoDamageStunOnNextIdle(character.NoDamageStun);
    }

    public override void OnStateEnter()
    {
        if(Global.MyCT == character.characterType)
		{
            PunManager._Instance.SendIdle();
		}

        character.PlayAnim("Idle");

        if(character.Stun > 0.0f)
            character.StunOnNextIdle(character.Stun, character.StunDelay, character.Mode);

        if (character.NoDamageStun > 0.0f)
            character.NoDamageStunOnNextIdle(character.NoDamageStun);


        /*
        foreach(var t in character.transform.GetComponentsInChildren<Transform>()
                                  .Where(x=> x != character.transform))
            t.gameObject.SetActive(false);
        */

        foreach(var transf in character.transform.GetComponentsInChildren<Transform>())
        {
            if(transf.GetComponent<HealthDisplay>() == null && transf != character.transform)
            {
                transf.gameObject.SetActive(false);
            }
        }

        if(character == CharacterController.Enemy)
        {
            character.transform.GetChild(character.transform.childCount - 1).gameObject.SetActive(true);
        }

        if (character == CharacterController.Player)
        {
            character.transform.GetChild(character.transform.childCount - 1).gameObject.SetActive(true);
        }
    }

}
