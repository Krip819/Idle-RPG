using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Image fillImage; // Ссылка на изображение полоски здоровья
    private Camera mainCamera; // Ссылка на главную камеру

    private Transform characterTransform; // Ссылка на объект персонажа
    private Vector3 initialScale; // Начальный масштаб полоски

    void Start()
    {
        // Находим главную камеру
        mainCamera = Camera.main;

        // Сохраняем начальный масштаб полоски
        initialScale = transform.localScale;

        // Сохраняем ссылку на родителя, чтобы получить позицию персонажа
        characterTransform = transform.parent;

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
        transform.position = characterTransform.position + Vector3.up * 2f; // Регулируйте высоту при необходимости

        // Сохраняем горизонтальное выравнивание полоски
        transform.rotation = Quaternion.Euler(0, 0, 0);

        // Сохраняем изначальный масштаб
        transform.localScale = initialScale;
    }

    /// <summary>
    /// Устанавливает текущее здоровье персонажа.
    /// </summary>
    public void SetHealth(float currentHealth, float maxHealth)
    {
        if (fillImage != null)
        {
            fillImage.fillAmount = Mathf.Clamp01(currentHealth / maxHealth);
            fillImage.color = Color.Lerp(Color.red, Color.green, fillImage.fillAmount); // Цвет от красного к зелёному
        }
    }
}
