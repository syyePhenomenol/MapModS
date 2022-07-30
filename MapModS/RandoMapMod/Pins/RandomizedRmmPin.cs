﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ConnectionMetadataInjector;
using ItemChanger;
using MapChanger;
using MapChanger.MonoBehaviours;
using RandoMapMod.Defs;
using UnityEngine;
using RM = RandomizerMod.RandomizerMod;

namespace RandoMapMod.Pins
{
    internal sealed class RandomizedRmmPin : RmmPin, IPeriodicUpdater
    {
        private AbstractPlacement placement;
        private RandoPlacementState placementState;

        private IEnumerable<AbstractItem> remainingItems;
        private int itemIndex = 0;

        private Dictionary<AbstractItem, string> itemPoolGroups;
        internal override HashSet<string> ItemPoolGroups => new(itemPoolGroups.Values);

        public float UpdateWaitSeconds { get; } = 1f;

        public IEnumerator PeriodicUpdate()
        {
            while (true)
            {
                yield return new WaitForSecondsRealtime(UpdateWaitSeconds);
                UpdatePinSprite();
            }
        }

        internal void Initialize(AbstractPlacement placement)
        {
            this.placement = placement;

            LocationPoolGroup = SupplementalMetadata.OfPlacementAndLocations(placement).Get(InjectedProps.LocationPoolGroup);

            itemPoolGroups = new();
            foreach (AbstractItem item in placement.Items)
            {
                itemPoolGroups[item] = SupplementalMetadata.Of(item).Get(InjectedProps.ItemPoolGroup);
            }

            if (SupplementalMetadata.Of(placement).Get(InteropProperties.HighlightScenes) is string[] highlightScenes)
            {
                // Place over panel
                Initialize();
                return;
            }

            Initialize(SupplementalMetadata.Of(placement).Get(InteropProperties.MapLocations));
        }

        protected private override bool ActiveByPoolSetting()
        {
            if (RandoMapMod.LS.GroupBy == Settings.GroupBySetting.Item)
            {
                foreach (string poolGroup in remainingItems.Select(item => itemPoolGroups[item]))
                {
                    Settings.PoolState poolState = RandoMapMod.LS.GetPoolGroupSetting(poolGroup);

                    if (poolState == Settings.PoolState.On || (poolState == Settings.PoolState.Mixed && RandoMapMod.LS.RandomizedOn))
                    {
                        return true;
                    }
                }
                return false;
            }
            else
            {
                Settings.PoolState poolState = RandoMapMod.LS.GetPoolGroupSetting(LocationPoolGroup);

                return poolState == Settings.PoolState.On || (poolState == Settings.PoolState.Mixed && RandoMapMod.LS.RandomizedOn);
            }
        }

        protected private override bool LocationNotCleared()
        {
            return placementState != RandoPlacementState.Cleared
                && (placementState != RandoPlacementState.ClearedPersistent || RandoMapMod.GS.PersistentOn);
        }

        protected private override void OnEnable()
        {
            UpdatePlacementState();
            UpdatePinSprite();
            UpdateBorderColor();

            StartCoroutine(PeriodicUpdate());

            base.OnEnable();
        }

        protected private override void UpdatePinSprite()
        {
            if (RandoMapMod.LS.SpoilerOn)
            {
                Sprite = GetItemSprite();
            }
            else
            {
                Sprite = placementState switch
                {
                    RandoPlacementState.UncheckedUnreachable
                    or RandoPlacementState.UncheckedReachable
                    or RandoPlacementState.OutOfLogicReachable => GetLocationSprite(),
                    RandoPlacementState.Previewed
                    or RandoPlacementState.ClearedPersistent => GetItemSprite(),
                    _ => GetLocationSprite(),
                };
            }

            Sprite GetLocationSprite()
            {
                return SupplementalMetadata.OfPlacementAndLocations(placement).Get(InteropProperties.LocationPinSprite).Value;
            }

            Sprite GetItemSprite()
            {
                itemIndex = (itemIndex + 1) % remainingItems.Count();
                return SupplementalMetadata.Of(remainingItems.ElementAt(itemIndex)).Get(InteropProperties.ItemPinSprite).Value;
            }
        }

        protected private override void UpdatePinSize()
        {
            Size = pinSizes[RandoMapMod.GS.PinSize];

            if (Selected)
            {
                Size *= SELECTED_MULTIPLIER;
            }
            else if (placementState is RandoPlacementState.UncheckedUnreachable)
            {
                Size *= UNREACHABLE_SIZE_MULTIPLIER;
            }
        }

        protected private override void UpdatePinColor()
        {
            Vector4 color = UnityEngine.Color.white;

            if (placementState == RandoPlacementState.UncheckedUnreachable && !Selected)
            {
                Color = new Vector4(color.x * UNREACHABLE_COLOR_MULTIPLIER, color.y * UNREACHABLE_COLOR_MULTIPLIER, color.z * UNREACHABLE_COLOR_MULTIPLIER, color.w);
                return;
            }

            Color = color;
        }

        protected private override void UpdateBorderColor()
        {
            Vector4 color = placementState switch
            {
                RandoPlacementState.OutOfLogicReachable => RmmColors.GetColor(RmmColorSetting.Pin_Out_of_logic),
                RandoPlacementState.Previewed => RmmColors.GetColor(RmmColorSetting.Pin_Previewed),
                RandoPlacementState.ClearedPersistent => RmmColors.GetColor(RmmColorSetting.Pin_Persistent),
                _ => RmmColors.GetColor(RmmColorSetting.Pin_Normal),
            };

            if (placementState == RandoPlacementState.UncheckedUnreachable && !Selected)
            {
                BorderColor = new Vector4(color.x * UNREACHABLE_COLOR_MULTIPLIER, color.y * UNREACHABLE_COLOR_MULTIPLIER, color.z * UNREACHABLE_COLOR_MULTIPLIER, color.w);
            }
            else
            {
                BorderColor = color;
            }
        }

        private void UpdatePlacementState()
        {
            if (RM.RS.TrackerData.clearedLocations.Contains(name))
            {
                if (placement.IsPersistent())
                {
                    placementState = RandoPlacementState.ClearedPersistent;
                }
                else
                {
                    placementState = RandoPlacementState.Cleared;
                }
            }
            else if (placement.CanPreview() && placement.IsPreviewed())
            {
                placementState = RandoPlacementState.Previewed;
            }
            else if (RM.RS.TrackerDataWithoutSequenceBreaks.uncheckedReachableLocations.Contains(name))
            {
                placementState = RandoPlacementState.UncheckedReachable;
            }
            else if (RM.RS.TrackerData.uncheckedReachableLocations.Contains(name))
            {
                placementState = RandoPlacementState.OutOfLogicReachable;
            }
            else
            {
                placementState = RandoPlacementState.UncheckedUnreachable;
            }

            itemIndex = -1;

            if (RandoMapMod.GS.PersistentOn)
            {
                remainingItems = placement.Items.Where(item => !item.WasEverObtained() || item.IsPersistent());
            }
            else
            {
                remainingItems = placement.Items.Where(item => !item.WasEverObtained());
            }
        }

        internal override void GetLookupText()
        {
            throw new NotImplementedException();
        }
    }
}
