﻿using System;
using System.Collections.Generic;
using MapChanger;
using MapChanger.Defs;
using MapChanger.UI;
using Modding;
using RandoMapMod.Modes;
using RandoMapMod.Pins;
using RandoMapMod.Rooms;
using RandoMapMod.Settings;
using RandoMapMod.Transition;
using RandoMapMod.UI;
using UnityEngine;

namespace RandoMapMod
{
    public class RandoMapMod : Mod, ILocalSettings<LocalSettings>, IGlobalSettings<GlobalSettings>
    {
        private static readonly string[] dependencies = new string[]
        {
            "MapChangerMod",
            "Randomizer 4",
            "CMICore",
        };

        private static readonly MapMode[] modes = new MapMode[]
        {
            new FullMapMode(),
            new AllPinsMode(),
            new PinsOverMapMode(),
            new TransitionNormalMode(),
            new TransitionVisitedOnlyMode(),
            new TransitionAllRoomsMode()
        };

        private static readonly Title title = new RmmTitle();

        private static readonly MainButton[] mainButtons = new MainButton[]
        {
            new ModEnabledButton(),
            new ModeButton(),
            new PinSizeButton(),
            new PinStyleButton(),
            new RandomizedButton(),
            new VanillaButton(),
            new SpoilersButton(),
            new PoolsPanelButton(),
            new PersistentButton(),
            new GroupByButton()
        };

        private static readonly ExtraButtonPanel[] extraButtonPanels = new ExtraButtonPanel[]
        {
            new PoolsPanel()
        };

        private static readonly UILayer[] mapUILayers = new UILayer[]
        {
            new RmmBottomRowText()
        };

        internal static RandoMapMod Instance;

        public RandoMapMod()
        {
            Instance = this;
        }

        public override string GetVersion() => "2.7.0";

        public override int LoadPriority() => 10;

        public static LocalSettings LS = new();
        public void OnLoadLocal(LocalSettings ls) => LS = ls;
        public LocalSettings OnSaveLocal() => LS;

        public static GlobalSettings GS = new();
        public void OnLoadGlobal(GlobalSettings gs) => GS = gs;
        public GlobalSettings OnSaveGlobal() => GS;

        public override void Initialize()
        {
            LogDebug($"Initializing");

            foreach (string dependency in dependencies)
            {
                if (ModHooks.GetMod(dependency) is not Mod)
                {
                    MapChangerMod.Instance.LogWarn($"Dependency not found for {GetType().Name}: {dependency}");
                    return;
                }
            }

            try
            {
                Interop.FindInteropMods();
                Finder.InjectLocations(JsonUtil.Deserialize<Dictionary<string, MapLocationDef>>("MapModS.RandoMapMod.Resources.locations.json"));

                Events.AfterEnterGame += OnEnterGame;
                Events.BeforeQuitToMenu += OnQuitToMenu;
            }
            catch (Exception e)
            {
                LogError(e);
            }

            LogDebug($"Initialization complete.");
        }

        private static void OnEnterGame()
        {
            if (RandomizerMod.RandomizerMod.RS.GenerationSettings is null) return;

            MapChanger.Settings.AddModes(modes);
            MapChanger.Settings.SetModEnabled(LS.ModEnabled);
            MapChanger.Settings.SetMode("RandoMapMod", LS.Mode.ToString().Replace('_', ' '));

            RmmColors.LoadCustomColors();

            Events.AfterSetGameMap += OnSetGameMap;

            RoomTexts.Load();
            BenchwarpInterop.Load();
            TransitionData.SetTransitionLookup();
            PathfinderData.Load();
            Pathfinder.Initialize();
            RmmPins.OnEnterGame();
            TransitionTracker.OnEnterGame();
        }

        private static void OnSetGameMap(GameObject goMap)
        {
            try
            {
                RoomTexts.Make(goMap);
                RmmPins.Make(goMap);

                LS.Initialize();

                title.Make();

                foreach (MainButton button in mainButtons)
                {
                    button.Make();
                }

                foreach (ExtraButtonPanel ebp in extraButtonPanels)
                {
                    ebp.Make();
                }

                foreach (UILayer uiLayer in mapUILayers)
                {
                    UIMaster.AddMapLayer(uiLayer);
                }
            }
            catch (Exception e)
            {
                Instance.LogError(e);
            }
        }

        private static void OnQuitToMenu()
        {
            Events.AfterSetGameMap -= OnSetGameMap;

            TransitionTracker.OnQuitToMenu();
            RmmPins.OnQuitToMenu();
        }
    }
}