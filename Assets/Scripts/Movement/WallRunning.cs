using System;
using UnityEngine;

public class WallRunning : MonoBehaviour
{
    
    [Header("WallRunning")]
    [SerializeField]private LayerMask wallMask;
    [SerializeField]private LayerMask groundMask;
    // [SerializeField] private float wallRunForce;
    [SerializeField] private float maxWallRunTime;
    private float wallRunTimer;
    [SerializeField] private float wallClimbingSpeed;
    
    [Header("Wall Jump")]
    [SerializeField] private float wallJumpUpForce;
    [SerializeField] private float wallJumpSideForce;
    
    [Header("Input")]
    private float horizontalInput;
    private float verticalInput;
    private KeyCode upwardRunKey = KeyCode.LeftAlt;
    private KeyCode downwardRunKey = KeyCode.LeftControl;
    private KeyCode jumpKey = KeyCode.Space;
    private bool upwardsRunning;
    private bool downwardsRunning;

    [Header("Detection")] 
    [SerializeField] private float wallCheckDistance;
    [SerializeField] private float minJumpHeight;
    private RaycastHit leftWallhit;
    private RaycastHit rightWallhit;
    private bool wallLeft;
    private bool wallRight;
    private bool wallAttached;
    
    [Header("Exiting Wall")]
    private bool exitingWall;
    [SerializeField]private float exitWallTime;
    private float exitWallTimer;
    [SerializeField]private float jumpingTime;
    private float jumpingTimer;

    [Header("References")] 
    public Transform orientation;
    [SerializeField]private PlayerCam playerCamScript;
    private PlayerMovement pm;
    private Rigidbody rb;

    [Header("Cum Effects")] 
    [SerializeField] private float fov;
    [SerializeField] private float tilt;


    private Vector3 lastWallNormal;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pm = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody>();
        
    }

    private void Update()
    {
        CheckForWall();
        StateMachine();
    }

    private void FixedUpdate()
    {
        StateMachine();
        if (pm.wallRunning)
        {
            WallRunningMovement();
        }
        // Debug.Log("Current Velocity: " + rb.linearVelocity);
    }

    private void CheckForWall()
    {
        wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallhit, wallCheckDistance, wallMask);
        wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallhit, wallCheckDistance, wallMask);
        
    }

    private void OnCollisionStay(Collision other)
    {
        pm.wallJumping = false;
        wallAttached = other.gameObject.layer == 7;
        if (wallAttached)
        {
            ContactPoint contact = other.contacts[0];
            lastWallNormal = contact.normal;   
        }
    }

    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.layer == 7)
        {
            wallAttached = false;
        }
    }

    private bool AboveGround()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, groundMask);
    }

    private void StateMachine()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        upwardsRunning = Input.GetKey(upwardRunKey);
        downwardsRunning = Input.GetKey(downwardRunKey);
        
        //state 1 - Wallrunning
        if (((wallLeft || wallRight) || wallAttached) && verticalInput > 0 && AboveGround() && !exitingWall)
        {
            if (!pm.wallRunning)
            {
                StartWallRun();
                pm.wallJumping = false;
            }

            if (Input.GetKeyDown(jumpKey)) WallJump();
        }
        
        //state 2 - Exiting wall
        else if (exitingWall)
        {
            if (pm.wallRunning)
                StopWallRun();

            if (exitWallTimer > 0)
            {
                exitWallTimer -= Time.deltaTime;
            }

            if (exitWallTimer <= 0)
            {
                exitingWall = false;
            }
        }

        else if (pm.wallJumping)
        {
            if (jumpingTimer > 0)
            {
                jumpingTimer -= Time.deltaTime;
            }
            else
            {
                jumpingTimer = 0;
            }

            if (jumpingTimer <= 0)
            {
                pm.wallJumping = false;
            }
        }
        else
        {
            if(pm.wallRunning)
                StopWallRun();
        }
    }

    private void StartWallRun()
    {
        playerCamScript.EnableSpeedEffect(true);
        pm.wallRunning = true;
        // playerCamScript.DoRotation(0.3f, 10f, (wallNormal > 0 ? 1 : -1), 2);
        playerCamScript.DoFov(fov);
        if (wallLeft) playerCamScript.DoTilt(-tilt);
        if (wallRight) playerCamScript.DoTilt(tilt);
    }

    private void WallRunningMovement()
    {
        rb.useGravity = false;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        
        Vector3 wallNormal = wallRight ? rightWallhit.normal : leftWallhit.normal;
        if (wallNormal == Vector3.zero)
        {
            wallNormal = lastWallNormal;
        }
        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);
        
        if ((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude)
        {
            wallForward = -wallForward;
        }

        Vector3 forceDirection = Vector3.Project(pm.moveDirection, wallForward);
        // Vector3 forceDirection = Vector3.Project(pm.moveDirection, wallForward);

        if (!exitingWall)
        {
            Vector3 ss = wallForward * pm.runningSpeed;
            rb.linearVelocity = ss;
        }
        

        if (upwardsRunning)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, wallClimbingSpeed, rb.linearVelocity.z);
        }

        if (downwardsRunning)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, -wallClimbingSpeed, rb.linearVelocity.z);
        }
    }

    private void StopWallRun()
    {
        pm.wallRunning = false;
        playerCamScript.EnableSpeedEffect(false);
        playerCamScript.DoFov(90f);
        playerCamScript.DoTilt(0f);
        
    }

    private void WallJump()
    {
        exitingWall = true;
        exitWallTimer = exitWallTime;
        pm.wallJumping = true;
        jumpingTimer = jumpingTime;
        
        Vector3 wallNormal = wallRight ? rightWallhit.normal : leftWallhit.normal;
        Vector3 forceToApply = transform.up * wallJumpUpForce + wallNormal * wallJumpSideForce;

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(forceToApply, ForceMode.Impulse);
    }
}