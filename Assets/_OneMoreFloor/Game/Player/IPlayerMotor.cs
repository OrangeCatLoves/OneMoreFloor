namespace OneMoreFloor.Game
{
    /// <summary>
    /// Moves a player's character. Everything else talks to movement through this, so PvP can later
    /// swap in host-authoritative movement without changing the rest of the game.
    /// </summary>
    public interface IPlayerMotor
    {
        /// <summary>Metres per second along the ground, measured from how far the character really moved.</summary>
        float Speed { get; }

        bool IsGrounded { get; }

        /// <summary>True only during the tick a jump started.</summary>
        bool JumpedThisTick { get; }

        void Tick(MoveCommand command, float deltaTime);
    }
}
