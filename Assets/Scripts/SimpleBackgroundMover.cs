using UnityEngine;

public class SimpleBackgroundMover : MonoBehaviour
{
    [Header("Настройки движения")]
    public Vector3 direction = new Vector3(-1, 0, 0); // Направление движения (влево по умолчанию)
    public float speed = 1f; // Скорость движения

    void Update()
    {
        // Движение объекта
        transform.Translate(direction.normalized * speed * Time.deltaTime);
    }
}
