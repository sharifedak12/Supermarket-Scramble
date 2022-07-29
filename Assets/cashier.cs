using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class cashier : MonoBehaviour
{

    public bool canTalk = false;
    private ListHandler listHandler;
    private Timer timer;
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
         if (canTalk & (Input.GetButtonDown("interact")) & (listHandler.shoppingComplete == true))   
         {
                    Talk(); 
        }
    }
        
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player")
        {
            Debug.Log("Can talk");
            canTalk = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player")
        {
            Debug.Log("Cannot talk");
            canTalk = false;
        }
    }
    
    private void Talk()
    {
        Transform child = transform.Find("Thanks");
        child.gameObject.SetActive(true);
        timer.TimerStop();
        SceneManager.LoadScene(sceneBuildIndex: 1);
    }

}
