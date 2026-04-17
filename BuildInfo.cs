namespace Fallen_LE_Mods
{
    public static class BuildInfo
    {
        public const string Author = "FallenStar";

#if IMPROVED_TOOLTIPS
        public const string Name = "FallenStar's Improved Tooltips";
        public const string Version = "3.2.1";
        public const string MainClass = "MyMod";
        public const string DownloadLink = "https://www.nexusmods.com/lastepoch/mods/10?tab=files";
#elif AUTO_ENABLER
        public const string Name = "FallenStar's Auto Enabler";
        public const string Version = "1.3.1";
        public const string MainClass = "MyMod";
        public const string DownloadLink = "https://www.nexusmods.com/lastepoch/mods/25?tab=files";
#elif IMPROVED_OBSERVATORY
        public const string Name = "FallenStar's Improved Observatory";
        public const string Version = "1.0.1";
        public const string MainClass = "MyMod";
        public const string DownloadLink = "https://www.nexusmods.com/games/lastepoch/mods/28?tab=files";
#else
        public const string Name = "FallenStar's LE Mods";
        public const string Version = "6.6.6";
        public const string MainClass = "MyMod";
        public const string DownloadLink = "https://github.com/FallenStar08/Fallen_LE_Mods";
#endif
    }
}