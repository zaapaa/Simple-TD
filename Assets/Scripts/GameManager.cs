using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public int maxLives;
    public float startingMoney;
    private int currentLives;
    private float currentMoney;
    private float gameSpeed;
    private bool isGameOver;
    private bool isPaused;
    public GameObject livesText;
    public GameObject moneyText;
    public GameObject waveTimerText;

    public static GameManager instance;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        instance = this;
    }
    void Start()
    {
        currentLives = maxLives;
        currentMoney = startingMoney;
        gameSpeed = 1f;
        isGameOver = false;
        isPaused = false;
        UpdateUI();
    }

    public void DecreaseLives(int count)
    {
        currentLives -= count;
        if (currentLives <= 0)
        {
            isGameOver = true;
        }
        UpdateUI();
    }
    public void IncreaseMoney(float amount)
    {
        currentMoney += amount;
        UpdateUI();
    }
    public bool HasEnoughMoney(float amount)
    {
        return currentMoney >= amount;
    }
    public void DecreaseMoney(float amount)
    {
        currentMoney -= amount;
        UpdateUI();
    }
    public void SetGameSpeed(float speed)
    {
        isPaused = false;
        gameSpeed = speed;
        Time.timeScale = gameSpeed;
    }
    public void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
    }
    private void UpdateUI()
    {
        livesText.GetComponent<TextMeshProUGUI>().text = currentLives.ToString();
        moneyText.GetComponent<TextMeshProUGUI>().text = currentMoney.ToString();
    }
}
