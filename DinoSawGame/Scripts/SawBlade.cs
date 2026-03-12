using UnityEngine;

/// <summary>
/// SawBlade – Controlează rotația vizuală și damage-ul unui fierăstrău.
///
/// Atașează pe: Prefab-ul fierăstrăului (sau direct pe fiecare obiect Saw în scenă).
/// Componente necesare:
///   - SpriteRenderer (sprite circular de fierăstrău)
///   - CircleCollider2D cu IsTrigger = TRUE
/// Inspector:
///   - rotationSpeed   → Grade/secundă (ex: 360)
///   - damagePerSecond → Damage aplicat pe secundă dinozaurilor aflați în contact
/// </summary>
public class SawBlade : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────────────────────────
    [Header("Rotație")]
    [Tooltip("Viteza de rotație a fierăstrăului în grade pe secundă.")]
    public float rotationSpeed = 360f;

    [Header("Damage")]
    [Tooltip("Damage aplicat pe secundă oricărui Enemy care atinge fierăstrăul.")]
    public float damagePerSecond = 25f;

    // ── Unity callbacks ───────────────────────────────────────────────────────
    private void Update()
    {
        // Rotație continuă în jurul axei Z (2D)
        // Multiplicăm cu Time.deltaTime pentru a fi frame-rate independent
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }

    /// <summary>
    /// OnTriggerStay2D – apelat la fiecare frame cât timp un collider stă în trigger.
    /// Aceasta implementează "damage continuu pe secundă".
    /// Alternativ, OnTriggerEnter2D ar da damage instant la prima atingere.
    /// </summary>
    private void OnTriggerStay2D(Collider2D other)
    {
        // Verificăm dacă obiectul care a intrat are componenta Enemy
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null)
        {
            // Damage proporțional cu Time.deltaTime → damage per secundă
            enemy.TakeDamage(damagePerSecond * Time.deltaTime);
        }
    }
}
