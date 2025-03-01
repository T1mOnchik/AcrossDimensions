using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Random = UnityEngine.Random;

public class PlayerCam : MonoBehaviour
{
    
    [SerializeField]private float mouseSensitivity;
    [SerializeField] private Transform cameraHolder;
    [SerializeField] private GameObject speedEffect;
    
    public Transform orientation;
    private float xRotation;
    private float yRotation;
    private Camera _camera;
    private bool isDashingEffect = false;
    private Quaternion originalRotation;
    
    // Start is called before the first frame update
    void Start()
    {
        _camera = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * mouseSensitivity;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * mouseSensitivity;

        xRotation -= mouseY;
        yRotation += mouseX;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        if (!isDashingEffect)
            cameraHolder.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }
    
    private void FixedUpdate()
    {
        orientation.rotation = Quaternion.Euler(orientation.rotation.x, yRotation, orientation.rotation.z);
    }

    public void DoFov(float endValue)
    {
        _camera.DOFieldOfView(endValue, 0.25f);
    }

    public void DoTilt(float zTilt)
    {
        transform.DOLocalRotate(new Vector3(0, 0, zTilt), 0.25f);
        // cameraHolder.DORotate(new Vector3(0, 0, zTilt), 0.25f);
    }

    public void DoRotation(float duration, float targetAngle, int direction, int mode)
    {
        StartCoroutine(DashShake(duration, targetAngle, direction, mode));
    }
    
    private IEnumerator DashShake(float duration, float targetAngle, int direction, int mode) // direction: 1 - right; -1 - left  
    {                                                                                         // mode: 1 - rotate/revert; 2 - rotate only; 3 - revert only
        isDashingEffect = true;
        // Vector3 originalPosition = _camera.transform.localPosition;
        originalRotation = transform.localRotation;
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetAngle * -direction);
        if (mode == 1 || mode == 2)
            yield return StartCoroutine(RotateCamera(duration - duration / 0.2f, targetRotation));    
        if (mode == 1 || mode == 3)
            yield return StartCoroutine(RevertRotation(duration / 0.2f, targetRotation));
        isDashingEffect = false;
    }
    
    private IEnumerator RotateCamera(float timeStamp, Quaternion targetRotation)
    {
        float elapsedTime = 0f;

        while (elapsedTime < timeStamp)
        {
            elapsedTime += Time.deltaTime;
            
            // _camera.transform.localPosition = new Vector3(originalPosition.x + offsetX, originalPosition.y, originalPosition.z);
            transform.localRotation = Quaternion.Lerp(originalRotation, targetRotation, elapsedTime / timeStamp);
            yield return null;
        }
        transform.localRotation = targetRotation;
    }

    private IEnumerator RevertRotation(float timeStamp, Quaternion targetRotation)
    {
        float elapsedTime = 0f;
        while (elapsedTime < timeStamp)
        {
            elapsedTime += Time.deltaTime;
            transform.localRotation = Quaternion.Lerp(targetRotation, originalRotation, elapsedTime / timeStamp);
            yield return null;
        }
        transform.localRotation = originalRotation;
    }

    public void EnableSpeedEffect(bool enable)
    {
        speedEffect.SetActive(enable);
    }
}
