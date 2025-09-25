namespace Turbo.Plugins.LightningMod
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;

    public class SkillBuildSetter
    {
        private readonly IController Hud;
        public int SpeedMultiplier { get; set; }

        private readonly IUiElement skillsWindow;
        private readonly IUiElement uiGameSkillsWindow_SkillList;
        private readonly IUiElement uiGameSkillsWindow_Passives;
        private readonly IUiElement uiGameSkillsWindow_Actives;

        private readonly Dictionary<ActionKey, IUiElement> SkillButtons;
        private readonly IUiElement[] PassiveButtons;
        private readonly IUiElement[] PassiveTexts;
        private readonly IUiElement[] SkillSelectorTexts;
        private readonly IUiElement[] SkillSelectorButtons;
        private readonly IUiElement[] RuneSelectorTexts;
        private readonly IUiElement[] RuneSelectorButtons;
        private readonly IUiElement[,] RuneTexts;
        private readonly IUiElement[,] RuneButtons;
        private readonly IUiElement SkillPagerRight;
        private readonly IUiElement SkillPagerLeft;
        private readonly IUiElement SkillSelectorAccept;
        private readonly IUiElement PassiveSelectorAccept;

        public SkillBuildSetter(IController hud)
        {
            Hud = hud;
            SpeedMultiplier = 1;

            skillsWindow = Hud.Render.RegisterUiElement("Root.NormalLayer.SkillPane_main.LayoutRoot", Hud.Render.InGameBottomHudUiElement, null);
            uiGameSkillsWindow_SkillList = Hud.Render.RegisterUiElement("Root.NormalLayer.SkillPane_main.LayoutRoot.SkillsList", skillsWindow, null);
            uiGameSkillsWindow_Passives = Hud.Render.RegisterUiElement("Root.NormalLayer.SkillPane_main.LayoutRoot.PassiveSkillSelect", skillsWindow, null);
            uiGameSkillsWindow_Actives = Hud.Render.RegisterUiElement("Root.NormalLayer.SkillPane_main.LayoutRoot.ActiveSkillSelect", skillsWindow, null);

            SkillButtons = new Dictionary<ActionKey, IUiElement>
            {
                [ActionKey.LeftSkill] = Hud.Render.RegisterUiElement("Root.NormalLayer.SkillPane_main.LayoutRoot.SkillsList.SkillLeftMouse.SkillButton", uiGameSkillsWindow_SkillList, null),
                [ActionKey.RightSkill] = Hud.Render.RegisterUiElement("Root.NormalLayer.SkillPane_main.LayoutRoot.SkillsList.SkillRightMouse.SkillButton", uiGameSkillsWindow_SkillList, null),
                [ActionKey.Skill1] = Hud.Render.RegisterUiElement("Root.NormalLayer.SkillPane_main.LayoutRoot.SkillsList.SkillAction1.SkillButton", uiGameSkillsWindow_SkillList, null),
                [ActionKey.Skill2] = Hud.Render.RegisterUiElement("Root.NormalLayer.SkillPane_main.LayoutRoot.SkillsList.SkillAction2.SkillButton", uiGameSkillsWindow_SkillList, null),
                [ActionKey.Skill3] = Hud.Render.RegisterUiElement("Root.NormalLayer.SkillPane_main.LayoutRoot.SkillsList.SkillAction3.SkillButton", uiGameSkillsWindow_SkillList, null),
                [ActionKey.Skill4] = Hud.Render.RegisterUiElement("Root.NormalLayer.SkillPane_main.LayoutRoot.SkillsList.SkillAction4.SkillButton", uiGameSkillsWindow_SkillList, null)
            };

            PassiveButtons = new IUiElement[4];
            PassiveButtons[0] = Hud.Render.RegisterUiElement("Root.NormalLayer.SkillPane_main.LayoutRoot.SkillsList.SelectedPassive1.PassiveButton", uiGameSkillsWindow_SkillList, null);
            PassiveButtons[1] = Hud.Render.RegisterUiElement("Root.NormalLayer.SkillPane_main.LayoutRoot.SkillsList.SelectedPassive2.PassiveButton", uiGameSkillsWindow_SkillList, null);
            PassiveButtons[2] = Hud.Render.RegisterUiElement("Root.NormalLayer.SkillPane_main.LayoutRoot.SkillsList.SelectedPassive3.PassiveButton", uiGameSkillsWindow_SkillList, null);
            PassiveButtons[3] = Hud.Render.RegisterUiElement("Root.NormalLayer.SkillPane_main.LayoutRoot.SkillsList.SelectedPassive4.PassiveButton", uiGameSkillsWindow_SkillList, null);

            PassiveTexts = new IUiElement[4];
            PassiveTexts[0] = Hud.Render.RegisterUiElement("Root.NormalLayer.SkillPane_main.LayoutRoot.SkillsList.SelectedPassive1.PassiveSkillName", uiGameSkillsWindow_SkillList, null);
            PassiveTexts[1] = Hud.Render.RegisterUiElement("Root.NormalLayer.SkillPane_main.LayoutRoot.SkillsList.SelectedPassive2.PassiveSkillName", uiGameSkillsWindow_SkillList, null);
            PassiveTexts[2] = Hud.Render.RegisterUiElement("Root.NormalLayer.SkillPane_main.LayoutRoot.SkillsList.SelectedPassive3.PassiveSkillName", uiGameSkillsWindow_SkillList, null);
            PassiveTexts[3] = Hud.Render.RegisterUiElement("Root.NormalLayer.SkillPane_main.LayoutRoot.SkillsList.SelectedPassive4.PassiveSkillName", uiGameSkillsWindow_SkillList, null);

            SkillSelectorTexts = new IUiElement[5];
            SkillSelectorTexts[0] = Hud.Render.RegisterUiElement("Root.NormalLayer.SkillPane_main.LayoutRoot.ActiveSkillSelect.SkillSelectionContainer.SkillSelectionList.SkillSlot1.SkillSelectionName", uiGameSkillsWindow_Actives, null);
            SkillSelectorTexts[1] = Hud.Render.RegisterUiElement("Root.NormalLayer.SkillPane_main.LayoutRoot.ActiveSkillSelect.SkillSelectionContainer.SkillSelectionList.SkillSlot2.SkillSelectionName", uiGameSkillsWindow_Actives, null);
            SkillSelectorTexts[2] = Hud.Render.RegisterUiElement("Root.NormalLayer.SkillPane_main.LayoutRoot.ActiveSkillSelect.SkillSelectionContainer.SkillSelectionList.SkillSlot3.SkillSelectionName", uiGameSkillsWindow_Actives, null);
            SkillSelectorTexts[3] = Hud.Render.RegisterUiElement("Root.NormalLayer.SkillPane_main.LayoutRoot.ActiveSkillSelect.SkillSelectionContainer.SkillSelectionList.SkillSlot4.SkillSelectionName", uiGameSkillsWindow_Actives, null);
            SkillSelectorTexts[4] = Hud.Render.RegisterUiElement("Root.NormalLayer.SkillPane_main.LayoutRoot.ActiveSkillSelect.SkillSelectionContainer.SkillSelectionList.SkillSlot5.SkillSelectionName", uiGameSkillsWindow_Actives, null);

            SkillSelectorButtons = new IUiElement[5];
            SkillSelectorButtons[0] = Hud.Render.RegisterUiElement("Root.NormalLayer.SkillPane_main.LayoutRoot.ActiveSkillSelect.SkillSelectionContainer.SkillSelectionList.SkillSlot1.SkillSelectionButton", uiGameSkillsWindow_Actives, null);
            SkillSelectorButtons[1] = Hud.Render.RegisterUiElement("Root.NormalLayer.SkillPane_main.LayoutRoot.ActiveSkillSelect.SkillSelectionContainer.SkillSelectionList.SkillSlot2.SkillSelectionButton", uiGameSkillsWindow_Actives, null);
            SkillSelectorButtons[2] = Hud.Render.RegisterUiElement("Root.NormalLayer.SkillPane_main.LayoutRoot.ActiveSkillSelect.SkillSelectionContainer.SkillSelectionList.SkillSlot3.SkillSelectionButton", uiGameSkillsWindow_Actives, null);
            SkillSelectorButtons[3] = Hud.Render.RegisterUiElement("Root.NormalLayer.SkillPane_main.LayoutRoot.ActiveSkillSelect.SkillSelectionContainer.SkillSelectionList.SkillSlot4.SkillSelectionButton", uiGameSkillsWindow_Actives, null);
            SkillSelectorButtons[4] = Hud.Render.RegisterUiElement("Root.NormalLayer.SkillPane_main.LayoutRoot.ActiveSkillSelect.SkillSelectionContainer.SkillSelectionList.SkillSlot5.SkillSelectionButton", uiGameSkillsWindow_Actives, null);

            RuneSelectorTexts = new IUiElement[6];
            RuneSelectorTexts[0] = Hud.Render.RegisterUiElement("Root.NormalLayer.SkillPane_main.LayoutRoot.ActiveSkillSelect.RuneSelectionContainer.RuneSelectionList.RuneSlot0.RuneSelectionName", uiGameSkillsWindow_Actives, null);
            RuneSelectorTexts[1] = Hud.Render.RegisterUiElement("Root.NormalLayer.SkillPane_main.LayoutRoot.ActiveSkillSelect.RuneSelectionContainer.RuneSelectionList.RuneSlot1.RuneSelectionName", uiGameSkillsWindow_Actives, null);
            RuneSelectorTexts[2] = Hud.Render.RegisterUiElement("Root.NormalLayer.SkillPane_main.LayoutRoot.ActiveSkillSelect.RuneSelectionContainer.RuneSelectionList.RuneSlot2.RuneSelectionName", uiGameSkillsWindow_Actives, null);
            RuneSelectorTexts[3] = Hud.Render.RegisterUiElement("Root.NormalLayer.SkillPane_main.LayoutRoot.ActiveSkillSelect.RuneSelectionContainer.RuneSelectionList.RuneSlot3.RuneSelectionName", uiGameSkillsWindow_Actives, null);
            RuneSelectorTexts[4] = Hud.Render.RegisterUiElement("Root.NormalLayer.SkillPane_main.LayoutRoot.ActiveSkillSelect.RuneSelectionContainer.RuneSelectionList.RuneSlot4.RuneSelectionName", uiGameSkillsWindow_Actives, null);
            RuneSelectorTexts[5] = Hud.Render.RegisterUiElement("Root.NormalLayer.SkillPane_main.LayoutRoot.ActiveSkillSelect.RuneSelectionContainer.RuneSelectionList.RuneSlot5.RuneSelectionName", uiGameSkillsWindow_Actives, null);

            RuneSelectorButtons = new IUiElement[6];
            RuneSelectorButtons[0] = Hud.Render.RegisterUiElement("Root.NormalLayer.SkillPane_main.LayoutRoot.ActiveSkillSelect.RuneSelectionContainer.RuneSelectionList.RuneSlot0.RuneSelectionButton", uiGameSkillsWindow_Actives, null);
            RuneSelectorButtons[1] = Hud.Render.RegisterUiElement("Root.NormalLayer.SkillPane_main.LayoutRoot.ActiveSkillSelect.RuneSelectionContainer.RuneSelectionList.RuneSlot1.RuneSelectionButton", uiGameSkillsWindow_Actives, null);
            RuneSelectorButtons[2] = Hud.Render.RegisterUiElement("Root.NormalLayer.SkillPane_main.LayoutRoot.ActiveSkillSelect.RuneSelectionContainer.RuneSelectionList.RuneSlot2.RuneSelectionButton", uiGameSkillsWindow_Actives, null);
            RuneSelectorButtons[3] = Hud.Render.RegisterUiElement("Root.NormalLayer.SkillPane_main.LayoutRoot.ActiveSkillSelect.RuneSelectionContainer.RuneSelectionList.RuneSlot3.RuneSelectionButton", uiGameSkillsWindow_Actives, null);
            RuneSelectorButtons[4] = Hud.Render.RegisterUiElement("Root.NormalLayer.SkillPane_main.LayoutRoot.ActiveSkillSelect.RuneSelectionContainer.RuneSelectionList.RuneSlot4.RuneSelectionButton", uiGameSkillsWindow_Actives, null);
            RuneSelectorButtons[5] = Hud.Render.RegisterUiElement("Root.NormalLayer.SkillPane_main.LayoutRoot.ActiveSkillSelect.RuneSelectionContainer.RuneSelectionList.RuneSlot5.RuneSelectionButton", uiGameSkillsWindow_Actives, null);

            SkillPagerRight = Hud.Render.RegisterUiElement("Root.NormalLayer.SkillPane_main.LayoutRoot.ActiveSkillSelect.ActiveSkillCategoryNext", uiGameSkillsWindow_Actives, null);
            SkillPagerLeft = Hud.Render.RegisterUiElement("Root.NormalLayer.SkillPane_main.LayoutRoot.ActiveSkillSelect.ActiveSkillCategoryBack", uiGameSkillsWindow_Actives, null);
            SkillSelectorAccept = Hud.Render.RegisterUiElement("Root.NormalLayer.SkillPane_main.LayoutRoot.ActiveSkillSelect.AcceptActiveSkillsButton", uiGameSkillsWindow_Actives, null);

            PassiveSelectorAccept = Hud.Render.RegisterUiElement("Root.NormalLayer.SkillPane_main.LayoutRoot.PassiveSkillSelect.AcceptPassiveSkillsButton", uiGameSkillsWindow_Passives, null);

            RuneButtons = new IUiElement[5, 4];
            RuneTexts = new IUiElement[5, 4];
            for (var row = 0; row <= 4; row++)
            {
                var rowPath = "Root.NormalLayer.SkillPane_main.LayoutRoot.PassiveSkillSelect.PassiveRow" + (row + 1).ToString("D", CultureInfo.InvariantCulture).PadLeft(2, '0');
                for (var col = 0; col <= 3; col++)
                {
                    var slotPath = rowPath + ".PassiveSlot" + ((row * 4) + col + 1).ToString("D", CultureInfo.InvariantCulture).PadLeft(2, '0');
                    RuneTexts[row, col] = Hud.Render.RegisterUiElement(slotPath + ".PassiveSkillTextContainer.PassiveSkillText", uiGameSkillsWindow_Passives, null);
                    RuneButtons[row, col] = Hud.Render.RegisterUiElement(slotPath + ".PassiveSkillIcon", uiGameSkillsWindow_Passives, null);
                }
            }
        }

        public bool SetCurrentSkillBuild(SkillBuild build)
        {
            foreach (var kvp in build.Skills)
            {
                var actionKey = kvp.Key;
                var snoPower = kvp.Value.SnoPower;
                var runeName = kvp.Value.RuneLocalizedName;

                var actionKeyIndex = (int)actionKey;
                var currentSkill = Hud.Game.Me.Powers.SkillSlots[actionKeyIndex];

                if ((currentSkill == null) || (currentSkill.SnoPower != snoPower) || (currentSkill.RuneNameLocalized != runeName))
                {
                    uiGameSkillsWindow_SkillList.Refresh();
                    if (!uiGameSkillsWindow_SkillList.Visible)
                    {
                        Hud.Interaction.DoAction(ActionKey.SkillsWindow);
                        if (!Hud.WaitFor(5000, 15, 600 / SpeedMultiplier, () => uiGameSkillsWindow_SkillList.Visible))
                        {
                            //EngineBase.Controller.ActionLogic.WriteStatus("EnsureBuild failed #1", 0);
                            return false;
                        }
                    }

                    SkillButtons[actionKey].Refresh();
                    Hud.Interaction.ClickUiElement(System.Windows.Forms.MouseButtons.Left, SkillButtons[actionKey]);
                    Hud.Wait(300 / SpeedMultiplier);
                    Hud.ReCollect();

                    IUiElement skillSelectorButton = null;
                    var safetyCounter = 0;
                    if (currentSkill == null || currentSkill.SnoPower != snoPower)
                    {
                        var toRight = AbstractSkillHandler.NextRandom(1, 50) <= 25;
                        while ((skillSelectorButton == null) && (safetyCounter < 15))
                        {
                            for (var j = 0; j < 5; j++)
                            {
                                SkillSelectorTexts[j].Refresh();
                                if (!SkillSelectorTexts[j].Visible)
                                    continue;

                                var skillSelectorText = SkillSelectorTexts[j].ReadText(Encoding.UTF8, true);
                                if (skillSelectorText == snoPower.NameLocalized)
                                {
                                    skillSelectorButton = SkillSelectorButtons[j];
                                    break;
                                }
                            }

                            if (skillSelectorButton == null)
                            {
                                var pager = toRight ? SkillPagerRight : SkillPagerLeft;
                                pager.Refresh();
                                Hud.Interaction.ClickUiElement(System.Windows.Forms.MouseButtons.Left, pager);
                                Hud.Wait(300 / SpeedMultiplier);
                                Hud.ReCollect();
                                safetyCounter++;
                            }
                        }

                        if (skillSelectorButton == null)
                        {
                            //EngineBase.Controller.ActionLogic.WriteStatus("EnsureBuild failed #2 (" + snoPower.NameLocalized + ")", 0);
                            return false;
                        }

                        skillSelectorButton.Refresh();
                        Hud.Interaction.ClickUiElement(System.Windows.Forms.MouseButtons.Left, skillSelectorButton);
                        Hud.Wait(300 / SpeedMultiplier);
                    }

                    IUiElement runeSelectorButton = null;
                    for (var safetyLoop = 0; safetyLoop < 4; safetyLoop++)
                    {
                        Hud.ReCollect();

                        for (var j = 0; j < 6; j++)
                        {
                            RuneSelectorTexts[j].Refresh();
                            if (!RuneSelectorTexts[j].Visible)
                                continue;
                            var runeText = RuneSelectorTexts[j].ReadText(Encoding.UTF8, true);
                            if (runeText == runeName)
                            {
                                runeSelectorButton = RuneSelectorButtons[j];
                                break;
                            }
                        }

                        if (runeSelectorButton != null)
                            break;
                        Hud.Wait(250);
                    }

                    if (runeSelectorButton == null)
                    {
                        //EngineBase.Controller.ActionLogic.WriteStatus("EnsureBuild failed #3", 0);
                        return false;
                    }

                    runeSelectorButton.Refresh();
                    Hud.Interaction.ClickUiElement(System.Windows.Forms.MouseButtons.Left, runeSelectorButton);
                    Hud.Wait(300 / SpeedMultiplier);

                    SkillSelectorAccept.Refresh();
                    Hud.Interaction.ClickUiElement(System.Windows.Forms.MouseButtons.Left, SkillSelectorAccept);
                    Hud.Wait(300 / SpeedMultiplier);
                }
            }

            for (var i = 0; i < build.Passives.Length; i++)
            {
                var snoPower = build.Passives[i];
                if (snoPower == null)
                    continue;

                if (Hud.Game.Me.Powers.UsedPassives.Contains(snoPower))
                    continue;

                uiGameSkillsWindow_SkillList.Refresh();
                if (!uiGameSkillsWindow_SkillList.Visible)
                {
                    Hud.Interaction.DoAction(ActionKey.SkillsWindow);
                    if (!Hud.WaitFor(5000, 15, 600 / SpeedMultiplier, () => uiGameSkillsWindow_SkillList.Visible))
                    {
                        //EngineBase.Controller.ActionLogic.WriteStatus("EnsureBuild failed #4", 0);
                        return false;
                    }
                }

                Hud.ReCollect();

                IUiElement passiveButtonToReplace = null;
                IUiElement passiveTextToReplace = null;
                for (var j = 0; j < 4; j++)
                {
                    PassiveTexts[j].Refresh();
                    var text = PassiveTexts[j].ReadText(Encoding.UTF8, true);
                    if (!build.Passives.Any(x => x.NameLocalized == text))
                    {
                        passiveButtonToReplace = PassiveButtons[j];
                        passiveTextToReplace = PassiveTexts[j];
                        break;
                    }
                }

                if (passiveButtonToReplace == null)
                {
                    //EngineBase.Controller.ActionLogic.WriteStatus("EnsureBuild failed #5", 0);
                    return false;
                }

                passiveButtonToReplace.Refresh();
                Hud.Interaction.ClickUiElement(System.Windows.Forms.MouseButtons.Left, passiveButtonToReplace);

                if (!Hud.WaitFor(5000, 15, 450 / SpeedMultiplier, () => uiGameSkillsWindow_Passives.Visible))
                {
                    //EngineBase.Controller.ActionLogic.WriteStatus("EnsureBuild failed #5.1", 0);
                    return false;
                }

                Hud.ReCollect();

                IUiElement passiveButton = null;
                for (var row = 0; row <= 4; row++)
                {
                    for (var col = 0; col <= 3; col++)
                    {
                        var slotText = RuneTexts[row, col].ReadText(Encoding.UTF8, true);
                        if (slotText == snoPower.NameLocalized)
                        {
                            passiveButton = RuneButtons[row, col];
                            break;
                        }
                    }

                    if (passiveButton != null)
                        break;
                }

                if (passiveButton == null)
                {
                    //EngineBase.Controller.ActionLogic.WriteStatus("EnsureBuild failed #6", 0);
                    return false;
                }

                Hud.Interaction.ClickUiElement(System.Windows.Forms.MouseButtons.Left, passiveButton);
                Hud.Wait(200 / SpeedMultiplier);

                PassiveSelectorAccept.Refresh();
                Hud.Interaction.ClickUiElement(System.Windows.Forms.MouseButtons.Left, PassiveSelectorAccept);
                Hud.Wait(200 / SpeedMultiplier);

                var ok = Hud.WaitFor(3000, 15, 300 / SpeedMultiplier, () =>
                {
                    var text = passiveTextToReplace.ReadText(Encoding.UTF8, true);
                    return text == snoPower.NameLocalized;
                });

                if (!ok)
                {
                    //EngineBase.Controller.ActionLogic.WriteStatus("EnsureBuild failed #7", 0);
                    return false;
                }
            }

            return true;
        }
    }
}