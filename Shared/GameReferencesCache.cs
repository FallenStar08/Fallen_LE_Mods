using HarmonyLib;
using Il2Cpp;
using Il2CppItemFiltering;
using Il2CppLE.Factions;
using Il2CppLE.UI;
using Il2CppSystem.Linq;


namespace Fallen_LE_Mods.Shared
{
    public interface ILazyRef
    {
        void Reset();
        void Prewarm();
    }
    public class LazyRef<T> : ILazyRef where T : class
    {
        private T? _value;
        private readonly System.Func<T?> _fetcher;

        public LazyRef(System.Func<T?> fetcher)
        {
            _fetcher = fetcher;
            GameReferencesCache.Register(this);
        }

        public T? Value
        {
            get
            {
                _value ??= _fetcher();
                return _value;
            }
        }

        public void Reset()
        {
            _value = null;
        }

        public void Prewarm()
        {
            _ = Value;
        }
    }

    [HarmonyPatch(typeof(LoadingScreen), "Disable")]
    public class GameReferencesCache
    {
        private static readonly List<ILazyRef> _registry = new();
        public static void Register(ILazyRef lazyRef)
        {
            _registry.Add(lazyRef);
        }

        public static readonly LazyRef<Actor> Player = new(() => PlayerFinder.getPlayerActor());
        public static readonly LazyRef<ItemFilterManager> ItemFilterManager = new(() => FallenUtils.GetFilterManager);
        public static readonly LazyRef<ActorVisuals> PlayerVisuals = new(() => PlayerFinder.getPlayerVisuals());
        public static readonly LazyRef<ItemContainersManager> ItemContainersManager = new(() => Il2Cpp.ItemContainersManager.Instance);
        public static readonly LazyRef<UIBase> GameUiBase = new(() => UIBase.instance);
        public static readonly LazyRef<Il2CppLE.Data.CharacterData> PlayerData = new(() => PlayerFinder.getPlayerData());
        public static readonly LazyRef<CharacterDataTracker> PlayerDataTracker = new(() => PlayerFinder.getPlayerDataTracker());
        public static readonly LazyRef<ExperienceTracker> ExpTracker = new(() => PlayerFinder.getExperienceTracker());
        public static readonly LazyRef<GoldTracker> GoldTracker = new(() => PlayerFinder.getLocalGoldTracker());
        public static readonly LazyRef<AncientBonesTracker> BoneTracker = new(() => PlayerFinder.getAncientBonesTracker());

        public static readonly LazyRef<Il2CppSystem.Collections.Generic.List<ItemContainer>> PlayerStash = new(() =>
            StashTabbedUIControls.instance?.container?.containers);

        public static readonly LazyRef<Faction> CircleOfFortune = new(() =>
        {
            var p = Player.Value;

            if (p == null)
            {
                FallenUtils.Error("Player is null? We're kinda VERY cooked here");
                return null;
            }
            if (p?.factionInfo == null)
            {
                FallenUtils.Error("p.factionInfo is null? We're kinda cooked here");
                return null;
            }

            var factions = p.factionInfo.GetFactions();

            var enumValues = Enum.GetValues(typeof(FactionID));
            foreach (FactionID v in enumValues)
            {
                FallenUtils.Log($"Faction : {v} State : {(p.FactionInfo.IsMemberOf(v) ? "joined" : "not joined")}");
            }


            if (factions == null)
            {
                FallenUtils.Error("No faction in player.factionInfo");
                return null;
            }

            for (int i = 0; i < factions.Count(); i++)
            {

                var f = factions.ElementAt(i);
                if (f != null && f.ID == FactionID.CircleOfFortune)
                {
                    return f;
                }
            }

            return null;
        });

        public static readonly LazyRef<CraftingManager> CraftingManager = new(() =>
            Il2Cpp.ItemContainersManager.Instance?.craftingManager);

        public static readonly LazyRef<MaterialContainers> MaterialContainers = new(() =>
            Il2Cpp.ItemContainersManager.Instance?.materials);

        [HarmonyPostfix]
        public static void Postfix()
        {
            foreach (var lazyRef in _registry)
            {
                lazyRef.Reset();
                lazyRef.Prewarm();
            }
        }
    }
}