using System;
using UnityEngine;

public class Swinging : MonoBehaviour
{
    
    [Header("References")] 
    private PlayerMovement pm;
    [SerializeField]private Transform cameraTransform;
    [SerializeField]private Transform gunTip;
    [SerializeField]private LayerMask whatIsGrappleable;
    [SerializeField] private LineRenderer lineRenderer;
    private Rigidbody rb;
    
    [Header("Input")]
    [SerializeField] private KeyCode swingKey = KeyCode.Mouse0;
    
    [Header("Grapple")]
    [SerializeField]private float maxGrappleDistance;
    private SpringJoint joint;
    private Vector3 grapplePoint;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pm = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(swingKey)) StartSwinging();
        else if (Input.GetKeyUp(swingKey)) StopSwinging();
        
    }

    private void LateUpdate()
    {
        DrawRope();
    }

    void StartSwinging()
    {
        pm.swinging = true;
        RaycastHit hit;
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, maxGrappleDistance, whatIsGrappleable))
        {
            grapplePoint = hit.point;
            joint = pm.gameObject.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = grapplePoint;
            
            float distanceFromPoint = Vector3.Distance(cameraTransform.position, grapplePoint);

            // Distance grapple try to keep from grapple point
            joint.maxDistance = distanceFromPoint * 0.8f;
            joint.minDistance = distanceFromPoint * 0.25f;

            joint.spring = 4.5f;
            joint.damper = 7f;
            joint.massScale = 4.5f;
            
            
        }
    }

    void StopSwinging()
    {
        lineRenderer.positionCount = 0;
        pm.swinging = false;
        // lineRenderer.enabled = false;
        Destroy(joint);
    }
    
    private void DrawRope()
    {
        lineRenderer.positionCount = 2;
        if (!joint) return;
        lineRenderer.enabled = true;
        Debug.Log(lineRenderer.positionCount);
        lineRenderer.SetPosition(0, gunTip.position);
        lineRenderer.SetPosition(1, grapplePoint);
    }

    // private void OdmGearMovement()
    // {
    //     // right
    //     if (Input.GetKey(KeyCode.D))
    //     {
    //         rb.AddForce(orien);
    //     }
    // }
}
