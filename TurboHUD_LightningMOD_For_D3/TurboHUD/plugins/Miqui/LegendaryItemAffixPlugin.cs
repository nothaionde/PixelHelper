using Turbo.Plugins.Default;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Turbo.Plugins.Miqui
{

    public enum AffixType
    {
        PERCENTAGE,
        SECONDS,
        NO_UNIT,
        USELESS
    }

    public class ItemAffixInfo
    {
        public uint Sno { get; }
        public AffixType AffixType { get; }
        public string AssociatedCode { get; }

        private ItemAffixInfo(uint sno, AffixType affixType) : this(sno, affixType, "Item_Power_Passive") { }

        public ItemAffixInfo(uint sno, AffixType affixType, string associatedCode)
        {
            Sno = sno;
            AffixType = affixType;
            AssociatedCode = associatedCode;
        }

        public static ItemAffixInfo PercentageFromSno(uint sno)
        {
            return new ItemAffixInfo(sno, AffixType.PERCENTAGE);
        }

        public static ItemAffixInfo SecondFromSno(uint sno)
        {
            return new ItemAffixInfo(sno, AffixType.SECONDS);
        }

        public static ItemAffixInfo NoUnitFromSno(uint sno)
        {
            return new ItemAffixInfo(sno, AffixType.NO_UNIT);
        }
    }

    public class LegendaryItemAffixPlugin : BasePlugin, IInGameTopPainter, ICustomizer
    {

        private bool debug = false;
        private bool LoggedStats = false;
        private long LastLog { get; set; }

        public IFont LegendaryAffixFont { get; set; }

        public bool ShowOnUnidentifiedOnly { get; set; } = true;

        public Dictionary<uint, ItemAffixInfo> ItemAffixInfos { get; set; }

        public static LegendaryItemAffixPlugin Instance;

        public LegendaryItemAffixPlugin()
        {
            Enabled = true;
            if (Instance == null)
            {
                Instance = this;
            }
        }

        public override void Load(IController hud)
        {
            base.Load(hud);

            LegendaryAffixFont = Hud.Render.CreateFont("arial", 7, 255, 0, 0, 0, true, false, false);
            LegendaryAffixFont.SetShadowBrush(128, 111, 134, 252, true);
        }

        public void PaintTopInGame(ClipState clipState)
        {
            if (clipState != ClipState.Inventory)
                return;

            int stashPage, stashTab, stashTabAbs;

            stashTab = Hud.Inventory.SelectedStashTabIndex;
            stashPage = Hud.Inventory.SelectedStashPageIndex;
            stashTabAbs = stashTab + (stashPage * Hud.Inventory.MaxStashTabCountPerPage);

            var items = Hud.Game.Items.Where(x => x.Location == ItemLocation.Inventory || x.Location == ItemLocation.Stash || x.Location.IsEquipped());
            foreach (var item in items)
            {
                if (ShowOnUnidentifiedOnly && !item.Unidentified)
                    continue;

                if (item.SocketedInto != null)
                    continue;

                if (item.Location == ItemLocation.Stash)
                {
                    var tabIndex = item.InventoryY / 10;
                    if (tabIndex != stashTabAbs)
                        continue;
                }

                if ((item.InventoryX < 0) || (item.InventoryY < 0))
                    continue;

                Log(" **************************************** " + item.FullNameEnglish + " (sno:" + item.SnoActor.Sno + ")");

                if (isItemLegendaryGem(item))
                    continue;

                ItemAffixInfo legendaryItemAffixInfo;
                bool wasFound = ItemAffixInfos.TryGetValue((uint)item.SnoActor.Sno, out legendaryItemAffixInfo);
                if (!wasFound)
                    continue;

                IItem[] gems = item.ItemsInSocket;
                // The list evolves afterwards : once a stat has been removed, we don't remove it again
                // The goal of that is : if a leg. gem has the same affix value as the legendary affix of the ring/amulet, the legendary affix of the ring/amulet is still shown
                List<IItemStat> gemStatsToRemove = getGemStatsSameCode(gems, legendaryItemAffixInfo);

                if (legendaryItemAffixInfo.AffixType != AffixType.USELESS && legendaryItemAffixInfo.AssociatedCode != null)
                    foreach (var stat in item.StatList)
                    {
                        bool drawn = DrawLegendaryAffix(item, stat, legendaryItemAffixInfo, gemStatsToRemove);
                        if (drawn)
                            break;
                    }
                Log("****************************************");
            }
            LoggedStats = true;
        }

        private List<IItemStat> getGemStatsSameCode(IItem[] gems, ItemAffixInfo legendaryItemAffixInfo)
        {
            List<IItemStat> stats = new List<IItemStat>();
            if (gems != null && gems.Count() == 1)
            {
                foreach (var gemStat in gems[0].StatList)
                {
                    if (gemStat.Attribute != null && gemStat.Attribute.Code != null && gemStat.Attribute.Code.Equals(legendaryItemAffixInfo.AssociatedCode))
                    {
                        stats.Add(gemStat);
                        Log(gemStat.Attribute.Code + " " + gemStat.DoubleValue);
                    }
                }
            }
            return stats;
        }

        private bool isItemLegendaryGem(IItem item)
        {
            bool isLegendaryGem = false;
            foreach (var stat in item.StatList)
            {
                if (stat.Attribute != null && stat.Attribute.Code != null && stat.Attribute.Code.Equals("Jewel_Rank"))
                    isLegendaryGem = true;
            }
            return isLegendaryGem;
        }

        private bool DrawLegendaryAffix(IItem item, IItemStat stat, ItemAffixInfo legendaryItemAffixInfo, List<IItemStat> gemStatsToRemove)
        {
            if (stat.Attribute != null && stat.Attribute.Code != null)
            {
                Log("" + stat.Attribute.Code + " : " + stat.DoubleValue + (stat.IntegerValue != null ? ("(" + stat.IntegerValue + ")") : ""));
                if (legendaryItemAffixInfo.AssociatedCode != null && stat.Attribute.Code.Equals(legendaryItemAffixInfo.AssociatedCode))
                {
                    if (gemStatsToRemove.Count() > 0)
                    {
                        for (int i = 0; i < gemStatsToRemove.Count(); i++)
                        {
                            if (gemStatsToRemove[i].DoubleValue.Equals(stat.DoubleValue))
                            {
                                // Remove in the gem stat List to make sure we still show the stat if the legendary gem & item have the same stat value
                                Log("Removed : " + gemStatsToRemove[i].Attribute.Code + " : " + gemStatsToRemove[i].DoubleValue);
                                gemStatsToRemove.RemoveAt(i);
                                return false;
                            }
                        }
                    }
                    try
                    {
                        double multiplyBy = legendaryItemAffixInfo.AffixType == AffixType.PERCENTAGE ? 100d : 1d;
                        string unit;
                        if (legendaryItemAffixInfo.AffixType == AffixType.PERCENTAGE)
                            unit = "%";
                        else if (legendaryItemAffixInfo.AffixType == AffixType.SECONDS)
                            unit = "s";
                        else
                            unit = "";

                        if (!handleSpecialCases(legendaryItemAffixInfo, stat))
                            return false;

                        double val = stat.DoubleValue * multiplyBy;

                        // Leoric's crown is annoying with rounding so we round ourselves
                        if (legendaryItemAffixInfo.Sno == (uint)ActorSnoEnum._helm_norm_unique_01)
                            val = Convert.ToInt32(val);

                        var rect = Hud.Inventory.GetItemRect(item);
                        var text = val.ToString("#.##", CultureInfo.CreateSpecificCulture("en-GB")) + unit;
                        var layout = LegendaryAffixFont.GetTextLayout(text);
                        LegendaryAffixFont.DrawText(layout, rect.X, rect.Y);
                        return true; // Set to false if you want to dislay all info of items in debug
                    }
                    catch (OverflowException)
                    {
                        // Don't do this
                    }
                }
            }
            return false;
        }

        private bool handleSpecialCases(ItemAffixInfo legendaryItemAffixInfo, IItemStat stat)
        {
            // Witching hour
            if (legendaryItemAffixInfo.Sno == (uint)ActorSnoEnum._belt_norm_unique_07)
                if (stat.DoubleValue > 1)
                    return false;

            // Tzo krin
            if (legendaryItemAffixInfo.Sno == (uint)ActorSnoEnum._spiritstone_norm_unique_12)
                if (stat.IntegerValue == 0)
                    return false;

            return true;
        }

        private void Log(string toLog)
        {
            if (!debug)
                return;

            if (toLog == null || toLog.Equals(""))
                return;

            if (Hud.Game.CurrentRealTimeMilliseconds - LastLog > 10 * 1000)
                LoggedStats = false;

            if (!LoggedStats)
            {
                Hud.TextLog.Log("itemLog", toLog);
                LastLog = Hud.Game.CurrentRealTimeMilliseconds;
            }
        }

        public void Customize()
        {
            if (!ShowOnUnidentifiedOnly)
                Hud.RunOnPlugin<InventoryAndStashPlugin>(plugin => plugin.SocketedLegendaryGemRankEnabled = false);
        }

        public double? GetAffixValue(IItem item)
        {
            if (item == null || item.StatList == null)
                return null;

            ItemAffixInfo legendaryItemAffixInfo;
            if (!ItemAffixInfos.TryGetValue((uint)item.SnoActor.Sno, out legendaryItemAffixInfo))
                return null;

            if (legendaryItemAffixInfo.AffixType == AffixType.USELESS || legendaryItemAffixInfo.AssociatedCode == null)
                return null;

            foreach (var stat in item.StatList)
            {
                if (stat.Attribute?.Code == legendaryItemAffixInfo.AssociatedCode)
                {
                    if (handleSpecialCases(legendaryItemAffixInfo, stat))
                    {
                        double multiplyBy = legendaryItemAffixInfo.AffixType == AffixType.PERCENTAGE ? 100d : 1d;
                        double val = stat.DoubleValue * multiplyBy;

                        // Special case: Leoric's Crown
                        if (legendaryItemAffixInfo.Sno == (uint)ActorSnoEnum._helm_norm_unique_01)
                            val = Math.Round(val);

                        return val;
                    }
                }
            }

            return null;
        }
    }
}