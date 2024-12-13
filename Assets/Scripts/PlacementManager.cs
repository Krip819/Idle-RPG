using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlacementManager : MonoBehaviour
{
    [Header("References")]
    public Camera mainCamera;            // Главная камера
    public LayerMask groundLayer;        // Слой земли для размещения персонажей
    public BattleManager battleManager;  // Ссылка на BattleManager для запуска боя
    public Button startBattleButton;     // Кнопка "Начать бой"

    [Header("Placement Settings")]
    public float verticalOffset = 0f;    // Вертикальный оффсет для размещения персонажа

    [Header("Preview Settings")]
    public Material previewMaterial;     // Материал для объекта-превью (прозрачный)

    private GameObject selectedPrefab;   // Текущий выбранный префаб для размещения
    private GameObject previewObject;    // Объект-превью, который следует за курсором
    private bool isPlacing = false;      // Флаг режима размещения
    private bool hasPlacedCharacter = false; // Флаг: был ли размещён хотя бы один персонаж

    void Start()
    {
        // Убедимся, что кнопка "Начать бой" изначально выключена
        if (startBattleButton != null)
        {
            startBattleButton.interactable = false;
        }
    }

    /// <summary>
    /// Вызывается при нажатии на кнопку выбора персонажа.
    /// </summary>
    /// <param name="prefab">Префаб персонажа для размещения.</param>
    public void SelectPrefab(GameObject prefab)
    {
        if (isPlacing && previewObject != null)
        {
            Destroy(previewObject);
        }

        selectedPrefab = prefab;
        isPlacing = true;

        // Создаём объект-превью
        previewObject = Instantiate(prefab);
        previewObject.name = "Preview_" + prefab.name;

        // Устанавливаем масштаб превью в соответствии с оригинальным префабом
        previewObject.transform.localScale = prefab.transform.localScale;

        // Отключаем компоненты, которые не нужны для превью
        Collider previewCollider = previewObject.GetComponent<Collider>();
        if (previewCollider != null)
        {
            previewCollider.enabled = false;
        }

        Rigidbody previewRigidbody = previewObject.GetComponent<Rigidbody>();
        if (previewRigidbody != null)
        {
            previewRigidbody.isKinematic = true;
            previewRigidbody.useGravity = false;
        }

        // Применяем отдельный прозрачный материал для превью
        Renderer[] renderers = previewObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in renderers)
        {
            Material[] mats = rend.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i] = new Material(previewMaterial);
                mats[i].color = new Color(mats[i].color.r, mats[i].color.g, mats[i].color.b, 0.5f);
            }
            rend.materials = mats;
        }

        // Отключаем кнопку, с которой был произведён выбор
        GameObject currentButton = EventSystem.current.currentSelectedGameObject;
        if (currentButton != null)
        {
            Button button = currentButton.GetComponent<Button>();
            if (button != null)
            {
                button.interactable = false;
            }
        }
    }

    void Update()
    {
        if (isPlacing && previewObject != null)
        {
            UpdatePreviewPosition();

            if (Input.GetMouseButtonDown(0) && !IsPointerOverUI())
            {
                PlaceCharacter();
            }

            if (Input.GetMouseButtonDown(1))
            {
                CancelPlacement();
            }
        }
    }

    /// <summary>
    /// Обновляет позицию объекта-превью, следуя за курсором мыши.
    /// </summary>
    private void UpdatePreviewPosition()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            float characterHeight = GetCharacterHeight(selectedPrefab);

            // Устанавливаем позицию превью с учётом высоты и вертикального оффсета
            previewObject.transform.position = hit.point + Vector3.up * (characterHeight / 2f + verticalOffset);

            // Гарантируем, что объект-превью видим
            if (!previewObject.activeSelf)
            {
                previewObject.SetActive(true);
            }
        }
        else
        {
            // Скрываем объект-превью, если курсор не над землёй
            if (previewObject.activeSelf)
            {
                previewObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Размещает персонажа на поле.
    /// </summary>
    private void PlaceCharacter()
    {
        if (previewObject == null || selectedPrefab == null) return;

        GameObject placedObject = Instantiate(selectedPrefab, previewObject.transform.position, Quaternion.identity);

        // Устанавливаем масштаб окончательного объекта в соответствии с оригинальным префабом
        placedObject.transform.localScale = selectedPrefab.transform.localScale;

        placedObject.name = selectedPrefab.name;

        Collider placedCollider = placedObject.GetComponent<Collider>();
        if (placedCollider != null)
        {
            placedCollider.enabled = true;
        }

        Rigidbody placedRigidbody = placedObject.GetComponent<Rigidbody>();
        if (placedRigidbody != null)
        {
            placedRigidbody.isKinematic = false;
            placedRigidbody.useGravity = true;
        }

        // Устанавливаем флаг, что персонаж был размещён
        hasPlacedCharacter = true;
        UpdateStartBattleButton();

        Destroy(previewObject);
        previewObject = null;
        selectedPrefab = null;
        isPlacing = false;
    }

    private void CancelPlacement()
    {
        if (previewObject != null)
        {
            Destroy(previewObject);
            previewObject = null;
        }

        selectedPrefab = null;
        isPlacing = false;
    }

    private float GetCharacterHeight(GameObject prefab)
    {
        Collider col = prefab.GetComponent<Collider>();
        if (col != null)
        {
            return col.bounds.size.y;
        }
        else
        {
            return 2f;
        }
    }

    private bool IsPointerOverUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }

    /// <summary>
    /// Активирует кнопку "Начать бой", если хотя бы один персонаж был размещён.
    /// </summary>
    private void UpdateStartBattleButton()
    {
        if (startBattleButton != null)
        {
            startBattleButton.interactable = hasPlacedCharacter;
        }
    }

    public void StartBattle()
    {
        if (battleManager != null)
        {
            battleManager.StartBattle();
        }

        if (isPlacing)
        {
            CancelPlacement();
        }
    }
}
