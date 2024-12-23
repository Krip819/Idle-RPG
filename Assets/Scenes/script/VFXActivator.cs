using System.Collections;
using UnityEngine;

[System.Serializable] // Делает структуру видимой в инспекторе Unity
public struct VFXObject
{
    public GameObject vfx; // Ссылка на объект VFX
    public float delay; // Задержка перед активацией VFX
    public float duration; // Продолжительность активности VFX
}

public class VFXActivator : MonoBehaviour
{
    public VFXObject[] vfxObjects; // Массив объектов VFX с задержками и продолжительностями

    void Update()
    {
        // Проверяем, была ли нажата левая кнопка мыши
        if (Input.GetMouseButtonDown(0)) // 0 означает левую кнопку мыши
        {
            // Проходим по каждому объекту VFX в списке
            foreach (var vfxObj in vfxObjects)
            {
                // Запускаем корутину для активации VFX с задержкой и отключения после проигрывания
                StartCoroutine(ActivateVFXWithDelay(vfxObj.vfx, vfxObj.delay, vfxObj.duration));
            }
        }
    }

    IEnumerator ActivateVFXWithDelay(GameObject vfx, float delay, float duration)
    {
        // Ждем указанное время задержки
        yield return new WaitForSeconds(delay);

        // Активируем VFX
        if (vfx != null)
        {
            vfx.SetActive(true);

            // Ожидаем продолжительность активности VFX
            yield return new WaitForSeconds(duration);

            // Выключаем VFX
            vfx.SetActive(false);
        }
    }
}
