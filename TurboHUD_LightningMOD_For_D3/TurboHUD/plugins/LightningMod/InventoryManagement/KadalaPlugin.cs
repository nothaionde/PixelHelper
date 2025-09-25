namespace Turbo.Plugins.LightningMod
{
    using SharpDX.DirectInput;
    using System.Windows.Forms;
    using Turbo.Plugins.Default;
    using System.Text;
    using System;
    using Turbo.Plugins.glq;
    public class KadalaPlugin : BasePlugin, IKeyEventHandler, IAfterCollectHandler, IInGameTopPainter
    {
        public bool Running { get; private set; }
        public IKeyEvent ToggleKeyEvent { get; set; }
        private IUiElement KadalaDialog { get; set; }
        private IUiElement BuyUI { get; set; }
        public IFont HeaderFont { get; private set; }
        public IFont InfoFont { get; private set; }
        public int IntervalMilliseconds { get; set; } = 10;
        private IWatch _timer;
        private IUiElement cast;
        private string str_Header;
        private string str_Info;
        private string str_Info2;
        private string str_NoCash;
        private string str_Running;
        public KadalaPlugin()
        {
            Enabled = true;
            Running = false;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            ToggleKeyEvent = Hud.Input.CreateKeyEvent(true, Key.F4, false, false, false);
            HeaderFont = Hud.Render.CreateFont("tahoma", 10, 255, 200, 200, 100, true, false, 255, 0, 0, 0, true);
            InfoFont = Hud.Render.CreateFont("tahoma", 8, 255, 200, 200, 0, true, false, 255, 0, 0, 0, true);
            KadalaDialog = Hud.Render.RegisterUiElement("Root.NormalLayer.shop_dialog_mainPage.panel", null, null);
            BuyUI = Hud.Render.RegisterUiElement("Root.TopLayer.item 2.stack.frame_instruction", null, null);
            cast = Hud.Render.RegisterUiElement("Root.TopLayer.item 2.stack.frame_cost.cost", null,null);
            _timer = Hud.Time.CreateAndStartWatch();
        }

        public void OnKeyEvent(IKeyEvent keyEvent)
        {
            if (ToggleKeyEvent.Matches(keyEvent))
            {
                if (keyEvent.IsPressed && KadalaDialog?.Visible == true)
                {
                    Running = !Running;
                    if(Running == false) Hud.Interaction.MouseUp(MouseButtons.Right);
                }
            }
        }

        public void AfterCollect()
        {
            if (!_timer.TimerTest(IntervalMilliseconds)) return;
            if (KadalaDialog?.Visible == false) return;
            if (Running)
            {
                Hud.Interaction.MouseDown(MouseButtons.Right);
                Hud.Interaction.MouseUp(MouseButtons.Right);
                _timer.Restart();
            }
        }
        
        public void PaintTopInGame(ClipState clipState)
        {
            try
            {
                if (KadalaDialog?.Visible == false)
                {
                    if (Running)
                    {
                        Hud.Interaction.MouseUp(MouseButtons.Right);
                        Running = false;
                    }
                    return;
                }

                if (Hud.CurrentLanguage == Language.zhCN)
                {
                    str_Header = "【雷电宏-快速购买】";
                    str_Info = "单击 " + ToggleKeyEvent.ToString() + " 开始自动购买";
                    str_Info2 = "鼠标移动到需要购买的物品\r\n单击 " + ToggleKeyEvent.ToString() + " 开始自动购买";
                    str_NoCash = "没有足够背包空间或者血岩";
                    str_Running = "自动购买中...\r\n按住 " + ToggleKeyEvent.ToString() + " 停止";
                }
                else if (Hud.CurrentLanguage == Language.zhTW)
                {
                    str_Header = "【雷電宏-快速購買】";
                    str_Info = "單擊 " + ToggleKeyEvent.ToString() + " 開始自動購買";
                    str_Info2 = "鼠標移動到需要購買的物品\r\n單擊 " + ToggleKeyEvent.ToString() + " 開始自動購買";
                    str_NoCash = "沒有足夠背包空間或者血巖";
                    str_Running = "自動購買中...\r\n按住 " + ToggleKeyEvent.ToString() + " 停止";
                }
                else if (Hud.CurrentLanguage == Language.ruRU)
                {
                    str_Header = "【МОД - Быстрая Покупка】";
                    str_Info = "Нажмите " + ToggleKeyEvent.ToString() + " для Старта";
                    str_Info2 = "Направить курсор на выбранный предмет для покупки\r\nНажать " + ToggleKeyEvent.ToString() + " для Старта";
                    str_NoCash = "Нет места в или недостаточно Кровавых Осколков";
                    str_Running = "Аитопокупка...\r\nНажать " + ToggleKeyEvent.ToString() + " для Остановки";
                }
                else
                {
                    str_Header = "【Quick purchase-MOD】";
                    str_Info = "Click " + ToggleKeyEvent.ToString() + " To Start";
                    str_Info2 = "Move the mouse to the item you need to buy\r\nClick " + ToggleKeyEvent.ToString() + " To Start";
                    str_NoCash = "Not enough space or blood shard";
                    str_Running = "Auto purchasing...\r\nClick " + ToggleKeyEvent.ToString() + " To Stop";
                }
                if (cast == null) return;
                var castshard = cast.ReadText(Encoding.UTF8, false);
                
                int _cast = 0;
                if (castshard.Contains("x1_shard"))
                {
                    castshard = PublicClassPlugin.Between(castshard, "{/c}", " {icon");
                    if (int.TryParse(castshard, out _cast) == false) return;
                }
                var inventoryFreeSpace = Hud.Game.Me.InventorySpaceTotal - Hud.Game.InventorySpaceUsed;
                bool Cannotbuy = false;
                if (inventoryFreeSpace < 2 || Hud.Game.Me.Materials.BloodShard < _cast)//没有背包空间或没有足够血岩碎片时停止购买
                {
                    if (Running)
                    {
                        Hud.Interaction.MouseUp(MouseButtons.Right);
                        Running = false;
                        return;
                    }
                    else
                    {
                        Cannotbuy = true;
                    }
                }
                var y = KadalaDialog.Rectangle.Y + KadalaDialog.Rectangle.Height * 0.02f;
                var layout = HeaderFont.GetTextLayout(str_Header);
                HeaderFont.DrawText(layout, KadalaDialog.Rectangle.X + (KadalaDialog.Rectangle.Width * 0.04f), y);
                y += layout.Metrics.Height * 1.3f;
                if (Running)
                {
                    if (BuyUI?.Visible == false)
                    {
                        if (Running)
                        {
                            Hud.Interaction.MouseUp(MouseButtons.Right);
                            Running = false;
                        }
                        return;
                    }
                    else
                    {
                        layout = InfoFont.GetTextLayout(str_Running);
                    }
                    InfoFont.DrawText(layout, KadalaDialog.Rectangle.X + (KadalaDialog.Rectangle.Width * 0.04f), y);
                }
                else
                {
                    if (BuyUI?.Visible == true)
                    {
                        if (Cannotbuy)
                        {
                            layout = InfoFont.GetTextLayout(str_NoCash);
                        }
                        else
                        {
                            layout = InfoFont.GetTextLayout(str_Info);
                        }
                    }
                    else
                    {
                        layout = InfoFont.GetTextLayout(str_Info2);
                    }
                    InfoFont.DrawText(layout, KadalaDialog.Rectangle.X + (KadalaDialog.Rectangle.Width * 0.04f), y);
                }
            }
            catch (NullReferenceException) { }
            finally
            {
            }
        }
    }
}