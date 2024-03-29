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
    public GameObject hands;

    public float maxGrabDistance;
    public float delayTime;

    private Vector3 grabPoint;

    public float cooldown;
    private float cooldownTimer;

    public KeyCode grabKey;

    private bool grabbing;
    //private SpringJoint joint;

    // Start is called before the first frame update
    void Start()
    {
        //lr.enabled = false;
        pm = GetComponent<Grab>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(grabKey))
        {
            Debug.Log("input hit");
            StartGrab();
        }

        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }
    }

    /*
    void lateUpdate()
    {
        if(grabbing)
        {
            lr.SetPosition(0, gunTip.position);
        }
    }
    */

    void StartGrab()
    {
        if (cooldownTimer > 0) return;

        grabbing = true;

        RaycastHit hit;
        if (Physics.Raycast(cam.position, cam.forward, out hit, maxGrabDistance, whatIsGrabbable))
        {
            grabPoint = hit.point;
            Debug.Log("hit grabbable");

            Invoke(nameof(ExecuteGrab), delayTime);
        }
        else
        {
            grabPoint = cam.position + cam.forward * maxGrabDistance;
            Invoke(nameof(ExecuteGrab), delayTime);
            Debug.Log("grab max distance");
        }

        //lr.enabled = true;
        //lr.SetPosition(1, grabPoint);
        
    }

    void ExecuteGrab()
    {
        grabbing = false;

        cooldown = cooldownTimer;

        //lr.enabled = false;
    }

    public bool IsGrabbing()
    {
        return grabbing;
    }

    public Vector3 GetGrabPoint()
    {
        return grabPoint;
    }


}
