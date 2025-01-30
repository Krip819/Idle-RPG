using UnityEngine;

public class UltimateAbilityManager : MonoBehaviour
{
    public Canvas abilityCanvas; // Канвас с кнопками суперспособностей
    public BattleManager battleManager; // Ссылка на менеджер боя
    public Transform spawnPoint; // Точка появления эффекта

    void Start()
    {
        // Скрываем канвас при старте
        if (abilityCanvas != null)
        {
            abilityCanvas.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // Показываем канвас после начала боя
        if (battleManager != null && battleManager.battleStarted && abilityCanvas != null)
        {
            abilityCanvas.gameObject.SetActive(true);
        }
    }

    public void ActivateUltimate(GameObject ultimateEffectPrefab)
    {
        if (ultimateEffectPrefab != null && spawnPoint != null)
        {
            Instantiate(ultimateEffectPrefab, spawnPoint.position, Quaternion.identity);
        }

        // Скрываем канвас после использования способности
        if (abilityCanvas != null)
        {
            abilityCanvas.gameObject.SetActive(false);
        }
    }
}
