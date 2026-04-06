using System.Collections;
using Il2Cpp;
using Il2CppRewired.Utils;
using Il2CppTMPro;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using static Fallen_LE_Mods.Shared.FallenUtils;

namespace Fallen_LE_Mods.Shared
{
    public static class FallenUI
    {
        private static object? _saveCoroutine;

        public static void DebouncedSave(MelonPreferences_Category category, float delay = 0.5f)
        {
            if (_saveCoroutine != null)
            {
                MelonCoroutines.Stop(_saveCoroutine);
            }

            _saveCoroutine = MelonCoroutines.Start(DelayedSave(delay, category));
        }

        private static IEnumerator DelayedSave(float delay, MelonPreferences_Category category)
        {
            yield return new WaitForSeconds(delay);

            if (category != null)
            {
                category.SaveToFile();
                LogDebug("[Config] Debounced Save Executed.");
            }

            _saveCoroutine = null;
        }
        public static GameObject? CreateHeader(Transform parent, string title, string objectName)
        {
            Transform original = parent.Find("Header-Social");
            if (original == null) return null;

            GameObject header = UnityEngine.Object.Instantiate(original.gameObject, parent);
            header.name = $"FallenHeader_{objectName}";

            //Cleanup Localization
            var textObj = header.GetComponentInChildren<TextMeshProUGUI>()?.gameObject;
            if (textObj != null)
            {
                foreach (var comp in textObj.GetComponents<MonoBehaviour>())
                    if (comp.GetIl2CppType().FullName.Contains("Localize")) UnityEngine.Object.Destroy(comp);

                var textComp = textObj.GetComponent<TextMeshProUGUI>();
                textComp.text = title.ToUpper();
                textComp.color = new Color(0.1f, 0.8f, 1f, 1f);
            }
            return header;
        }

        public static GameObject? CreateToggle(Transform parent, string labelText, string sublabelText, MelonPreferences_Entry<bool> pref)
        {
            //EHG can't spell "Toggle" consistently in their UI :P
            //Surely this one toggle won't change its name across updates :copium:
            Transform original = parent.Find("Toogle - Profanity Filter");
            if (original == null) return null;

            GameObject toggleGo = UnityEngine.Object.Instantiate(original.gameObject, parent);
            toggleGo.name = $"FallenToggle_{pref.Identifier}";

            foreach (var script in toggleGo.GetComponentsInChildren<MonoBehaviour>(true))
            {
                string fName = script.GetIl2CppType().FullName;
                if (fName.Contains("Settings") || fName.Contains("Localize")) UnityEngine.Object.Destroy(script);
            }

            var label = toggleGo.transform.Find("Input Labels/Label")?.GetComponent<TextMeshProUGUI>();
            var subLabel = toggleGo.transform.Find("Input Labels/Sublabel")?.GetComponent<TextMeshProUGUI>();
            if (label != null) label.text = labelText;
            if (subLabel != null) subLabel.text = sublabelText;

            var toggleComp = toggleGo.GetComponentInChildren<Toggle>();
            if (toggleComp != null)
            {
                toggleComp.onValueChanged.RemoveAllListeners();
                toggleComp.isOn = pref.Value;

                toggleComp.onValueChanged.AddListener(new Action<bool>((val) =>
                {
                    pref.Value = toggleComp.isOn;
                    pref.Category.SaveToFile();
                    Log($"[Config] {pref.Identifier} set to: {pref.Value}");
                }));
            }
            return toggleGo;
        }

        private static IEnumerator DelaySetText(TMP_InputField field, string val)
        {
            yield return new WaitForEndOfFrame();
            LogDebug($"[UI Hijack] Delayed text set for {field.gameObject.name} to {val}");
            field?.SetTextWithoutNotify(val);
        }

        public static GameObject? CreateSlider(Transform parent, string labelText, string sublabelText, float minValue, float maxValue, MelonPreferences_Entry<float> pref)
        {

            //Source slider from master volume
            GameObject soundContainer = GameObject.Find("GUI/Panel System/Panel Stacks/Left Panel Stack/Settings Panel(Clone)/Content/Sound/Viewport/Sound-container");
            if (soundContainer.IsNullOrDestroyed())
            {
                LogDebug("[UI Hijack] Sound-container not found");
                return null;

            }

            Transform original = soundContainer.transform.Find("Slider - Master");
            if (original.IsNullOrDestroyed())
            {
                LogDebug("[UI Hijack] Original Slider not found");
                return null;
            }

            GameObject sliderGo = UnityEngine.Object.Instantiate(original.gameObject, parent);
            sliderGo.name = $"FallenSlider_{pref.Identifier}";

            //clean scripts
            foreach (var script in sliderGo.GetComponentsInChildren<MonoBehaviour>(true))
            {
                string fName = script.GetIl2CppType().FullName;
                if (fName.Contains("Settings") || fName.Contains("Localize") || fName.Contains("Sound"))
                    UnityEngine.Object.Destroy(script);
            }

            var label = sliderGo.transform.Find("Input Labels/Label")?.GetComponent<TextMeshProUGUI>();
            var subLabel = sliderGo.transform.Find("Input Labels/Sublabel")?.GetComponent<TextMeshProUGUI>();
            label?.SetText(labelText);
            if (subLabel.IsNotNullOrDestroyed())
            {
                subLabel!.text = sublabelText;
                subLabel.gameObject.SetActive(true);
            }

            //Setup Sliders
            var sliderComp = sliderGo.GetComponentInChildren<Slider>();
            var sliderInput = sliderGo.GetComponentInChildren<SliderInput>();

            var inputField = sliderGo.GetComponentInChildren<TMP_InputField>();
            inputField.contentType = TMP_InputField.ContentType.Standard;
            inputField.characterValidation = TMP_InputField.CharacterValidation.None;
            //This is 100% useless
            inputField.onEndEdit.RemoveAllListeners();
            inputField.onSelect.RemoveAllListeners();
            inputField.onDeselect.RemoveAllListeners();
            inputField.onTextSelection.RemoveAllListeners();
            inputField.onEndTextSelection.RemoveAllListeners();
            inputField.onSubmit.RemoveAllListeners();
            inputField.keyboardType = TouchScreenKeyboardType.DecimalPad;


            // cringe Il2cpp slider subcomponent we may need
            var numberFloatInput = sliderGo.GetComponentInChildren<NumberFloatInput>();

            string cleanStartValue = pref.Value.ToString("F1", System.Globalization.CultureInfo.InvariantCulture);
            inputField?.onEndEdit.AddListener(new System.Action<string>((text) =>
            {
                if (string.IsNullOrEmpty(text)) return;

                string cleanText = text.Replace(',', '.');

                if (float.TryParse(cleanText, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float val))
                {
                    val = Mathf.Clamp(val, minValue, maxValue);
                    val = (float)System.Math.Round(val, 1);

                    sliderComp?.Set(val, false);

                    if (pref.Value != val)
                    {
                        pref.Value = val;
                        DebouncedSave(pref.Category);
                        LogDebug($"[Config] (field) Distance value changed to {val}");
                    }

                    inputField.SetTextWithoutNotify(val.ToString("F1", System.Globalization.CultureInfo.InvariantCulture));
                }
            }));

            if (sliderComp != null)
            {
                sliderComp.minValue = minValue;
                sliderComp.maxValue = maxValue;
                sliderComp.wholeNumbers = false;
                sliderComp.value = pref.Value;
                if (sliderInput != null)
                {
                    sliderInput.MinValue = minValue;
                    sliderInput.MaxValue = maxValue;
                    sliderInput.Value = pref.Value;
                }

                sliderComp.onValueChanged.RemoveAllListeners();
                sliderComp.onValueChanged.AddListener(new Action<float>((discard) =>
                {
                    float rounded = (float)Math.Round(sliderComp.value, 1);
                    inputField?.SetTextWithoutNotify(rounded.ToString("F1", System.Globalization.CultureInfo.InvariantCulture));

                    if (pref.Value != rounded)
                    {
                        pref.Value = rounded;
                        DebouncedSave(pref.Category);
                        LogDebug($"[CONFIG] (slider) Distance value changed to {rounded}");
                    }

                }));


            }
            //Idk bro the constructor of this shit formats the field as a percentage so we just do be hacky hacking
            //I should have copied another slider but too late
            if (inputField.IsNotNullOrDestroyed())
            {
                //force this retarded input field to our initial value
                //otherwise its init script round to int and add "%" at the end until updated

                //Set values immediately to try and beat the frame (we don't)
                numberFloatInput?.SetInputFromString(cleanStartValue);
                inputField!.text = cleanStartValue;

                //This actually cleans up the field, who would've thought
                MelonCoroutines.Start(DelaySetText(inputField, cleanStartValue));
            }
            return sliderGo;
        }
    }
}
