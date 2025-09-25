using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace Turbo.Plugins.PixelDrama
{
    [Serializable]
    public class LootFilterItem
    {
        public string ItemName { get; set; }
        public bool IsAncient { get; set; }
        public string MinAffix { get; set; }
        public string GroupName { get; set; }

        public override string ToString()
        {
            var result = ItemName;
            if (IsAncient)
                result += " (Ancient)";
            if (!string.IsNullOrEmpty(MinAffix))
                result += $" [{MinAffix}]";
            return result;
        }
    }

    [Serializable]
    public class TreeNodeData
    {
        public string Name { get; set; }
        public bool Checked { get; set; }
        public bool Expanded { get; set; }
        public List<TreeNodeData> Children { get; set; } = new List<TreeNodeData>();

        public LootFilterItem LootItem { get; set; }

        public string DisplayName => LootItem?.ToString() ?? Name;
    }

    [Serializable]
    public class PixelHelperSettings
    {
        private static readonly string ConfigPath = Path.Combine(
            Application.StartupPath,
            "PixelHelperSettings.xml"
        );
        
        public bool EnableZoom { get; set; }
        public bool AutoSalvage { get; set; }
        public bool AlwaysActOne { get; set; }
        public bool AlwaysUsePara { get; set; }
        public bool CheckBox2 { get; set; }
        public string AutoGambleItem { get; set; } = "";
        public string AutoGemUpItem { get; set; } = "";
        public string AutoOpenRiftType { get; set; } = "";

        public bool AutoPickUp { get; set; }
        public bool PickMaterials { get; set; }
        public bool PickDeathBreath { get; set; }
        public bool PickWhite { get; set; }
        public bool PickBlue { get; set; }
        public bool PickYellow { get; set; }
        public bool PickLegendaries { get; set; }

        public bool Oreks { get; set; }
        public bool Festering { get; set; }
        public bool Battlefields { get; set; }
        public bool Desert { get; set; }
        public bool Cemetry { get; set; }
        public bool FieldsOfMisery { get; set; }
        public bool Moors { get; set; }

        public bool AutoGamble { get; set; }
        public bool AutoOpenRift { get; set; }
        public bool AutoGemUp { get; set; }
        public int AutoGambleIndex { get; set; }
        public int AutoGemUpIndex { get; set; }
        public int RiftOrGRIndex { get; set; }
        public bool AutoAcceptGR { get; set; }

        public bool AutoPylons { get; set; }
        public bool OnlyWithNems { get; set; }
        public bool Power { get; set; }
        public bool Speed { get; set; }
        public bool Conduit { get; set; }
        public bool Shield { get; set; }
        public bool Channeling { get; set; }

        public bool KeepAncients { get; set; }
        public bool KeepPrimals { get; set; }

        public List<TreeNodeData> LootTree { get; set; } = new List<TreeNodeData>();
        
        private static PixelHelperSettings _instance;
        private static bool _isDirty = false;
        private static System.Timers.Timer _saveTimer;

        public static PixelHelperSettings Instance
        {
            get
            {
                if (_instance == null)
                    _instance = Load();
                return _instance;
            }
        }
        
        public void MarkDirty()
        {
            _isDirty = true;
        }
        
        public static PixelHelperSettings Load()
        {
            if (File.Exists(ConfigPath))
            {
                try
                {
                    var serializer = new XmlSerializer(typeof(PixelHelperSettings));
                    using (var reader = new StreamReader(ConfigPath))
                    {
                        return (PixelHelperSettings)serializer.Deserialize(reader);
                    }
                }
                catch
                {
                }
            }
            
            var defaultSettings = new PixelHelperSettings();
            try
            {
                var serializer = new XmlSerializer(typeof(PixelHelperSettings));
                using (var writer = new StreamWriter(ConfigPath))
                    serializer.Serialize(writer, defaultSettings);
            }
            catch { }

            return defaultSettings;
        }
        
        public void Save(PixelHelper form = null)
        {
            if (form != null)
            {
                UpdateTreeFromForm(form.itemsToSaveTreeView);
            }
            try
            {
                var serializer = new XmlSerializer(typeof(PixelHelperSettings));
                using (var writer = new StreamWriter(ConfigPath))
                    serializer.Serialize(writer, this);
                _isDirty = false;
            }
            catch { }
        }
        
        public static void StartAutoSaveTimer()
        {
            if (_saveTimer != null)
                return;

            _saveTimer = new System.Timers.Timer(500);
            _saveTimer.Elapsed += (s, e) =>
            {
                if (_isDirty)
                {
                    _isDirty = false;
                    Instance?.Save();
                }
            };
            _saveTimer.AutoReset = true;
            _saveTimer.Start();
        }
        
        public void ApplyTree(TreeView tree)
        {
            tree.Nodes.Clear();
            foreach (var node in LootTree)
                tree.Nodes.Add(CreateTreeNode(node));
        }

        private TreeNode CreateTreeNode(TreeNodeData data)
        {
            var node = new TreeNode(data.DisplayName)
            {
                Checked = data.Checked,
                Tag = data.LootItem
            };

            if (data.Expanded)
                node.Expand();

            foreach (var child in data.Children)
                node.Nodes.Add(CreateTreeNode(child));

            return node;
        }

        public void UpdateTreeFromForm(TreeView tree)
        {
            LootTree = tree.Nodes.Cast<TreeNode>()
                .Select(ToTreeNodeData)
                .ToList();
        }

        private TreeNodeData ToTreeNodeData(TreeNode node)
        {
            return new TreeNodeData
            {
                Name = node.Text,
                Checked = node.Checked,
                Expanded = node.IsExpanded,
                Children = node.Nodes.Cast<TreeNode>()
                    .Select(ToTreeNodeData)
                    .ToList(),
                LootItem = node.Tag as LootFilterItem
            };
        }
        
        public void ApplyToForm(PixelHelper form)
        {
            form._isLoading = true;
            
            form.enableZoomCheckbox.Checked = EnableZoom;
            form.autoSalvage.Checked = AutoSalvage;
            form.alwaysActOneCheckBox.Checked = AlwaysActOne;
            form.alwaysUseParaCheckbox.Checked = AlwaysUsePara;
            form.checkBox2.Checked = CheckBox2;

            form.autoPickUpCheckBox.Checked = AutoPickUp;
            form.pickMaterialsCheckBox.Checked = PickMaterials;
            form.pickDreathBreathCheckBox.Checked = PickDeathBreath;
            form.pickWhiteCheckBox.Checked = PickWhite;
            form.pickBlueCheckBox.Checked = PickBlue;
            form.pickYellowCheckBox.Checked = PickYellow;
            form.pickLegendariesCheckBox.Checked = PickLegendaries;

            form.oreksCheckBox.Checked = Oreks;
            form.festeringCheckBox.Checked = Festering;
            form.battlefieldsCheckBox.Checked = Battlefields;
            form.desertCheckBox.Checked = Desert;
            form.cemetryCheckBox.Checked = Cemetry;
            form.fieldsOfMiseryCheckBox.Checked = FieldsOfMisery;
            form.moorsCheckBox.Checked = Moors;

            form.autoGambleBox.Checked = AutoGamble;
            form.autoGemUpCheckBox.Checked = AutoGemUp;
            form.autoOpenRift.Checked = AutoOpenRift;
            form.checkBox1.Checked = AutoAcceptGR;

            form.autoPylonsCheckBox.Checked = AutoPylons;
            form.onlyWithNemsCheckBox.Checked = OnlyWithNems;
            form.powerCheckBox.Checked = Power;
            form.speedCheckBox.Checked = Speed;
            form.condiCheckBox.Checked = Conduit;
            form.sheldCheckBox.Checked = Shield;
            form.chanCheckBox.Checked = Channeling;

            form.keepAncientsCheckBox.Checked = KeepAncients;
            form.keepPrimalsCheckBox.Checked = KeepPrimals;

            ApplyTree(form.itemsToSaveTreeView);
            
            if (AutoGambleIndex >= 0 && AutoGambleIndex < form.autoGambleComboBox.Items.Count)
                form.autoGambleComboBox.SelectedIndex = AutoGambleIndex;

            if (AutoGemUpIndex >= 0 && AutoGemUpIndex < form.autoGemUpComboBox.Items.Count)
                form.autoGemUpComboBox.SelectedIndex = AutoGemUpIndex;

            if (RiftOrGRIndex >= 0 && RiftOrGRIndex < form.riftOrGrCombox.Items.Count)
                form.riftOrGrCombox.SelectedIndex = RiftOrGRIndex;
            
            SubscribeToControlEvents(form);
        }
        
        private void SubscribeToControlEvents(PixelHelper form)
        {
            // Чекбоксы
            SubscribeCheckBox(form.enableZoomCheckbox, val => EnableZoom = val);
            SubscribeCheckBox(form.autoSalvage, val => AutoSalvage = val);
            SubscribeCheckBox(form.alwaysActOneCheckBox, val => AlwaysActOne = val);
            SubscribeCheckBox(form.alwaysUseParaCheckbox, val => AlwaysUsePara = val);
            SubscribeCheckBox(form.checkBox2, val => CheckBox2 = val);

            SubscribeCheckBox(form.autoPickUpCheckBox, val => AutoPickUp = val);
            SubscribeCheckBox(form.pickMaterialsCheckBox, val => PickMaterials = val);
            SubscribeCheckBox(form.pickDreathBreathCheckBox, val => PickDeathBreath = val);
            SubscribeCheckBox(form.pickWhiteCheckBox, val => PickWhite = val);
            SubscribeCheckBox(form.pickBlueCheckBox, val => PickBlue = val);
            SubscribeCheckBox(form.pickYellowCheckBox, val => PickYellow = val);
            SubscribeCheckBox(form.pickLegendariesCheckBox, val => PickLegendaries = val);

            SubscribeCheckBox(form.oreksCheckBox, val => Oreks = val);
            SubscribeCheckBox(form.festeringCheckBox, val => Festering = val);
            SubscribeCheckBox(form.battlefieldsCheckBox, val => Battlefields = val);
            SubscribeCheckBox(form.desertCheckBox, val => Desert = val);
            SubscribeCheckBox(form.cemetryCheckBox, val => Cemetry = val);
            SubscribeCheckBox(form.fieldsOfMiseryCheckBox, val => FieldsOfMisery = val);
            SubscribeCheckBox(form.moorsCheckBox, val => Moors = val);

            SubscribeCheckBox(form.autoGambleBox, val => AutoGamble = val);
            SubscribeCheckBox(form.autoGemUpCheckBox, val => AutoGemUp = val);
            SubscribeCheckBox(form.autoOpenRift, val => AutoOpenRift = val);
            SubscribeCheckBox(form.checkBox1, val => AutoAcceptGR = val);

            SubscribeCheckBox(form.autoPylonsCheckBox, val => AutoPylons = val);
            SubscribeCheckBox(form.onlyWithNemsCheckBox, val => OnlyWithNems = val);
            SubscribeCheckBox(form.powerCheckBox, val => Power = val);
            SubscribeCheckBox(form.speedCheckBox, val => Speed = val);
            SubscribeCheckBox(form.condiCheckBox, val => Conduit = val);
            SubscribeCheckBox(form.sheldCheckBox, val => Shield = val);
            SubscribeCheckBox(form.chanCheckBox, val => Channeling = val);

            SubscribeCheckBox(form.keepAncientsCheckBox, val => KeepAncients = val);
            SubscribeCheckBox(form.keepPrimalsCheckBox, val => KeepPrimals = val);
            
            form.autoGambleComboBox.SelectedIndexChanged += (s, e) =>
            {
                if (form._isLoading)
                    return;
                AutoGambleIndex = form.autoGambleComboBox.SelectedIndex;
                AutoGambleItem = form.autoGambleComboBox.SelectedItem?.ToString() ?? "";
                MarkDirty();
            };

            form.autoGemUpComboBox.SelectedIndexChanged += (s, e) =>
            {
                if (form._isLoading)
                    return;
                AutoGemUpIndex = form.autoGemUpComboBox.SelectedIndex;
                AutoGemUpItem = form.autoGemUpComboBox.SelectedItem?.ToString() ?? "";
                MarkDirty();
            };

            form.riftOrGrCombox.SelectedIndexChanged += (s, e) =>
            {
                if (form._isLoading)
                    return;
                RiftOrGRIndex = form.riftOrGrCombox.SelectedIndex;
                AutoOpenRiftType = form.riftOrGrCombox.SelectedItem?.ToString() ?? "";
                MarkDirty();
            };
            
            form.itemsToSaveTreeView.AfterCheck += (s, e) =>
            {
                if (form._isLoading)
                    return;
                UpdateTreeFromForm(form.itemsToSaveTreeView);
                MarkDirty();
            };

            form.itemsToSaveTreeView.AfterExpand += (s, e) =>
            {
                if (form._isLoading)
                    return;
                UpdateTreeFromForm(form.itemsToSaveTreeView);
                MarkDirty();
            };

            form.itemsToSaveTreeView.AfterCollapse += (s, e) =>
            {
                if (form._isLoading)
                    return;
                UpdateTreeFromForm(form.itemsToSaveTreeView);
                MarkDirty();
            };

            form.itemsToSaveTreeView.AfterLabelEdit += (s, e) =>
            {
                if (form._isLoading)
                    return;
                UpdateTreeFromForm(form.itemsToSaveTreeView);
                MarkDirty();
            };

            form._isLoading = false;
        }
        
        private void SubscribeCheckBox(CheckBox cb, Action<bool> assign)
        {
            cb.CheckedChanged += (s, e) =>
            {
                if (((PixelHelper)cb.FindForm())._isLoading)
                    return;
                assign(cb.Checked);
                MarkDirty();
            };
        }

        public List<LootFilterItem> GetItemsToSave()
        {
            var result = new List<LootFilterItem>();
            CollectCheckedLootItems(LootTree, result);
            return result;
        }

        private void CollectCheckedLootItems(List<TreeNodeData> nodes, List<LootFilterItem> result)
        {
            if (nodes == null)
                return;

            foreach (var node in nodes)
            {
                if (node.Checked && node.LootItem != null)
                {
                    result.Add(node.LootItem);
                }

                CollectCheckedLootItems(node.Children, result);
            }
        }
    }
}