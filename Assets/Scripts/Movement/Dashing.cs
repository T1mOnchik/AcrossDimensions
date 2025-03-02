using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Dashing : MonoBehaviour
{
    
    [Header("References")]
    [SerializeField]private Transform orientation;
    [SerializeField]private Transform playerCam;
    private Rigidbody rb;
    private PlayerMovement pm;
    private const KeyCode DASH_KEY = KeyCode.LeftShift;
    
    [Header("Dashing")]
    public float dashForce;
    public float dashUpwardForce;
    public float dashTime;
    private Vector3 delayedDashForce;
    [SerializeField]private float maxDashYSpeed;

    [Header("CameraEffects")] 
    public PlayerCam playerCamScript;
    [SerializeField]private float FOVWhileDashing;
    [SerializeField]private float horizontalDashCameraRotationAngle;
    private float FOVDefault;
    
    [Header("Cooldown")]
    [SerializeField]private float cooldown;
    private float cooldownTimer;

    [Header("Settings")] 
    [SerializeField] private bool useCameraForward;
    [SerializeField] private bool allowAllDirections;
    [SerializeField] private bool disableGravity;
    public bool resetVel;
        
    private float horizontalInput;
    private float verticalInput;
        
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovement>();
        FOVDefault = Camera.main.fieldOfView;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(DASH_KEY) && !pm.activeGrapple)
        {
            Dash();
        }

        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
    }

    private void Dash()
    {
        if (cooldownTimer > 0) return;
        else cooldownTimer = cooldown;
        pm.dashing = true;

        float defaultMaxYSpeed = pm.maxYSpeed;
        pm.maxYSpeed = maxDashYSpeed;

        Transform forwardT;
        if (useCameraForward)
            forwardT = playerCam;
        else
            forwardT = orientation;
        
        Vector3 forceToApply = GetDirection(forwardT) * dashForce + orientation.up * dashUpwardForce;

        if (disableGravity)
        {
            rb.useGravity = false;
        }
        StartCoroutine(DelayedDashForce(forceToApply, defaultMaxYSpeed));
        // rb.linearVelocity = forceToApply;
        
    }

    private IEnumerator DelayedDashForce(Vector3 forceToApply, float defaultMaxYSpeed)
    {
        if (resetVel)
        {
            rb.linearVelocity = Vector3.zero;
        }
        yield return new WaitForSeconds(0.025f);
        rb.AddForce(forceToApply, ForceMode.Impulse);
        StartCoroutine(ResetDash(defaultMaxYSpeed, (horizontalInput != 0 && verticalInput == 0)));
    }

    private IEnumerator ResetDash(float defaultMaxYSpeed, bool isDashHorizontal)
    {
        // float defaultFOV = mainCam.fieldOfView;
        // mainCam.fieldOfView *= FOVmultiplier;
        if (isDashHorizontal)
        {
            playerCamScript.DoRotation(dashTime, horizontalDashCameraRotationAngle, (horizontalInput > 0 ? 1 : -1), 1);
            playerCamScript.EnableHorizontalSpeedEffect((horizontalInput > 0), true);
        }
        else
        {
            playerCamScript.DoFov(FOVWhileDashing);
            playerCamScript.EnableSpeedEffect(true);
        }
        yield return new WaitForSeconds(dashTime);
        pm.dashing = false;
        pm.maxYSpeed = defaultMaxYSpeed;
        playerCamScript.DoFov(FOVDefault);
        if (disableGravity)
        {
            rb.useGravity = true;
        }
        
        yield return new WaitForSeconds(0.25f);
        playerCamScript.EnableSpeedEffect(false);
        playerCamScript.EnableHorizontalSpeedEffect((horizontalInput > 0), false);
    }

    private Vector3 GetDirection(Transform forwardT)
    {
        Vector3 direction;

        if (allowAllDirections)
            direction = forwardT.forward * verticalInput + forwardT.right * horizontalInput;
        else
            direction = forwardT.forward;

        if (verticalInput == 0 && horizontalInput == 0)
        {
            direction = forwardT.forward;
        }
        return direction.normalized;
    }

    // private void OnCollisionEnter(Collision other)
    // {
    //     if (other.gameObject.layer == 7)
    //     {
    //         StopAllCoroutines();
    //     }
    // }
}
