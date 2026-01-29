using System;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }
    public int CurrentScore { get; private set; }

    private const string PlayerPrefsKey = "ASCENSION_SCORE_HISTORY_V1";
    private const int MaxEntries = 30;

    [Serializable]
    public class ScoreEntry
    {
        public long utcTicks;
        public int score;
        public int roomsCleared;
        public int enemiesKilled;
        public string playerClass;
        public string result; // "GameOver" | "Victory" etc.
    }

    [Serializable]
    private class ScoreHistory
    {
        public List<ScoreEntry> entries = new();
    }

    private ScoreHistory history;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this; DontDestroyOnLoad(gameObject);

        LoadHistory();
    }

    public void Add(int amount)
    {
        CurrentScore += amount;
        // TODO: UI hook
    }

    public void ResetScore() => CurrentScore = 0;

    public IReadOnlyList<ScoreEntry> GetHistory()
    {
        history ??= new ScoreHistory();
        history.entries ??= new List<ScoreEntry>();
        return history.entries;
    }

    public void RecordRunAndSave(string result, int roomsCleared, int enemiesKilled, string playerClass)
    {
        history ??= new ScoreHistory();
        history.entries ??= new List<ScoreEntry>();

        var entry = new ScoreEntry
        {
            utcTicks = DateTime.UtcNow.Ticks,
            score = CurrentScore,
            roomsCleared = roomsCleared,
            enemiesKilled = enemiesKilled,
            playerClass = string.IsNullOrWhiteSpace(playerClass) ? "Unknown" : playerClass,
            result = string.IsNullOrWhiteSpace(result) ? "Unknown" : result,
        };

        history.entries.Insert(0, entry);
        if (history.entries.Count > MaxEntries)
        {
            history.entries.RemoveRange(MaxEntries, history.entries.Count - MaxEntries);
        }

        SaveHistory();
    }

    public void ClearHistory()
    {
        history = new ScoreHistory();
        SaveHistory();
    }

    private void LoadHistory()
    {
        history = new ScoreHistory();

        if (!PlayerPrefs.HasKey(PlayerPrefsKey))
            return;

        string json = PlayerPrefs.GetString(PlayerPrefsKey, string.Empty);
        if (string.IsNullOrWhiteSpace(json))
            return;

        try
        {
            var loaded = JsonUtility.FromJson<ScoreHistory>(json);
            if (loaded != null && loaded.entries != null)
            {
                history = loaded;
            }
        }
        catch
        {
            // Si el JSON está corrupto, no rompemos el juego.
            history = new ScoreHistory();
        }
    }

    private void SaveHistory()
    {
        history ??= new ScoreHistory();
        string json = JsonUtility.ToJson(history);
        PlayerPrefs.SetString(PlayerPrefsKey, json);
        PlayerPrefs.Save();
    }
}
