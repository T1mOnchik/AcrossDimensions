using System;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public class DeathZone : MonoBehaviour
{
    [SerializeField] Transform checkpoint;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    private void OnTriggerEnter(Collider other)
    {
        other.transform.position = checkpoint.position;
    }
}
