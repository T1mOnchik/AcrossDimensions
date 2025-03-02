using UnityEngine;

public class HeadBobController : MonoBehaviour
{

    [SerializeField] private bool enable = true;

    [SerializeField, Range(0, 0.1f)] private float amplitude = 0.015f;
    [SerializeField, Range(0, 30f)] private float frequency = 10f;

    [SerializeField] private Transform mCamera;
    [SerializeField] private Transform cameraHolder;

    private float toggleSpeed = 3f;
    private Vector3 startPos;
    // character controller
    private Rigidbody rb;
    private PlayerMovement pm;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        // controller = get controller;
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovement>();
        startPos = mCamera.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if (!enable) return;
        CheckMotion();
        ResetPosition();
        mCamera.LookAt(FocusTarget());
    }

    private void CheckMotion()
    {
        float speed = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude;
        if (speed < toggleSpeed) return;
        if (!pm.grounded) return;
        PlayMotion(FootStepMotion());
    }
    
    private void PlayMotion(Vector3 motion){
        mCamera.localPosition += motion; 
    }

    private Vector3 FootStepMotion()
    {
        Vector3 pos = Vector3.zero;
        pos.y += Mathf.Sin(Time.time * frequency) * amplitude;
        pos.x += Mathf.Cos(Time.time * frequency / 2) * amplitude * 2;
        return pos;
    }

    private void ResetPosition()
    {
        if (mCamera.localPosition == startPos) return;
        mCamera.localPosition = Vector3.Lerp(mCamera.localPosition, startPos, 1 * Time.deltaTime);
    }

    private Vector3 FocusTarget()
    {
        Vector3 pos = new Vector3(transform.position.x, transform.position.y + cameraHolder.localPosition.y, transform.position.z);
        pos += cameraHolder.forward * 15f;
        return pos;
    }
}
