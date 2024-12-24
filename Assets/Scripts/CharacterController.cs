using UnityEngine;
using System.Collections.Generic;

public enum CharacterTeam { Allies, Enemies }
public enum CharacterAttackType { Melee, Ranged }
public enum ForwardAxis { Z, NegativeZ, X, NegativeX }

public class CharacterController : MonoBehaviour
{
    [Header("Stats")]
    public CharacterTeam team = CharacterTeam.Allies;
    public CharacterAttackType attackType = CharacterAttackType.Melee;
    public float maxHealth = 100f;
    public float health;
    public float moveSpeed = 1f;
    public float attackDamage = 10f;

    [Header("Attack Settings")]
    public float attackRange = 5f;
    public float attackCooldown = 1f;
    private float attackTimer = 0f;
    private CharacterController currentTarget;

    [Header("Ranged Attack")]
    public GameObject bulletPrefab;
    public float bulletSpeed = 10f;

    [Header("Projectile Settings")]
    public Transform projectileSpawnPoint;

    [Header("Impact Effect Settings")]
    public GameObject impactEffectPrefab;
    public Transform impactEffectSpawnPoint;

    [Header("Health Bar")]
    public GameObject healthBarPrefab;
    public float healthBarHeight = 2f;
    private HealthBar healthBar;

    [Header("Animation")]
    public Animator animator;

    [Header("Direction Settings")]
    public ForwardAxis forwardAxis = ForwardAxis.Z;
    public float rotationSpeed = 5f;

    [Header("Obstacle Avoidance")]
    public float avoidanceAngle = 45f;

    [Header("Position Settings")]
    [Tooltip("Вертикальное смещение персонажа от земли.")]
    public float verticalOffset = 0f;

    private bool isInBattle = false;
    private bool isDead = false;
    private bool isAttacking = false;
    private List<CharacterController> myTeamList;
    private List<CharacterController> enemyTeamList;
    private BattleManager battleManager;

    private Rigidbody rb;

    [Header("Attack Animations Settings")]
    public float attack1Duration = 1f;
    public float attack2Duration = 1f;

    private int currentAttackIndex = 0;
    private int totalAttackAnimations = 2;

    // >>>>> Добавлено для Специальной VFX Атаки
    [Header("Special VFX Attack Settings")]
    [Tooltip("Визуальный эффект, активируемый на каждой N-й атаке.")]
    public GameObject specialVFXPrefab; // Префаб VFX, который вы укажете в инспекторе
    [Tooltip("Точка спавна VFX. Если не указана, будет использована текущая позиция персонажа.")]
    public Transform specialVFXSpawnPoint; // Точка спавна VFX, которую можно указать в инспекторе
    [Tooltip("Интервал атак для активации VFX (например, 4 означает каждую 4-ю атаку).")]
    public int specialAttackInterval = 4; // По умолчанию каждая 4-я атака

    private int attackCount = 0; // Счётчик атак
    // <<<<< Конец добавлений для Специальной VFX Атаки

    void Awake()
    {
        health = maxHealth;
        animator = GetComponent<Animator>();

        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Чтобы персонажа не толкала физика
            rb.isKinematic = true;
            // Физика не будет вращать персонажа
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }
    }

    void Start()
    {
        if (healthBarPrefab != null)
        {
            GameObject hb = Instantiate(healthBarPrefab);
            healthBar = hb.GetComponent<HealthBar>();
            if (healthBar != null)
            {
                healthBar.Init(transform, healthBarHeight);
                healthBar.SetHealth(health, maxHealth);
            }
        }

        StickToGround();
    }

    void Update()
    {
        // Если мертв или не в бою — ничего не делаем
        if (isDead || !isInBattle) return;

        currentTarget = FindNearestEnemy();
        if (currentTarget == null)
        {
            // Нет цели, просто стоим
            animator.SetBool("IsRunning", false);
            return;
        }

        float dist = Vector3.Distance(transform.position, currentTarget.transform.position);

        LookAtTarget();

        // Проверяем, нужно ли бежать (цель далеко и не атакуем)
        bool shouldMove = dist > attackRange && !isAttacking;
        animator.SetBool("IsRunning", shouldMove);

        if (shouldMove)
        {
            MoveTowards(currentTarget.transform.position);
        }
        else
        {
            // Если не двигаемся, возможно атакуем
            if (!isAttacking)
            {
                if (attackTimer <= 0f)
                {
                    Attack();
                    attackTimer = attackCooldown;
                }
            }
        }

        // Кулдаун атаки
        attackTimer = Mathf.Max(0, attackTimer - Time.deltaTime);

        // Обновляем полоску здоровья
        if (healthBar != null)
        {
            healthBar.SetHealth(health, maxHealth);
        }
    }

    private void MoveTowards(Vector3 targetPos)
    {
        if (isAttacking || isDead) return;

        Vector3 dir = (targetPos - transform.position).normalized;
        dir.y = 0f;

        if (IsObstacleInPath(dir))
        {
            dir = AvoidObstacle(dir);
        }

        // Передвигаем вручную (т.к. isKinematic)
        transform.position += dir * moveSpeed * Time.deltaTime;

        // Здесь убрали вызов animator.SetBool("IsRunning", true);
        // Теперь isRunning ставится в Update() исходя из shouldMove

        if (dir != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(dir);
            transform.rotation = SmoothRotateToTarget(targetRotation);
        }

        StickToGround();
    }

    private void LookAtTarget()
    {
        if (currentTarget == null || isDead) return;

        Vector3 directionToTarget = (currentTarget.transform.position - transform.position).normalized;
        directionToTarget.y = 0f;

        if (directionToTarget != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            transform.rotation = SmoothRotateToTarget(targetRotation);
        }
    }

    private Quaternion SmoothRotateToTarget(Quaternion targetRotation)
    {
        Quaternion adjustedRotation = AdjustRotationToForwardAxis(targetRotation);
        return Quaternion.Slerp(transform.rotation, adjustedRotation, Time.deltaTime * rotationSpeed);
    }

    private bool IsObstacleInPath(Vector3 direction)
    {
        return Physics.Raycast(transform.position + Vector3.up * 0.5f, direction, attackRange);
    }

    private Vector3 AvoidObstacle(Vector3 direction)
    {
        Vector3 leftDir = Quaternion.Euler(0, -avoidanceAngle, 0) * direction;
        Vector3 rightDir = Quaternion.Euler(0, avoidanceAngle, 0) * direction;

        if (!IsObstacleInPath(leftDir)) return leftDir;
        if (!IsObstacleInPath(rightDir)) return rightDir;

        return direction;
    }

    private Quaternion AdjustRotationToForwardAxis(Quaternion targetRotation)
    {
        switch (forwardAxis)
        {
            case ForwardAxis.Z:
                return targetRotation;
            case ForwardAxis.NegativeZ:
                return targetRotation * Quaternion.Euler(0, 180, 0);
            case ForwardAxis.X:
                return targetRotation * Quaternion.Euler(0, 90, 0);
            case ForwardAxis.NegativeX:
                return targetRotation * Quaternion.Euler(0, -90, 0);
            default:
                return targetRotation;
        }
    }

    private void Attack()
    {
        if (isDead || isAttacking) return;

        isAttacking = true;
        animator.SetBool("IsAttacking", true);

        // >>>>> Добавлено для Специальной VFX Атаки
        attackCount++; // Увеличиваем счётчик атак

        bool isSpecialAttack = (attackCount % specialAttackInterval) == 0;

        if (isSpecialAttack && specialVFXPrefab != null)
        {
            // Активируем специальный VFX
            Vector3 spawnPos = (specialVFXSpawnPoint != null) ? specialVFXSpawnPoint.position : transform.position;
            Quaternion spawnRot = (specialVFXSpawnPoint != null) ? specialVFXSpawnPoint.rotation : Quaternion.identity;
            Instantiate(specialVFXPrefab, spawnPos, spawnRot);
        }
        // <<<<< Конец добавлений для Специальной VFX Атаки

        // Триггерим анимацию атаки
        currentAttackIndex = (currentAttackIndex + 1) % totalAttackAnimations;
        animator.SetTrigger($"Attack{currentAttackIndex + 1}");

        float duration = (currentAttackIndex == 0) ? attack1Duration : attack2Duration;
        Invoke(nameof(EndAttack), duration);
    }

    private void EndAttack()
    {
        isAttacking = false;
        animator.SetBool("IsAttacking", false);
    }

    public void LaunchProjectile()
    {
        if (bulletPrefab == null || currentTarget == null || isDead) return;

        Vector3 spawnPosition = projectileSpawnPoint != null 
            ? projectileSpawnPoint.position 
            : transform.position + Vector3.up * 1.5f;

        GameObject bullet = Instantiate(bulletPrefab, spawnPosition, Quaternion.identity);
        BulletController bulletController = bullet.GetComponent<BulletController>();
        if (bulletController != null)
        {
            bulletController.Init(currentTarget, attackDamage, bulletSpeed);
        }
    }

    private void SpawnImpactEffect()
    {
        if (impactEffectPrefab == null) return;

        Vector3 spawnPosition = impactEffectSpawnPoint != null
            ? impactEffectSpawnPoint.position
            : (currentTarget != null ? currentTarget.transform.position : transform.position);

        GameObject effect = Instantiate(impactEffectPrefab, spawnPosition, Quaternion.identity);
        Destroy(effect, 2f);
    }

    public void ApplyDamageToTarget()
    {
        if (isDead) return;
        if (currentTarget != null && !currentTarget.isDead)
        {
            currentTarget.TakeDamage(attackDamage);
        }
        SpawnImpactEffect();
    }

    public void TakeDamage(float dmg)
    {
        if (isDead) return;

        health -= dmg;
        if (health < 0f) health = 0f;

        if (healthBar != null)
        {
            healthBar.SetHealth(health, maxHealth);
        }

        if (health <= 0f)
        {
            Die();
        }
    }

    // <<< Вариант 2.1: делаем коллайдер триггером, чтобы труп не мешал, и фиксируем его на месте
    private void Die()
    {
        if (isDead) return;
        isDead = true;
        isAttacking = false;

        animator.SetBool("IsDead", true);
        animator.SetBool("IsRunning", false);
        animator.SetBool("IsAttacking", false);
        animator.ResetTrigger("Attack1");
        animator.ResetTrigger("Attack2");
        animator.SetTrigger("Die");

        if (battleManager != null)
            battleManager.RemoveCharacter(this);

        // 1) Один раз «прижимаем» персонажа к земле
        StickToGround();

        // 2) Делаем Rigidbody полностью неподвижным,
        //    чтобы не падал и не сдвигался
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }

        // 3) Делаем коллайдер триггером (не мешает другим, но не падает)
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            // Важно: оставляем включённым, но делаем isTrigger = true
            col.isTrigger = true;
        }

        // 4) Выключаем визуализацию полоски здоровья
        if (healthBar != null)
            healthBar.gameObject.SetActive(false);

        // Отключаем логику скрипта — персонаж более не управляется
        enabled = false;
    }

    private CharacterController FindNearestEnemy()
    {
        CharacterController nearest = null;
        float minDist = Mathf.Infinity;
        foreach (var enemy in enemyTeamList)
        {
            if (enemy == null || enemy.isDead) continue;
            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = enemy;
            }
        }
        return nearest;
    }

    private void StickToGround()
    {
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out RaycastHit hit, 2f))
        {
            transform.position = new Vector3(transform.position.x, hit.point.y + verticalOffset, transform.position.z);
        }
        else
        {
            // Если луч не столкнулся с поверхностью, используем вертикальное смещение относительно текущей позиции
            transform.position = new Vector3(transform.position.x, verticalOffset, transform.position.z);
        }
    }

    public void Init(BattleManager manager, List<CharacterController> myTeam, List<CharacterController> enemyTeam)
    {
        this.battleManager = manager;
        this.myTeamList = myTeam;
        this.enemyTeamList = enemyTeam;
        isInBattle = true;

        StickToGround();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position + Vector3.up * 0.5f, transform.position + Vector3.down * 1.5f);
        
        // Визуализация вертикального смещения
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y + verticalOffset, transform.position.z), 0.1f);
    }
}
