using UnityEngine;

/// <summary>
/// SawWallGenerator – Generează automat un rând (sau grilă) de fierăstraie
/// la pornirea jocului, pe baza parametrilor din Inspector.
///
/// Atașează pe: un GameObject gol numit "SawWall", poziționat în partea dreaptă a scenei.
/// Inspector:
///   - sawPrefab   → Prefab-ul fierăstrăului (cu SawBlade.cs + CircleCollider2D Trigger)
///   - columns     → Număr de coloane (ex: 3 pentru adâncime)
///   - rows        → Număr de rânduri verticale (ex: 8)
///   - spacing     → Distanța dintre fierăstraie (ex: 0.6)
/// </summary>
public class SawWallGenerator : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────────────────────────
    [Header("Prefab fierăstrău")]
    [Tooltip("Prefab-ul fierăstrăului care conține SawBlade.cs și CircleCollider2D(Trigger).")]
    public GameObject sawPrefab;

    [Header("Dimensiuni zid")]
    [Tooltip("Număr de coloane de fierăstraie (adâncime pe axa X).")]
    [Range(1, 10)]
    public int columns = 3;

    [Tooltip("Număr de rânduri verticale de fierăstraie.")]
    [Range(1, 30)]
    public int rows = 8;

    [Tooltip("Spațierea dintre fierăstraie pe ambele axe (unități Unity).")]
    public float spacing = 0.65f;

    [Header("Offset pornire")]
    [Tooltip("Offsetul față de acest Transform pentru primul fierăstrău (colț stânga-jos al zidului).")]
    public Vector2 startOffset = Vector2.zero;

    // ── Unity callbacks ───────────────────────────────────────────────────────
    private void Awake()
    {
        GenerateWall();
    }

    // ── Privat ────────────────────────────────────────────────────────────────
    private void GenerateWall()
    {
        if (sawPrefab == null)
        {
            Debug.LogError("[SawWallGenerator] sawPrefab nu este setat!");
            return;
        }

        for (int col = 0; col < columns; col++)
        {
            for (int row = 0; row < rows; row++)
            {
                // Calculăm poziția fiecărui fierăstrău relativ la parent
                Vector3 localPos = new Vector3(
                    startOffset.x + col * spacing,
                    startOffset.y + row * spacing,
                    0f
                );

                // Instanțierea ca copil al acestui Transform (SawWall)
                GameObject saw = Instantiate(sawPrefab, transform);
                saw.transform.localPosition = localPos;
                saw.name = $"SawBlade_C{col}_R{row}";
            }
        }

        Debug.Log($"[SawWallGenerator] Generat {columns * rows} fierăstraie.");
    }
}
