using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
public float countdown = 90;
public TMP_Text TimerDisplay;
public bool timerStop;

public void Update() 
{     
  if(countdown > 0 && timerStop == false)     
  {         
    countdown -= Time.deltaTime;     
  }
  else
  {
    countdown = 0;
  }     
    DisplayTime(countdown);
    TimerDone();
}

public void DisplayTime(float timeToDisplay)
{
    if (timeToDisplay < 0)
    {
    timeToDisplay = 0;
    } 
    float minutes = Mathf.Floor(timeToDisplay / 60);
    float seconds = Mathf.Floor(timeToDisplay % 60);
    string niceTime = string.Format("{0:0}:{1:00}", minutes, seconds);
    TimerDisplay.text = niceTime;
    if (timeToDisplay <= 10)
    {
        TimerDisplay.color = Color.red;
    }
}

public void TimerDone()
{
  if(countdown == 0)     
  {         
    SceneManager.LoadScene(sceneBuildIndex: 0);  
  } 
}

public void TimerStop()
{
  timerStop = true;
}

}