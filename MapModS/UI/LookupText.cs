﻿using MapModS.CanvasUtil;
using MapModS.Data;
using MapModS.Map;
using MapModS.Settings;
using System.Linq;
using UnityEngine;

namespace MapModS.UI
{
    internal class LookupText
    {
        public static GameObject Canvas;

        private static CanvasPanel _infoPanel;
        private static CanvasPanel _instructionPanel;

        private static string selectedLocation = "None";

        public static bool LookupActive()
        {
            return MapModS.LS.ModEnabled
                && (MapModS.LS.mapMode == MapMode.FullMap
                    || MapModS.LS.mapMode == MapMode.AllPins
                    || MapModS.LS.mapMode == MapMode.PinsOverMap);
        }

        public static void ShowWorldMap()
        {
            if (Canvas == null || GameManager.instance.gameMap == null || _infoPanel == null) return;

            if (!LookupActive() || TransitionText.TransitionModeActive())
            {
                Hide();
                return;
            }

            GameMap gameMap = GameManager.instance.gameMap.GetComponent<GameMap>();

            if (gameMap != null)
            {
                gameMap.panMinX = -29f;
                gameMap.panMaxX = 26f;
                gameMap.panMinY = -25f;
                gameMap.panMaxY = 20f;
            }

            Canvas.SetActive(true);

            _infoPanel.SetActive(MapModS.LS.lookupOn, MapModS.LS.lookupOn);
        }

        public static void Hide()
        {
            if (Canvas == null || _infoPanel == null) return;

            Canvas.SetActive(false);
        }

        public static void Initialize()
        {
            selectedLocation = "None";
        }

        public static void BuildText(GameObject _canvas)
        {
            Canvas = _canvas;
            //_infoPanel = new CanvasPanel
            //    (_canvas, GUIController.Instance.Images["ButtonsMenuBG"], new Vector2(10f, 20f), new Vector2(1200f, 0f), new Rect(0f, 0f, 0f, 0f));
            //_infoPanel.AddText("Info", "None", new Vector2(20f, 0f), Vector2.zero, GUIController.Instance.TrajanNormal, 14);

            //_infoPanel.SetActive(true, true);

            _instructionPanel = new CanvasPanel
                (_canvas, GUIController.Instance.Images["ButtonsMenuBG"], new Vector2(10f, 20f), new Vector2(1346f, 0f), new Rect(0f, 0f, 0f, 0f));
            _instructionPanel.AddText("Control", "", new Vector2(-37f, 0f), Vector2.zero, GUIController.Instance.TrajanNormal, 14, FontStyle.Normal, TextAnchor.UpperRight);

            _instructionPanel.SetActive(true, true);

            _infoPanel = new CanvasPanel
                (_canvas, GUIController.Instance.Images["LookupBG"], new Vector2(1200f, 200f), new Vector2(GUIController.Instance.Images["LookupBG"].width, GUIController.Instance.Images["LookupBG"].height), new Rect(0f, 0f, GUIController.Instance.Images["LookupBG"].width, GUIController.Instance.Images["LookupBG"].height));
            _infoPanel.AddText("Info", "None", new Vector2(5f, 30f), new Vector2(GUIController.Instance.Images["LookupBG"].width - 20f, GUIController.Instance.Images["LookupBG"].height), GUIController.Instance.Perpetua, 19);

            _infoPanel.SetActive(false, false);

            SetTexts();
        }

        public static void Update()
        {
            if (Canvas == null
                || HeroController.instance == null
                || WorldMap.CustomPins == null
                || !LookupActive()
                || TransitionText.TransitionModeActive()) return;

            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                && Input.GetKeyDown(KeyCode.L))
            {
                MapModS.LS.ToggleLookup();
                SetTexts();

                if (MapModS.LS.lookupOn)
                {
                    _infoPanel.SetActive(true, true);

                    UpdateSelectedPin();
                }
                else
                {
                    _infoPanel.SetActive(false, false);

                    WorldMap.CustomPins.ResizePins();
                }
            }
        }

        // Called every 0.1 seconds
        public static void UpdateSelectedPinCoroutine()
        {
            if (Canvas == null
                || WorldMap.CustomPins == null
                || !_infoPanel.Active
                || HeroController.instance == null
                || GameManager.instance.IsGamePaused()
                || !LookupActive()
                || !MapModS.LS.lookupOn)
            {
                return;
            }

            if (WorldMap.CustomPins.GetPinClosestToMiddle(selectedLocation, out selectedLocation))
            {
                UpdateSelectedPin();
            }
        }

        public static void UpdateSelectedPin()
        {
            WorldMap.CustomPins.UpdateSelectedPin(selectedLocation);
            SetTexts();
        }

        public static void SetTexts()
        {
            SetControlText();
            SetInstructionsText();
        }

        public static void SetControlText()
        {
            string controlText = "";

            if (MapModS.LS.lookupOn)
            {
                controlText += "Toggle lookup (Ctrl-L): On";
            }
            else
            {
                controlText += "Toggle lookup (Ctrl-L): Off";
            }

            _instructionPanel.GetText("Control").UpdateText(controlText);
        }

        public static void SetInstructionsText()
        {
            string instructionsText = $"{selectedLocation}";

            PinDef pd = DataLoader.GetUsedPinDef(selectedLocation);

            if (pd != null)
            {
                instructionsText += $"\n\nRoom: {pd.sceneName}";
                instructionsText += $"\n\nLocation Pool: {StringUtils.ToCleanGroup(pd.locationPoolGroup)}";

                if (DataLoader.IsInLogicLookup(selectedLocation))
                {
                    instructionsText += $"\n\nLogic: {DataLoader.GetRawLogic(selectedLocation)}";
                }

                if (MapModS.LS.SpoilerOn && pd.randoItems != null && pd.randoItems.Any())
                {
                    instructionsText += $"\n\nSpoiler item(s):";

                    foreach (ItemDef item in pd.randoItems)
                    {
                        instructionsText += $" {item.itemName},";
                    }

                    instructionsText = instructionsText.Substring(0, instructionsText.Length - 1);
                }
            }

            _infoPanel.GetText("Info").UpdateText(instructionsText);
        }
    }
}