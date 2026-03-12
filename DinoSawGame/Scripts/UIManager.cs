using UnityEngine;
using TMPro; // TextMeshPro – asigură-te că pachetul TMP e instalat în Package Manager

/// <summary>
/// UIManager – Actualizează textele UI cu scorul, kill-urile și valul curent.
///
/// Atașează pe: un GameObject "UIManager" în scenă (poate fi același cu Canvas sau un copil al lui).
/// Inspector – drag &amp; drop referințele de TextMeshPro:
///   - scoreText  → TMP_Text care arată scorul
///   - killsText  → TMP_Text care arată dinozaurii uciși
///   - waveText   → TMP_Text care arată valul curent
/// </summary>
public class UIManager : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────────────────────────
    [Header("Referințe UI (TextMeshPro)")]
    [Tooltip("Text pentru scor.")]
    public TMP_Text scoreText;

    [Tooltip("Text pentru numărul de dinozauri uciși.")]
    public TMP_Text killsText;

    [Tooltip("Text pentru valul curent.")]
    public TMP_Text waveText;

    // ── Unity callbacks ───────────────────────────────────────────────────────
    private void Start()
    {
        // Abonăm callback-urile la evenimentele GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStatsChanged  += RefreshStats;
            GameManager.Instance.OnWaveStarted   += RefreshWave;
        }

        // Afișăm valorile inițiale
        RefreshStats();
        RefreshWave(1);
    }

    private void OnDestroy()
    {
        // Dezabonare pentru a evita memory leaks
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStatsChanged  -= RefreshStats;
            GameManager.Instance.OnWaveStarted   -= RefreshWave;
        }
    }

    // ── Callbacks ─────────────────────────────────────────────────────────────
    private void RefreshStats()
    {
        if (GameManager.Instance == null) return;

        if (scoreText != null)
            scoreText.text = $"SCOR: {GameManager.Instance.Score}";

        if (killsText != null)
            killsText.text = $"UCIȘI: {GameManager.Instance.Kills}";
    }

    private void RefreshWave(int wave)
    {
        if (waveText != null)
            waveText.text = $"VAL: {wave}";
    }
}
