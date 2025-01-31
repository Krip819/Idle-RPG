using UnityEngine;
using System.Collections;

public class HealAllAllies : MonoBehaviour
{
    public float healAmount = 50f; // Количество восстанавливаемого HP
    private ParticleSystem particleSystem;

    void Start()
    {
        HealAllies();

        // Ищем `Particle System` внутри префаба
        particleSystem = GetComponentInChildren<ParticleSystem>();

        if (particleSystem != null)
        {
            StartCoroutine(DestroyAfterParticles()); // Ждём проигрывания эффекта
        }
        else
        {
            Destroy(gameObject); // Если частиц нет, удаляем сразу
        }
    }

    void HealAllies()
    {
        CharacterController[] allAllies = FindObjectsOfType<CharacterController>();

        foreach (CharacterController ally in allAllies)
        {
            if (ally.team == CharacterTeam.Allies) // Лечим только союзников
            {
                ally.Heal(healAmount);
            }
        }
    }

    IEnumerator DestroyAfterParticles()
    {
        yield return new WaitUntil(() => !particleSystem.IsAlive()); // Ждём, пока частицы не завершатся
        Destroy(gameObject); // Удаляем объект после окончания эффекта
    }
}
