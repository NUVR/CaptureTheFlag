using UnityEngine;

//simple first person controller using rigidbody, by Damián González, specially for portals asset.

namespace DamianGonzalez.Portals {
    [DefaultExecutionOrder(110)] //110 Movement > 111 Teleport > 112 PortalCamMovement > 113 PortalSetup (rendering) > 114 PortalRenderer
    public class SimpleFPS_rb : MonoBehaviour {
        public static Transform thePlayer; //easy access
        Rigidbody rb;
        Transform cam;
        public float walkSpeed = 5f;
        public float runSpeed  = 15f;
        public Vector2 mouseSensitivity = new Vector2(1f, 1f);
        float rotX;
        public float maxVelY = 10f;
        public float jumpImpulse = 10f;
        public bool standStraight = true;

        void Start() {
            thePlayer = transform;
            rb = GetComponent<Rigidbody>();
            cam = Camera.main.transform;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            rotX = cam.eulerAngles.x;
        }
        public float maxVelocity = 10f;
        void Update() {
            float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
            Vector3 forwardNotTilted = new Vector3(transform.forward.x, 0, transform.forward.z);

            rb.velocity = (
                forwardNotTilted * speed * Input.GetAxis("Vertical")    //move forward
                +
                transform.right * speed * Input.GetAxis("Horizontal")   //slide to sides
                +
                new Vector3(0, Mathf.Clamp(rb.velocity.y, -999f, maxVelY), 0) //allow jumping & falling
            );

            //rb.AddForce(transform.forward * speed * Input.GetAxis("Vertical"), ForceMode.Impulse);    //move forward
            //rb.AddForce(transform.right * speed * Input.GetAxis("Horizontal"), ForceMode.Impulse);    //slide to sides


            //limit velocity
            //if (rb.velocity.magnitude > maxVelocity) rb.velocity = rb.velocity.normalized * maxVelocity;

            //look up and down
            rotX += Input.GetAxis("Mouse Y") * mouseSensitivity.y * -1;
            rotX = Mathf.Clamp(rotX, -60f, 60f); //clamp look 
            cam.localRotation = Quaternion.Euler(rotX, 0, 0);


            //rotate player left/right
            transform.Rotate(transform.up, Input.GetAxis("Mouse X") * mouseSensitivity.x);

            if (standStraight) {
                //and try to make player stand straight if tilted
                rb.MoveRotation(Quaternion.Lerp(
                    transform.rotation,
                    Quaternion.Euler(0, transform.eulerAngles.y, 0),
                    .1f
                ));
            }


            if (Input.GetButtonDown("Jump") && Physics.CheckSphere(transform.position - new Vector3(0, 1.5f, 0), .5f))
                rb.AddForce(0, jumpImpulse, 0, ForceMode.Impulse);


        }

    }
}