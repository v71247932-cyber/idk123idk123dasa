using UnityEngine;

/// <summary>
/// GameManager – Singleton care gestionează scorul, progresia valurilor
/// și starea generală a jocului.
///
/// Atașează pe: un GameObject gol numit "GameManager" în scenă.
/// </summary>
public class GameManager : MonoBehaviour
{
    // ── Singleton ────────────────────────────────────────────────────────────
    public static GameManager Instance { get; private set; }

    // ── Inspector ─────────────────────────────────────────────────────────────
    [Header("Puncte per dinozaur")]
    [Tooltip("Câte puncte primește jucătorul pentru fiecare dinozaur ucis.")]
    public int pointsPerKill = 10;

    [Header("Progresie valuri")]
    [Tooltip("La câte secunde crește dificultatea (spawn rate / HP).")]
    public float waveDuration = 15f;

    [Tooltip("Factorul cu care se înmulțește viteza de spawn la fiecare val. " +
             "Ex: 1.2 => +20% mai repede.")]
    public float spawnRateMultiplier = 1.2f;

    [Tooltip("HP suplimentar adăugat la fiecare dinozaur pentru fiecare val nou.")]
    public float hpBonusPerWave = 10f;

    // ── State intern ──────────────────────────────────────────────────────────
    private int  _score;
    private int  _kills;
    private int  _wave;
    private float _waveTimer;

    // ── Properties publice (citite de UIManager) ──────────────────────────────
    public int  Score => _score;
    public int  Kills => _kills;
    public int  Wave  => _wave;

    // ── Events ────────────────────────────────────────────────────────────────
    /// <summary>Declanșat ori de câte ori scorul sau kill-urile se schimbă.</summary>
    public System.Action OnStatsChanged;
    /// <summary>Declanșat la fiecare val nou.</summary>
    public System.Action<int> OnWaveStarted;

    // ── Unity callbacks ───────────────────────────────────────────────────────
    private void Awake()
    {
        // Singleton pattern simplu
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        _wave = 1;
        _waveTimer = 0f;
        OnWaveStarted?.Invoke(_wave);
    }

    private void Update()
    {
        _waveTimer += Time.deltaTime;
        if (_waveTimer >= waveDuration)
        {
            _waveTimer = 0f;
            AdvanceWave();
        }
    }

    // ── API public ────────────────────────────────────────────────────────────
    /// <summary>Apelat de Enemy când HP ajunge la 0.</summary>
    public void RegisterKill()
    {
        _kills++;
        _score += pointsPerKill;
        OnStatsChanged?.Invoke();
    }

    /// <summary>Returnează bonus-ul de HP pentru valul curent (util în Spawner).</summary>
    public float GetCurrentHPBonus() => (_wave - 1) * hpBonusPerWave;

    // ── Privat ────────────────────────────────────────────────────────────────
    private void AdvanceWave()
    {
        _wave++;

        // Notifică Spawner-ul să îi crească rata de spawn
        EnemySpawner spawner = FindObjectOfType<EnemySpawner>();
        if (spawner != null)
            spawner.ApplyWaveMultiplier(spawnRateMultiplier);

        OnWaveStarted?.Invoke(_wave);
        Debug.Log($"[GameManager] Val nou: {_wave}");
    }
}
