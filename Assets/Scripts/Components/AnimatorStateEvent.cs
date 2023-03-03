using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorStateEvent : MonoBehaviour
{
    [SerializeField]
    private Animator m_Animator;

    public void SetBool(string nameParameter, bool state, bool destroyOnplayed = false, float destroyTime = 0f)
    {
        m_Animator.SetBool(nameParameter, state);

        if(destroyOnplayed)
        {
            Destroy(gameObject, destroyTime);
        }
    }
}
