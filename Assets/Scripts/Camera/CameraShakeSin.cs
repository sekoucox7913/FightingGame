using UnityEngine;
using System;
using System.Collections;
using UnityEngine.Events;

namespace CameraUtility
{
    public class CameraShakeSin : MonoBehaviour
    {
        #region fields
        [SerializeField] private float startDistance = 0.8f;
        [SerializeField] private float decreaseRate = 0.5f;
        [SerializeField] private float speed = 55f;
        [SerializeField] private int cycleNumber = 3;
        [SerializeField] private bool xShake, yShake, zShake;

        private int xMul, yMul, zMul;
        private Vector3 m_StoredInitialPosition;

        [Header("Camera Shake Events")]
        public UnityEvent OnCameraShakeStart;
        public UnityEvent OnCameraShakeEnd;

        private Coroutine m_Coroutine;
        #endregion

        #region methods
        void OnEnable()
        {
            var axes = new BoolVector3(xShake, yShake, zShake);
            OnCameraShakeStart.Invoke();

            if (m_Coroutine != null) return;

            m_Coroutine = StartCoroutine(Shake(startDistance, decreaseRate, speed, cycleNumber, axes, () =>
            {
                m_Coroutine = null;
                OnCameraShakeEnd.Invoke();
                enabled = false;
            }));
        }

        public IEnumerator Shake(float startDistance, float decrease, float speed, float cycleNumber, BoolVector3 shakeAxes, Action onFinish)
        {
            SetAxis(shakeAxes);

            var thisTransform = transform;
            var initialPosition = thisTransform.localPosition;
            var hitTime = Time.time;
            var timer = 0f;
            var motion = 0f;

            m_StoredInitialPosition = initialPosition;

            while (cycleNumber > 0)
            {
                timer = (Time.time - hitTime) * speed;
                motion = Mathf.Sin(timer) * startDistance;

                thisTransform.localPosition = new Vector3(initialPosition.x + motion * xMul, initialPosition.y + motion * yMul, initialPosition.z + motion * zMul);

                if(timer > Mathf.PI * 2)
                {
                    hitTime = Time.time;
                    startDistance *= decreaseRate;
                    cycleNumber--;
                }

                yield return null;
            }

            thisTransform.localPosition = initialPosition;
            if (onFinish != null) onFinish();
        }

        void SetAxis(BoolVector3 axes)
        {
            xMul = axes.X ? 1 : 0;
            yMul = axes.Y ? 1 : 0;
            zMul = axes.Z ? 1 : 0;
        }

        // read value from slider
        public void OnValueChanged(float value)
        {
            if(value > 0.99 && isActiveAndEnabled)
            {
                if(m_Coroutine != null)
                {
                    Interrupt();
                }
            }
        }

        public void Interrupt()
        {
            if (m_Coroutine != null)
            {
                StopCoroutine(m_Coroutine);
                m_Coroutine = null;
            }

            transform.localPosition = m_StoredInitialPosition;
            enabled = false;
        }
        #endregion
    }

    public struct BoolVector3
    {
        public bool X;
        public bool Y;
        public bool Z;

        public BoolVector3(bool x, bool y, bool z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}
