using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DamianGonzalez.Portals {
    [DefaultExecutionOrder(999)] //last
    public class CountRenderPasses : MonoBehaviour {
        PortalCamMovement[] allCameras;
        TMPro.TextMeshProUGUI text;

        void Start() {
            allCameras = FindObjectsOfType<PortalCamMovement>();
            text = GetComponent<TMPro.TextMeshProUGUI>();
        }

        void OnEnable() {
            InvokeRepeating(nameof(SlowUpdate), 0, .2f);
        }

        void OnDisable() {
            CancelInvoke();
        }

        void SlowUpdate() {
            int a = 0;
            foreach (PortalCamMovement script in allCameras) {
                a += script.int_renderPasses;

            }
            text.text = "Total render passes: " + a.ToString();
        }
    }
}