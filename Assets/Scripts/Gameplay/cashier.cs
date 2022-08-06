using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class cashier : MonoBehaviour
{

    private bool canTalk = false;
    private ListHandler listHandler;
    private Timer timer;
    private string currentScene;

    // Start is called before the first frame update
    void Start()
    {
        Transform child = transform.Find("Thanks");
        child.gameObject.SetActive(false);
        listHandler = GameObject.Find("ListHandler").GetComponent<ListHandler>();
        timer = GameObject.Find("Timer").GetComponent<Timer>();
    }

    // Update is called once per frame
    void Update()
    {
        
         if (canTalk && (Input.GetButtonDown("interact")) && (listHandler.shoppingComplete == true))   
         {
            Talk();
        }
    }
        
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player")
        {

            canTalk = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player")
        {
            canTalk = false;
        }
    }
    
    private void Talk()
    {
        Transform child = transform.Find("Thanks");
        child.gameObject.SetActive(true);
        timer.TimerStop();
        LevelCompleteItems.AddItems(listHandler.bag);
        currentScene = SceneManager.GetActiveScene().name;
        switch (currentScene)
            {
                case "Level 1":
                    GlobalVariables.nextLevel = "Level 2 Intro";
                    break;
                case "Level 2":
                    GlobalVariables.nextLevel = "Level 3 Intro";
                    break;
                case "Level 3":
                    GlobalVariables.nextLevel = "Level 4 Intro";
                    break;
                case "Level 4":
                    GlobalVariables.nextLevel = "Level 5 Intro";
                    break;
            }
        StartCoroutine(Wait());
    }

    IEnumerator Wait() {
        yield return new WaitForSeconds(2);
        SceneManager.LoadScene(sceneBuildIndex: 1);

 }
}
