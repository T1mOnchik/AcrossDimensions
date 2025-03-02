using System;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public class DeathZone : MonoBehaviour
{
    [SerializeField] private Transform checkpoint;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    private void OnTriggerEnter(Collider other)
    {
        other.gameObject.GetComponent<TransitionEffect>().StartTransition();
        other.transform.position = checkpoint.position;
        other.transform.rotation = checkpoint.rotation;
        other.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
    }

    public void SetNewCheckpoint(Transform newCheckpoint)
    {
        checkpoint = newCheckpoint;
    }
}
