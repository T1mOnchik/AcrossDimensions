using System;
using UnityEngine;

public class Swinging : MonoBehaviour
{
    
    [Header("References")] 
    private PlayerMovement pm;
    
    [SerializeField]private Transform cameraTransform;
    [SerializeField]public Transform gunTip;
    [SerializeField]private LayerMask whatIsGrappleable;
    [SerializeField] private PlayerCam playerCameraScript;
    
    private Rigidbody rb;
    [SerializeField]private Transform orientation;
    
    [Header("Input")]
    [SerializeField] private KeyCode swingKey = KeyCode.Mouse0;
    
    [Header("Grapple")]
    [SerializeField]private float maxGrappleDistance;
    private SpringJoint joint;
    public Vector3 grapplePoint;

    [Header("Force")] 
    [SerializeField] private float horizontalThrustForce;
    [SerializeField] private float forwardThrustForce;
    
    [Header("Cable")]
    [SerializeField] private float extendCableSpeed;

    [Header("Prediction")] 
    [SerializeField] private RaycastHit predictionHit;
    [SerializeField] private float predictionSphereCastRadius; 
    [SerializeField] private Transform predictionCastPoint;
    
    
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
        CheckForSwingPoints();
    }

    private void LateUpdate()
    {
        // DrawRope();
        if(joint) OdmGearMovement();
    }

    // ReSharper disable Unity.PerformanceAnalysis
    void StartSwinging()
    {
        // RaycastHit hit;
        // if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, maxGrappleDistance, whatIsGrappleable))
        if (predictionHit.point == Vector3.zero) return;

        if (GetComponent<Grappling>() != null)
            GetComponent<Grappling>().StopGrapple();
        pm.ResetRestrictionsFunctions();
        
        pm.swinging = true;   
        
            grapplePoint = predictionHit.point;
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
            
            playerCameraScript.EnableSpeedEffect(true);
    }

    void StopSwinging()
    {
        pm.swinging = false;
        Destroy(joint);
        playerCameraScript.EnableSpeedEffect(false);
    }

    private void OdmGearMovement()
    {
        // right
        if (Input.GetKey(KeyCode.D))
        {
            rb.AddForce(orientation.right * (horizontalThrustForce));
        }

        // left
        if (Input.GetKey(KeyCode.A))
        {
            rb.AddForce(-orientation.right * (horizontalThrustForce ));
        }

        // forward
        if (Input.GetKey(KeyCode.W))
        {
            rb.AddForce(orientation.forward * (forwardThrustForce ));
        }

        // shorten cable
        if (Input.GetKey(KeyCode.Space))
        {
            Vector3 directionToPoint = grapplePoint - transform.position;
            rb.AddForce(directionToPoint.normalized * forwardThrustForce);

            float distanceFromPoint = Vector3.Distance(transform.position, grapplePoint);
            joint.maxDistance = distanceFromPoint * 0.8f;
            joint.minDistance = distanceFromPoint * 0.25f;
        }

        // extend cable
        if (Input.GetKey(KeyCode.S))
        {
            float extendedDistanceFromPoint = Vector3.Distance(transform.position, grapplePoint) + extendCableSpeed;
            joint.maxDistance = extendedDistanceFromPoint * 0.8f;
            joint.minDistance = extendedDistanceFromPoint * 0.25f;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 direction = transform.TransformDirection(cameraTransform.forward);
        Gizmos.DrawRay(cameraTransform.position, direction * maxGrappleDistance);
    }

    // Predict Swing Point
    private void CheckForSwingPoints()
    {
        if(joint) return;
        RaycastHit sphereCastHit;
        Physics.SphereCast(cameraTransform.position, predictionSphereCastRadius, cameraTransform.forward, out sphereCastHit, maxGrappleDistance, whatIsGrappleable);

        RaycastHit raycastHit;
        Physics.Raycast(cameraTransform.position, cameraTransform.forward, out raycastHit, maxGrappleDistance, whatIsGrappleable);

        Vector3 realHitPoint;
        
        // Direct Hit
        if (raycastHit.point != Vector3.zero)
            realHitPoint = raycastHit.point;

        // Indirect (predicted) Hit
        else if(sphereCastHit.point != Vector3.zero)
            realHitPoint = sphereCastHit.point;
        
        // Miss
        else
            realHitPoint = Vector3.zero;

        if (realHitPoint != Vector3.zero)
        {
            predictionCastPoint.gameObject.SetActive(true);
            predictionCastPoint.position = realHitPoint;
        }
        else
        {
            predictionCastPoint.gameObject.SetActive(false);
        }
        
        predictionHit = raycastHit.point == Vector3.zero ? sphereCastHit : raycastHit;
    }
}
