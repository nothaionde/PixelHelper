namespace Turbo.Plugins.LightningMod
{
    using Turbo.Plugins.Default;
    public class CloseQuestDialogPlugin : BasePlugin, IAfterCollectHandler, IInGameTopPainter
    {
        public IFont InfoFont { get; private set; }
        protected IUiElement uiQuestReward;
        protected IUiElement uiBountyReward;
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
        public CloseQuestDialogPlugin()
        {
            Enabled = true;
        }
        public override void Load(IController hud)
        {
            base.Load(hud);
            InfoFont = Hud.Render.CreateFont("tahoma", 8, 255, 200, 200, 0, true, false, 255, 0, 0, 0, true);
            uiQuestReward = Hud.Render.RegisterUiElement("Root.NormalLayer.questreward_dialog", null, null);
            uiBountyReward = Hud.Render.RegisterUiElement("Root.NormalLayer.BountyReward_main.LayoutRoot", null, null);
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
            if (Hud.CurrentLanguage == Language.zhCN)
            {
                str_Info = "请在“游戏选项”“按键绑定”中设置“关闭所有打开的窗口”热键后可自动跳过所有奖励框";
            }
            else if (Hud.CurrentLanguage == Language.zhTW)
            {
                str_Info = "請在“設定”“按鍵設定”中設置“關閉所有已開啟視窗”熱鍵后可自動跳過所有獎勵框";
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
            if (!Hud.Interaction.IsHotKeySet(ActionKey.Close)) return;
            if (uiQuestReward?.Visible == true || uiBountyReward?.Visible == true)
            {
                Hud.Interaction.DoActionAutoShift(ActionKey.Close);
            }
        }
        private bool isUIVisible()
        {
            if ((Hud.Game.MapMode == MapMode.WaypointMap) || (Hud.Game.MapMode == MapMode.ActMap) || (Hud.Game.MapMode == MapMode.Map) || uiTransmuteButton?.Visible == true || uiSkilllist?.Visible == true || uiParagonPointSelect?.Visible == true || uiProfile?.Visible == true || uiLeaderboard?.Visible == true || uiAchievements?.Visible == true || uiStore?.Visible == true || uiGamemenu?.Visible == true || uiGuild?.Visible == true || uiSocialDialogs?.Visible == true || Hud.Inventory.InventoryMainUiElement.Visible || ChatUI.Visible)
            {
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
            if (Hud.Interaction.IsHotKeySet(ActionKey.Close)) return;
            if (uiQuestReward?.Visible == true)
            {
                var layout = InfoFont.GetTextLayout(str_Info);
                InfoFont.DrawText(layout, uiQuestReward.Rectangle.Left, uiQuestReward.Rectangle.Top);
            }
            if (uiBountyReward?.Visible == true)
            {
                var layout = InfoFont.GetTextLayout(str_Info);
                InfoFont.DrawText(layout, uiBountyReward.Rectangle.Left, uiBountyReward.Rectangle.Top);
            }


        }
    }
}