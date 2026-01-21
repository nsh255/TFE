using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }
    public int CurrentScore { get; private set; }

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this; DontDestroyOnLoad(gameObject);
    }

    public void Add(int amount)
    {
        CurrentScore += amount;
        // TODO: UI hook
    }

    public void ResetScore() => CurrentScore = 0;
}
