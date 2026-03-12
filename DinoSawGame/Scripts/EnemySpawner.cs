using UnityEngine;

/// <summary>
/// EnemySpawner – Generează dinozauri la intervale regulate din partea stângă a ecranului.
///
/// Atașează pe: un GameObject gol numit "EnemySpawner", plasat în afara ecranului, stânga.
/// Inspector:
///   - enemyPrefab   → Prefab-ul dinozaurului (cu Enemy.cs)
///   - spawnInterval → Secunde între spawn-uri (ex: 1.5)
///   - spawnPoint    → Transform-ul punctului de spawn (colț stânga)
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────────────────────────
    [Header("Referințe")]
    [Tooltip("Prefab-ul dinozaurului de spawnat.")]
    public GameObject enemyPrefab;

    [Tooltip("Punctul din care apar dinozaurii (stânga scenei).")]
    public Transform spawnPoint;

    [Header("Setări spawn")]
    [Tooltip("Intervalul inițial între spawn-uri, în secunde.")]
    public float spawnInterval = 1.5f;

    [Tooltip("Intervalul minim posibil (spawn rate nu va scădea sub această valoare).")]
    public float minSpawnInterval = 0.3f;

    [Header("Variații vizuale")]
    [Tooltip("Variație aleatorie de scală aplicată fiecărui dinozaur (0 = fără variație).")]
    public float scaleVariation = 0.2f;

    // ── Stat intern ───────────────────────────────────────────────────────────
    private float _timer;

    // ── Unity callbacks ───────────────────────────────────────────────────────
    private void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= spawnInterval)
        {
            _timer = 0f;
            SpawnEnemy();
        }
    }

    // ── API public (apelat de GameManager la fiecare val) ─────────────────────
    /// <summary>Reduce intervalul de spawn cu factorul dat (min = minSpawnInterval).</summary>
    public void ApplyWaveMultiplier(float multiplier)
    {
        spawnInterval = Mathf.Max(minSpawnInterval, spawnInterval / multiplier);
        Debug.Log($"[EnemySpawner] Nou interval spawn: {spawnInterval:F2}s");
    }

    // ── Privat ────────────────────────────────────────────────────────────────
    private void SpawnEnemy()
    {
        if (enemyPrefab == null || spawnPoint == null)
        {
            Debug.LogWarning("[EnemySpawner] enemyPrefab sau spawnPoint nu sunt setate!");
            return;
        }

        // Instanțiează dinozaurul la punctul de spawn
        GameObject dino = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);

        // ── Variație de scală ──────────────────────────────────────────────
        if (scaleVariation > 0f)
        {
            float randomScale = 1f + Random.Range(-scaleVariation, scaleVariation);
            dino.transform.localScale *= randomScale;
        }

        // ── Aplică bonus HP de val ─────────────────────────────────────────
        Enemy enemy = dino.GetComponent<Enemy>();
        if (enemy != null && GameManager.Instance != null)
        {
            enemy.AddBonusHP(GameManager.Instance.GetCurrentHPBonus());
        }
    }
}
