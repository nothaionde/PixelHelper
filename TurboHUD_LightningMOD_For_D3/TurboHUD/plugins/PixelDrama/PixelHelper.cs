using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Turbo.Plugins.Default;

namespace Turbo.Plugins.PixelDrama
{
    public partial class PixelHelper : Form
    {
        public bool _isLoading = false;
        private List<string> _allLegendaries;
        private string _currentFilter = string.Empty;
        public Helper _helper;
        public bool _zoomShouldBeEnabled = false;

        public PixelHelper()
        {
            InitializeComponent();
            FillComboBoxes();
            itemsToSaveTreeView.LabelEdit = true;
            itemsToSaveTreeView.AllowDrop = true;
            PixelHelperSettings.StartAutoSaveTimer();
            PixelHelperSettings.Instance.ApplyToForm(this);

            InitializeStripToolMenu();
            InitializeDragAndDrop();
            UpdateGroupComboBox();

            if (selectedItemSortedToPick != null)
            {
                selectedItemSortedToPick.TextChanged += selectedItemSortedToPick_TextChanged;
            }
        }

        public void SetLegendaries(List<string> legendaries)
        {
            _allLegendaries = legendaries.OrderBy(x => x).ToList();
            UpdateListBox();
        }

        private void selectedItemSortedToPick_TextChanged(object sender, EventArgs e)
        {
            _currentFilter = selectedItemSortedToPick.Text;
            UpdateListBox();
        }

        private void UpdateListBox()
        {
            if (_allLegendaries == null || allLegsFromGame == null)
                return;

            allLegsFromGame.Items.Clear();

            if (string.IsNullOrWhiteSpace(_currentFilter))
            {
                foreach (var item in _allLegendaries)
                {
                    allLegsFromGame.Items.Add(item);
                }
            }
            else
            {
                var filtered = _allLegendaries
                    .Where(
                        name => name.StartsWith(_currentFilter, StringComparison.OrdinalIgnoreCase)
                    )
                    .ToList();

                foreach (var item in filtered)
                {
                    allLegsFromGame.Items.Add(item);
                }
            }
        }

        private void FillComboBoxes()
        {
            if (autoGambleComboBox.Items.Count == 0)
            {
                autoGambleComboBox.DataSource = KadalaItemType.All;
                autoGambleComboBox.DisplayMember = "Name";
            }

            if (autoGemUpComboBox.Items.Count == 0)
            {
                autoGemUpComboBox.Items.Add("Lowest Gems First");
            }

            if (riftOrGrCombox.Items.Count == 0)
            {
                riftOrGrCombox.Items.Add("Normal Rift");
                riftOrGrCombox.Items.Add("Greater Rift");
            }

            if (minQualityCombobox.Items.Count == 0)
            {
                minQualityCombobox.Items.Add("Any");
                minQualityCombobox.Items.Add("Ancient");
                minQualityCombobox.SelectedIndex = 0;
            }
        }

        private void InitializeStripToolMenu()
        {
            var contextMenu = new ContextMenuStrip();
            itemsToSaveTreeView.ContextMenuStrip = contextMenu;

            itemsToSaveTreeView.NodeMouseClick += (s, e) =>
            {
                if (e.Button == MouseButtons.Right)
                    itemsToSaveTreeView.SelectedNode = e.Node;
            };
            
            itemsToSaveTreeView.AfterLabelEdit += (s, e) =>
            {
                if (e.Label != null && _isLoading == false)
                {
                    e.Node.Text = e.Label;
                    UpdateTreeFromForm();
                    PixelHelperSettings.Instance.MarkDirty();
                    UpdateGroupComboBox();
                }
            };
            
            itemsToSaveTreeView.Leave += (s, e) =>
            {
                if (itemsToSaveTreeView.SelectedNode != null && itemsToSaveTreeView.SelectedNode.IsEditing)
                {
                    itemsToSaveTreeView.SelectedNode.EndEdit(false);
                }
            };

            EnsureDefaultCategory();

            contextMenu.Opening += (s, e) =>
            {
                contextMenu.Items.Clear();
                var node = itemsToSaveTreeView.SelectedNode;

                if (node == null)
                {
                    contextMenu.Items.Add(
                        new ToolStripMenuItem(
                            "Add New Group",
                            null,
                            (snd, ea) =>
                            {
                                var newNode = new TreeNode("New Group") { Checked = true };
                                itemsToSaveTreeView.Nodes.Add(newNode);
                                itemsToSaveTreeView.SelectedNode = newNode;
                                newNode.BeginEdit();
                            }
                        )
                    );
                }
                else if (node.Parent == null)
                {
                    if (node.Text != "Default")
                    {
                        contextMenu.Items.Add(
                            new ToolStripMenuItem(
                                "Delete Group",
                                null,
                                (snd, ea) =>
                                {
                                    node.Remove();
                                    UpdateTreeFromForm();
                                    PixelHelperSettings.Instance.MarkDirty();
                                    UpdateGroupComboBox();
                                }
                            )
                        );
                        contextMenu.Items.Add(
                            new ToolStripMenuItem(
                                "Rename",
                                null,
                                (snd, ea) =>
                                {
                                    node.BeginEdit();
                                }
                            )
                        );
                    }

                    contextMenu.Items.Add(
                        new ToolStripMenuItem(
                            "Add New Group",
                            null,
                            (snd, ea) =>
                            {
                                var newNode = new TreeNode("New Group") { Checked = true };
                                itemsToSaveTreeView.Nodes.Add(newNode);
                                itemsToSaveTreeView.SelectedNode = newNode;
                                newNode.BeginEdit();
                            }
                        )
                    );
                }
                else
                {
                    contextMenu.Items.Add(
                        new ToolStripMenuItem(
                            "Delete Item",
                            null,
                            (snd, ea) =>
                            {
                                node.Remove();
                                UpdateTreeFromForm();
                                PixelHelperSettings.Instance.MarkDirty();
                            }
                        )
                    );
                }

                if (contextMenu.Items.Count == 0)
                    e.Cancel = true;
            };
        }

        private void InitializeDragAndDrop()
        {
            itemsToSaveTreeView.ItemDrag += (s, e) =>
            {
                if (e.Item is TreeNode node && node.Parent != null)
                    DoDragDrop(e.Item, DragDropEffects.Move);
            };

            itemsToSaveTreeView.DragOver += (s, e) =>
            {
                if (e.Data.GetDataPresent(typeof(TreeNode)))
                {
                    var targetPoint = itemsToSaveTreeView.PointToClient(new Point(e.X, e.Y));
                    var targetNode = itemsToSaveTreeView.GetNodeAt(targetPoint);

                    if (targetNode != null && targetNode.Parent == null)
                    {
                        e.Effect = DragDropEffects.Move;
                        itemsToSaveTreeView.SelectedNode = targetNode;
                    }
                    else
                    {
                        e.Effect = DragDropEffects.None;
                    }
                }
            };

            itemsToSaveTreeView.DragDrop += (s, e) =>
            {
                if (e.Data.GetDataPresent(typeof(TreeNode)))
                {
                    var draggedNode = (TreeNode)e.Data.GetData(typeof(TreeNode));
                    var targetPoint = itemsToSaveTreeView.PointToClient(new Point(e.X, e.Y));
                    var targetNode = itemsToSaveTreeView.GetNodeAt(targetPoint);

                    if (targetNode != null && targetNode.Parent == null)
                    {
                        draggedNode.Remove();
                        targetNode.Nodes.Add(draggedNode);
                        targetNode.Expand();
                        UpdateTreeFromForm();
                        PixelHelperSettings.Instance.MarkDirty();
                        UpdateGroupComboBox();
                    }
                }
            };
        }

        private void EnsureDefaultCategory()
        {
            var hasDefault = itemsToSaveTreeView.Nodes
                .Cast<TreeNode>()
                .Any(n => n.Text == "Default");
            if (!hasDefault)
            {
                var defaultNode = new TreeNode("Default") { Checked = true };
                itemsToSaveTreeView.Nodes.Insert(0, defaultNode);
            }
        }

        private void UpdateGroupComboBox()
        {
            groupComboBox.Items.Clear();
            EnsureDefaultCategory();

            foreach (TreeNode node in itemsToSaveTreeView.Nodes)
            {
                if (node.Parent == null)
                {
                    groupComboBox.Items.Add(node.Text);
                }
            }

            if (groupComboBox.SelectedIndex < 0 && groupComboBox.Items.Count > 0)
                groupComboBox.SelectedIndex = 0;
        }

        private void DiscordButton_Click(object sender, EventArgs e)
        {
            Process.Start(
                new ProcessStartInfo("https://discord.gg/F7egGsnBm3") { UseShellExecute = true }
            );
        }

        private void groupComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedGroup = groupComboBox.SelectedItem?.ToString() ?? "Default";
            Console.WriteLine("Selected group: " + selectedGroup);
        }

        private void addToLootFilter_Click(object sender, EventArgs e)
        {
            if (allLegsFromGame.SelectedItem == null)
            {
                MessageBox.Show(
                    "Choose item from list!",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            string itemName = allLegsFromGame.SelectedItem.ToString();

            string groupName = groupComboBox.SelectedItem?.ToString() ?? "Default";

            bool isAncient = minQualityCombobox.SelectedItem?.ToString() == "Ancient";

            string minAffix = string.IsNullOrWhiteSpace(textBox1.Text) ? null : textBox1.Text;

            var lootItem = new LootFilterItem
            {
                ItemName = itemName,
                IsAncient = isAncient,
                MinAffix = minAffix,
                GroupName = groupName
            };

            TreeNode groupNode = null;
            foreach (TreeNode node in itemsToSaveTreeView.Nodes)
            {
                if (node.Text == groupName)
                {
                    groupNode = node;
                    break;
                }
            }

            if (groupNode == null)
            {
                groupNode = new TreeNode(groupName) { Checked = true };
                itemsToSaveTreeView.Nodes.Add(groupNode);
            }

            var itemNode = new TreeNode(lootItem.ToString())
            {
                Checked = true,
                Tag = lootItem
            };

            groupNode.Nodes.Add(itemNode);
            groupNode.Expand();

            UpdateTreeFromForm();
            PixelHelperSettings.Instance.MarkDirty();
        }

        // Вспомогательный метод для обновления
        private void UpdateTreeFromForm()
        {
            PixelHelperSettings.Instance.UpdateTreeFromForm(itemsToSaveTreeView);
        }

        public void SetHelper(Helper helper)
        {
            _helper = helper;
            if (_helper != null && _zoomShouldBeEnabled)
            {
                _helper.ToggleZoom(true);
            }
        }
        private void enableZoomCheckbox_CheckedChanged(object sender, EventArgs e) 
        {
            _zoomShouldBeEnabled = enableZoomCheckbox.Checked;
            _helper?.ToggleZoom(enableZoomCheckbox.Checked);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            PixelHelperSettings.Instance.Save(this);
            base.OnFormClosed(e);
        }
    }
}
