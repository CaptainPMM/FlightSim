namespace World.Lights {
    public enum LightType : byte {
        Undefined = 0,
        Nav,
        Beacon,
        Strobe,
        Taxi,
        Landing,
        Misc = 255
    }
}