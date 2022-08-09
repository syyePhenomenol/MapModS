﻿using System;
using System.Collections.Generic;
using GlobalEnums;
using MapChanger;
using MapChanger.Defs;
using MapChanger.MonoBehaviours;
using RandoMapMod.Modes;
using RandoMapMod.Settings;
using RandoMapMod.Transition;
using RandomizerCore.Logic;
using UnityEngine;
using L = RandomizerMod.Localization;

namespace RandoMapMod.Pins
{
    internal abstract class RmmPin : BorderedPin, ISelectable
    {
        /// <summary>
        /// The number of pins that don't belong anywhere on the map, and will get placed
        /// into an arbitrary grid
        /// </summary>
        internal static int MiscPinsCount { get; set; } = 0;

        private const float MISC_BASE_OFFSET_X = -11.5f;
        private const float MISC_BASE_OFFSET_Y = -11f;
        private const float MISC_SPACING = 0.5f;
        private const int MISC_ROW_COUNT = 25;

        private const float SMALL_SCALE = 0.56f;
        private const float MEDIUM_SCALE = 0.67f;
        private const float LARGE_SCALE = 0.8f;

        private protected const float UNREACHABLE_SIZE_MULTIPLIER = 0.7f;
        private protected const float UNREACHABLE_COLOR_MULTIPLIER = 0.5f;

        private protected const float SELECTED_MULTIPLIER = 1.3f;

        private protected static readonly Dictionary<PinSize, float> pinSizes = new()
        {
            { PinSize.Small, SMALL_SCALE },
            { PinSize.Medium, MEDIUM_SCALE },
            { PinSize.Large, LARGE_SCALE }
        };

        private bool selected = false;
        public virtual bool Selected
        {
            get => selected;
            set
            {
                if (selected != value)
                {
                    selected = value;
                    UpdatePinSize();
                    UpdatePinColor();
                    UpdateBorderColor();
                }
            }
        }

        internal string LocationPoolGroup { get; protected private set; }
        internal abstract HashSet<string> ItemPoolGroups { get; }
        internal string SceneName { get; protected private set; }
        internal MapZone MapZone { get; private set; } = MapZone.NONE;
        internal string Logic { get; private set; }

        public void Initialize((string, float, float)[] mapLocations)
        {
            if (mapLocations is not null && MiscPinsCount > 300)
            {
                MapPosition mlp = new(mapLocations);
                MapPosition = mlp;
                MapZone = mlp.MapZone;
            }
            else
            {
                AbsMapPosition amp = new((MISC_BASE_OFFSET_X + MiscPinsCount % MISC_ROW_COUNT * MISC_SPACING,
                    MISC_BASE_OFFSET_Y - MiscPinsCount / MISC_ROW_COUNT * MISC_SPACING));
                MapPosition = amp;
                MiscPinsCount++;
            }

            Initialize();
        }

        //public void UpdatePosition(MapLocation[] mapLocations)
        //{
        //    MapPosition mlp = new(mapLocations);
        //    MapPosition = mlp;
        //    MapZone = mlp.MapZone;
        //}

        public override void Initialize()
        {
            base.Initialize();

            //RandoMapMod.Instance.LogDebug($"{name}: {SceneName}");

            ActiveModifiers.AddRange
            (
                new Func<bool>[]
                {
                    CorrectMapOpen,
                    ActiveByCurrentMode,
                    ActiveByPoolSetting,
                    LocationNotCleared
                }
            );

            if (RandomizerMod.RandomizerMod.RS.TrackerData.lm.LogicLookup.TryGetValue(name, out OptimizedLogicDef logic))
            {
                Logic = logic.ToInfix();
            }

            BorderSprite = new EmbeddedSprite("Pins.Border").Value;
            BorderPlacement = BorderPlacement.InFront;
        }

        public bool CanSelect()
        {
            return Sr.isVisible;
        }

        public (string, Vector2) GetKeyAndPosition()
        {
            return (name, transform.position);
        }

        public override void OnMainUpdate(bool active)
        {
            if (!active) return;

            UpdatePinSprite();
            UpdatePinSize();
            UpdatePinColor();
            UpdateBorderColor();
        }

        protected private abstract void UpdatePinSprite();

        protected private abstract void UpdatePinSize();

        protected private abstract void UpdatePinColor();

        protected private abstract void UpdateBorderColor();

        protected private bool CorrectMapOpen()
        {
            return States.WorldMapOpen || (States.QuickMapOpen && States.CurrentMapZone == MapZone);
        }

        protected private bool ActiveByCurrentMode()
        {
            return MapChanger.Settings.CurrentMode() is FullMapMode or AllPinsMode
                || (MapChanger.Settings.CurrentMode() is PinsOverMapMode && Utils.HasMapSetting(MapZone))
                || (Conditions.TransitionRandoModeEnabled() && (SceneName is null || TransitionTracker.GetRoomActive(SceneName)));
        }

        protected private abstract bool ActiveByPoolSetting();

        protected private abstract bool LocationNotCleared();

        internal virtual string GetLookupText()
        {;
            string text = $"{name.ToCleanName()}";

            if (SceneName is not null)
            {
                text += $"\n\n{L.Localize("Room")}: {SceneName}";
            }

            return text;
        }
    }
}
