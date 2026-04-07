namespace Fallen_LE_Mods
{
    public static class BuildInfo
    {
        public const string Author = "FallenStar";

#if IMPROVED_TOOLTIPS
        public const string Name = "FallenStar's Improved Tooltips";
        public const string Version = "3.1.3";
        public const string MainClass = "MyMod";
#elif AUTO_ENABLER
        public const string Name = "FallenStar's Auto Enabler";
        public const string Version = "1.1.0";
        public const string MainClass = "MyMod";
#else
        public const string Name = "FallenStar's LE Mods";
        public const string Version = "6.6.6";
        public const string MainClass = "MyMod";
#endif
    }
}