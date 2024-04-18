//using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


namespace DamianGonzalez.Portals {
    [DefaultExecutionOrder(114)] //110 Movement > 111 Teleport > 112 PortalCamMovement > 113 PortalSetup (rendering) > 114 PortalRenderer
    public class PortalSetup : MonoBehaviour {

        #region declarations

        public string groupId = "";     //not necessary, but useful for debugging

        [Serializable]
        public class Player {
            [Header("Player setup (not necessary)")]
            [Tooltip("Optional. Default value is main camera")]
            public Transform playerCamera;  //this is optional. Default value is main camera.

            [Tooltip("Optional. Default value is main camera's parent")]
            public Transform playerMainObj;

            public enum ControllerType { Rigidbody, CharacterController, Auto }
            [Tooltip("Optional. How the player moves. 'Auto' will try to guess")]
            public ControllerType controllerType = ControllerType.Auto;

            [Tooltip("Optional. Ref. to player's character controller")]
            public CharacterController playerCc;

            [Tooltip("Optional. Ref. to player's rigid body")]
            public Rigidbody playerRb;

            [Tooltip("Check this if your player changes its 'field of view'")]
            public bool alwaysMimicPlayersFOV = false;
        }
        public Player player;


        [HideInInspector] public bool setupComplete = false;

        [System.Serializable]
        public class InternalReferences {
            [HideInInspector] public int screenHeight;
            [HideInInspector] public int screenWidth;

            [Header("Shader")]
            [Tooltip("Already assigned by default. It should be 'screenRender.shader'")]
            public Shader planeShader;


            [HideInInspector] public GameObject objCamA;
            [HideInInspector] public GameObject objCamB;

            [HideInInspector] public Camera cameraA;
            [HideInInspector] public Camera cameraB;

            [HideInInspector] public Transform planeA;
            [HideInInspector] public Transform planeB;

            [HideInInspector] public Transform portalA;
            [HideInInspector] public Transform portalB;

            [HideInInspector] public Teleport scriptPortalA;
            [HideInInspector] public Teleport scriptPortalB;

            [HideInInspector] public PortalCamMovement scriptCamA;
            [HideInInspector] public PortalCamMovement scriptCamB;

            [HideInInspector] public Renderer rendererA;
            [HideInInspector] public Renderer rendererB;

            [HideInInspector] public Transform functionalFolderA;
            [HideInInspector] public Transform functionalFolderB;
        }

        [Tooltip("Internal automatic references.")]
        public InternalReferences refs;

        private float timeStartResize = -1;

        //[HideInInspector] public bool doubleSided = false; -> only in "full" version




        [Serializable]
        public class Filters {
            public string playerTag = "Player";

            public enum OthersCanCross { Everything, OnlySpecificTags, NothingOnlyPlayer };
            [Header("Main filter")]
            [Tooltip("Obviously, player always can cross. But what about other objects?")]
            public OthersCanCross otherObjectsCanCross;

            [Header("Positive tags")]
            [Tooltip("In case you chose 'specific tags' above, name here which objects can cross.")]
            public List<string> tagsCanCross = new List<string>();

            [Header("Gandalf filter")]
            [Tooltip("Whatever you chose above, objects with these tags shall not pass.")]
            public List<string> tagsCannotCross = new List<string>();
        }
        [Tooltip("Obviously, player always can cross this set of portals. But what about other objects?")]
        public Filters filters;

        [Serializable]
        public class Clones {
            [Header("Use of clones")]
            [Tooltip("If you use clones, objects will show in both portals while crossing. See documentation for details.")]
            public bool useClones = true;

            [Tooltip("If your player has a visible body (or parts of it) then you should check this option. See documentation for details.")]
            public bool clonePlayerToo = true;
        }
        [Tooltip("If you use clones, objects will show in both portals while crossing. See documentation for details")]
        public Clones clones;




        [Serializable]
        public class AfterTeleportOptions {
            [Header("When player teleports")]
            [Tooltip("Remain checked if your player has a character controller component")]
            public bool tryResetCharacterController = true;
            [Tooltip("Some advanced characters may need this option to be repositioned")]
            public bool tryResetCameraObject = true;
            [Tooltip("Some advanced characters may need this option to be repositioned")]
            public bool tryResetCameraScripts = true;
            [Tooltip("Check this if the camera is outside of the player")]
            public bool mantainPlayerOffset = false;

            [Header("Useful for debugging")]
            [Tooltip("If checked, the game will pause when the player teleports")]
            public bool pauseWhenPlayerTeleports = false;
            [Tooltip("If checked, the game will pause when other objects (not the player) teleport")]
            public bool pauseWhenOtherTeleports = false;

        }
        public AfterTeleportOptions afterTeleport;

        [Tooltip("When checked, you'll get a lot of info in the console.")]
        public bool verboseDebug = false;




        [Serializable]
        public class Advanced {

            [Header("Elastic plane")]
            [Tooltip("This value has to be equal or slightly higher than your player camera's near clipping plane")]
            [Range(.02f, 1f)] public float elasticPlaneOffset = .2f;
            [HideInInspector] public bool useElasticPlane = true;

            [Tooltip("At what distance from the plane the elastic plane should begin moving (see documentation)")]
            [Range(-.5f, .5f)] public float elasticPlaneMinTreshold = -.2f;
            [Tooltip("At what distance from the plane player teleports (see documentation)")]
            [Range(-.5f, .5f)] public float elasticPlaneTeleportTreshold = .2f;

            [Header("Behave differently with fast player? (Not recommended)")]
            [Tooltip("Unnecessary. Should an offset based on velocity be applied? (see documentation)")]
            public bool dynamicOffsetBasedOnVelocity = false;
            [Tooltip("Unnecessary. Value of 1 covers the same distance the player is moving (see documentation)")]
            [Range(0f, 3f)] public float elasticPlaneVelocityFactor = 2f;

            [Header("Advanced visual settings")]
            [Tooltip("Always true if you don't want weird effects when portals are skewd or in narrow places")]
            public bool alignNearClippingPlane = true;
            [Tooltip("Add a little if you see a blank gap between portals")]
            [Range(-5f, 5f)] public float clippingOffset = .5f;
            [Tooltip("Always set a little number here, so you can travel without flickering")]
            [Range(.01f, 1f)] public float dontAlignNearerThan = .1f;
            [Tooltip("Unnecesary")]
            [Range(-2f, 2f)] public float dotCalculationOffset = 0;
            public int depthTexture = 16;

            [Tooltip("Enable for more compatibility with URP (specially with recursive rendering)")]
            public bool avoidZeroViewport = true;

        }

        [Tooltip("WARNING: Changing this values can produce unwanted results")]
        public Advanced advanced;



        /*
        [Serializable] public class NestedPortals { }
        public NestedPortals nestedPortals;   -> only in "full" version
        */



        [Serializable]
        public class Optimization {

            [Header("Performance optimization")]
            [Tooltip("Check to only render cameras which projects on visible portals")]
            public bool disableUnnecessaryCameras = true;
            [Tooltip("Unnecesary when the previous option is checked")]
            public bool disableDistantCameras = false;
            [Tooltip("At what distance from the player portals should not render")]
            public float fartherThan = 100f;
            [Tooltip("Recommended. If checked, all portls will render the first frame, so 'something' is shown on them")]
            public bool renderCamerasOnFirstFrame = true; //so the portals with disabled cameras can show something

        }
        [Tooltip("Having many portals can be expensive. These options help reducing camera rendering as much as possible")]
        public Optimization optimization;


        /*
        [Serializable] public class Recursive {}
        public Recursive recursiveRendering;        -> only in "full" version


        [Serializable] public class OnOffSwitch {}
        public OnOffSwitch onOffSwitch;             -> only in "full" version
        */




        private bool isFirstFrame = true;
        [HideInInspector] public bool forceActivateCamsInNextFrame = false;
        [HideInInspector] public bool teleportInProgress = false;
        [HideInInspector] public float lastTeleportTime = 0;

        /*
        public enum StartWorkflow { Runtime, Deployed }
        public StartWorkflow startWorkflow = StartWorkflow.Runtime;     -> only in "full" version
        */


        #endregion


        void Awake() {
            PortalInitialization();
        }


        void PortalInitialization() {
            DeployAndSetupMaterials();

            //done
            setupComplete = true;
            PortalEvents.setupComplete?.Invoke(groupId, this);
        }

        public void DeployAndSetupMaterials() {

            //if not provided, use default values
            if (player.playerCamera == null) player.playerCamera = Camera.main.transform;
            if (player.playerMainObj == null) player.playerMainObj = GameObject.FindGameObjectWithTag("Player").transform;
            if (groupId == "") groupId = transform.name;


            if (player.controllerType == Player.ControllerType.CharacterController && player.playerCc == null) {
                player.playerCc = player.playerMainObj.GetComponent<CharacterController>();
            }
            if (player.controllerType == Player.ControllerType.Rigidbody && player.playerRb == null) {
                player.playerRb = player.playerMainObj.GetComponent<Rigidbody>();
            }
            if (player.controllerType == Player.ControllerType.Auto) {
                player.playerCc = player.playerMainObj.GetComponent<CharacterController>();
                player.playerRb = player.playerMainObj.GetComponent<Rigidbody>();
                if (player.playerCc != null) player.controllerType = Player.ControllerType.CharacterController;
                if (player.playerRb != null) player.controllerType = Player.ControllerType.Rigidbody;
            }


            //check that players camera's clipping plane is compatible with portal's
            Camera cameraComp = player.playerCamera.GetComponent<Camera>();
            if (cameraComp.nearClipPlane >= advanced.elasticPlaneOffset) {
                float newValue = Mathf.Max(advanced.elasticPlaneOffset / 2f, .01f);
                Debug.LogWarning(
                    $"Portal 'elastic plane offset' ({advanced.elasticPlaneOffset}) should be greater " +
                    $"than player camera's near clipping plane ({cameraComp.nearClipPlane}). " +
                    $"Camera near plane will be now adjusted to {newValue} to avoid flickering.");

                cameraComp.nearClipPlane = newValue;

            }

            //reference to each portal of this set
            refs.portalA = transform.GetChild(0);
            refs.portalB = transform.GetChild(1);

            refs.functionalFolderA = refs.portalA.GetChild(0);
            refs.functionalFolderB = refs.portalB.GetChild(0);

            Transform triggerA = refs.functionalFolderA.Find("trigger");
            Transform triggerB = refs.functionalFolderB.Find("trigger");

            refs.planeA = refs.functionalFolderA.Find("plane");
            refs.planeB = refs.functionalFolderB.Find("plane");

            refs.rendererA = refs.planeA.GetComponent<Renderer>();
            refs.rendererB = refs.planeB.GetComponent<Renderer>();


            //generate the empty objects for the cameras
            refs.objCamA = new GameObject("Camera (around A on plane B)");
            refs.objCamB = new GameObject("Camera (around B on plane A)");

            //and put them inside the containers. 
            refs.objCamA.transform.SetParent(refs.functionalFolderA, true);
            refs.objCamB.transform.SetParent(refs.functionalFolderB, true);

            //add camera components to the cameras
            refs.cameraA = refs.objCamA.AddComponent<Camera>();
            refs.cameraB = refs.objCamB.AddComponent<Camera>();

            //and its scripts
            refs.scriptCamA = refs.cameraA.gameObject.AddComponent<PortalCamMovement>();
            refs.scriptCamB = refs.cameraB.gameObject.AddComponent<PortalCamMovement>();

            //give this new cameras same setup than main camera
            refs.cameraA.CopyFrom(cameraComp);
            refs.cameraB.CopyFrom(cameraComp);

            //but if player camera has a non-standard viewport rect, correct the portal cameras
            float _zero = 0;
            if (advanced.avoidZeroViewport) _zero = 0.0001f; //URP issues
            refs.cameraA.rect = new Rect(0, _zero, 1, 1);
            refs.cameraB.rect = new Rect(0, _zero, 1, 1);



            //Setup both camera's scripts
            refs.scriptCamA.playerCamera = player.playerCamera;
            refs.scriptCamA.playerCameraComp = cameraComp;
            refs.scriptCamA.currentViewer = player.playerCamera;
            refs.scriptCamA.currentViewerCameraComp = cameraComp;
            refs.scriptCamA._camera = refs.cameraA;
            refs.scriptCamA._plane = refs.planeA;
            refs.scriptCamA.portal = refs.portalA;
            refs.scriptCamA._renderer = refs.rendererA;
            refs.scriptCamA._filter = refs.planeA.GetComponent<MeshFilter>();
            refs.scriptCamA._collider = triggerA.GetComponent<Collider>();
            refs.scriptCamA.otherScript = refs.scriptCamB;
            refs.scriptCamA.setup = this;
            refs.scriptCamA.mainObject = transform;
            refs.scriptCamA.cameraId = groupId + ".a";
            refs.scriptCamA.inverted = false;

            refs.scriptCamB.playerCamera = player.playerCamera;
            refs.scriptCamB.playerCameraComp = cameraComp;
            refs.scriptCamB.currentViewer = player.playerCamera;
            refs.scriptCamB.currentViewerCameraComp = cameraComp;
            refs.scriptCamB._camera = refs.cameraB;
            refs.scriptCamB._plane = refs.planeB;
            refs.scriptCamB.portal = refs.portalB;
            refs.scriptCamB._renderer = refs.rendererB;
            refs.scriptCamB._filter = refs.planeB.GetComponent<MeshFilter>();
            refs.scriptCamB._collider = triggerB.GetComponent<Collider>();
            refs.scriptCamB.otherScript = refs.scriptCamA;
            refs.scriptCamB.setup = this;
            refs.scriptCamB.mainObject = transform;
            refs.scriptCamB.cameraId = groupId + ".b";
            refs.scriptCamB.inverted = true;

            //and setup both portal's script
            refs.scriptPortalA = triggerA.GetComponent<Teleport>();
            refs.scriptPortalB = triggerB.GetComponent<Teleport>();

            refs.scriptPortalA.setup = this;
            refs.scriptPortalA.cameraScript = refs.scriptCamA;
            refs.scriptPortalA.otherScript = refs.scriptPortalB;
            refs.scriptPortalA.mainObject = transform;
            refs.scriptPortalA.portal = refs.portalA;
            refs.scriptPortalA.plane = refs.planeA;
            refs.scriptPortalA._collider = refs.scriptPortalA.GetComponent<BoxCollider>();
            refs.scriptPortalA.planeIsInverted = false;


            refs.scriptPortalB.setup = this;
            refs.scriptPortalB.cameraScript = refs.scriptCamB;
            refs.scriptPortalB.otherScript = refs.scriptPortalA;
            refs.scriptPortalB.mainObject = transform;
            refs.scriptPortalB.portal = refs.portalB;
            refs.scriptPortalB.plane = refs.planeB;
            refs.scriptPortalB._collider = refs.scriptPortalB.GetComponent<BoxCollider>();
            refs.scriptPortalB.planeIsInverted = true;


            //Create materials with the shader
            //and asign those materials to the planes (here is where they cross)
            SetupMaterials();


            //camera objects enabled, but cameras components disabled, we'll use manual rendering, even if is not recursive
            refs.objCamA.SetActive(true);
            refs.objCamB.SetActive(true);
            refs.cameraA.enabled = false;
            refs.cameraB.enabled = false;



#if UNITY_EDITOR
            if (Application.isEditor) {
                //finally, mark changes as dirty, otherwise they won't be saved
                UnityEditor.EditorUtility.SetDirty(gameObject);
                UnityEditor.EditorUtility.SetDirty(this);
                UnityEditor.EditorUtility.SetDirty(refs.scriptCamA);
                UnityEditor.EditorUtility.SetDirty(refs.scriptCamB);
                UnityEditor.EditorUtility.SetDirty(refs.scriptPortalA);
                UnityEditor.EditorUtility.SetDirty(refs.scriptPortalB);
            }
#endif

        }

        //public void Undeploy() { } //-> only in "full" version

        void Start() {

            //nested portals    -> only in "full" version

        }



        private void Update() {
            //1 second after resize is done, it updates the render textures
            if (timeStartResize == -1 && (refs.screenHeight != Screen.height || refs.screenWidth != Screen.width)) timeStartResize = Time.time;

            if (timeStartResize > 0 && Time.time > timeStartResize + 1f) ResizeScreen();

            //manually render cameras (only when necessary)
            refs.scriptCamA.int_renderPasses = 0;
            refs.scriptCamB.int_renderPasses = 0;

            refs.scriptCamA.ManualRenderIfNecessary();
            refs.scriptCamB.ManualRenderIfNecessary();


            if (forceActivateCamsInNextFrame) forceActivateCamsInNextFrame = false;
            if (optimization.renderCamerasOnFirstFrame && isFirstFrame) forceActivateCamsInNextFrame = true; //renders 2nd frame, actually, when every camera are set and placed
            isFirstFrame = false;


        }


        void SetupMaterials() {

            int _width = Mathf.Max(Screen.width, 100);
            int _height = Mathf.Max(Screen.height, 100);


            timeStartResize = -1;

            //Create materials with the shader
            Material matA = new Material(refs.planeShader);
            refs.cameraA.targetTexture?.Release();
            refs.cameraA.targetTexture = new RenderTexture(_width, _height, advanced.depthTexture);
            matA.mainTexture = refs.cameraA.targetTexture;
            matA.SetTexture("_MainTex", refs.cameraA.targetTexture);



            Material matB = new Material(refs.planeShader);
            refs.cameraB.targetTexture?.Release();
            refs.cameraB.targetTexture = new RenderTexture(_width, _height, advanced.depthTexture);
            matB.mainTexture = refs.cameraB.targetTexture;

            //and asign those materials to the planes (here is where they cross)
            refs.rendererA.material = matB;
            refs.rendererB.material = matA;

            refs.screenHeight = _height;
            refs.screenWidth = _width;

        }

        void ResizeScreen() {
            SetupMaterials();
            PortalEvents.gameResized?.Invoke(
                groupId,
                transform,
                new Vector2(refs.screenHeight, refs.screenWidth),
                new Vector2(Screen.height, Screen.width)
            );
        }

        //Transform CreateShadowClone(GameObject obj) {     -> only in "full" version


        void OnValidate() {
            //when values (like clippingOffset) changes in the editor, it should apply the changes immediatly
            if (refs.scriptCamA == null) return;

            refs.scriptCamA.currentClippingOffset = advanced.clippingOffset;
            refs.scriptCamB.currentClippingOffset = advanced.clippingOffset;

            refs.scriptCamA.ApplyAdvancedOffset();
            refs.scriptCamB.ApplyAdvancedOffset();
        }


        //public void DisableBothPortals() { } -> only in "full" version

        //public void EnableBothPortals() { } -> only in "full" version

    }
}