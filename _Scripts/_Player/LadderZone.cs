using UnityEngine;
/*
    Зона лестницы. Должна иметь триггер-коллайдер.
    Игрок, входя в зону, получает возможность "зацепиться" за лестницу по кнопке.
*/
public class LadderZone : MonoBehaviour
{
    [Header("Ladder Points")]
    [Tooltip("Нижняя точка лестницы (опорная позиция)")]
    public Transform bottomPoint;

    [Tooltip("Верхняя точка лестницы (опорная позиция)")]
    public Transform topPoint;

    [Header("Settings")]
    [Tooltip("Насколько вверх поднимаем игрока при входе на лестницу")]
    public float enterLiftHeight = 0.5f;

    public Vector3 GetLadderForward()
    {
        // Разворачиваем в противоположную сторону от transform.forward,
        // чтобы игрок смотрел "на лестницу", если лестница смотрит наружу.
        Vector3 f = -transform.forward;
        f.y = 0f;
        return f.sqrMagnitude > 0.0001f ? f.normalized : Vector3.forward;
    }

    private void OnTriggerEnter(Collider other)
    {
        var movement = other.GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.EnterLadderZone(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var movement = other.GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.ExitLadderZone(this);
        }
    }
}

