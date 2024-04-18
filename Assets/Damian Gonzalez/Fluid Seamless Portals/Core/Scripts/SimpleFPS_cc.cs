using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace DamianGonzalez {

    [DefaultExecutionOrder(110)] //110 Movement > 111 Teleport > 112 PortalCamMovement > 113 PortalSetup (rendering) > 114 PortalRenderer
    public class SimpleFPS_cc : MonoBehaviour {
        public static Transform thePlayer; //easy access
        public static Camera cameraComponent;

        CharacterController cc;
        Transform cameraTr;
        float rotX = 0;
        [SerializeField] float walkSpeed = 10f;
        [SerializeField] float runSpeed = 20f;
        [SerializeField] float slowSpeed = 2f;
        [SerializeField] float mouseSensitivity = 1f;

        Transform groundReference;

        [SerializeField] LayerMask floorLayerMask;
        [SerializeField] float checkDistance = .3f;
        bool grounded;

        [SerializeField] float gravity = 9.81f;
        float verticalVelocity = 0;

        [SerializeField] float jumpForce = 4f;
        bool locked = false;
        [SerializeField] bool pressEscToLock = true;

        //public bool isMultiGravity = false;       -> only in "full" version
        //CustomGravity gravityScript;              -> only in "full" version
        //public GameObject interactionMsgInCanvas; -> only in "full" version

        [SerializeField] bool correctInclination = true;
        [Range(0.01f, .5f)] [SerializeField] float inclinationCorrectionSpeed = .1f;
        [HideInInspector] public bool cameraIsChildOfPlayer = true;

        private void Awake() {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

        }

        void Start() {
            thePlayer = transform;
            cc = GetComponent<CharacterController>();
            cameraComponent = Camera.main;
            cameraTr = cameraComponent.transform;

            groundReference = transform.GetChild(1);
            rotX = cameraTr.eulerAngles.x;
        }


        void Update() {

            FallOrJump();

            Move();

            RotateAndLook();

            LockControls();

            //InteractionWithSwitches();    -> only in "full" version

        }

        private void FixedUpdate() {
            CorrectInclination();
        }

        private void LockControls() {
            //ESC to lock/unlock
            if (pressEscToLock && Input.GetKeyDown(KeyCode.Escape)) {
                locked = !locked;
            }
        }

        private void RotateAndLook() {
            if (!locked) {
                //rotate player left/right
                transform.Rotate(0, Input.GetAxis("Mouse X") * mouseSensitivity, 0);

                //look up and down
                rotX += Input.GetAxis("Mouse Y") * mouseSensitivity * -1;
                rotX = Mathf.Clamp(rotX, -90f, 90f); //clamp look 

                
                cameraTr.rotation = transform.rotation * Quaternion.Euler(rotX, 0, 0);
                
            }
        }

        void CorrectInclination() {
            if (correctInclination) {

                //try to make player stand straight when tilted
                transform.rotation = Quaternion.Lerp(
                    transform.rotation,
                    Quaternion.Euler(0, transform.eulerAngles.y, 0),
                    inclinationCorrectionSpeed
                );

            }
        }

        private void Move() {
            float speed = walkSpeed;
            if (Input.GetKey(KeyCode.LeftShift)) speed = runSpeed;
            if (Input.GetKey(KeyCode.LeftControl)) speed = slowSpeed;


            if (!locked) {
                cc.Move(
                    transform.forward * speed * Input.GetAxis("Vertical") * Time.deltaTime    //move forward
                    +
                    transform.right * speed * Input.GetAxis("Horizontal") * Time.deltaTime  //slide to sides
                    +
                    Down() * -verticalVelocity * Time.deltaTime //jump and fall
                );
            }
        }


        private void FallOrJump() {
            grounded = cc.isGrounded || Physics.Raycast(groundReference.position, -Vector3.up, checkDistance, floorLayerMask);

            //if not grounded, make the player fall
            if (!grounded) {
                verticalVelocity -= gravity * Time.deltaTime;
            } else {
                verticalVelocity = -.3f;
                //since he's grounded, he can jump
                if (Input.GetButtonDown("Jump")) verticalVelocity = jumpForce;
            }
        }

        Vector3 Down() => -Vector3.up;

    }
}