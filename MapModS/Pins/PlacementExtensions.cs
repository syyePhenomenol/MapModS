﻿using ItemChanger;
using RandomizerCore;
using RandomizerMod.IC;
using RandomizerMod.RC;
using System.Linq;
using RM = RandomizerMod.RandomizerMod;

namespace MapModS.Pins
{
    internal static class PlacementExtensions
    {
        internal static void LogDebug(this AbstractPlacement placement)
        {
            MapModS.Instance.LogDebug($"- Name: {placement.Name}");
            MapModS.Instance.LogDebug($"- - Visited: {placement.Visited}");
            MapModS.Instance.LogDebug($"- - AllObtained: {placement.AllObtained()}");
            MapModS.Instance.LogDebug($"- - Tags: {placement.tags.Count}");

            foreach (Tag tag in placement.tags)
            {
                MapModS.Instance.LogDebug($"- - - String: {tag}");
            }

            MapModS.Instance.LogDebug($"- - Items: {placement.Items.Count}");

            foreach (AbstractItem item in placement.Items)
            {
                MapModS.Instance.LogDebug($"- - - Name: {item.name}");
                MapModS.Instance.LogDebug($"- - - IsObtained: {item.IsObtained()}");
                MapModS.Instance.LogDebug($"- - - WasEverObtained: {item.WasEverObtained()}");
                MapModS.Instance.LogDebug($"- - - PreviewName: {item.GetPreviewName()}");
                MapModS.Instance.LogDebug($"- - - Tags: {item.tags.Count}");

                foreach (Tag tag in item.tags)
                {
                    MapModS.Instance.LogDebug($"- - - - String: {tag}");
                }
            }
        }

        internal static void LogDebug(this GeneralizedPlacement placement)
        {
            MapModS.Instance.LogDebug($"- Location: {placement.Location.Name}");
            MapModS.Instance.LogDebug($"- - Item: {placement.Item.Name}");
        }

        // Next five helper functions are based on BadMagic100's Rando4Stats RandoExtensions
        // MIT License

        // Copyright(c) 2022 BadMagic100

        // Permission is hereby granted, free of charge, to any person obtaining a copy
        // of this software and associated documentation files(the "Software"), to deal
        // in the Software without restriction, including without limitation the rights
        // to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
        // copies of the Software, and to permit persons to whom the Software is
        // furnished to do so, subject to the following conditions:

        // The above copyright notice and this permission notice shall be included in all
        // copies or substantial portions of the Software.
        internal static ItemPlacement RandoPlacement(this AbstractItem item)
        {
            if (item.GetTag(out RandoItemTag tag))
            {
                return RM.RS.Context.itemPlacements[tag.id];
            }
            return default;
        }

        //internal static string RandoItemName(this AbstractItem item)
        //{
        //    return item.RandoPlacement().Item.Name ?? default;
        //}

        //internal static string RandoLocationName(this AbstractItem item)
        //{
        //    return item.RandoPlacement().Location.Name ?? default;
        //}

        //internal static int RandoItemId(this AbstractItem item)
        //{
        //    if (item.GetTag(out RandoItemTag tag))
        //    {
        //        return tag.id;
        //    }
        //    return default;
        //}

        internal static bool IsPreviewed(this AbstractPlacement placement)
        {
            return placement.CheckVisitedAll(VisitState.Previewed);
        }

        internal static bool CanPreview(this AbstractPlacement placement)
        {
            return !placement.HasTag<ItemChanger.Tags.DisableItemPreviewTag>();
        }

        internal static string[] PreviewText(this AbstractPlacement placement)
        {
            if (placement.GetTag(out ItemChanger.Tags.MultiPreviewRecordTag multiTag))
            {
                return multiTag.previewTexts;
            }

            if (placement.GetTag(out ItemChanger.Tags.PreviewRecordTag tag))
            {
                return new[] { tag.previewText };
            }

            return default;
        }

        //internal static string[] PreviewText(this string name)
        //{
        //    if (!ItemChanger.Internal.Ref.Settings.Placements.TryGetValue(name, out AbstractPlacement placement)) return default;

        //    if (placement.GetTag(out ItemChanger.Tags.MultiPreviewRecordTag multiTag))
        //    {
        //        return multiTag.previewTexts;
        //    }

        //    if (placement.GetTag(out ItemChanger.Tags.PreviewRecordTag tag))
        //    {
        //        return new[] { tag.previewText };
        //    }

        //    return default;
        //}

        internal static bool IsPersistent(this AbstractPlacement placement)
        {
            return placement.Items.Any(item => item.HasTag<ItemChanger.Tags.PersistentItemTag>());
        }

        internal static bool IsPersistent(this AbstractItem item)
        {
            return item.HasTag<ItemChanger.Tags.PersistentItemTag>();
        }
    }
}
