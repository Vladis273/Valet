public interface IPlayerStateProvider
{
    bool IsGrounded();
    bool IsRunning();
    bool IsMoving();
}
