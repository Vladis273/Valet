using UnityEngine;

/// <summary>
/// Интерфейс для объектов, которые могут получать урон
/// </summary>
public interface IDamageable
{
    /// <summary>
    /// Получить урон
    /// </summary>
    /// <param name="damage">Количество урона</param>
    /// <param name="hitPoint">Точка попадания в мировом пространстве</param>
    /// <param name="hitDirection">Направление удара</param>
    void TakeDamage(float damage, Vector3 hitPoint, Vector3 hitDirection);
    
    /// <summary>
    /// Есть ли объект жив
    /// </summary>
    bool IsAlive { get; }
    
    /// <summary>
    /// Текущее здоровье (опционально)
    /// </summary>
    float CurrentHealth { get; }
    
    /// <summary>
    /// Максимальное здоровье (опционально)
    /// </summary>
    float MaxHealth { get; }
}
