using UnityEngine;

public class GameSpeedController : MonoBehaviour
{
    [Range(0.1f, 3f)]
    public float gameSpeed = 1f; // Поле для управления скоростью в инспекторе

    void Update()
    {
        Time.timeScale = gameSpeed; // Устанавливаем скорость игры
    }

    private void OnValidate()
    {
        // Обновляем Time.timeScale при изменении значения в инспекторе
        Time.timeScale = gameSpeed;
    }
}
