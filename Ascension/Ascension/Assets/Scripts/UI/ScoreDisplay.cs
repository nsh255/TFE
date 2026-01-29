using UnityEngine;
using TMPro;

/// <summary>
/// Muestra la puntuación actual en el HUD.
/// Se actualiza automáticamente desde ScoreManager.
/// </summary>
public class ScoreDisplay : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("TextMeshProUGUI component to display score")]
    public TextMeshProUGUI scoreText;
    
    [Header("Format")]
    [Tooltip("Prefix text before score number")]
    public string prefix = "Score: ";
    
    [Tooltip("Number format (e.g. N0 for thousands separator)")]
    public string numberFormat = "N0";

    /// <summary>
    /// Actualiza el texto de puntuación cada frame leyendo de ScoreManager.
    /// </summary>
    private void Update()
    {
        if (ScoreManager.Instance != null && scoreText != null)
        {
            int score = ScoreManager.Instance.CurrentScore;
            scoreText.text = prefix + score.ToString(numberFormat);
        }
        else if (scoreText != null && ScoreManager.Instance == null)
        {
            scoreText.text = prefix + "0";
        }
    }
}
