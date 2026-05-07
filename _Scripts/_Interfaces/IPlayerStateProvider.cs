public enum PlayerPose
{
    Stand,
    Crouch,
    AltCrouch,
    Prone
}

public interface IPlayerStateProvider
{
    bool IsGrounded();
    bool IsRunning();
    bool IsMoving();
    PlayerPose GetCurrentPose();
}
