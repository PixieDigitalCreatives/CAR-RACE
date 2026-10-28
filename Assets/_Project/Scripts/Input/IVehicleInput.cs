namespace ThegameioRacing.Input
{
    /// <summary>
    /// Decoupled input interface for vehicle control.
    /// Supports Player Keyboard, Mobile Touch UI, and AI Drivers seamlessly.
    /// </summary>
    public interface IVehicleInput
    {
        /// <summary>
        /// Steering input from -1.0 (full left) to +1.0 (full right).
        /// </summary>
        float SteerInput { get; }

        /// <summary>
        /// Throttle input from 0.0 (idle) to 1.0 (full acceleration).
        /// </summary>
        float ThrottleInput { get; }

        /// <summary>
        /// Brake / Reverse input from 0.0 to 1.0.
        /// </summary>
        float BrakeInput { get; }

        /// <summary>
        /// Handbrake status for drift and sharp rotation.
        /// </summary>
        bool HandbrakeInput { get; }

        /// <summary>
        /// Trigger to reset vehicle upright when flipped or stuck.
        /// </summary>
        bool ResetTriggered { get; }
    }
}
