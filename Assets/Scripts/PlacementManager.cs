using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlacementManager : MonoBehaviour
{
    [Header("References")]
    public Camera mainCamera;
    public LayerMask groundLayer;
    public BattleManager battleManager;
    public Button startBattleButton;

    [Header("Placement Settings")]
    public float verticalOffset = 0f;
    public Vector3 characterRotation = Vector3.zero;

    [Header("Preview Settings")]
    public Material previewMaterial;

    [Header("UI Buttons")]
    public List<Button> characterButtons; // Все кнопки выбора персонажей

    private GameObject selectedPrefab;
    private GameObject previewObject;
    private bool isPlacing = false;
    private bool hasPlacedCharacter = false;
    private Button lastSelectedButton; // Последняя нажатая кнопка

    void Start()
    {
        if (startBattleButton != null)
            startBattleButton.interactable = false;
    }

    public void SelectPrefab(GameObject prefab)
    {
        if (isPlacing && previewObject != null)
        {
            Destroy(previewObject);
        }

        selectedPrefab = prefab;
        isPlacing = true;

        // Получаем кнопку, которая вызвала метод
        GameObject clickedButton = EventSystem.current.currentSelectedGameObject;
        if (clickedButton != null)
        {
            lastSelectedButton = clickedButton.GetComponent<Button>();
        }

        previewObject = Instantiate(prefab);
        previewObject.name = "Preview_" + prefab.name;

        Collider previewCollider = previewObject.GetComponent<Collider>();
        if (previewCollider != null)
            previewCollider.enabled = false;

        Rigidbody previewRigidbody = previewObject.GetComponent<Rigidbody>();
        if (previewRigidbody != null)
        {
            previewRigidbody.isKinematic = true;
            previewRigidbody.useGravity = false;
        }

        ApplyPreviewMaterial(previewObject);
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

    private void UpdatePreviewPosition()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            float characterHeight = GetCharacterHeight(selectedPrefab);
            previewObject.transform.position = hit.point + Vector3.up * (characterHeight / 2f + verticalOffset);
            previewObject.transform.rotation = Quaternion.Euler(characterRotation);

            if (!previewObject.activeSelf)
                previewObject.SetActive(true);
        }
        else
        {
            if (previewObject.activeSelf)
                previewObject.SetActive(false);
        }
    }

    private void PlaceCharacter()
    {
        if (previewObject == null || selectedPrefab == null) return;

        GameObject placedObject = Instantiate(selectedPrefab, previewObject.transform.position, Quaternion.Euler(characterRotation));
        placedObject.name = selectedPrefab.name;

        Collider placedCollider = placedObject.GetComponent<Collider>();
        if (placedCollider != null)
            placedCollider.enabled = true;

        Rigidbody placedRigidbody = placedObject.GetComponent<Rigidbody>();
        if (placedRigidbody != null)
        {
            placedRigidbody.isKinematic = false;
            placedRigidbody.useGravity = true;
        }

        hasPlacedCharacter = true;
        UpdateStartBattleButton();

        if (lastSelectedButton != null)
        {
            DisableButtonAfterPlacement(lastSelectedButton);
        }

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
        return col != null ? col.bounds.size.y : 2f;
    }

    private bool IsPointerOverUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }

    private void UpdateStartBattleButton()
    {
        if (startBattleButton != null)
            startBattleButton.interactable = hasPlacedCharacter;
    }

    private void ApplyPreviewMaterial(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
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
    }

    private void DisableButtonAfterPlacement(Button button)
    {
        button.interactable = false;
        ColorBlock colors = button.colors;
        colors.disabledColor = Color.gray;
        button.colors = colors;
    }
}
