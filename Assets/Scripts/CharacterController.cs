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

    private bool isInBattle = false;
    private bool isDead = false;
    private bool isAttacking = false;
    private List<CharacterController> myTeamList;
    private List<CharacterController> enemyTeamList;
    private BattleManager battleManager;

    private Rigidbody rb;

    private int currentAttackIndex = 0;
    private int totalAttackAnimations = 2; // Количество доступных анимаций удара

    void Awake()
    {
        health = maxHealth;
        animator = GetComponent<Animator>();

        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true; // Полностью отключаем физику
            rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
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
        if (isDead || !isInBattle) return;

        currentTarget = FindNearestEnemy();
        if (currentTarget == null)
        {
            if (animator != null)
                animator.SetBool("IsRunning", false);
            return;
        }

        float dist = Vector3.Distance(transform.position, currentTarget.transform.position);

        LookAtTarget();

        if (dist > attackRange && !isAttacking)
        {
            MoveTowards(currentTarget.transform.position);
        }
        else
        {
            if (!isAttacking)
            {
                if (animator != null)
                    animator.SetBool("IsRunning", false);

                if (attackTimer == 0f)
                {
                    Attack();
                    attackTimer = attackCooldown;
                }
            }
        }

        attackTimer = Mathf.Max(0, attackTimer - Time.deltaTime);

        if (healthBar != null)
            healthBar.SetHealth(health, maxHealth);
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

        // Небольшое случайное отклонение для предотвращения скоплений
        dir = Quaternion.Euler(0, Random.Range(-5f, 5f), 0) * dir;

        transform.position += dir * moveSpeed * Time.deltaTime;

        if (animator != null)
            animator.SetBool("IsRunning", true);

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

        if (IsAllyInPath(direction))
        {
            if (!IsObstacleInPath(leftDir)) return leftDir;
            if (!IsObstacleInPath(rightDir)) return rightDir;
        }

        if (!IsObstacleInPath(leftDir)) return leftDir;
        if (!IsObstacleInPath(rightDir)) return rightDir;

        return direction;
    }

    private bool IsAllyInPath(Vector3 direction)
    {
        foreach (var ally in myTeamList)
        {
            if (ally != this && !ally.isDead)
            {
                float dist = Vector3.Distance(transform.position, ally.transform.position);
                Vector3 dirToAlly = (ally.transform.position - transform.position).normalized;

                if (dist < attackRange && Vector3.Dot(dirToAlly, direction) > 0.7f)
                {
                    return true;
                }
            }
        }
        return false;
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

        if (animator != null)
        {
            isAttacking = true;

            if (attackType == CharacterAttackType.Melee)
            {
                currentAttackIndex = (currentAttackIndex + 1) % totalAttackAnimations;
                animator.SetTrigger($"Attack{currentAttackIndex + 1}");
            }
            else if (attackType == CharacterAttackType.Ranged)
            {
                animator.SetTrigger("RangedAttack");
                LaunchProjectile();
            }
        }

        Invoke(nameof(EndAttack), 0.5f);
    }

    private void LaunchProjectile()
    {
        if (bulletPrefab == null || currentTarget == null || isDead) return;

        Vector3 spawnPosition = transform.position + Vector3.up * 1.5f;
        GameObject bullet = Instantiate(bulletPrefab, spawnPosition, Quaternion.identity);
        BulletController bulletController = bullet.GetComponent<BulletController>();
        if (bulletController != null)
        {
            bulletController.Init(currentTarget, attackDamage, bulletSpeed);
        }
    }

    private void EndAttack()
    {
        isAttacking = false;

        if (animator != null)
        {
            animator.ResetTrigger("Attack1");
            animator.ResetTrigger("Attack2");
            animator.ResetTrigger("RangedAttack");
        }
    }

    public void ApplyDamageToTarget()
    {
        if (isDead) return;

        if (currentTarget != null && !currentTarget.isDead)
        {
            currentTarget.TakeDamage(attackDamage);
        }
    }

    public void TakeDamage(float dmg)
    {
        if (isDead) return;

        health -= dmg;
        if (health < 0f) health = 0f;

        if (healthBar != null)
            healthBar.SetHealth(health, maxHealth);

        if (health <= 0f)
            Die();
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        isAttacking = false;

        if (animator != null)
        {
            animator.ResetTrigger("Attack1");
            animator.ResetTrigger("Attack2");
            animator.ResetTrigger("RangedAttack");
            animator.SetTrigger("Die");
        }

        if (battleManager != null)
            battleManager.RemoveCharacter(this);

        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = false;

        if (healthBar != null)
            healthBar.gameObject.SetActive(false);

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
            transform.position = new Vector3(transform.position.x, hit.point.y, transform.position.z);
        }
    }

    public void Init(BattleManager manager, List<CharacterController> myTeam, List<CharacterController> enemyTeam)
    {
        this.battleManager = manager;
        this.myTeamList = myTeam;
        this.enemyTeamList = enemyTeam;
        isInBattle = true;

        // Игнорируем коллизии между союзниками
        foreach (var ally in myTeam)
        {
            if (ally != this)
            {
                Collider myCollider = GetComponent<Collider>();
                Collider allyCollider = ally.GetComponent<Collider>();
                if (myCollider != null && allyCollider != null)
                {
                    Physics.IgnoreCollision(myCollider, allyCollider);
                }
            }
        }

        StickToGround();
    }
}
