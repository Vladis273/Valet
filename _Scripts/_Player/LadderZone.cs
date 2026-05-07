using UnityEngine;

/// <summary>
/// Зона лестницы. Должна иметь триггер-коллайдер.
/// Использует точки для точного позиционирования игрока при входе/выходе.
/// </summary>
public class LadderZone : MonoBehaviour
{
    [Header("Ladder Points")]
    [Tooltip("Нижняя точка входа на лестницу (позиция игрока при старте подъёма)")]
    public Transform bottomEnterPoint;
    
    [Tooltip("Верхняя точка выхода с лестницы (позиция игрока при завершении подъёма)")]
    public Transform topExitPoint;
    
    [Tooltip("Направление взгляда игрока при подъёме (опционально, если не задано - используется forward лестницы)")]
    public Transform lookDirectionPoint;

    [Header("Settings")]
    [Tooltip("Дистанция от центра лестницы, на которой игрок может начать подъём")]
    public float enterRange = 2f;
    
    [Tooltip("Скорость подъёма/спуска по лестнице")]
    public float climbSpeed = 2f;
    
    [Tooltip("Время анимации перехода на лестницу")]
    public float transitionDuration = 0.3f;

    /// <summary>
    /// Получить точку входа на лестницу
    /// </summary>
    public Vector3 GetEnterPosition()
    {
        return bottomEnterPoint != null ? bottomEnterPoint.position : transform.position + Vector3.up * 0.5f;
    }

    /// <summary>
    /// Получить точку выхода с лестницы
    /// </summary>
    public Vector3 GetExitPosition()
    {
        return topExitPoint != null ? topExitPoint.position : transform.position + transform.up * 3f;
    }

    /// <summary>
    /// Получить направление взгляда игрока на лестнице
    /// </summary>
    public Vector3 GetClimbLookDirection()
    {
        if (lookDirectionPoint != null)
            return lookDirectionPoint.forward.normalized;
        
        // По умолчанию - противоположно forward лестницы (игрок смотрит на лестницу)
        Vector3 dir = -transform.forward;
        dir.y = 0f;
        return dir.sqrMagnitude > 0.0001f ? dir.normalized : Vector3.forward;
    }

    /// <summary>
    /// Проверить, может ли игрок войти на лестницу из текущей позиции
    /// </summary>
    public bool CanEnterFromPosition(Vector3 playerPosition, Vector3 playerForward)
    {
        float distanceToBottom = Vector3.Distance(playerPosition, GetEnterPosition());
        if (distanceToBottom > enterRange)
            return false;
        
        // Проверяем, смотрит ли игрок в сторону лестницы
        Vector3 directionToLadder = (GetEnterPosition() - playerPosition).normalized;
        float dotProduct = Vector3.Dot(playerForward.normalized, directionToLadder);
        
        // Игрок должен смотреть примерно в сторону лестницы (угол меньше 90 градусов)
        // Допускаем угол до 120 градусов для удобства
        return dotProduct > -0.5f;
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
        if (movement != null && movement.IsOnLadder())
        {
            // Игрок всё ещё на лестнице - не выходим автоматически
            // Выход происходит только через кнопку или достижение верха/низа
            return;
        }
        if (movement != null)
        {
            movement.ExitLadderZone(this);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        
        Vector3 bottom = GetEnterPosition();
        Vector3 top = GetExitPosition();
        
        // Рисуем линию лестницы
        Gizmos.DrawLine(bottom, top);
        Gizmos.DrawWireSphere(bottom, 0.3f);
        Gizmos.DrawWireSphere(top, 0.3f);
        
        // Рисуем направление взгляда
        Vector3 lookDir = GetClimbLookDirection();
        Vector3 midPoint = Vector3.Lerp(bottom, top, 0.5f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(midPoint, lookDir * 1.5f);
        
        // Рисуем радиус входа
        Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
        Gizmos.DrawWireSphere(bottom, enterRange);
    }
}

