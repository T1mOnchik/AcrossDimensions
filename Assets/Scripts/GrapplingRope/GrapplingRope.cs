using System;
using UnityEngine;

public class GrapplingRope : MonoBehaviour
{
    [SerializeField] private PlayerMovement pm;
    [SerializeField] private Swinging swinging;
    [SerializeField] private Grappling grappling;
    private LineRenderer lineRenderer;
    [SerializeField] private int quality;
    private Vector3 currentGrapplePoint;
    private Spring spring;

    [SerializeField] private float damper;
    [SerializeField] private float strength;
    [SerializeField] private float velocity;
    [SerializeField] private float waveCount;
    [SerializeField] private float waveHeight;
    [SerializeField] private AnimationCurve affectCurve;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        spring = new Spring();
        spring.SetTarget(0);
    }

    private void LateUpdate()
    {
        DrawRope();
    }

    private void DrawRope()
    {
        // lineRenderer.positionCount = 2;
        // no grapple, no draw rope
        if (!pm.swinging && !grappling.grappling)
        {
            currentGrapplePoint = swinging.gunTip.position;
            spring.Reset();
            if (lineRenderer.positionCount > 0)
            {
                lineRenderer.positionCount = 0;
            }
            return;
        }

        if (lineRenderer.positionCount == 0)
        {
            spring.SetVelocity(velocity);
            lineRenderer.positionCount = quality + 1;
        }
        
        spring.SetDamper(damper);
        spring.SetStrength(strength);
        spring.Update(Time.deltaTime);

        Vector3 grapplePoint;
        Vector3 gunTipPosition;
        Vector3 up;
        
        if (grappling.grappling)
        {
            grapplePoint = grappling.grapplePoint;
            gunTipPosition = grappling.gunTip.position;
            up = Quaternion.LookRotation((grapplePoint - gunTipPosition).normalized) * Vector3.up;    
        }

        else
        {
            grapplePoint = swinging.grapplePoint;
            gunTipPosition = swinging.gunTip.position;
            up = Quaternion.LookRotation((grapplePoint - gunTipPosition).normalized) * Vector3.up;
        }
        

        currentGrapplePoint = Vector3.Lerp(currentGrapplePoint, grapplePoint, Time.deltaTime * 12f);

        for (int i = 0; i < quality + 1; i++)
        {
            float delta = i / (float)quality;
            Vector3 offset = up * (waveHeight * Mathf.Sin(delta * waveCount * Mathf.PI) * spring.Value * affectCurve.Evaluate(delta));
            lineRenderer.SetPosition(i, Vector3.Lerp(gunTipPosition, currentGrapplePoint, delta) + offset);
        }
        
        lineRenderer.enabled = true;
        // lineRenderer.SetPosition(0, gunTip.position);
        // lineRenderer.SetPosition(1, grapplePoint);
        
    }
}
