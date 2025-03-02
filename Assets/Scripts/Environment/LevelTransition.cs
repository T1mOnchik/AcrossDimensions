using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelTransition : MonoBehaviour
{
    [SerializeField] private GameObject transitionText;
    [SerializeField] private string levelToLoad;
    // [SerializeField] private TransitionEffect transitionEffect;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            transitionText.SetActive(true);
            if (Input.GetKeyDown(KeyCode.F))
            {
                SceneManager.LoadScene(levelToLoad, LoadSceneMode.Single);    
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            transitionText.SetActive(false);
    }
}
