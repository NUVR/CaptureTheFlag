//using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DamianGonzalez.Portals {

    [DefaultExecutionOrder(111)] //110 Movement > 111 Teleport > 112 PortalCamMovement > 113 PortalSetup (rendering) > 114 PortalRenderer
    public class Teleport : MonoBehaviour {

        /*
         * Quick reminder (for visitors and for myself) of how this works.
         * 
         * Player uses elastic plane, so he/she teleports on Update, the trigger does nothing on him/her
         * Other objects follows this plan:
         *  - when they enter the trigger, a clone is made on the other side, but the original (and it physics) keeps on this side
         *  - while they are inside, the clone is updated
         *  - if player crosses sides while the object is still inside, clone and original are swapped (for physics)
         *  - when it exits the trigger, it teleports and the clone is destroyed
         * 
         * 
         * */

        public bool portalIsEnabled = true;

        private Vector3 originalPlanePosition;


        [HideInInspector] public BoxCollider _collider;               //
        [HideInInspector] public Transform plane;                     //
        [HideInInspector] public Transform portal;                    // these variables are automatically writen
        [HideInInspector] public PortalSetup setup;                   // by PortalSetup 
        [HideInInspector] public Transform mainObject;                //
        [HideInInspector] public PortalCamMovement cameraScript;      //
        [HideInInspector] public Teleport otherScript;                //


        [HideInInspector] public bool planeIsInverted; //only for elastic mode
        [HideInInspector] public bool changeGravity = true;
        [HideInInspector] public bool teleportPlayerOnExit = false;
        [HideInInspector] public bool dontTeleport = false;

        private CharacterController _playerCc;
        private Rigidbody _playerRb;

        Dictionary<Transform, Transform> clones = new Dictionary<Transform, Transform>(); //original => clone
        GameObject cloneParent;


        private void Start() {
            originalPlanePosition = plane.localPosition;
        }

        public void DisableThisPortal() {
            SetEnabled(false);
        }

        public void EnableThisPortal() {
            SetEnabled(true);
        }

        public void SetEnabled(bool _enabled) {
            portalIsEnabled = _enabled;
            gameObject.SetActive(portalIsEnabled);                //trigger (functional)
            cameraScript.gameObject.SetActive(portalIsEnabled);   //camera (functional)
            plane.gameObject.SetActive(portalIsEnabled);          //plane (visual)
        }



        Vector3 GetVelocity(Transform obj) {

            if (obj == setup.player.playerMainObj) {
                if (setup.player.controllerType == PortalSetup.Player.ControllerType.CharacterController) return setup.player.playerCc.velocity;
                if (setup.player.controllerType == PortalSetup.Player.ControllerType.Rigidbody) return setup.player.playerRb.velocity;
            } else {
                if (obj.TryGetComponent(out Rigidbody rb)) return rb.velocity;
                if (obj.TryGetComponent(out CharacterController cc)) return cc.velocity;
            }
            return Vector3.zero;
        }

        Vector3 TowardDestination(Transform obj) => TowardDestinationSingleSided();


        Vector3 TowardDestinationSingleSided() => planeIsInverted ? -portal.forward : portal.forward;

        public bool IsGoodSide(Transform obj) {

            //not about facing, but about velocity. where is it going?
            Vector3 velocityOrPosition = GetVelocity(obj);
            float dotProduct;
            if (velocityOrPosition != Vector3.zero) {
                //it has velocity
                dotProduct = Vector3.Dot(-TowardDestinationSingleSided(), velocityOrPosition);
            } else {
                //it hasn't velocity, let's try with its position (it may fail with very fast objects)
                dotProduct = Vector3.Dot(-TowardDestinationSingleSided(), portal.position - velocityOrPosition);
            }

            return dotProduct < 0;
        }

        public bool IsGoodSide(Vector3 dir) => Vector3.Dot(-TowardDestinationSingleSided(), dir) < 0;
        




        bool CandidateToTeleport(Transform objectToTeleport) {
            /*
             * an object is candidate to teleport now if:
             * a) it's player, or not player but it passes the tag filters
             * and
             * b) portal is double-sided, or single-sided but in the good side
             * and
             * c) object is not too far from the portal -> no longer necessary from v1.4
             */

            //a)
            //bool isPlayer = objectToTeleport.CompareTag(setup.filters.playerTag); //for better readability
            if (!ThisObjectCanCross(objectToTeleport)) {
                if (setup.verboseDebug) Debug.Log($"{objectToTeleport.name} will not teleport. Reason: filters");
                return false;
            }

            //b) 
            if (!IsGoodSide(objectToTeleport)) {
                if (setup.verboseDebug) Debug.Log($"{objectToTeleport.name} will not teleport. Reason: not the good side"); 
                return false;
            }

            //c)
            /*
            bool tooFar = Vector3.Distance(objectToTeleport.position, portal.position) > setup.advanced.maximumDistance;
            if (tooFar) {
                if (setup.verboseDebug) Debug.Log($"{objectToTeleport.name} will not teleport. Reason: too far");
                return false;
            }
            */
            return true;
        }

        void DoTeleport(Transform objectToTeleport, bool fireEvents = true) { //note: we're still inside OnTrigger...

            bool isPlayer = objectToTeleport.CompareTag(setup.filters.playerTag);
            if (isPlayer) playerInsideTrigger = false;

            if (isPlayer) {
                /*
                 * If you need to do something to your player RIGHT BEFORE teleporting,
                 * this is when.
                */
            }

            


            //Before teleporting, check some old positions
            Vector3 oldPosition = objectToTeleport.position;
            Vector3 rbToPlaneOffset = Vector3.zero;
            Rigidbody rb = objectToTeleport.GetComponent<Rigidbody>();
            if (rb != null) rbToPlaneOffset = transform.InverseTransformPoint(rb.position);
            Vector3 camToPlayerOffset = setup.player.playerMainObj.InverseTransformPoint(setup.player.playerCamera.position);


            //position
            objectToTeleport.position = otherScript.portal.TransformPoint(
                portal.InverseTransformPoint(objectToTeleport.position)
            );


            //rotation
            objectToTeleport.rotation =
                otherScript.portal.rotation
                * Quaternion.Inverse(portal.rotation)
                * objectToTeleport.rotation;



            //velocity (if object has rigidbody)
            if (rb != null) {
                rb.velocity = otherScript.portal.TransformDirection(
                    portal.InverseTransformDirection(rb.velocity)
                );
                rb.position = transform.position + rbToPlaneOffset; //not entirely necessary
            }

            //change of gravity => only in "full" version


            if (isPlayer) {

                //player has crossed. If using clones, may be necessary to swap clones and originals (see documentation)
                
                if (setup.clones.useClones) {
                    SwapOriginalsAndClones();
                }
                


                //avoid miscalulating the distance from previous frame
                lastFramePosition = Vector3.zero;
                otherScript.lastFramePosition = Vector3.zero;


                //refresh camera position before rendering, in order to avoid flickering
                otherScript.cameraScript.Recalculate();
                cameraScript.Recalculate();
                //MoveElasticPlane(.1f);
                //otherScript.MoveElasticPlane(.1f);

                if (setup.afterTeleport.tryResetCharacterController) {
                    //reset player's character controller (if there is one)
                    //otherwise it won't allow the change of position
                    if (objectToTeleport.TryGetComponent(out CharacterController cc)) {
                        cc.enabled = false;
                        cc.enabled = true;
                    }
                }

                if (setup.afterTeleport.tryResetCameraObject) {
                    //reset player's camera (may solve issues)
                    setup.player.playerCamera.gameObject.SetActive(false);
                    setup.player.playerCamera.gameObject.SetActive(true);
                }

                if (setup.afterTeleport.mantainPlayerOffset) {
                    //move the camera to where it was, relative to portal
                    setup.player.playerCamera.position = setup.player.playerMainObj.transform.TransformPoint(camToPlayerOffset);
                }

                if (setup.afterTeleport.tryResetCameraScripts) {
                    //reset scripts in camera
                    foreach (MonoBehaviour scr in setup.player.playerCamera.GetComponents<MonoBehaviour>()) {
                        if (scr.isActiveAndEnabled) {
                            scr.enabled = false;
                            scr.enabled = true;
                        }
                    }
                }

                /*
                 * If you need to do something to your player AFTER teleporting, this is when.
                 * See online documentation about controllers (pipasjourney.com/damianGonzalez/portals/#controller)
                 * and how to implement a 3rd party controller with these portals
                */

                if (setup.afterTeleport.pauseWhenPlayerTeleports) Debug.Break();

            } else {
                // If you need to do something to other crossing object, this is when.
                if (setup.afterTeleport.pauseWhenOtherTeleports) Debug.Break();
            }

            //finally, fire event
            if (fireEvents)
                PortalEvents.teleport?.Invoke(
                    setup.groupId,
                    portal,
                    otherScript.portal,
                    objectToTeleport,
                    oldPosition,
                    objectToTeleport.position
                );
        }

        
        public void SwapOriginalsAndClones() {
            if (clones.Count == 0 && otherScript.clones.Count == 0) return;


            //first, let's destroy all clones (they are no longer necessary)
            // - get originals and clones from both sides
            List<Transform> originalsHere = new List<Transform>();
            List<Transform> clonesHere = new List<Transform>();
            foreach (KeyValuePair<Transform, Transform> clone in clones) {
                clonesHere.Add(clone.Value);
                if (clone.Key != setup.player.playerMainObj) originalsHere.Add(clone.Key);
            }

            List<Transform> originalsThere = new List<Transform>();
            List<Transform> clonesThere = new List<Transform>();
            foreach (KeyValuePair<Transform, Transform> clone in otherScript.clones) {
                clonesThere.Add(clone.Value);
                originalsThere.Add(clone.Key);
            }

            // - delete the actual objects and remove from "clones"
            foreach (Transform clone in clonesHere) TryDestroyClone(clone);
            foreach (Transform clone in clonesThere) otherScript.TryDestroyClone(clone);


            //then, let's teleport the originals (they will "enter" the trigger on the other side and make a clone in this side)

            foreach (Transform original in originalsHere) DoTeleport(original);
            foreach (Transform original in originalsThere) otherScript.DoTeleport(original);
            


        }
        

        bool thisOne = false;
        void OnTriggerEnter(Collider other) {

            bool isPlayer = other.CompareTag(setup.filters.playerTag);

            //player detection
            if (isPlayer) { 
                playerCollider = other;
                playerInsideTrigger = true;
            }

            //make clone?
            if (setup.clones.useClones && (!isPlayer || setup.clones.clonePlayerToo)) {
                CreateCloneOnTheOtherSide(other.transform);
            }

        }

        bool ConsiderTeleporting(Collider other) { //note: we're still inside OnTrigger...
            bool isPlayer = other.CompareTag(setup.filters.playerTag);

            //process timeout (and continues)
            if (Time.time > setup.lastTeleportTime + .05f) {
                thisOne = false;
                otherScript.thisOne = false;
                setup.teleportInProgress = false;
            }

            //process ends, but doesn't continue
            if (setup.teleportInProgress) {
                if (!thisOne) {
                    otherScript.thisOne = false;
                    setup.teleportInProgress = false;
                }
                if (setup.verboseDebug) Debug.Log($"{other.name} will not teleport. Reason: too soon");
                return false;
            }


            if (!CandidateToTeleport(other.transform)) {
                return false;
            }

            //ok, it's candidate to teleport
            if (setup.verboseDebug) Debug.Log($"{other.name} passed the filters, will teleport.");

            setup.teleportInProgress = true;
            setup.lastTeleportTime = Time.time;
            thisOne = true;


            //bool createClone = setup.clones.useClones; // && vel.magnitude > setup.advanced.maxVelocityForClones;

            DoTeleport(other.transform);
            
            return true;
        }

        void TryDestroyClone(Transform clone_or_original) {
            foreach (KeyValuePair<Transform, Transform> cl in clones) {
                if (cl.Value.Equals(clone_or_original) || cl.Key.Equals(clone_or_original)) {
                    //this is a clone. Delete the object
                    Destroy(cl.Value.gameObject);

                    //and the reference
                    clones.Remove(cl.Key);

                    return;
                }
            }

        }

        void OnTriggerExit(Collider other) {

            bool isPlayer = other.CompareTag(setup.filters.playerTag);
            if (isPlayer) playerInsideTrigger = false;

            //make clone?
            if (setup.clones.useClones && (!isPlayer || setup.clones.clonePlayerToo)) {
                TryDestroyClone(other.transform);
                if (!isPlayer) ConsiderTeleporting(other);
            }

            if (setup.teleportInProgress) return; //do not disturb



            //if using elastic plane and player crosses too fast (1 or 2 frames), it should consider to be teleported
            if (setup.advanced.useElasticPlane && isPlayer && Time.time > setup.lastTeleportTime + .05f) {
                if (setup.verboseDebug) Debug.Log($"Emergency teleport! {other.name} has left the trigger.");
                ConsiderTeleporting(other);
                
            }


        }

        public void RestorePlaneOriginalPosition() {
            plane.localPosition = originalPlanePosition;
            
            SetClippingOffset(setup.advanced.clippingOffset);
            cameraScript.ApplyAdvancedOffset();

            otherScript.SetClippingOffset(setup.advanced.clippingOffset);
            otherScript.cameraScript.ApplyAdvancedOffset();
        }

        

        public float trespassProgress;
        public bool playerInsideTrigger = false;
        Collider playerCollider;
        void OnTriggerStay(Collider other) {
            

            //other side has a clone? update it
            if (setup.clones.useClones && clones.ContainsKey(other.transform)) UpdateClone(other.transform);

            //elastic plane
            bool isPlayer = other.CompareTag(setup.filters.playerTag);
            if (isPlayer) { 
                playerInsideTrigger = true;
                playerCollider = other;
            }
 
        }

        private void Update() {
            //calculate trespass progress
            trespassProgress = DistanceCameraPlane();

            if (playerInsideTrigger && setup.advanced.useElasticPlane) {

                //move this plane
                MoveElasticPlane(trespassProgress);

                //teleport player when the progress treshold is reached
                if (setup.advanced.useElasticPlane && trespassProgress > setup.advanced.elasticPlaneTeleportTreshold) {
                    if (setup.verboseDebug) Debug.Log($"teleported because {trespassProgress} > {setup.advanced.elasticPlaneTeleportTreshold}");
                    ConsiderTeleporting(playerCollider);
                }
            } else {
                //RestorePlaneOriginalPosition();
            }
        }

        public float DistanceCameraPlane() {
            return Vector3.Dot(
                -TowardDestination(setup.player.playerMainObj), 
                portal.position - setup.player.playerCamera.position
            );
        }

        Vector3 lastFramePosition = Vector3.zero;
        Vector3 deltaMov;


        private void MoveElasticPlane(float _trespassProgress, bool forced = false) {
            if (_trespassProgress > setup.advanced.elasticPlaneMinTreshold  && _trespassProgress <= setup.advanced.elasticPlaneTeleportTreshold) {
                
                //calculate offset by velocity
                float relativeSpeed = 0;
                if (setup.advanced.dynamicOffsetBasedOnVelocity) {

                    //if player just crossed this frame, don't recalculate deltaMov, use last frame's speed
                    if (lastFramePosition != Vector3.zero) {
                        deltaMov = setup.player.playerCamera.position - lastFramePosition;
                    }

                    relativeSpeed = Vector3.Dot(TowardDestination(setup.player.playerMainObj), deltaMov);
                } 

                
                //calculate offset
                float totalOffset = 
                    setup.advanced.elasticPlaneOffset * 1                           //1. the constant minimum offset
                    + _trespassProgress                                             //2. how much player has crossed
                    + relativeSpeed * setup.advanced.elasticPlaneVelocityFactor     //3. an extra according to velocity
                    ;


                //apply
                plane.localPosition = originalPlanePosition;
                plane.position += TowardDestination(setup.player.playerMainObj) * totalOffset;

                SetClippingOffset(-(_trespassProgress) + setup.advanced.clippingOffset);
                
            } else {
                //RestorePlaneOriginalPosition();
            }

            lastFramePosition = setup.player.playerCamera.position;
        }

        void SetClippingOffset(float value) {
            cameraScript.currentClippingOffset = value;
            otherScript.cameraScript.currentClippingOffset = value;
        }


        Transform CreateCloneOnTheOtherSide(Transform original) {
            if (setup.verboseDebug) Debug.Log($"Creating clone for {original.name}");
            if (cloneParent == null) {
                cloneParent = GameObject.Find("Portal Clones") ?? new GameObject("Portal Clones");
            }

            //this clone already exists? remove it
            if (clones.ContainsKey(original)) TryDestroyClone(original);


            Transform clone = Instantiate(original.gameObject, cloneParent.transform).transform;
            clone.name = "(portal clone) " + original.name;

            clones.Add(original, clone);

            //destroy some components from itself and childrens, to obtain a simplified version of the object

            foreach (Rigidbody rb in clone.GetComponentsInChildren<Rigidbody>()) Destroy(rb);
            foreach (Collider col in clone.GetComponentsInChildren<Collider>()) Destroy(col);
            foreach (Camera cam in clone.GetComponentsInChildren<Camera>()) cam.enabled = false; //destroying not allowed in URP/HDRP
            foreach (CharacterController cc in clone.GetComponentsInChildren<CharacterController>()) Destroy(cc);
            foreach (AudioListener lis in clone.GetComponentsInChildren<AudioListener>()) Destroy(lis);
            foreach (MonoBehaviour scr in clone.GetComponentsInChildren<MonoBehaviour>()) {
                bool destroyScript = true;
                if (typeof(TMPro.TextMeshPro) != null && scr.GetType() == typeof(TMPro.TextMeshPro)) destroyScript = false;
                if (destroyScript) Destroy(scr);
            }


            clone.gameObject.tag = "Untagged";
            return clone;
        }

        void UpdateClone(Transform original) {
            if (!clones.ContainsKey(original)) return;

            Transform clone = clones[original];


            //similar calculations to the actual teleporting

            //--position
            clone.position = otherScript.portal.TransformPoint(
                portal.InverseTransformPoint(original.position)
            );

            //--rotation
            clone.rotation =
                 (Quaternion.Inverse(portal.rotation)
                 * otherScript.portal.rotation)
                 * original.rotation
            ;

        }

        bool ThisObjectCanCross(Transform obj) {
            //player always can cross
            if (obj.CompareTag(setup.filters.playerTag)) return true;

            //negative filter
            if (setup.filters.tagsCannotCross.Contains(obj.tag)) return false;

            //main filter
            switch (setup.filters.otherObjectsCanCross) {
                case PortalSetup.Filters.OthersCanCross.Everything: 
                    return true;

                case PortalSetup.Filters.OthersCanCross.NothingOnlyPlayer:
                    return false;

                case PortalSetup.Filters.OthersCanCross.OnlySpecificTags:
                    if (setup.filters.tagsCanCross.Count > 0 && !setup.filters.tagsCanCross.Contains(obj.tag)) return false;
                    return true;
            }

            return true;
        }


    }
}