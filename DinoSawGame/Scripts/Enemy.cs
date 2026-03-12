using UnityEngine;

/// <summary>
/// Enemy – Controlează mișcarea, HP-ul și moartea unui dinozaur.
///
/// Atașează pe: Prefab-ul dinozaurului.
/// Componente necesare pe același GameObject:
///   - SpriteRenderer (sau MeshRenderer pentru 3D)
///   - Rigidbody2D (Body Type = Kinematic, pentru a nu fi afectat de gravitație lateral)
///   - Collider2D (BoxCollider2D sau CapsuleCollider2D) cu IsTrigger = FALSE
/// Inspector:
///   - moveSpeed       → Viteza de deplasare (ex: 2.0)
///   - maxHP           → Viaţa maximă (ex: 50)
///   - deathParticles  → Prefab ParticleSystem de sânge/efecte
///   - speedVariation  → Variație aleatorie de viteză (ex: 0.5)
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────────────────────────
    [Header("Mișcare")]
    [Tooltip("Viteza de bază de deplasare spre dreapta (unități/secundă).")]
    public float moveSpeed = 2f;

    [Tooltip("Variație aleatorie aplicată vitezei (+/-). Adaugă diversitate.")]
    public float speedVariation = 0.5f;

    [Header("Viaţă")]
    [Tooltip("HP-ul de start al dinozaurului.")]
    public float maxHP = 50f;

    [Header("Efecte moarte")]
    [Tooltip("Prefab cu ParticleSystem care se instanțiază la moarte (sânge, bucăți).")]
    public GameObject deathParticlesPrefab;

    [Tooltip("Durata după care instanța de particule este distrusă (secunde).")]
    public float particleLifetime = 2f;

    // ── Stat intern ───────────────────────────────────────────────────────────
    private float      _currentHP;
    private float      _actualSpeed;
    private Rigidbody2D _rb;
    private bool       _isDead;

    // ── Unity callbacks ───────────────────────────────────────────────────────
    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();

        // Kinematic: nu vrem fizică realistă, doar deplasare liniară
        _rb.bodyType = RigidbodyType2D.Kinematic;
        _rb.gravityScale = 0f;
    }

    private void Start()
    {
        // Setează HP inițial și aplică variație de viteză
        _currentHP   = maxHP;
        _actualSpeed = moveSpeed + Random.Range(-speedVariation, speedVariation);
        _actualSpeed = Mathf.Max(0.5f, _actualSpeed); // viteza minimă
    }

    private void Update()
    {
        if (_isDead) return;

        // Deplasare spre dreapta (axa X pozitivă)
        transform.Translate(Vector3.right * _actualSpeed * Time.deltaTime);
    }

    // ── API public ────────────────────────────────────────────────────────────
    /// <summary>
    /// Aplică damage dinozaurului. Apelat de SawBlade.
    /// </summary>
    public void TakeDamage(float damage)
    {
        if (_isDead) return;

        _currentHP -= damage;

        if (_currentHP <= 0f)
            Die();
    }

    /// <summary>
    /// Adaugă HP bonus (pentru progresie valuri). Apelat de EnemySpawner.
    /// </summary>
    public void AddBonusHP(float bonus)
    {
        maxHP      += bonus;
        _currentHP += bonus; // actualizăm și HP-ul curent
    }

    // ── Privat ────────────────────────────────────────────────────────────────
    private void Die()
    {
        if (_isDead) return;
        _isDead = true;

        // Efecte de particule la poziția dinozaurului
        if (deathParticlesPrefab != null)
        {
            GameObject particles = Instantiate(
                deathParticlesPrefab,
                transform.position,
                Quaternion.identity
            );
            Destroy(particles, particleLifetime);
        }

        // Înregistrează kill-ul la GameManager
        if (GameManager.Instance != null)
            GameManager.Instance.RegisterKill();

        // Distruge GameObject-ul
        Destroy(gameObject);
    }
}
