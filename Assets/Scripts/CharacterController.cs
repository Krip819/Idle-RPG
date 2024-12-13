using UnityEngine;
using System.Collections.Generic;

public enum CharacterTeam { Allies, Enemies }
public enum CharacterAttackType { Melee, Ranged }

public class CharacterController : MonoBehaviour
{
    [Header("Stats")]
    public CharacterTeam team = CharacterTeam.Allies;
    public CharacterAttackType attackType = CharacterAttackType.Melee;
    public float maxHealth = 100f;
    public float health;
    public float moveSpeed = 1f;
    public float attackDamage = 10f;

    [Header("Attack Ranges")]
    public float meleeRange = 2f;    // Дальность для мили атаки
    public float rangedRange = 5f;   // Дальность для рендж атаки

    [Header("Attack Settings")]
    public float attackCooldown = 1f;
    private float attackTimer = 0f;

    [Header("Ranged Settings")]
    public GameObject bulletPrefab;  // Префаб пули, назначить в инспекторе для рендж атаки
    public float bulletSpeed = 10f;

    [Header("Health Bar")]
    public GameObject healthBarPrefab;        // Префаб полоски здоровья, назначить в инспекторе
    public Vector3 healthBarScale = new Vector3(0.01f, 0.01f, 0.01f); // Настройка масштаба полоски
    public float healthBarHeight = 2f;        // Высота размещения полоски здоровья над персонажем
    private HealthBar healthBar;

    private bool isInBattle = false;
    private List<CharacterController> myTeamList;
    private List<CharacterController> enemyTeamList;
    private BattleManager battleManager;

    void Awake()
    {
        // Устанавливаем текущее здоровье на максимум при создании
        health = maxHealth;
    }

    void Start()
    {
        if (healthBarPrefab != null)
        {
            // Создаем полоску здоровья как дочерний объект
            GameObject hb = Instantiate(healthBarPrefab, transform);

            // Настраиваем позицию полоски (например, над головой персонажа)
            hb.transform.localPosition = new Vector3(0, healthBarHeight, 0);

            // Настраиваем масштаб полоски
            hb.transform.localScale = healthBarScale;

            // Получаем ссылку на компонент HealthBar
            healthBar = hb.GetComponent<HealthBar>();
            if (healthBar != null)
            {
                // Устанавливаем начальное значение здоровья
                healthBar.SetHealth(health, maxHealth);
            }
        }
    }

    void Update()
    {
        if (!isInBattle) return;

        CharacterController target = FindNearestEnemy();
        if (target == null) return;

        float currentRange = (attackType == CharacterAttackType.Melee) ? meleeRange : rangedRange;
        float dist = Vector3.Distance(transform.position, target.transform.position);

        if (dist > currentRange)
        {
            MoveTowards(target.transform.position);
        }
        else
        {
            attackTimer += Time.deltaTime;
            if (attackTimer >= attackCooldown)
            {
                Attack(target);
                attackTimer = 0f;
            }
        }

        // Обновляем полоску здоровья
        if (healthBar != null)
        {
            healthBar.SetHealth(health, maxHealth);
        }
    }

    /// <summary>
    /// Инициализирует персонажа для участия в бою.
    /// </summary>
    /// <param name="manager">Ссылка на BattleManager.</param>
    /// <param name="myTeam">Список своей команды.</param>
    /// <param name="enemyTeam">Список противников.</param>
    public void Init(BattleManager manager, List<CharacterController> myTeam, List<CharacterController> enemyTeam)
    {
        this.battleManager = manager;
        this.myTeamList = myTeam;
        this.enemyTeamList = enemyTeam;
        isInBattle = true;
    }

    /// <summary>
    /// Находит ближайшего врага из списка противников.
    /// </summary>
    /// <returns>Ближайший враг.</returns>
    private CharacterController FindNearestEnemy()
    {
        CharacterController nearest = null;
        float minDist = Mathf.Infinity;
        foreach (var enemy in enemyTeamList)
        {
            if (enemy == null) continue;
            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = enemy;
            }
        }
        return nearest;
    }

    /// <summary>
    /// Двигается к цели.
    /// </summary>
    /// <param name="targetPos">Позиция цели.</param>
    private void MoveTowards(Vector3 targetPos)
    {
        Vector3 dir = (targetPos - transform.position).normalized;
        transform.position += dir * moveSpeed * Time.deltaTime;
    }

    /// <summary>
    /// Атакует цель.
    /// </summary>
    /// <param name="target">Цель атаки.</param>
    private void Attack(CharacterController target)
    {
        if (target == null) return;

        if (attackType == CharacterAttackType.Melee)
        {
            // Мили атака наносит урон напрямую
            target.TakeDamage(attackDamage);
        }
        else
        {
            // Рендж атака: стреляем пулей
            if (bulletPrefab != null)
            {
                // Определяем точку стрельбы (например, чуть выше персонажа)
                Vector3 bulletSpawnPosition = transform.position + Vector3.up * 1.5f;
                GameObject bulletObj = Instantiate(bulletPrefab, bulletSpawnPosition, Quaternion.identity);
                BulletController bullet = bulletObj.GetComponent<BulletController>();
                if (bullet != null)
                {
                    bullet.Init(target, attackDamage, bulletSpeed);
                }
            }
        }
    }

    /// <summary>
    /// Получает урон.
    /// </summary>
    /// <param name="dmg">Количество урона.</param>
    public void TakeDamage(float dmg)
    {
        health -= dmg;
        if (health < 0f) health = 0f;

        // Обновляем полоску здоровья
        if (healthBar != null)
        {
            healthBar.SetHealth(health, maxHealth);
        }

        if (health <= 0f)
        {
            Die();
        }
    }

    /// <summary>
    /// Умирает и удаляется из игры.
    /// </summary>
    private void Die()
    {
        if (battleManager != null)
        {
            battleManager.RemoveCharacter(this);
        }

        // Уничтожаем полоску здоровья
        if (healthBar != null)
        {
            Destroy(healthBar.gameObject);
        }

        Destroy(gameObject);
    }
}
