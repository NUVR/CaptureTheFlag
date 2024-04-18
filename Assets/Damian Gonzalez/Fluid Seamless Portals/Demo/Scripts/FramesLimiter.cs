#if UNITY_EDITOR
using UnityEngine;

namespace DamianGonzalez {
    public class FramesLimiter : MonoBehaviour {
        [SerializeField] private int desiredFrameRate = 30;
        private int previousFrameRate=0;

        void OnEnable() {
            InvokeRepeating(nameof(SlowUpdate), 0f, 1f);
        }

        private void OnDisable() {
            CancelInvoke();
        }

        void SlowUpdate()   //once per second
        {
            if (desiredFrameRate != previousFrameRate) ChangeFrameRate();
        }

        void ChangeFrameRate() {
            Application.targetFrameRate = desiredFrameRate;
            QualitySettings.vSyncCount = 0;
            previousFrameRate = desiredFrameRate;
        }

    }
}
#endif