using System;
using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    
    [SerializeField]private DeathZone deathZone;
    [SerializeField]private Transform checkPoint;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            deathZone.SetNewCheckpoint(checkPoint);
    }
}
