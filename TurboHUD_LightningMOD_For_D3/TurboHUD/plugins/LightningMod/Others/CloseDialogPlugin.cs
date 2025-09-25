namespace Turbo.Plugins.LightningMod
{
    using Turbo.Plugins.Default;

    public class CloseDialogPlugin : BasePlugin, IAfterCollectHandler, IInGameTopPainter
    {

        private IUiElement uiDialog { get; set; }
        private IUiElement followerswapDialog { get; set; }
        private IUiElement scriptedDialog { get; set; }
        public IFont InfoFont { get; private set; }
        protected IUiElement uiTransmuteButton;
        protected IUiElement uiSkilllist;
        protected IUiElement uiParagonPointSelect;
        protected IUiElement uiProfile;
        protected IUiElement uiLeaderboard;
        protected IUiElement uiAchievements;
        protected IUiElement uiStore;
        protected IUiElement uiGamemenu;
        protected IUiElement uiGuild;
        protected IUiElement uiSocialDialogs;
        protected IUiElement ChatUI;
        private string str_Info;
        private IWatch delay;
        public CloseDialogPlugin()
        {
            Enabled = true;
        }
        public override void Load(IController hud)
        {
            base.Load(hud);
            uiDialog = Hud.Render.RegisterUiElement("Root.NormalLayer.conversation_dialog_main", null, null);//剧情任务对话框
            followerswapDialog = Hud.Render.RegisterUiElement("Root.TopLayer.follower_swap", null, null);//随从确认对话框
            scriptedDialog = Hud.Render.RegisterUiElement("Root.TopLayer.scripted_sequence", null, null);//BOSS场景脚本
            InfoFont = Hud.Render.CreateFont("tahoma", 8, 255, 200, 200, 0, true, false, 255, 0, 0, 0, true);
            uiTransmuteButton = Hud.Render.RegisterUiElement("Root.NormalLayer.vendor_dialog_mainPage.transmute_dialog.LayoutRoot.transmute_button", null, null);
            uiSkilllist = Hud.Render.RegisterUiElement("Root.NormalLayer.SkillPane_main.LayoutRoot.SkillsList", null, null);
            uiParagonPointSelect = Hud.Render.RegisterUiElement("Root.NormalLayer.Paragon_main.LayoutRoot.ParagonPointSelect", null, null);
            uiProfile = Hud.Render.RegisterUiElement("Root.NormalLayer.BattleNetProfile_main.LayoutRoot.OverlayContainer", null, null);
            uiLeaderboard = Hud.Render.RegisterUiElement("Root.NormalLayer.BattleNetLeaderboard_main.LayoutRoot.OverlayContainer", null, null);
            uiAchievements = Hud.Render.RegisterUiElement("Root.NormalLayer.BattleNetAchievements_main.LayoutRoot.OverlayContainer", null, null);
            uiStore = Hud.Render.RegisterUiElement("Root.NormalLayer.BattleNetStore_main.LayoutRoot.OverlayContainer", null, null);
            uiGamemenu = Hud.Render.RegisterUiElement("Root.NormalLayer.gamemenu_dialog.gamemenu_bkgrnd.button_resumeGame", null, null);
            uiGuild = Hud.Render.RegisterUiElement("Root.NormalLayer.Guild_main.LayoutRoot.OverlayContainer", null, null);
            uiSocialDialogs = Hud.Render.RegisterUiElement("Root.TopLayer.BattleNetSocialDialogs_main.LayoutRoot.DialogWriteNote.DialogWriteNoteTitle", null, null);
            ChatUI = Hud.Render.RegisterUiElement("Root.NormalLayer.chatentry_dialog_backgroundScreen.chatentry_content.chat_editline", null, null);
            delay = Hud.Time.CreateWatch();
            if (Hud.CurrentLanguage == Language.zhCN)
            {
                str_Info = "请在“游戏选项”“按键绑定”中设置“关闭所有打开的窗口”热键后可自动跳过所有剧情对话内容";
            }
            else if (Hud.CurrentLanguage == Language.zhTW)
            {
                str_Info = "請在“設定”“按鍵設定”中設置“關閉所有已開啟視窗”熱鍵后可自動跳過所有劇情對話內容";
            }
            else if (Hud.CurrentLanguage == Language.ruRU)
            {
                str_Info = "Назначте клавишу для <Закрыть все открытые окна> в <НАСТРОЙКИ><ГОРЯЧИИ КЛАВИШИ>";
            }
            else
            {
                str_Info = "Plese set <Close All Open Windows> key in <OPTIONS><KEY BINDINGS>";
            }
        }

        public void AfterCollect()
        {
            if (isUIVisible()) return;
            if (followerswapDialog.Visible || scriptedDialog.Visible)//BOSS场景脚本和随从确认对话框时200毫秒一次点击关闭键，防止掉线
            {
                if (Hud.Interaction.IsHotKeySet(ActionKey.Close))
                {
                    if (!delay.IsRunning || (delay.IsRunning && delay.ElapsedMilliseconds > 200))
                    {
                        Hud.Interaction.DoActionAutoShift(ActionKey.Close);
                        delay.Restart();
                    }

                }
            }
            else if (uiDialog.Visible)//剧情对话时30毫秒一次点击关闭键
            {
                if (Hud.Interaction.IsHotKeySet(ActionKey.Close))
                {
                    if (!delay.IsRunning || (delay.IsRunning && delay.ElapsedMilliseconds > 30))
                    {
                        Hud.Interaction.DoActionAutoShift(ActionKey.Close);
                        delay.Restart();
                    }

                }
            }
            else
            {
                if (delay.IsRunning)
                {
                    delay.Stop();
                }
            }
        }

        private bool isUIVisible()
        {
            if((Hud.Game.MapMode == MapMode.WaypointMap) || (Hud.Game.MapMode == MapMode.ActMap) || (Hud.Game.MapMode == MapMode.Map) || uiTransmuteButton?.Visible == true || uiSkilllist?.Visible == true || uiParagonPointSelect?.Visible == true || uiProfile?.Visible == true || uiLeaderboard?.Visible == true || uiAchievements?.Visible == true || uiStore?.Visible == true || uiGamemenu?.Visible == true || uiGuild?.Visible == true || uiSocialDialogs?.Visible == true || Hud.Inventory.InventoryMainUiElement.Visible || ChatUI.Visible)
            {
                delay.Stop();
                return true;
            }
            else
            {
                return false;
            }
        }
        public void PaintTopInGame(ClipState clipState)
        {
            if (isUIVisible()) return;
            if (!uiDialog.Visible && !followerswapDialog.Visible && !scriptedDialog.Visible) return;
            if (Hud.Interaction.IsHotKeySet(ActionKey.Close)) return;
            var layout = InfoFont.GetTextLayout(str_Info);
            InfoFont.DrawText(layout, uiDialog.Rectangle.Left, uiDialog.Rectangle.Top);
        }
    }
}