using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabRope : MonoBehaviour
{
    private Spring spring;
    public LineRenderer lr;
    public Grab grab;
    private Vector3 currentGrabPos;

    public int quality;
    public float damper;
    public float strength;
    public float velocity;
    public float waveCount;
    public float waveHeight;
    public AnimationCurve affectCurve;

   
         // Start is called before the first frame update
    void Awake()
    {
        lr.enabled = false;
        //pm = GetComponent<Grab>();

        spring = new Spring();
        spring.SetTarget(0);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        DrawRope();
    }

    void DrawRope()
    {
        if (!grab.IsGrabbing())
        {
            currentGrabPos = grab.gunTip.position;
            spring.Reset();

            if (lr.positionCount > 0)
            {
                lr.positionCount = 0;
            }
                
            return;

        }

        if (lr.positionCount == 0)
        {
            spring.SetVelocity(velocity);
            lr.positionCount = quality + 1;
        }

        spring.SetDamper(damper);
        spring.SetStrength(strength);
        spring.Update(Time.deltaTime);

        var grabPoint = grab.GetGrabPoint();
        var gunTipPos = grab.gunTip.position;
        var up = Quaternion.LookRotation((grabPoint - gunTipPos).normalized) * Vector3.up;


        currentGrabPos = Vector3.Lerp(currentGrabPos, grabPoint, Time.deltaTime);

        for (var i = 0; i < quality + 1; i++)
        {
            var delta = i / (float) quality;
            var offset = up * waveHeight * Mathf.Sin(delta * waveCount * Mathf.PI)
                * spring.Value * affectCurve.Evaluate(delta);

            lr.SetPosition(i, Vector3.Lerp(gunTipPos, currentGrabPos, delta) + offset);
        }
    }
}
