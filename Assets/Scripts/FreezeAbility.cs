using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FreezeAbility : MonoBehaviour
{
    public float freezeDuration = 3f; // Сколько секунд враги заморожены
    public Color freezeColor = Color.cyan; // Цвет во время заморозки
    private Dictionary<Renderer, Color> originalColors = new Dictionary<Renderer, Color>(); // Сохраняем оригинальные цвета

    void Start()
    {
        StartCoroutine(FreezeEnemies());
    }

    IEnumerator FreezeEnemies()
    {
        CharacterController[] allEnemies = FindObjectsOfType<CharacterController>();

        // Останавливаем врагов и меняем цвет
        foreach (CharacterController enemy in allEnemies)
        {
            if (enemy.team == CharacterTeam.Enemies)
            {
                Animator animator = enemy.GetComponent<Animator>();
                if (animator != null) animator.speed = 0; // Останавливаем анимацию

                enemy.enabled = false; // Отключаем движение и атаки

                // Меняем цвет материала
                Renderer renderer = enemy.GetComponentInChildren<Renderer>();
                if (renderer != null)
                {
                    if (!originalColors.ContainsKey(renderer))
                    {
                        originalColors[renderer] = renderer.material.color; // Сохраняем оригинальный цвет
                    }
                    renderer.material.color = freezeColor; // Меняем цвет на замороженный
                }
            }
        }

        yield return new WaitForSeconds(freezeDuration);

        // Размораживаем врагов и возвращаем цвет
        foreach (CharacterController enemy in allEnemies)
        {
            if (enemy.team == CharacterTeam.Enemies)
            {
                Animator animator = enemy.GetComponent<Animator>();
                if (animator != null) animator.speed = 1; // Включаем анимацию

                enemy.enabled = true; // Включаем обратно движение и атаки

                // Возвращаем оригинальный цвет
                Renderer renderer = enemy.GetComponentInChildren<Renderer>();
                if (renderer != null && originalColors.ContainsKey(renderer))
                {
                    renderer.material.color = originalColors[renderer];
                }
            }
        }

        Destroy(gameObject); // Удаляем эффект после завершения
    }
}
