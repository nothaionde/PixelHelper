namespace Turbo.Plugins.PixelDrama.Town
{
    using System;
    using System.Text;
    using System.Windows.Forms;
    using Turbo.Plugins.Default;

    public class AutoOpenRift
    {
        private readonly IController Hud;
        protected IUiElement uiGRmainPage;
        protected IUiElement uiOnGreaterRift;
        protected IUiElement uiOnNormalRift;
        protected IUiElement uiAcceptButton;
        protected IUiElement uiJoinPatryAcceptButton;
        private bool needToMoveMouse = false;
        public string debug = "";

        public AutoOpenRift(IController hud)
        {
            Hud = hud;

            uiGRmainPage = Hud.Render.RegisterUiElement("Root.NormalLayer.rift_dialog_mainPage", null, null);
            uiOnGreaterRift = Hud.Render.RegisterUiElement("Root.NormalLayer.rift_dialog_mainPage.LayoutRoot.RiftRadioButtons.GreaterRiftButton", null, null);
            uiOnNormalRift = Hud.Render.RegisterUiElement("Root.NormalLayer.rift_dialog_mainPage.LayoutRoot.RiftRadioButtons.NephalemRiftButton", null, null);
            uiAcceptButton = Hud.Render.RegisterUiElement("Root.NormalLayer.rift_dialog_mainPage.LayoutRoot.accept_Button", null, null);
            uiJoinPatryAcceptButton = Hud.Render.RegisterUiElement(
                "Root.NormalLayer.rift_join_party_main.LayoutRoot.Background.buttons.accept",
                null,
                null
            );
        }

        public void TryToOpenRift()
        {
            debug = PixelHelperSettings.Instance.AutoOpenRiftType;
            if (Hud.Game.IsLoading || !Hud.Window.IsForeground || !Hud.Game.Me.IsInTown)
            {
                return;
            }
            if (uiGRmainPage.Visible)
            {
                var tempX = Hud.Window.CursorX;
                var tempY = Hud.Window.CursorY;
                if (PixelHelperSettings.Instance.AutoOpenRiftType.Equals("Normal Rift")
                    || PixelHelperSettings.Instance.AutoOpenRiftType.Equals("Rift"))
                {
                    if (uiOnNormalRift.AnimState == 7)
                    {
                        Hud.Interaction.ClickUiElement(MouseButtons.Left, uiOnNormalRift);
                        needToMoveMouse = true;
                    }
                    if (uiOnNormalRift.AnimState == 9)
                    {
                        Hud.Interaction.ClickUiElement(MouseButtons.Left, uiAcceptButton);
                        needToMoveMouse = true;
                    }
                }
                else if (PixelHelperSettings.Instance.AutoOpenRiftType.Equals("Greater Rift"))
                {
                    if (Hud.Game.Me.Materials.GreaterRiftKeystone <= 0)
                    {
                        return;
                    }
                    if (uiOnGreaterRift.AnimState == 3)
                    {
                        Hud.Interaction.ClickUiElement(MouseButtons.Left, uiOnGreaterRift);
                        needToMoveMouse = true;
                    }
                    if (uiOnGreaterRift.AnimState == 5)
                    {
                        Hud.Interaction.ClickUiElement(MouseButtons.Left, uiAcceptButton);
                        needToMoveMouse = true;
                    }
                }
                else
                {
                    return;
                }
                if (needToMoveMouse)
                {
                    Hud.Interaction.MouseMove(tempX, tempY, 1, 1);
                    needToMoveMouse = false;
                }
            }
        }
        public void TryToAcceptGr()
        {
            if (PixelHelperSettings.Instance.AutoAcceptGR)
            {
                if (LayerVisible("Root.NormalLayer.rift_join_party_main.LayoutRoot"))
                {
                    Hud.Interaction.ClickUiElement(MouseButtons.Left, uiJoinPatryAcceptButton);
                }
            }
        }

        private bool LayerVisible(string path)
        {
            var layer = Hud.Render.GetUiElement(path);
            return layer != null && layer.Visible;
        }
    }
}
