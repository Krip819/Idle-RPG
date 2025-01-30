using UnityEngine;

public class UltimateAbility : MonoBehaviour
{
    public float damage = 50f; // Урон всем врагам
    private ParticleSystem particleSystem;

    void Start()
    {
        ApplyDamageToAllEnemies();

        // Находим систему частиц внутри префаба
        particleSystem = GetComponentInChildren<ParticleSystem>();

        if (particleSystem != null)
        {
            // Удаляем объект, когда частицы полностью проиграются
            Destroy(gameObject, particleSystem.main.duration + particleSystem.main.startLifetime.constantMax);
        }
        else
        {
            // Если системы частиц нет, сразу уничтожаем
            Destroy(gameObject);
        }
    }

    void ApplyDamageToAllEnemies()
    {
        CharacterController[] allEnemies = FindObjectsOfType<CharacterController>();

        foreach (CharacterController enemy in allEnemies)
        {
            if (enemy.team == CharacterTeam.Enemies) // Бьём только врагов
            {
                enemy.TakeDamage(damage);
            }
        }
    }
}
