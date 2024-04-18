using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace DamianGonzalez.Portals {
    [DefaultExecutionOrder(115)]
    public class TunnelEffect : MonoBehaviour {

        void Awake() {

            PortalCamMovement b1 = transform.GetChild(0).GetChild(1).GetComponentInChildren<PortalCamMovement>();
            PortalCamMovement a2 = transform.GetChild(1).GetChild(0).GetComponentInChildren<PortalCamMovement>();

            b1.nested.Add(a2);
            a2.nested.Add(b1);
        }

    }
}