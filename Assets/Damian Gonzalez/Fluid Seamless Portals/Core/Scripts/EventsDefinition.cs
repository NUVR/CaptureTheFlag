using System;
using UnityEngine;
using System.Collections.Generic;

//see documentation, or script portalEventsListenerExample.cs, for instructions on how to listen these events.

namespace DamianGonzalez.Portals {
    public static class PortalEvents {


        public static Action<
            string,     //groupId
            PortalSetup //portal set
        > setupComplete;


        public static Action<
            string,     //groupId
            Transform,  //portalFrom
            Transform,  //portalTo
            Transform,  //objTeleported
            Vector3,    //positionFrom
            Vector3     //positionTo
        > teleport;


        public static Action<
            string,     //groupId
            Transform,  //portal
            Vector2,    //oldSize
            Vector2     //newSize
        > gameResized;

        /*
        public static Action<
            Vector3,                //from
            Vector3,                //lastHit
            List<PortalRaycast.RayInfo> //subrays details
        > rayThroughPortals;

        public static Action<
            string,     //groupId
            PortalSetup,//portal set
            bool        //new state
        > onOffStateChanged;
        */
    }
}