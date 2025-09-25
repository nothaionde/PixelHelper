namespace Turbo.Plugins.LightningMod
{
    using SharpDX.DirectInput;
    using System.Windows.Forms;
    using Turbo.Plugins.Default;
    using System;
    using System.Linq;
    public class UpgradeLegendaryGemPlugin : BasePlugin, IKeyEventHandler, IAfterCollectHandler, IInGameTopPainter
    {
        public bool Running { get; private set; }
        public bool AutoRunning { get; private set; }
        private bool stop;
        public IKeyEvent ToggleKeyEvent { get; set; }
        private IUiElement gemUpgradePane { get; set; }
        private IUiElement UpgradeButton { get; set; }
        private IUiElement itembutton { get; set; }
        private IUiElement item1 { get; set; }
        private IUiElement Upgrading { get; set; }
        public IFont HeaderFont { get; private set; }
        public IFont InfoFont { get; private set; }
        private IWatch _timer;
        private string str_Header;
        private string str_Info;
        private string str_Info2;
        private string str_Running;
        private string str_AutoRunning; 
        private int UpgradeTimes;
        public UpgradeLegendaryGemPlugin()
        {
            Enabled = false;
            Running = false;
            AutoRunning = false;
            stop = false;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            ToggleKeyEvent = Hud.Input.CreateKeyEvent(true, Key.F4, false, false, false);
            HeaderFont = Hud.Render.CreateFont("tahoma", 10, 255, 200, 200, 100, true, false, 255, 0, 0, 0, true);
            InfoFont = Hud.Render.CreateFont("tahoma", 8, 255, 200, 200, 0, true, false, 255, 0, 0, 0, true);
            gemUpgradePane = Hud.Render.RegisterUiElement("Root.NormalLayer.vendor_dialog_mainPage.riftReward_dialog.LayoutRoot.gemUpgradePane", null, null);
            UpgradeButton = Hud.Render.RegisterUiElement("Root.NormalLayer.vendor_dialog_mainPage.riftReward_dialog.LayoutRoot.gemUpgradePane.upgrade_button", null, null);
            itembutton = Hud.Render.RegisterUiElement("Root.NormalLayer.vendor_dialog_mainPage.riftReward_dialog.LayoutRoot.gemUpgradePane.item_button", null, null);
            item1 = Hud.Render.RegisterUiElement("Root.NormalLayer.vendor_dialog_mainPage.riftReward_dialog.LayoutRoot.gemUpgradePane.items_list._content._stackpanel._tilerow0._item0.Item", null, null);
            Upgrading = Hud.Render.RegisterUiElement("Root.NormalLayer.vendor_dialog_mainPage.riftReward_dialog.LayoutRoot.gemUpgradePane.gemStatusText", null, null);
            _timer = Hud.Time.CreateWatch();
            
        }

        public void OnKeyEvent(IKeyEvent keyEvent)
        {
            if (ToggleKeyEvent.Matches(keyEvent))
            {
                if (keyEvent.IsPressed && gemUpgradePane?.Visible == true)
                {
                    if(UpgradeButton.AnimState != 27)
                    {
                        Running = !Running;
                    }
                    else
                    {
                        AutoRunning = false;
                        Running = false;
                        stop = true;
                    }
                    
                }
            }
        }
        private bool isAllJewelRankMax()
        {
            bool AllMax = true;
            foreach(var jewel in Hud.Game.Items.Where(item => item.Location != ItemLocation.Floor && item.IsLegendary && item.SnoItem.MainGroupCode == "gems_unique"))
            {
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
                }
                if(jewel.JewelRank < max)
                {
                    AllMax = false;
                }
            }
            return AllMax;
        }

        private bool PresetGem()
        {
            return Hud.Inventory.ItemsInInventory.Any(x => x.InventoryX == 0 && x.InventoryY == 0 && x.IsLegendary && x.SnoItem.MainGroupCode == "gems_unique" && (isAllJewelRankMax() ? true : x.JewelRank < 150));
        }
        public void PressUpgradeButton()
        {
            if (UpgradeButton.AnimState != 27 && _timer.IsRunning && _timer.ElapsedMilliseconds > 200)
            {
                int tempX = Hud.Window.CursorX;
                int tempY = Hud.Window.CursorY;
                Hud.Interaction.ClickUiElement(MouseButtons.Left, UpgradeButton);//点击升级传奇宝石
                Hud.Interaction.MouseMove(tempX, tempY);
                _timer.Restart();
            }
            if (UpgradeTimes < 3 && Hud.Game.Me.AnimationState != AcdAnimationState.CastingPortal)
            {
                Hud.Interaction.DoAction(ActionKey.TownPortal);
            }
        }
        public void AfterCollect()
        {
            if (gemUpgradePane?.Visible == false)
            {
                if (_timer.IsRunning) { _timer.Reset(); _timer.Stop(); } 
                return;
            }
            else
            {
                if (!_timer.IsRunning) _timer.Start();
            }
            UpgradeTimes = Hud.Game.Me.GetAttributeValueAsInt(Hud.Sno.Attributes.Jewel_Upgrades_Bonus, 2147483647, 0) + Hud.Game.Me.GetAttributeValueAsInt(Hud.Sno.Attributes.Jewel_Upgrades_Max, 2147483647, 0) - Hud.Game.Me.GetAttributeValueAsInt(Hud.Sno.Attributes.Jewel_Upgrades_Used, 2147483647, 0);
            if (UpgradeTimes == 0)
            {
                Running = false;
                AutoRunning = false;
                stop = false;
                return;
            }
            if (Running || AutoRunning)
            {
                PressUpgradeButton();
            }
            else if(PresetGem() && stop == false)
            {
                if (itembutton.AnimState == -1)
                {
                    if (_timer.IsRunning && _timer.ElapsedMilliseconds > 200)
                    {
                        int tempX = Hud.Window.CursorX;
                        int tempY = Hud.Window.CursorY;
                        Hud.Interaction.ClickUiElement(MouseButtons.Left, item1);
                        Hud.Interaction.MouseMove(tempX, tempY);
                        _timer.Restart();
                    }
                } else if (AutoRunning == false && stop == false && _timer.IsRunning && _timer.ElapsedMilliseconds > 500)
                {
                    if(UpgradeButton.AnimState != 27)
                    {
                        AutoRunning = true;
                    }
                    else
                    {
                        stop = true;
                    }
                }
            }
        }
        
        public void PaintTopInGame(ClipState clipState)
        {
            try
            {
                if (gemUpgradePane?.Visible == false)
                {
                    Running = false;
                    AutoRunning = false;
                    return;
                }
                if (UpgradeTimes == 0)
                {
                    return;
                }
                if (Hud.CurrentLanguage == Language.zhCN)
                {
                    str_Header = "【雷电宏-升级传奇宝石】";
                    str_Info = "单击 " + ToggleKeyEvent.ToString() + " 开始自动升级传奇宝石\r\n如果背包中第一格是传奇宝石则将自动开始并升级它";
                    str_Info2 = "请选择你要升级的传奇宝石\r\n如果背包中第一格是传奇宝石则将自动开始并升级它";
                    str_Running = "自动升级传奇宝石中...\r\n按住 " + ToggleKeyEvent.ToString() + " 停止";
                    str_AutoRunning = "自动升级背包第1格的传奇宝石中...\r\n按住 " + ToggleKeyEvent.ToString() + " 停止";
                }
                else if (Hud.CurrentLanguage == Language.zhTW)
                {
                    str_Header = "【雷電宏-升級傳奇寶石】";
                    str_Info = "單擊 " + ToggleKeyEvent.ToString() + " 開始自動升級傳奇寶石\r\n如果背包中第一格是傳奇寶石則將自動開始並升級它";
                    str_Info2 = "請選擇你要升級的傳奇寶石\r\n如果背包中第一格是傳奇寶石則將自動開始並升級它";
                    str_Running = "自動升級背包傳奇寶石中...\r\n按住 " + ToggleKeyEvent.ToString() + " 停止";
                    str_AutoRunning = "自动升级背包第1格的传奇宝石中...\r\n按住 " + ToggleKeyEvent.ToString() + " 停止";
                }
                else if (Hud.CurrentLanguage == Language.ruRU)
                {
                    str_Header = "【МОД-Улучшение Легендарного Самоцвета】";
                    str_Info = "Нажать " + ToggleKeyEvent.ToString() + " для Старта\r\nЕсли в первой клетке инвентаря легендарный камень,то запустится автоулучшение";
                    str_Info2 = "Выбрать легендарный самоцвет для улучшения\r\nЕсли в первой клетке инвентаря легендарный камень,то запустится автоулучшение";
                    str_Running = "АвтоУлучшение...\r\nНажать " + ToggleKeyEvent.ToString() + " для Остановки";
                    str_AutoRunning = "АвтоУлучшение Легендарного Самоцвета в первой ячейки инвентаря...\r\nНажать " + ToggleKeyEvent.ToString() + " для Остановки";
                }
                else
                {
                    str_Header = "【UpgradeLegendaryGem-MOD】";
                    str_Info = "Click " + ToggleKeyEvent.ToString() + " To Start\r\nIf the first grid in the inventory is legendary gem, it will start and upgrade automatically";
                    str_Info2 = "Please choose which LegendaryGem you want to upgrade\r\nIf the first grid in the inventory is legendary gem, it will start and upgrade automatically";
                    str_Running = "Auto Upgrading...\r\nClick " + ToggleKeyEvent.ToString() + " To Stop";
                    str_AutoRunning = "Auto Upgrading the LegendaryGem in the first grid of inventory...\r\nClick " + ToggleKeyEvent.ToString() + " To Stop";
                }

                var y = gemUpgradePane.Rectangle.Y + gemUpgradePane.Rectangle.Height * 0.02f;
                var layout = HeaderFont.GetTextLayout(str_Header);
                HeaderFont.DrawText(layout, gemUpgradePane.Rectangle.X + (gemUpgradePane.Rectangle.Width * 0.04f), y);
                y += layout.Metrics.Height * 1.3f;
                if (Running || AutoRunning)
                {
                    layout = InfoFont.GetTextLayout(AutoRunning ? str_AutoRunning : str_Running);
                    InfoFont.DrawText(layout, gemUpgradePane.Rectangle.X + (gemUpgradePane.Rectangle.Width * 0.04f), y);
                }
                if(!Running && !AutoRunning)
                {
                    if (UpgradeButton.AnimState != 27)
                    {
                        layout = InfoFont.GetTextLayout(str_Info);//未选择传奇宝石
                    }
                    else
                    {
                        layout = InfoFont.GetTextLayout(str_Info2);//选择了传奇宝石
                    }
                    InfoFont.DrawText(layout, gemUpgradePane.Rectangle.X + (gemUpgradePane.Rectangle.Width * 0.04f), y);
                }
            }
            catch (NullReferenceException) { }
            finally
            {
            }
        }
    }
}