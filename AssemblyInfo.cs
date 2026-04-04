using System.Reflection;
using Fallen_LE_Mods;
using MelonLoader;

#if IMPROVED_TOOLTIPS
[assembly: MelonInfo(typeof(MyMod), "FallenStar's Improved Tooltips", "3.1.2", "FallenStar")]
#elif AUTO_ENABLER
[assembly: MelonInfo(typeof(MyMod), "FallenStar's Auto Enabler", "1.0.0", "FallenStar")]
#else
[assembly: MelonInfo(typeof(MyMod), "FallenStar's LE Mods", "1.0.0", "FallenStar")]
#endif