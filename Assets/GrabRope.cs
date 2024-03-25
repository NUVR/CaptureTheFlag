using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabRope : MonoBehaviour
{
    private Spring spring;
    public LineRenderer lr;
    public GameObject hands;
    public Grab grab;
    private Vector3 currentGrabPos;

    public int quality;
    public float damper;
    public float strength;
    public float velocity;
    public float waveCount;
    public float waveHeight;
    public AnimationCurve affectCurve;

   

    void Awake()
    {
        spring = new Spring();
        spring.SetTarget(0);
    }

    //update method
    void LateUpdate()
    {
        DrawRope();
    }

    //draw the rope 
    void DrawRope()
    {
        //is not grabbing bring it back
        if (!grab.IsGrabbing())
        {
            //TODO HERE: bring back the hands and line!
            //setting equal just  "deletes it"
            currentGrabPos = grab.gunTip.position;
            spring.Reset();
            //for loop reverse math? bring it back
            

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
            hands.transform.position = lr.GetPosition(i);
        }
    }
}
