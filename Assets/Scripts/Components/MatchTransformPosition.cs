using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchTransformPosition : MonoBehaviour
{
    [SerializeField]
    private Transform m_Target;

    private Vector2 m_Offset;

    public void SetTargetTransform(Transform target, Vector2 offset)
    {
        m_Target = target;
        m_Offset = offset;
    }

    private void Update()
    {
        if (m_Target != null)
        {
            transform.position = new Vector2(m_Target.position.x + m_Offset.x, m_Target.position.y + m_Offset.y);
        }
    }
}
