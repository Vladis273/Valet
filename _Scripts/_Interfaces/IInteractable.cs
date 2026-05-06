using UnityEngine;

/// <summary>
/// Интерфейс для объектов, с которыми можно взаимодействовать
/// </summary>
public interface IInteractable
{
    /// <summary>
    /// Получить подсказку взаимодействия
    /// </summary>
    string GetInteractionPrompt();
    
    /// <summary>
    /// Выполнить взаимодействие
    /// </summary>
    void OnInteract(PlayerInteractor interactor);
}