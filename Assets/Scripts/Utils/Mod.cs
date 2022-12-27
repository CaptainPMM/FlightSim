namespace Utils {
    public static class Mod {
        public static int Calc(int x, int mod) {
            return (x % mod + mod) % mod;
        }

        public static float Calc(float x, float mod) {
            return (x % mod + mod) % mod;
        }
    }
}