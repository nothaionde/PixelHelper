namespace Turbo.Plugins.PixelDrama
{
    using System.Linq;

    public class KadalaItemType
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public int UiRegionIndex { get; set; }
        public int UiTabIndex { get; set; }
        public int RequiredTabAnimState { get; set; }
        public int ShardCost { get; set; }
        public int InventorySlots { get; set; }

        public KadalaItemType(
            int index,
            string name,
            int uiRegion,
            int uiTab,
            int animState,
            int shards,
            int slots
        )
        {
            Index = index;
            Name = name;
            UiRegionIndex = uiRegion;
            UiTabIndex = uiTab;
            RequiredTabAnimState = animState;
            ShardCost = shards;
            InventorySlots = slots;
        }

        public override string ToString() => Name;

        public static readonly KadalaItemType[] All = new KadalaItemType[]
        {
            new KadalaItemType(0, "<None>", 0, 0, 0, 0, 0),
            new KadalaItemType(1, "1-H Weapon", 0, 0, 49, 75, 2),
            new KadalaItemType(2, "2-H Weapon", 1, 0, 49, 75, 2),
            new KadalaItemType(3, "Quiver", 2, 0, 49, 25, 2),
            new KadalaItemType(4, "Orb", 3, 0, 49, 25, 2),
            new KadalaItemType(5, "Mojo", 4, 0, 49, 25, 2),
            new KadalaItemType(6, "Phylactery", 5, 0, 49, 25, 2),
            new KadalaItemType(7, "Helm", 0, 1, 40, 25, 2),
            new KadalaItemType(8, "Gloves", 1, 1, 40, 25, 2),
            new KadalaItemType(9, "Boots", 2, 1, 40, 25, 2),
            new KadalaItemType(10, "Chest Armor", 3, 1, 40, 25, 2),
            new KadalaItemType(11, "Belt", 4, 1, 40, 25, 1),
            new KadalaItemType(12, "Shoulders", 5, 1, 40, 25, 2),
            new KadalaItemType(13, "Pants", 6, 1, 40, 25, 2),
            new KadalaItemType(14, "Bracers", 7, 1, 40, 25, 2),
            new KadalaItemType(15, "Shield", 8, 1, 40, 25, 2),
            new KadalaItemType(16, "Ring", 0, 2, 46, 50, 1),
            new KadalaItemType(17, "Amulet", 1, 2, 46, 100, 1),
        };

        public static KadalaItemType GetByIndex(int index)
        {
            if (index < 0 || index >= All.Length)
                return All[0]; // <None>
            return All[index];
        }

        public static KadalaItemType GetByName(string name)
        {
            return All.FirstOrDefault(i => i.Name == name) ?? All[0];
        }
    }
}
