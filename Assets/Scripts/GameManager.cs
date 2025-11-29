using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameObject playerGameOverPanel;
    public GameObject bossVictoryPanel;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Make sure panels are hidden at start
        if (playerGameOverPanel != null) playerGameOverPanel.SetActive(false);
        if (bossVictoryPanel != null) bossVictoryPanel.SetActive(false);
    }

    public void PlayerDied()
    {
        if (playerGameOverPanel != null)
            playerGameOverPanel.SetActive(true);

        Time.timeScale = 0f; // stop game
    }

    public void BossDied()
    {
        if (bossVictoryPanel != null)
            bossVictoryPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    public void ReloadScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
