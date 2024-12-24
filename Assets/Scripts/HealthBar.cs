using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider healthSlider; // Ссылка на слайдер здоровья
    private Camera mainCamera; // Ссылка на главную камеру

    private Transform characterTransform; // Ссылка на объект персонажа
    private Vector3 initialScale; // Начальный масштаб полоски
    private float heightOffset = 2f; // Высота размещения полоски здоровья (настраивается)

    /// <summary>
    /// Инициализация полоски здоровья.
    /// </summary>
    /// <param name="characterTransform">Трансформ персонажа.</param>
    /// <param name="heightOffset">Смещение по высоте для полоски здоровья.</param>
    public void Init(Transform characterTransform, float heightOffset)
    {
        this.characterTransform = characterTransform;
        this.heightOffset = heightOffset;
    }

    void Start()
    {
        // Находим главную камеру
        mainCamera = Camera.main;

        // Сохраняем начальный масштаб полоски
        initialScale = transform.localScale;

        // Если Init не был вызван, попробуем найти родителя
        if (characterTransform == null)
        {
            characterTransform = transform.parent;
        }

        // Открепляем полоску от родителя, чтобы масштаб не наследовался
        transform.SetParent(null);
    }

    void LateUpdate()
    {
        if (characterTransform == null)
        {
            // Если персонаж уничтожен, удаляем полоску здоровья
            Destroy(gameObject);
            return;
        }

        // Устанавливаем позицию полоски над персонажем
        transform.position = characterTransform.position + Vector3.up * heightOffset;

        // Сохраняем изначальный масштаб
        transform.localScale = initialScale;

        // Полоска всегда смотрит на камеру
        if (mainCamera != null)
        {
            transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                             mainCamera.transform.rotation * Vector3.up);
        }
    }

    /// <summary>
    /// Устанавливает текущее здоровье персонажа.
    /// </summary>
    public void SetHealth(float currentHealth, float maxHealth)
    {
        if (healthSlider != null)
        {
            healthSlider.value = Mathf.Clamp01(currentHealth / maxHealth);
        }
    }
}
