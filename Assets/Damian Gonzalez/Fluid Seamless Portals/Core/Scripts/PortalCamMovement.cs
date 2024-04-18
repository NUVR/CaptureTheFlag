using System.Collections.Generic;
using System;
using UnityEngine;


/* 
 * this script controls what the plane on THIS portal displays,
 * moving the camera around the OTHER portal
 */

namespace DamianGonzalez.Portals {
    [DefaultExecutionOrder(112)] //110 Movement > 111 Teleport > 112 PortalCamMovement > 113 PortalSetup (rendering) > 114 PortalRenderer
    public class PortalCamMovement : MonoBehaviour {
        //these 10 variables are assigned automatically by the Setup script

        [HideInInspector] public Transform playerCamera;
        [HideInInspector] public Transform currentViewer;            //usually playerCamera, except on nesting portals
        [HideInInspector] public Camera currentViewerCameraComp;     
        [HideInInspector] public PortalSetup setup;
        [HideInInspector] public Transform mainObject;
        [HideInInspector] public PortalCamMovement otherScript;
        [HideInInspector] public Transform portal;
        [HideInInspector] public Camera _camera;
        [HideInInspector] public Transform _plane;
        [HideInInspector] public Renderer _renderer;
        [HideInInspector] public MeshFilter _filter;
        [HideInInspector] public Collider _collider;
        [HideInInspector] public bool inverted;
        [HideInInspector] public Transform shadowClone;
        [HideInInspector] public Camera playerCameraComp;
        [HideInInspector] public float currentClippingOffset = 0;

        public List<PortalCamMovement> nested = new List<PortalCamMovement>();


        public string cameraId;                     //useful if some debugging is needed. This is automatically assigned by portalSetup


        [Header ("Debug info")]
        [HideInInspector] public string renderPasses;   //as text, for a visual info
        [HideInInspector] public int int_renderPasses;  //as int, just for counting

        [System.Serializable]
        public class PosAndRot {
            public Vector3 position = Vector3.zero;
            public Quaternion rotation = Quaternion.identity;

            public void Zero() { 
                position = Vector3.zero;
                rotation = Quaternion.identity;
            }
        }
        

        private void Start() {

            if (playerCamera == null) playerCamera = Camera.main.transform;
            if (playerCameraComp == null) playerCameraComp = Camera.main;

            setup.refs.scriptPortalA.RestorePlaneOriginalPosition();
        }


        public PosAndRot CalculateTeleportedPositionAndRotation(Transform tr, Transform reference, Transform thisPortal, Transform otherPortal) {
            
            //rotation
            Quaternion _rotation =
                 (thisPortal.rotation
                 * Quaternion.Inverse(otherPortal.rotation))
                 * reference.rotation
            ;


            //position
            Vector3 distanceFromPlayerToPortal = (reference.position) - (otherPortal.position);
            Vector3 whereTheOtherCamShouldBe = thisPortal.position + (distanceFromPlayerToPortal);// + offset.position + tempPos;
            Vector3 _position = RotatePointAroundPivot(
                whereTheOtherCamShouldBe,
                thisPortal.position ,
                (thisPortal.rotation  * Quaternion.Inverse(otherPortal.rotation ) ).eulerAngles
            );

            return new PosAndRot() {
                position = _position,
                rotation = _rotation
            };
        }

        
        public void ApplyAdvancedOffset(bool ignoreDistance = false) {
            //if player is too near to the plane, don't apply the advanced offset
            if (!ignoreDistance && Vector3.Distance(
                    otherScript._collider.ClosestPoint(currentViewer.position),
                    currentViewer.position 
            ) < setup.advanced.dontAlignNearerThan) {
                _camera.projectionMatrix = playerCameraComp.projectionMatrix;
                return;
            }

            //not too near (or forced). continue.

            Vector3 point =
                _plane.position
                + portal.forward * setup.advanced.dotCalculationOffset * (inverted ? -1f : 1f)
                - transform.position;

            int dot = Math.Sign(Vector3.Dot(portal.forward, point));

            //rotate near clipping plane, so it matches portal rotation

            Plane p = new Plane(portal.forward * dot, _plane.position);
            Vector4 clipPlane = new Vector4(
                p.normal.x, 
                p.normal.y, 
                p.normal.z,
                p.distance + playerCameraComp.nearClipPlane + currentClippingOffset
            );

            Vector4 clipPlaneCameraSpace = Matrix4x4.Transpose(Matrix4x4.Inverse(_camera.worldToCameraMatrix)) * clipPlane;
            var newMatrix = playerCameraComp.CalculateObliqueMatrix(clipPlaneCameraSpace);

            if (!_camera.projectionMatrix.isIdentity)
                _camera.projectionMatrix = newMatrix;
           

        }


        public Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles) {
            Vector3 dir = point - pivot; // get point direction relative to pivot
            dir = Quaternion.Euler(angles) * dir; // rotate it
            point = dir + pivot; // calculate rotated point
            return point; // return it
        }


        public void Recalculate() {
            
            PosAndRot pr = CalculateTeleportedPositionAndRotation(transform, currentViewer, portal, otherScript.portal);
            transform.SetPositionAndRotation(pr.position, pr.rotation);
        }


        public void ManualRenderIfNecessary() {
            
            if (currentViewer == null) currentViewer = playerCamera;
            if (currentViewerCameraComp == null) currentViewerCameraComp = playerCameraComp;

            Recalculate();


            //only render cameras when player is seeing the planes that render them
            if (!ShouldRenderCamera(otherScript._renderer, currentViewerCameraComp, otherScript._plane)) {

                //has nested portals? then set default camera to those planes
                foreach (PortalCamMovement pcm in nested) {
                    pcm.otherScript.currentViewer = playerCamera;
                    pcm.otherScript.currentViewerCameraComp = playerCameraComp;

                }

                return;

            } else {
                //this camera will be rendered below
                //if any other portal is nested (visible from this portal),
                //trick those other cameras to render as if player was the in other side of this portal
                CheckNestedRendering();
                
            }

            

            //if asked, mimic the field of view of the main camera
            if (setup.player.alwaysMimicPlayersFOV) {
                _camera.fieldOfView = playerCameraComp.fieldOfView;
            }


            ManualRenderNotRecursive();

        }

        public void ManualRenderNotRecursive() {

            if (setup.advanced.alignNearClippingPlane) ApplyAdvancedOffset();

            
            try {
                _camera.Render();
            }
            catch (Exception) {
            }

            int_renderPasses++;

        }


        void ManualRenderRecursive() { }        // -> only in "full" version
        void CheckNestedRendering() {
            foreach (PortalCamMovement pcm in nested) {
                pcm.otherScript.currentViewer = _camera.transform;
                pcm.otherScript.currentViewerCameraComp = _camera;

                //pcm.ManualRenderNotRecursive(true); //and force it to render now, even if not visible

            }
        }       
        
        public bool ShouldRenderCamera(Renderer renderer, Camera camera, Transform plane) {
            if (setup.forceActivateCamsInNextFrame) return true;
            if (!setup.optimization.disableUnnecessaryCameras) return true;
            

            //1st filter: distance
            if (setup.optimization.disableDistantCameras) {
                if (Vector3.Distance(plane.position, currentViewer.position) > setup.optimization.fartherThan) return false;
            }

            //2nd filter: portal is visible?
            Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(camera);
            if (!GeometryUtility.TestPlanesAABB(frustumPlanes, renderer.bounds)) return false;


            float dotProduct = Vector3.Dot(
                inverted ? -otherScript.portal.forward : otherScript.portal.forward,
                currentViewer.position - otherScript.portal.position
            );

            float dotMarginForCams = .1f;
            return (dotProduct > 0 - dotMarginForCams); //instead of 0, let's give it a safe margin in case player is crossing sideways
                                                        //(my worst nightmare!)

        }


    }
}