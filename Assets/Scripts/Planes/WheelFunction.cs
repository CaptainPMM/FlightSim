namespace Planes {
    [System.Flags]
    public enum WheelFunction : byte {
        None = 0,
        Brake = 1 << 0,
        Steer = 1 << 1,
        Torque = 1 << 2
    }
}