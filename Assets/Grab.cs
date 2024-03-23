using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grab : MonoBehaviour
{
    private Grab pm;
    public Transform cam;
    public Transform gunTip;
    public LayerMask whatIsGrabbable;
    public LineRenderer lr;

    public float maxGrabDistance;
    public float delayTime;

    private Vector3 grabPoint;

    public float cooldown;
    private float cooldownTimer;

    public KeyCode grabKey = KeyCode.G;

    private bool grabbing;

    // Start is called before the first frame update
    void Start()
    {
        pm = GetComponent<Grab>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(grabKey)) StartGrab();

        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }
    }

    void lateUpdate()
    {
        if(grabbing)
        {
            lr.SetPosition(0, gunTip.position);
        }
    }

    void StartGrab()
    {
        if (cooldownTimer > 0) return;

        grabbing = true;

        RaycastHit hit;
        if (Physics.Raycast(cam.position, cam.forward, out hit, maxGrabDistance, whatIsGrabbable))
        {
            grabPoint = hit.point;

            Invoke(nameof(ExecuteGrab), delayTime);
        }
        else
        {
            grabPoint = cam.position + cam.forward * maxGrabDistance;
        }

        lr.enabled = true;
        lr.SetPosition(1, grabPoint);
        
    }

    void ExecuteGrab()
    {
        grabbing = false;

        cooldown = cooldownTimer;

        lr.enabled = false;
    }
}
