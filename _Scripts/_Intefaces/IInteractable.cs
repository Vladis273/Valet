public interface IInteractable
{
    string GetInteractionPrompt();
    void OnInteract(PlayerInteractor interactor);
}