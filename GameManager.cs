using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    Scene scn;
    public static GameManager gm;
    public GameObject pausePannel;
    bool isPaused = false;
    public bool canMove = true;

    void Start()
    {
        gm = this;
         scn = SceneManager.GetActiveScene();
         pausePannel.SetActive(false);
         Time.timeScale = 1;
    }

    void Update()
    {
        
    }

    public void LoadNextLevel()
    {
        SceneManager.LoadScene(scn.buildIndex + 1);
    }

}
