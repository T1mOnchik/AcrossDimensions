using System.Collections;
using UnityEngine;

public class Grappling : MonoBehaviour
{

    [Header("References")] 
    private PlayerMovement pm;
    [SerializeField]private Transform cameraTransform;
    [SerializeField]public Transform gunTip;
    [SerializeField]private LayerMask whatIsGrappleable;
    
    [Header("Grapple")]
    [SerializeField]private float maxGrappleDistance;
    [SerializeField]private float grappleDelayTime;
    public float overshootYAxis;
    private SpringJoint joint;
    
    public Vector3 grapplePoint;
    
    [Header("Cooldown")]
    [SerializeField]private float cooldown;
    private float cooldownTimer;
    
    [Header("Input")]
    [SerializeField] private KeyCode grappleKey = KeyCode.Mouse1;
    
    [Header("CameraEffects")] 
    public PlayerCam playerCamScript;
    [SerializeField]private float FOVWhileGrappling;
    private float FOVDefault;

    public bool grappling;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pm = GetComponent<PlayerMovement>();
        FOVDefault = playerCamScript.gameObject.GetComponent<Camera>().fieldOfView;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(grappleKey)) StartGrapple();

        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }
    }

    // private void LateUpdate()
    // {
    //     if (grappling)
    //     {
    //         // lineRenderer.SetPosition(0, gunTip.position);
    //         // DrawRope();
    //     }
    // }

    private void StartGrapple()
    {
        if (cooldownTimer > 0) return;
        
        grappling = true;
        pm.freeze = true;
        
        RaycastHit hit;
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, maxGrappleDistance, whatIsGrappleable))
        {
            grapplePoint = hit.point;
            StartCoroutine(InvokeGrappleFunction(true, grappleDelayTime));
            playerCamScript.EnableSpeedEffect(true);
        }
        else
        {
            grapplePoint = cameraTransform.position + cameraTransform.forward * maxGrappleDistance;
            StartCoroutine(InvokeGrappleFunction(false, grappleDelayTime));
        }

        // lineRenderer.enabled = true;
        // lineRenderer.positionCount = 2;
        // lineRenderer.SetPosition(1, grapplePoint);
    }

    // private void DrawRope()
    // {
    //     if (!joint) return;
    //     lineRenderer.enabled = true;
    //     lineRenderer.SetPosition(0, gunTip.position);
    //     lineRenderer.SetPosition(1, grapplePoint);
    // }

    private IEnumerator InvokeGrappleFunction(bool isGrappleable, float time)
    {
        yield return new WaitForSeconds(time);
        if (isGrappleable)
            ExecuteGrapple();
        else
            StopGrapple();
    }

    private void ExecuteGrapple()
    {
        pm.freeze = false;
        
        Vector3 lowestPoint = new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z);

        float grapplePointRelativeYPos = grapplePoint.y - lowestPoint.y;
        float highestPointOnArc = grapplePointRelativeYPos + overshootYAxis;

        if (grapplePointRelativeYPos < 0) highestPointOnArc = overshootYAxis;
        
        playerCamScript.DoFov(FOVWhileGrappling);
        
        pm.GrapplingJumpToPosition(grapplePoint, highestPointOnArc);
        StartCoroutine(InvokeGrappleFunction(false, 1f));
    }
    
    public void StopGrapple()
    {
        playerCamScript.EnableSpeedEffect(false);
        playerCamScript.DoFov(FOVDefault);
        pm.freeze = false;
        pm.activeGrapple = false;
        grappling = false;
        cooldownTimer = cooldown;
        // lineRenderer.positionCount = 0;
        // lineRenderer.enabled = false;
        // Destroy(joint);
    }

    public bool IsGrappling()
    {
        if (grappling != (joint != null))
        {
            Debug.Log("Grappling(" + grappling + ") != " + "joint(" + joint != null + ")");
        }
        return grappling;
    }

    public Vector3 GetGrapplePoint()
    {
        return grapplePoint;
    }
    
}
