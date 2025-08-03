using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    public GameObject gameOverPanel;
    public TextMeshProUGUI carsDestroyedText;
    public TextMeshProUGUI timeSurvivedText;
    public bool Paused { get; private set;}
    private bool gameOver;

    private int carsDestroyed;
    
    private void Awake()
    {
        if(Instance == null) Instance = this;
        else Destroy(Instance);
        
        gameOverPanel.SetActive(false);
    }

    private void OnEnable()
    {
        Button rideAgainButton = gameOverPanel.GetComponentInChildren<Button>();
        rideAgainButton?.onClick.AddListener(RestartScene);

        var truckHealth = FindFirstObjectByType<Truck>().GetComponent<Health>();
        truckHealth.OnDestroyed += GameOver;
    }

    private void Update()
    {
        if(!gameOver) return;
        
        if(Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            RestartScene();
    }

    [ContextMenu("Game Over")]
    public void GameOver(GameObject obj)
    {
        if(!obj.TryGetComponent(out Truck truck)) return;
        
        Paused = true;
        gameOver = true;
        
        carsDestroyedText.text = carsDestroyed.ToString();
        timeSurvivedText.text = GetFormattedTime();
        gameOverPanel.SetActive(true);
        
        //enable cursor
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    
    private void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    private string GetFormattedTime()
    {
        float time = Time.time;

        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);

        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void CarDestroyed()
    {
        carsDestroyed++;
    }
}
