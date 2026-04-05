#if RELEASE
using Fallen_LE_Mods.MonoScripts;
using Fallen_LE_Mods.Shared;
using HarmonyLib;
using Il2CppLE.UI;
using Il2CppRewired.Utils;
using Il2CppTMPro;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;

namespace Fallen_LE_Mods.Dev
{
    [HarmonyPatch(typeof(LoadingScreen), "Disable")]
    public class InfoCorner : MelonMod
    {
        private static TextMeshProUGUI? text;
        private static bool initialized = false;
        private static GameObject? rectangleObject;
        private static bool isVisible = true;
        private const KeyCode ToggleKey = KeyCode.F1;


        public static void Postfix(ref LoadingScreen __instance)
        {
            if (!initialized) { Init(); }
        }

        public static void ModifyCornerInfoText(string content)
        {
            if (text != null)
            {
                text.text = content;
                ResizeBackgroundToText();
            }
        }

        private static void Init()
        {
            GameObject canvas = GameObject.Find("Canvas");
            GameObject questTitle = GameObject.Find("QuestName");
            TMP_FontAsset? questFont = null;
            if (!questTitle.IsNullOrDestroyed())
            {
                var questText = questTitle.GetComponent<TextMeshProUGUI>();
                questFont = questText.font;
            }

            if (!canvas.IsNullOrDestroyed())
            {
                Melon<MyMod>.Logger.Msg("Version object found!");
                // Make our bg & txt holding compo
                GameObject textObject = new("Fallen_CornerInfoText")
                {
                    layer = LayerMask.NameToLayer("UI")
                };

                rectangleObject = new GameObject("Fallen_BlackBackground")
                {
                    layer = LayerMask.NameToLayer("UI")
                };

                Image image = rectangleObject.AddComponent<Image>();

                image.color = new Color(0, 0, 0, 0.5f);

                RectTransform backgroundRectTransform = rectangleObject.GetComponent<RectTransform>();
                rectangleObject.transform.SetParent(canvas.transform);
                FallenUtils.SetDefaultRectTransformProperties(backgroundRectTransform);
                backgroundRectTransform.sizeDelta = new Vector2(200, 200);

                textObject.transform.SetParent(rectangleObject.transform);
                text = textObject.AddComponent<TextMeshProUGUI>();
                Melon<MyMod>.Logger.Msg("TextMeshPro Created");
                text.fontSize = 16;
                if (!questFont.IsNullOrDestroyed())
                {
                    text.font = questFont;
                }

                var textRectTransform = textObject.GetComponent<RectTransform>();
                FallenUtils.SetDefaultRectTransformProperties(textRectTransform);
                textObject.transform.localPosition = new Vector3(10, -30, 0);
                GameObject updaterObj = new("Fallen_CornerUpdater");
                updaterObj.transform.SetParent(canvas.transform);
                var updater = updaterObj.AddComponent<CornerUpdater>();
                updater.textToUpdate = text;
                updater.backgroundObject = rectangleObject;

                initialized = true;
            }
            else
            {
                Melon<MyMod>.Logger.Msg("Canvas object not found.");
            }
        }

        public static void ResizeBackgroundToText()
        {
            if (text != null && rectangleObject != null)
            {
                Vector2 textSize = text.GetPreferredValues();

                RectTransform backgroundRectTransform = rectangleObject.GetComponent<RectTransform>();
                backgroundRectTransform.sizeDelta = new Vector2(textSize.x + 20, textSize.y + 50);
            }
        }

        private static void ToggleVisibility()
        {
            if (rectangleObject != null)
            {
                isVisible = !isVisible;
                rectangleObject.SetActive(isVisible);
                text?.gameObject.SetActive(isVisible);
            }
        }

        public override void OnUpdate()
        {
            if (Input.GetKeyDown(ToggleKey))
            {
                ToggleVisibility();
            }
        }
    }
}
#endif