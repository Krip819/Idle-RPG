using UnityEngine;
using System.Collections.Generic;

public class BattleManager : MonoBehaviour
{
    public bool battleStarted = false;
    public Canvas startCanvas; // Ссылка на стартовый канвас, назначается в инспекторе

    private List<CharacterController> allCharacters = new List<CharacterController>();
    private List<CharacterController> allies = new List<CharacterController>();
    private List<CharacterController> enemies = new List<CharacterController>();

    void Update()
    {
        // Проверяем нажатие клавиши пробела для запуска боя
        if (!battleStarted && Input.GetKeyDown(KeyCode.Space))
        {
            StartBattle();
        }
    }

    /// <summary>
    /// Запускает бой, собирает всех персонажей и инициализирует их.
    /// </summary>
    public void StartBattle()
    {
        if (battleStarted) return;
        battleStarted = true;

        // Скрываем стартовый канвас
        if (startCanvas != null)
            startCanvas.gameObject.SetActive(false);

        // Находим всех персонажей в сцене
        allCharacters.Clear();
        allCharacters.AddRange(FindObjectsOfType<CharacterController>());

        // Разделяем на команды
        allies.Clear();
        enemies.Clear();
        foreach (var c in allCharacters)
        {
            if (c == null) continue;
            if (c.team == CharacterTeam.Allies)
                allies.Add(c);
            else
                enemies.Add(c);
        }

        // Передаем списки персонажам
        foreach (var ally in allies)
        {
            ally.Init(this, allies, enemies);
        }
        foreach (var enemy in enemies)
        {
            enemy.Init(this, enemies, allies);
        }
    }

    /// <summary>
    /// Удаляет персонажа из списка после его смерти.
    /// </summary>
    /// <param name="character">Персонаж, который умер.</param>
    public void RemoveCharacter(CharacterController character)
    {
        // Если персонаж погиб - удаляем его из списка
        if (character.team == CharacterTeam.Allies)
        {
            allies.Remove(character);
        }
        else
        {
            enemies.Remove(character);
        }

        // Проверяем победу одной из сторон
        if (allies.Count == 0 || enemies.Count == 0)
        {
            battleStarted = false;
            Debug.Log(allies.Count == 0 ? "Enemies Win!" : "Allies Win!");
        }
    }
}
