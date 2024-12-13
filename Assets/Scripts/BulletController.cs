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
            // Наносим урон цели
            target.TakeDamage(damage);
            Destroy(gameObject);
        }

        // Уменьшаем время жизни пули
        lifeTime -= Time.deltaTime;
        if (lifeTime <= 0f)
        {
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Дополнительно можно обработать столкновения
        Destroy(gameObject);
    }
}
