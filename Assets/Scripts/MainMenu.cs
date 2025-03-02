using UnityEngine;

public class MainMenu : MonoBehaviour
{
    
    // [SerializeField] private GameObject mainMenuUI;
    [SerializeField] private GameObject player;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Time.timeScale = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            gameObject.SetActive(false);
            player.GetComponent<PlayerMovement>().enabled = true;
            player.GetComponent<Dashing>().enabled = true;
            Time.timeScale = 1;
            // player.GetComponent<>().enabled = true;
        }
    }
}
