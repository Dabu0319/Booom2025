using UnityEngine;
using UnityEngine.SceneManagement;

public class Dabu10_GameManager : MonoBehaviour
{
    public static Dabu10_GameManager instance;

    public GameObject defeatedUI;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartLevel();
        }
    }

    public void RestartLevel()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
    
    public void Success()
    {
        defeatedUI.SetActive(true);
        Time.timeScale = 0.1f; // 暂停游戏
        
        //restart game after 3 seconds
        Invoke("RestartLevel", 3f);
    }
}