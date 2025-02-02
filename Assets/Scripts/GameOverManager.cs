using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public Canvas gameOverCanvas; // Канвас экрана победы
    public BattleManager battleManager; // Ссылка на менеджер боя
    public string menuSceneName = "MainMenu"; // Имя сцены меню

    private bool hasGameStarted = false; // Проверяет, начался ли бой
    private bool gameOverShown = false; // Проверяет, показывали ли `Canvas` уже

    void Start()
    {
        // ✅ Гарантированно скрываем `Canvas` в начале игры
        if (gameOverCanvas != null)
        {
            gameOverCanvas.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (battleManager == null) return;

        // ⚡ Ждём, пока бой реально начнётся, прежде чем следить за его завершением
        if (battleManager.battleStarted)
        {
            hasGameStarted = true;
        }

        // ✅ Показываем `Canvas`, ТОЛЬКО если бой был начат и теперь завершился
        if (hasGameStarted && !battleManager.battleStarted && !gameOverShown)
        {
            ShowGameOverCanvas();
        }
    }

    void ShowGameOverCanvas()
    {
        if (gameOverCanvas != null)
        {
            gameOverCanvas.gameObject.SetActive(true); // 🎯 Включаем `Canvas`
            gameOverShown = true; // 🛑 Чтобы не включался повторно
        }
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene(menuSceneName); // 📌 Загружаем сцену меню
    }
}
