using UnityEngine;

public class BulletController : MonoBehaviour
{
    private CharacterController target;
    private float damage;
    private float speed;
    private float lifeTime = 5f; // Максимальное время жизни пули

    public void Init(CharacterController target, float damage, float speed)
    {
        this.target = target;
        this.damage = damage;
        this.speed = speed;

        // Убедимся, что у пули отключено физическое воздействие
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true; // Снаряд не взаимодействует с физикой
        }

        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true; // Отключаем физическое столкновение
        }
    }

    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // Двигаемся к цели
        Vector3 direction = (target.transform.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;

        // Проверяем, достигли ли цель
        float dist = Vector3.Distance(transform.position, target.transform.position);
        if (dist < 0.5f)
        {
            ApplyDamage();
            Destroy(gameObject);
        }

        // Уменьшаем время жизни пули
        lifeTime -= Time.deltaTime;
        if (lifeTime <= 0f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Проверяем, попали ли в цель
        if (target != null && other.gameObject == target.gameObject)
        {
            ApplyDamage();
            Destroy(gameObject);
        }
    }

    private void ApplyDamage()
    {
        if (target != null)
        {
            target.TakeDamage(damage);
        }
    }
}
