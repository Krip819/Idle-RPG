using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UltimateAbilityManager : MonoBehaviour
{
    [System.Serializable]
    public class UltimateAbilityData
    {
        public Button abilityButton; // Кнопка способности
        public Slider abilitySlider; // Слайдер для заполнения энергии
        public GameObject effectPrefab; // Префаб эффекта
        public float chargeTime = 5f; // Время полного заполнения шкалы
        private float currentCharge = 0f; // Текущее заполнение шкалы
        private bool isReady = false; // Готова ли способность к использованию

        public void UpdateCharge()
        {
            if (!isReady)
            {
                currentCharge += Time.deltaTime;
                abilitySlider.value = currentCharge / chargeTime;

                if (currentCharge >= chargeTime)
                {
                    isReady = true;
                    abilityButton.interactable = true; // Разблокируем кнопку
                }
            }
        }

        public void UseAbility(Transform spawnPoint)
        {
            if (!isReady) return;

            if (effectPrefab != null && spawnPoint != null)
            {
                Instantiate(effectPrefab, spawnPoint.position, Quaternion.identity);
            }

            ResetCharge(); // Сбрасываем заряд после использования
        }

        private void ResetCharge()
        {
            currentCharge = 0f;
            isReady = false;
            abilityButton.interactable = false; // Делаем кнопку неактивной
            abilitySlider.value = 0f;
        }
    }

    public List<UltimateAbilityData> abilities = new List<UltimateAbilityData>(); // Список способностей
    public BattleManager battleManager; // Ссылка на менеджер боя
    public Transform spawnPoint; // Точка появления эффекта
    public Canvas abilityCanvas; // Канвас с ультимативными кнопками

    void Start()
    {
        // Скрываем канвас с ультимативными кнопками при старте
        if (abilityCanvas != null)
        {
            abilityCanvas.gameObject.SetActive(false);
        }

        // Отключаем кнопки и сбрасываем слайдеры
        foreach (var ability in abilities)
        {
            if (ability.abilityButton != null)
                ability.abilityButton.interactable = false;

            if (ability.abilitySlider != null)
                ability.abilitySlider.value = 0f;
        }
    }

    void Update()
    {
        if (battleManager != null)
        {
            if (battleManager.battleStarted)
            {
                // Включаем канвас после начала боя
                if (abilityCanvas != null && !abilityCanvas.gameObject.activeSelf)
                {
                    abilityCanvas.gameObject.SetActive(true);
                }

                // Заполняем шкалу у каждой способности
                foreach (var ability in abilities)
                {
                    ability.UpdateCharge();
                }
            }
            else
            {
                // Если бой завершился, скрываем канвас
                if (abilityCanvas != null && abilityCanvas.gameObject.activeSelf)
                {
                    abilityCanvas.gameObject.SetActive(false);
                }
            }
        }
    }

    public void ActivateUltimate(int index)
    {
        if (index >= 0 && index < abilities.Count)
        {
            abilities[index].UseAbility(spawnPoint);
        }
    }
}
