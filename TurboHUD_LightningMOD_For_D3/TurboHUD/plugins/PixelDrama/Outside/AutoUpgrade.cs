namespace Turbo.Plugins.PixelDrama.Outside
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Windows.Forms;
    using Turbo.Plugins.Default;

    public class AutoUpgrade
    {
        private readonly IController Hud;
        private IUiElement UpgradeButton { get; set; }
        private IUiElement ItemButton { get; set; }
        private IUiElement ScrollButtonUp { get; set; }
        private IUiElement ScrollButtonDown { get; set; }
        private readonly string UrshiPanelPath =
            "Root.NormalLayer.vendor_dialog_mainPage.riftReward_dialog.LayoutRoot.gemUpgradePane.items_list._content";
        private readonly List<string> Slots = new List<string>
        {
            "Root.NormalLayer.vendor_dialog_mainPage.riftReward_dialog.LayoutRoot.gemUpgradePane.items_list._content._stackpanel._tilerow0._item0.Item",
            "Root.NormalLayer.vendor_dialog_mainPage.riftReward_dialog.LayoutRoot.gemUpgradePane.items_list._content._stackpanel._tilerow0._item1.Item",
            "Root.NormalLayer.vendor_dialog_mainPage.riftReward_dialog.LayoutRoot.gemUpgradePane.items_list._content._stackpanel._tilerow0._item2.Item",
            "Root.NormalLayer.vendor_dialog_mainPage.riftReward_dialog.LayoutRoot.gemUpgradePane.items_list._content._stackpanel._tilerow0._item3.Item",
            "Root.NormalLayer.vendor_dialog_mainPage.riftReward_dialog.LayoutRoot.gemUpgradePane.items_list._content._stackpanel._tilerow0._item4.Item",
            "Root.NormalLayer.vendor_dialog_mainPage.riftReward_dialog.LayoutRoot.gemUpgradePane.items_list._content._stackpanel._tilerow1._item10.Item",
            "Root.NormalLayer.vendor_dialog_mainPage.riftReward_dialog.LayoutRoot.gemUpgradePane.items_list._content._stackpanel._tilerow1._item6.Item",
            "Root.NormalLayer.vendor_dialog_mainPage.riftReward_dialog.LayoutRoot.gemUpgradePane.items_list._content._stackpanel._tilerow1._item7.Item",
            "Root.NormalLayer.vendor_dialog_mainPage.riftReward_dialog.LayoutRoot.gemUpgradePane.items_list._content._stackpanel._tilerow1._item8.Item",
            "Root.NormalLayer.vendor_dialog_mainPage.riftReward_dialog.LayoutRoot.gemUpgradePane.items_list._content._stackpanel._tilerow1._item9.Item",
            "Root.NormalLayer.vendor_dialog_mainPage.riftReward_dialog.LayoutRoot.gemUpgradePane.items_list._content._stackpanel._tilerow2._item12.Item",
            "Root.NormalLayer.vendor_dialog_mainPage.riftReward_dialog.LayoutRoot.gemUpgradePane.items_list._content._stackpanel._tilerow2._item13.Item",
            "Root.NormalLayer.vendor_dialog_mainPage.riftReward_dialog.LayoutRoot.gemUpgradePane.items_list._content._stackpanel._tilerow2._item14.Item",
            "Root.NormalLayer.vendor_dialog_mainPage.riftReward_dialog.LayoutRoot.gemUpgradePane.items_list._content._stackpanel._tilerow2._item15.Item",
            "Root.NormalLayer.vendor_dialog_mainPage.riftReward_dialog.LayoutRoot.gemUpgradePane.items_list._content._stackpanel._tilerow2._item16.Item",
            "Root.NormalLayer.vendor_dialog_mainPage.riftReward_dialog.LayoutRoot.gemUpgradePane.items_list._content._stackpanel._tilerow3._item18.Item",
            "Root.NormalLayer.vendor_dialog_mainPage.riftReward_dialog.LayoutRoot.gemUpgradePane.items_list._content._stackpanel._tilerow3._item19.Item",
            "Root.NormalLayer.vendor_dialog_mainPage.riftReward_dialog.LayoutRoot.gemUpgradePane.items_list._content._stackpanel._tilerow3._item20.Item",
            "Root.NormalLayer.vendor_dialog_mainPage.riftReward_dialog.LayoutRoot.gemUpgradePane.items_list._content._stackpanel._tilerow3._item21.Item",
            "Root.NormalLayer.vendor_dialog_mainPage.riftReward_dialog.LayoutRoot.gemUpgradePane.items_list._content._stackpanel._tilerow3._item22.Item",
            "Root.NormalLayer.vendor_dialog_mainPage.riftReward_dialog.LayoutRoot.gemUpgradePane.items_list._content._stackpanel._tilerow4._item24.Item",
            "Root.NormalLayer.vendor_dialog_mainPage.riftReward_dialog.LayoutRoot.gemUpgradePane.items_list._content._stackpanel._tilerow4._item25.Item",
            "Root.NormalLayer.vendor_dialog_mainPage.riftReward_dialog.LayoutRoot.gemUpgradePane.items_list._content._stackpanel._tilerow4._item26.Item",
            "Root.NormalLayer.vendor_dialog_mainPage.riftReward_dialog.LayoutRoot.gemUpgradePane.items_list._content._stackpanel._tilerow4._item27.Item",
            "Root.NormalLayer.vendor_dialog_mainPage.riftReward_dialog.LayoutRoot.gemUpgradePane.items_list._content._stackpanel._tilerow4._item28.Item",
            "Root.NormalLayer.vendor_dialog_mainPage.riftReward_dialog.LayoutRoot.gemUpgradePane.items_list._content._stackpanel._tilerow5._item30.Item",
            "Root.NormalLayer.vendor_dialog_mainPage.riftReward_dialog.LayoutRoot.gemUpgradePane.items_list._content._stackpanel._tilerow5._item31.Item",
            "Root.NormalLayer.vendor_dialog_mainPage.riftReward_dialog.LayoutRoot.gemUpgradePane.items_list._content._stackpanel._tilerow5._item32.Item",
            "Root.NormalLayer.vendor_dialog_mainPage.riftReward_dialog.LayoutRoot.gemUpgradePane.items_list._content._stackpanel._tilerow5._item33.Item",
            "Root.NormalLayer.vendor_dialog_mainPage.riftReward_dialog.LayoutRoot.gemUpgradePane.items_list._content._stackpanel._tilerow5._item34.Item",
            "Root.NormalLayer.vendor_dialog_mainPage.riftReward_dialog.LayoutRoot.gemUpgradePane.items_list._content._stackpanel._tilerow6._item36.Item",
            "Root.NormalLayer.vendor_dialog_mainPage.riftReward_dialog.LayoutRoot.gemUpgradePane.items_list._content._stackpanel._tilerow6._item37.Item",
            "Root.NormalLayer.vendor_dialog_mainPage.riftReward_dialog.LayoutRoot.gemUpgradePane.items_list._content._stackpanel._tilerow6._item38.Item",
            "Root.NormalLayer.vendor_dialog_mainPage.riftReward_dialog.LayoutRoot.gemUpgradePane.items_list._content._stackpanel._tilerow6._item39.Item",
            "Root.NormalLayer.vendor_dialog_mainPage.riftReward_dialog.LayoutRoot.gemUpgradePane.items_list._content._stackpanel._tilerow6._item40.Item",
            "Root.NormalLayer.vendor_dialog_mainPage.riftReward_dialog.LayoutRoot.gemUpgradePane.items_list._content._stackpanel._tilerow7._item42.Item",
            "Root.NormalLayer.vendor_dialog_mainPage.riftReward_dialog.LayoutRoot.gemUpgradePane.items_list._content._stackpanel._tilerow7._item43.Item",
            "Root.NormalLayer.vendor_dialog_mainPage.riftReward_dialog.LayoutRoot.gemUpgradePane.items_list._content._stackpanel._tilerow7._item44.Item",
            "Root.NormalLayer.vendor_dialog_mainPage.riftReward_dialog.LayoutRoot.gemUpgradePane.items_list._content._stackpanel._tilerow7._item45.Item",
            "Root.NormalLayer.vendor_dialog_mainPage.riftReward_dialog.LayoutRoot.gemUpgradePane.items_list._content._stackpanel._tilerow7._item46.Item",
            "Root.NormalLayer.vendor_dialog_mainPage.riftReward_dialog.LayoutRoot.gemUpgradePane.items_list._content._stackpanel._tilerow8._item48.Item",
            "Root.NormalLayer.vendor_dialog_mainPage.riftReward_dialog.LayoutRoot.gemUpgradePane.items_list._content._stackpanel._tilerow8._item49.Item",
            "Root.NormalLayer.vendor_dialog_mainPage.riftReward_dialog.LayoutRoot.gemUpgradePane.items_list._content._stackpanel._tilerow8._item50.Item",
            "Root.NormalLayer.vendor_dialog_mainPage.riftReward_dialog.LayoutRoot.gemUpgradePane.items_list._content._stackpanel._tilerow8._item51.Item",
            "Root.NormalLayer.vendor_dialog_mainPage.riftReward_dialog.LayoutRoot.gemUpgradePane.items_list._content._stackpanel._tilerow8._item52.Item",
            "Root.NormalLayer.vendor_dialog_mainPage.riftReward_dialog.LayoutRoot.gemUpgradePane.items_list._content._stackpanel._tilerow9._item54.Item",
            "Root.NormalLayer.vendor_dialog_mainPage.riftReward_dialog.LayoutRoot.gemUpgradePane.items_list._content._stackpanel._tilerow9._item55.Item",
            "Root.NormalLayer.vendor_dialog_mainPage.riftReward_dialog.LayoutRoot.gemUpgradePane.items_list._content._stackpanel._tilerow9._item56.Item",
            "Root.NormalLayer.vendor_dialog_mainPage.riftReward_dialog.LayoutRoot.gemUpgradePane.items_list._content._stackpanel._tilerow9._item57.Item",
            "Root.NormalLayer.vendor_dialog_mainPage.riftReward_dialog.LayoutRoot.gemUpgradePane.items_list._content._stackpanel._tilerow9._item58.Item",
        };
        private readonly List<(IItem item, IUiElement slotElement)> reusableGemSlots =
            new List<(IItem item, IUiElement slotElement)>();

        private int UpgradeTimes;
        private IWatch _timer;
        private DateTime lastActionTime = DateTime.MinValue;
        private DateTime lastGemSelectionTime = DateTime.MinValue;
        private DateTime lastScrollTime = DateTime.MinValue;

        public AutoUpgrade(IController hud)
        {
            Hud = hud;

            UpgradeButton = Hud.Render.RegisterUiElement(
                "Root.NormalLayer.vendor_dialog_mainPage.riftReward_dialog.LayoutRoot.gemUpgradePane.upgrade_button",
                null,
                null
            );
            ItemButton = Hud.Render.RegisterUiElement(
                "Root.NormalLayer.vendor_dialog_mainPage.riftReward_dialog.LayoutRoot.gemUpgradePane.item_button",
                null,
                null
            );
            ScrollButtonUp = Hud.Render.RegisterUiElement(
                "Root.NormalLayer.vendor_dialog_mainPage.riftReward_dialog.LayoutRoot.gemUpgradePane.items_list._Scrollbar.up",
                null,
                null
            );
            ScrollButtonDown = Hud.Render.RegisterUiElement(
                "Root.NormalLayer.vendor_dialog_mainPage.riftReward_dialog.LayoutRoot.gemUpgradePane.items_list._Scrollbar.down",
                null,
                null
            );
            _timer = Hud.Time.CreateWatch();
        }

        public void TryToUpgrade()
        {
            var urshiPanel = Hud.Render.GetUiElement(UrshiPanelPath);
            if (urshiPanel == null || !urshiPanel.Visible)
            {
                if (_timer.IsRunning)
                {
                    _timer.Reset();
                    _timer.Stop();
                }
                return;
            }
            else
            {
                if (!_timer.IsRunning)
                    _timer.Start();
            }

            UpgradeTimes =
                Hud.Game.Me.GetAttributeValueAsInt(Hud.Sno.Attributes.Jewel_Upgrades_Bonus, 2147483647, 0)
                + Hud.Game.Me.GetAttributeValueAsInt(Hud.Sno.Attributes.Jewel_Upgrades_Max, 2147483647, 0)
                - Hud.Game.Me.GetAttributeValueAsInt(Hud.Sno.Attributes.Jewel_Upgrades_Used, 2147483647, 0);

            reusableGemSlots.Clear();
            foreach (var slotPath in Slots)
            {
                var slotElement = Hud.Render.GetUiElement(slotPath);
                if (slotElement == null || !slotElement.Visible)
                    continue;

                var item = Hud.Game.Items.FirstOrDefault(x => x.AcdId == slotElement.LegendaryGemAcdId);
                if (item == null)
                    continue;

                reusableGemSlots.Add((item, slotElement));
            }

            var targetGem = reusableGemSlots
                .OrderBy(t => t.item.JewelRank >= -1 ? t.item.JewelRank : int.MaxValue)
                .FirstOrDefault(t => IsSuitable(t.item));

            if (targetGem.item == null || UpgradeTimes <= 0)
                return;

            var rect = targetGem.slotElement.Rectangle;
            int tempX = Hud.Window.CursorX;
            int tempY = Hud.Window.CursorY;

            if (rect.Y >= 600 && rect.Y <= 800)
            {
                if (ItemButton.AnimState == -1 && _timer.ElapsedMilliseconds > 80)
                {
                    Hud.Interaction.MouseMove((int)(rect.X + rect.Width / 2), (int)(rect.Y + rect.Height / 2));
                    Hud.Interaction.MouseDown(MouseButtons.Left);
                    Hud.Interaction.MouseUp(MouseButtons.Left);
                    Hud.Interaction.MouseMove(tempX, tempY);
                    _timer.Restart();
                    return;
                }

                if (UpgradeButton.AnimState != 27 && _timer.ElapsedMilliseconds > 100)
                {
                    Hud.Interaction.MouseMove((int)(rect.X + rect.Width / 2), (int)(rect.Y + rect.Height / 2));
                    Hud.Interaction.MouseDown(MouseButtons.Left);
                    Hud.Interaction.MouseUp(MouseButtons.Left);
                    Hud.Interaction.ClickUiElement(MouseButtons.Left, UpgradeButton);
                    Hud.Interaction.MouseMove(tempX, tempY);
                    _timer.Restart();
                }

                if (UpgradeTimes < 3 && Hud.Game.Me.AnimationState != AcdAnimationState.CastingPortal)
                {
                    Hud.Interaction.DoAction(ActionKey.TownPortal);
                }
            }
            else
            {
                if (_timer.ElapsedMilliseconds > 60)
                {
                    if (ScrollButtonUp?.Visible == true && ScrollButtonDown?.Visible == true)
                    {
                        if (rect.Y >= 800)
                            Hud.Interaction.ClickUiElement(MouseButtons.Left, ScrollButtonDown);
                        else if (rect.Y <= 600)
                            Hud.Interaction.ClickUiElement(MouseButtons.Left, ScrollButtonUp);

                        Hud.Interaction.MouseMove(tempX, tempY);
                        _timer.Restart();
                    }
                }
            }
        }

        private bool IsSuitable(IItem jewel)
        {
            bool AllMax = false;
            int max = 150;
            switch (jewel.SnoItem.NameEnglish)
            {
                case "Boon of the Hoarder":
                    max = 50;
                    break;
                case "Iceblink":
                    max = 50;
                    break;
                case "Legacy of Dreams":
                    max = 99;
                    break;
                case "Esoteric Alteration":
                    max = 100;
                    break;
                case "Mutilation Guard":
                    max = 100;
                    break;
                case "Gogok of Swiftness":
                    max = 150;
                    break;
            }
            if (jewel.JewelRank < max)
            {
                AllMax = true;
            }
            return AllMax;
        }
    }
}