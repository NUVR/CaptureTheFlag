using System.Collections.Generic;
using UnityEngine;

//this script is not part of the portals system, it's only for the demo scene,
//only to ilustrate how other object can (or can't) cross portals

namespace DamianGonzalez {

    public class ShootBalls : MonoBehaviour {
        public GameObject prefabBall;
        public float shortThrowForce = 100f;
        public float longThrowForce = 250f;
        public int maxAmount = 10;
        Transform recycleBin;
        Queue<GameObject> ballsPool = new Queue<GameObject>();

        public enum WhichButton { LeftClick, RightClick };
        public WhichButton button;

        private void Start() {
            recycleBin = (GameObject.Find("recycle bin") ?? new GameObject("recycle bin")).transform;
            //InvokeRepeating(nameof(ThrowProjectile), 1f, 1f);
        }

        void Update() {
            if (button == WhichButton.LeftClick && Input.GetMouseButtonDown(0)) ThrowProjectile();
            if (button == WhichButton.RightClick && Input.GetMouseButtonDown(1)) ThrowProjectile();


            //restart current scene
            if (Input.GetKeyDown(KeyCode.R)) {
                UnityEngine.SceneManagement.SceneManager.LoadScene(
                    UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
                );
            }

        }

        void ThrowProjectile() {
            //new projectile slightly in front of player
            GameObject projectile = NewBall();

            //reposition
            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            rb.velocity = Vector3.zero;
            projectile.SetActive(false);
            projectile.transform.position = transform.position + (transform.forward * 1f);
            projectile.SetActive(true);

            //add force
            float force = shortThrowForce;
            if (Input.GetKey(KeyCode.LeftShift)) force *= 2.5f;
            if (Input.GetKey(KeyCode.LeftControl)) force *= .2f;

            rb.velocity = Vector3.zero;
            rb.AddForce(transform.forward * force, ForceMode.Impulse);

        }

        GameObject NewBall() {
            GameObject ball;
            if (ballsPool.Count < maxAmount) {
                //instantiate
                ball = Instantiate(
                    prefabBall,
                    recycleBin
                );

                //random color
                ball.GetComponent<MeshRenderer>().material.color = Random.ColorHSV();

                //and a name
                ball.name = "ball";
            } else {
                ball = ballsPool.Dequeue();
            }
            ballsPool.Enqueue(ball);
            return ball;
        }
    }
}