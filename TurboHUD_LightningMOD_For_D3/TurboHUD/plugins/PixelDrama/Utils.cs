namespace Turbo.Plugins.PixelDrama
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Turbo.Plugins;

    public static class Utils
    {
        private static readonly HashSet<uint> NonSalvageableSno = new HashSet<uint>
        {
            1661412389,
            1815806856,
            3382510415,
            4176712417,
            3931575626,
            1236604967,
            111732407,
            3659697712,
            88665049
        };

        private static readonly HashSet<string> NonSalvageableMainGroupCodes = new HashSet<string>
        {
            "riftkeystone",
            "horadriccache",
            "-",
            "pony",
            "plans"
        };

        private static readonly HashSet<string> NonSalvageableNames = new HashSet<string>
        {
            "Staff of Herding",
            "Hellforge Ember"
        };

        public static List<string> GetAllSalvageableLegendaryAndSetItems(IController hud)
        {
            var snoItemsObj = hud.Sno.SnoItems;
            var itemType = typeof(ISnoItem);

            var itemProperties = snoItemsObj
                .GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.PropertyType == itemType)
                .ToList();

            var uniqueNames = new HashSet<string>();
            var result = new List<string>();

            foreach (var prop in itemProperties)
            {
                try
                {
                    var item = prop.GetValue(snoItemsObj) as ISnoItem;
                    if (item == null)
                        continue;

                    if (!uniqueNames.Add(item.NameLocalized))
                        continue;

                    if (IsNonSalvageable(item))
                        continue;

                    if (item.LegendaryPower == null && item.SetItemBonusesSno == 0)
                        continue;

                    result.Add(item.NameLocalized);
                }
                catch { }
            }

            result.Sort();
            return result;
        }

        private static bool IsNonSalvageable(ISnoItem item)
        {
            if (NonSalvageableSno.Contains(item.Sno))
                return true;

            if (NonSalvageableMainGroupCodes.Contains(item.MainGroupCode))
                return true;
            if (item.MainGroupCode.Contains("cosmetic"))
                return true;

            if (NonSalvageableNames.Contains(item.NameEnglish))
                return true;

            if (item.SnoItemType?.Code == "UpgradeableJewel")
                return true;

            return false;
        }
    }
}

