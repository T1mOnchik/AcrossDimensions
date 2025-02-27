using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    [Header("Movement")] 
    float moveSpeed;
    
    [SerializeField] public float runningSpeed;
    [SerializeField] private float dashSpeed;
    [SerializeField] private float dashSpeedChangeFactor;
    [SerializeField] private float groundDrag;

    [SerializeField] private float jumpForce;
    [SerializeField] private float jumpCooldown;
    [SerializeField] private float airMultiplier;
    public float maxYSpeed;
    private bool readyToJump = true;
    
    private const KeyCode JUMP_KEY = KeyCode.Space;
    
    
    
    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask groundLayer;
    bool grounded;
    
    
    
    [Header("Slope Handling")]
    [SerializeField]private float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;
    
    
    [SerializeField] private Transform orientation;
    private float horizontalInput;
    private float verticalInput;
    public Vector3 moveDirection;
    private const float IMPULSE_AMPLIFIER = 10f; // increases the impulse to make movement more reactive

    // Speed Momentum
    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;
    private MovementState lastState;
    private bool keepMomentum;
    private float speedChangeFactor;
    
    private Rigidbody rb;
    
    // Operational
    private Coroutine speedSmoothLerpCoroutine;
    
    public enum MovementState
    {
        freeze,
        wallRunning,
        wallJumping,
        running,
        dashing,
        air,
        grappling
    }
    public MovementState movementState;
    
    public bool dashing;
    private bool canDash;
    public bool wallRunning;
    public bool wallJumping;
    public bool freeze;
    public bool activeGrapple;
    
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        
        // Cursor.lockstate = CursorLockMode.Locked;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        if (freeze)
        {
            rb.linearVelocity = Vector3.zero;
        }
        
        // ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, groundLayer); // distance to the ground = (player height / 2) + inaccuracy 
        myInput();
        // SpeedControl();
        MovePlayer();
        
        // ground drag handler
        if (grounded && !activeGrapple)
            rb.linearDamping = groundDrag;
        else
            rb.linearDamping = 0;
    }

    void FixedUpdate()
    {
        StateHandler();
    }

    private void StateHandler()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        bool onSlope = OnSlope();

        if (freeze)
        {
            movementState = MovementState.freeze;
            desiredMoveSpeed = 0;
            // rb.linearVelocity = Vector3.zero;
        }
        
        else if (activeGrapple)
        {
            movementState = MovementState.grappling;
            desiredMoveSpeed = runningSpeed;
        }
        
        else if (dashing)
        {
            movementState = MovementState.dashing; 
            desiredMoveSpeed = dashSpeed;
            speedChangeFactor = dashSpeedChangeFactor;

        }
        
        else if (wallRunning)
        {
            movementState = MovementState.wallRunning;
        }

        else if (wallJumping)
        {
            movementState = MovementState.wallJumping;
        }
        
        else if (grounded)
        {
            movementState = MovementState.running;
            desiredMoveSpeed = runningSpeed;
        }
        else if (!grounded)
        {
            movementState = MovementState.air;
            // targetVelocity = moveDirection.normalized * (moveSpeed * airMultiplier);
            desiredMoveSpeed = runningSpeed;
        }

        bool desiredMoveSpeedHasChanged = desiredMoveSpeed != lastDesiredMoveSpeed;
        if (lastState == MovementState.dashing) keepMomentum = true;
        
        if (desiredMoveSpeedHasChanged)
        {
            if (speedSmoothLerpCoroutine != null) 
                StopCoroutine(speedSmoothLerpCoroutine);
            
            if (keepMomentum)
            {
                speedSmoothLerpCoroutine = StartCoroutine(SmoothlyLerpMoveSpeed());
            }
            else
            {
                moveSpeed = desiredMoveSpeed;
            }
        }
        
        lastDesiredMoveSpeed = desiredMoveSpeed;    //  Merged guy's logic and mine
        lastState = movementState;                  //  
        // rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);   //
        
        // canDash = true;
    }
    
    private void MovePlayer()
    {
        if (activeGrapple) return;
        
        if (wallRunning) return;
        if (movementState == MovementState.dashing) return;
        
        Vector3 targetVelocity = rb.linearVelocity;
        if (movementState == MovementState.dashing) return;

        // calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // on ground
        if (grounded)
            targetVelocity = moveDirection.normalized * moveSpeed;

        // in air
        else if (!grounded)
        {
            if (wallJumping)
            {
                // targetVelocity = moveDirection.normalized *  + moveSpeed;
                Vector3 currentVelocity = rb.linearVelocity;
                rb.linearVelocity = currentVelocity + moveDirection.normalized ;
                return;
            }
            targetVelocity = moveDirection.normalized * (moveSpeed * airMultiplier);
        }

        rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);
        // turn gravity off while on slope
        rb.useGravity = !OnSlope();
    }

    private void myInput()
    {
        if (dashing) return; 
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        if (Input.GetKey(JUMP_KEY) && readyToJump && grounded)
        {
            readyToJump = false;
            Jump();
        }
        
    }

    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        float boostFactor = speedChangeFactor;

        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);
            time += Time.deltaTime * boostFactor;
            yield return null;
        }

        moveSpeed = desiredMoveSpeed;
        speedChangeFactor = 1f;
        keepMomentum = false;
    }

    private void SpeedControl()
    {
        Vector3 flatVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

        // limit velocity if needed
        if (flatVelocity.magnitude > moveSpeed)
        {
            Vector3 limitVelocity = flatVelocity.normalized * moveSpeed;
            rb.linearVelocity = new Vector3(limitVelocity.x, rb.linearVelocity.y, limitVelocity.z);
        }

        // limit Y velocity
        if (maxYSpeed != 0 && rb.linearVelocity.y > maxYSpeed)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, maxYSpeed, rb.linearVelocity.z);
        }
    }

    private void Jump()
    {
        exitingSlope = true;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z); // reset y velocity
        // rb.AddForce(transform.up * jumpForce, ForceMode.Impulse); //jump
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
        StartCoroutine(ResetJump()); // Start cooldown
    }

    private IEnumerator ResetJump()
    {
        yield return new WaitForSeconds(jumpCooldown);
        readyToJump = true;
        exitingSlope = false;
    }

    private bool enableMovementOnNextTouch;
    public void GrapplingJumpToPosition(Vector3 targetPosition, float trajectoryHeight)
    {
        activeGrapple = true;
        grapplingVelocityToSet = CalculateJumpVelocity(transform.position, targetPosition, trajectoryHeight);
        StartCoroutine(InvokeSettingGrapplingVelocity());
        StartCoroutine(InvokeResettingRestrictionsFunctions(3f));
    }


    private Vector3 grapplingVelocityToSet;

    private IEnumerator InvokeSettingGrapplingVelocity()
    {
        yield return new WaitForSeconds(0.1f);
        SetGrapplingVelocity();
    }
    private void SetGrapplingVelocity()
    {
        enableMovementOnNextTouch = true;
        rb.linearVelocity = grapplingVelocityToSet;
    }

    private IEnumerator InvokeResettingRestrictionsFunctions(float time)
    {
        yield return new WaitForSeconds(time);
        ResetRestrictionsFunctions();
    }
    public void ResetRestrictionsFunctions()
    {
        activeGrapple = false;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (enableMovementOnNextTouch)
        {
            enableMovementOnNextTouch = false;
            ResetRestrictionsFunctions();
            GetComponent<Grappling>().StopGrapple();
        }
    }

    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.4f, groundLayer))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            // Debug.Log(angle);
            return angle <= maxSlopeAngle && angle != 0;
        }

        return false;
    }

    private Vector3 GetSlopeVector()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }
    
    private Vector3 GetSlopeAngle()
    {
        // Debug.Log(Mathf.Atan2(slopeHit.normal.x, slopeHit.normal.z) * Mathf.Rad2Deg);
        // return Mathf.Atan2(slopeHit.normal.x, slopeHit.normal.z) * Mathf.Rad2Deg;
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal);
    }

    private Vector3 CalculateJumpVelocity(Vector3 startPoint, Vector3 endPoint, float trajectoryHeight)
    {
        float gravity = Physics.gravity.y;
        float displacementY = endPoint.y - startPoint.y;
        Vector3 displacementXZ = new Vector3(endPoint.x - startPoint.x, 0f, endPoint.z - startPoint.z);
        
        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * trajectoryHeight);
        Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * trajectoryHeight / gravity) + Mathf.Sqrt(2 * (displacementY - trajectoryHeight) / gravity));
        
        return velocityXZ + velocityY;
    }
}
