using UnityEngine;

namespace DamianGonzalez {
    public class CameraFollowPlayer : MonoBehaviour {
        [SerializeField] Transform playerToFollow;
        [SerializeField] [Range(.1f,1f)] float tightness = .9f;
        Vector3 initialOffset;
        void Start() {
            if (playerToFollow == null) {
                playerToFollow = GameObject.FindGameObjectWithTag("Player").transform;
            }

            initialOffset = playerToFollow.InverseTransformPoint(transform.position);

        }

        void FixedUpdate() {
            Vector3 destination =  playerToFollow.TransformPoint(initialOffset);
            transform.position = Vector3.Lerp(transform.position, destination, tightness);
        }
    }
}