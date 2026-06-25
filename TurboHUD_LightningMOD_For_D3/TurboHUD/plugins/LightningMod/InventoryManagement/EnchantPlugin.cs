namespace Turbo.Plugins.LightningMod
{
    using SharpDX.DirectInput;
    using System.Linq;
    using System.Windows.Forms;
    using Turbo.Plugins.Default;
    using Turbo.Plugins.glq;
    using System;
    using System.Text;
    using System.Text.RegularExpressions;
    public class EnchantPlugin : BasePlugin, IKeyEventHandler, IInGameTopPainter, IAfterCollectHandler
    {
        public bool TurnedOn { get; set; }
        public IKeyEvent ToggleKeyEvent { get; set; }
        public bool Running { get; private set; }
        private int currentCount = 0;
        private bool Perfect = false;
        public int enchantCount { get; set; }//最大附魔次数
        public int ReservedArcaneDust { get; set; }//保留奥术之尘
        public int ReservedVeiledCrystal { get; set; }//保留水晶
        public int ReservedForgottenSoul { get; set; }//保留遗忘之魂
        public int ReservedDeathsBreath { get; set; }//保留死亡气息
        public int ReservedGem { get; set; }//保留宝石
        public long ReservedGold { get; set; }//保留金币
        private IUiElement vendorPage;
        private IUiElement enchantDialog;
        private IUiElement enchantItem0;
        private IUiElement enchantItem1;
        private IUiElement enchantItem2;
        private IUiElement enchantItem0Selected;
        private IUiElement enchantItem1Selected;
        private IUiElement enchantItem2Selected;
        private IUiElement enchantButton;
        private IUiElement affixListDialog;
        public IFont HeaderFont { get; private set; }
        public IFont InfoFont { get; private set; }
        private string str_Header;
        private string str_InfoLock;
        private string str_InfoCommunity;
        private string str_Info;
        private string str_InfoPre;
        private string str_NoItem;
        private string str_NoAffixListShow;
        private string str_Running;
        private string str_times;
        private string str_temp1;
        private string str_temp2;
        private string str_temp3;
        private int tempX;
        private int tempY;
		private bool isIncommunity = false;
        private string EnchantItemText = "";
        private string AlternateProperty = "";
        private long EnchantGold = 0;
        private enum EnchantState : int
        {
			start = 0,		   //启动状态机
            generated = 1,     //生成了新属性
            selected = 2,      //选择了新属性
            changed = 3,        //改变了属性
            end = 4,        //状态机完成
            Unknown = int.MaxValue,
        }
        private EnchantState enchant_state = EnchantState.start;

        public EnchantPlugin()
        {
            Enabled = true;
            TurnedOn = false;
            Running = false;
            Perfect = false;
            enchantCount = 100000;//最大附魔次数
            ReservedArcaneDust = 0;//保留奥术之尘
            ReservedVeiledCrystal = 0;//保留水晶
            ReservedForgottenSoul = 0;//保留遗忘之魂
            ReservedDeathsBreath = 0;//保留死亡气息
            ReservedGem = 0;//保留宝石
            ReservedGold = 0;//保留金币
    }

        public override void Load(IController hud)
        {
            base.Load(hud);

            ToggleKeyEvent = Hud.Input.CreateKeyEvent(true, Key.F3, false, false, false);

            vendorPage = Hud.Render.RegisterUiElement("Root.NormalLayer.vendor_dialog_mainPage", null, null);
            enchantDialog = Hud.Render.RegisterUiElement("Root.NormalLayer.vendor_dialog_mainPage.enchant_dialog", null, null);
            enchantItem0 = Hud.Render.RegisterUiElement("Root.NormalLayer.vendor_dialog_mainPage.enchant_dialog.LayoutRoot.item_affix_list._content._stackpanel._item0.list_item_window.text", null, null); //第1条属性内容
            enchantItem1 = Hud.Render.RegisterUiElement("Root.NormalLayer.vendor_dialog_mainPage.enchant_dialog.LayoutRoot.item_affix_list._content._stackpanel._item1.list_item_window.text", null, null); //第2条属性内容
            enchantItem2 = Hud.Render.RegisterUiElement("Root.NormalLayer.vendor_dialog_mainPage.enchant_dialog.LayoutRoot.item_affix_list._content._stackpanel._item2.list_item_window.text", null, null); //第3条属性内容
            enchantItem0Selected = Hud.Render.RegisterUiElement("Root.NormalLayer.vendor_dialog_mainPage.enchant_dialog.LayoutRoot.item_affix_list._content._stackpanel._item0.list_item_window.selected_background", null, null); //属性选中为true
            enchantItem1Selected = Hud.Render.RegisterUiElement("Root.NormalLayer.vendor_dialog_mainPage.enchant_dialog.LayoutRoot.item_affix_list._content._stackpanel._item1.list_item_window.selected_background", null, null); //属性选中为true
            enchantItem2Selected = Hud.Render.RegisterUiElement("Root.NormalLayer.vendor_dialog_mainPage.enchant_dialog.LayoutRoot.item_affix_list._content._stackpanel._item2.list_item_window.selected_background", null, null); //属性选中为true
            enchantButton = Hud.Render.RegisterUiElement("Root.NormalLayer.vendor_dialog_mainPage.enchant_dialog.LayoutRoot.enchant_button", null, null); //属性生成按钮
            affixListDialog = Hud.Render.RegisterUiElement("Root.NormalLayer.validItemAffixes_dialog", null, null); //可替换属性窗口

            HeaderFont = Hud.Render.CreateFont("tahoma", 12, 255, 200, 200, 100, true, false, 255, 0, 0, 0, true);
            InfoFont = Hud.Render.CreateFont("tahoma", 8, 255, 200, 200, 0, true, false, 255, 0, 0, 0, true);
        }

        private bool PlaceanItemtoReplaceaProperty()
        {
            var str = enchantButton.ReadText(Encoding.UTF8, true);
            return str.Contains("放置一项物品来替换一项属性") //zhCN
                || str.Contains("放置物品以替換其屬性") //zhTW
                || str.Contains("Place an Item to Replace a Property") //enUS
                || str.Contains("Выберите предмет")//ruRU
                || str.Contains("속성을 교체할 아이템을 올려놓으십시오")//koKR
                || str.Contains("Zum Austausch Gegenstand platzieren")//deDE
                || str.Contains("Placez un objet")//frDR
                || str.Contains("Colloca un oggetto per incantarlo")//itIT
                || str.Contains("Colocar objeto")//esES
                || str.Contains("Coloca un objeto")//esMX
                || str.Contains("Umieść przedmiot")//plPL
                || str.Contains("Insira um Item")//ptPT
                 ;
        }
        private bool ReplaceProperty()
        {
            var str = enchantButton.ReadText(Encoding.UTF8, true);
            return str.Contains("替换属性") 
                || str.Contains("替換屬性") 
                || str.Contains("Replace Property") 
                || str.Contains("Изменить")
                || str.Contains("속성 교체")
                || str.Contains("Eigenschaft austauschen")
                || str.Contains("Remplacer la propriété")
                || str.Contains("Sostituisci proprietà")
                || str.Contains("Reemplazar propiedad")
                || str.Contains("Reemplazar propiedad")
                || str.Contains("Zmień właściwość")
                || str.Contains("Substituir Propriedade")
                 ;
        }
        private bool SelectProperty()
        {
            var str = enchantButton.ReadText(Encoding.UTF8, true);
            return str.Contains("选择属性")
                || str.Contains("選擇屬性")
                || str.Contains("Select Property")
                || str.Contains("Выбрать свойство")
                || str.Contains("속성 선택")
                || str.Contains("Eigenschaft auswählen")
                || str.Contains("Choisir une propriété")
                || str.Contains("Seleziona proprietà")
                || str.Contains("Seleccionar propiedad")
                || str.Contains("Seleccionar propiedad")
                || str.Contains("Wybierz właściwość")
                || str.Contains("Selecionar Propriedade")
                ;
        }
        public void OnKeyEvent(IKeyEvent keyEvent)
        {
            if (ToggleKeyEvent.Matches(keyEvent))
            {
                if (keyEvent.IsPressed)
                {
                    if(AlternateProperty == "")
                    {
                        return;
                    }
                    if (!TurnedOn)
                    {
                        TurnedOn = true;
                        Running = true;
                        currentCount = 0;
                        Perfect = false;
                        enchant_state = EnchantState.start;
                        EnchantItemText = getAffixItemTextByCursor();
                    }
                    else
                    {
                        TurnedOn = false;
                        Running = false;
                        currentCount = 0;
                        AlternateProperty = "";//停止后清除选择的附魔词缀
                    }
                    TestTurnedOn();
                }
            }
        }
        public void PaintTopInGame(ClipState clipState)
        {
            if (Hud.CurrentLanguage == Language.zhCN)
            {
                str_Header = "【雷电宏-自动附魔属性】";
                str_InfoPre = "将鼠标放在右侧窗口\r\n指向在要附魔的属性上\r\n";
                str_Info = "单击 " + ToggleKeyEvent.ToString() + " 开始自动附魔";
                str_NoItem = "请放置需要附魔的物品";
                str_NoAffixListShow = "请选中需要附魔的属性并点击对应的问号\r\n以显示并选择可替换的属性";
                str_Running = "自动附魔中...\r\n单击 " + ToggleKeyEvent.ToString() + " 停止";
                str_times = "附魔次数：";
                str_temp2 = "替换属性";
                str_temp3 = "选择属性";
            }
            else if (Hud.CurrentLanguage == Language.zhTW)
            {
                str_Header = "【雷電宏——自動附魔】";
                str_InfoPre = "將滑鼠放到右側視窗\r\n指向在要附魔的屬性上\r\n";
                str_Info = "單擊 " + ToggleKeyEvent.ToString() + " 開始自動附魔";
                str_NoItem = "請放置需要附魔的物品";
                str_NoAffixListShow = "請選中需要附魔的屬性n並點擊對應的問號\r\n以顯示並選擇可替換的屬性";
                str_Running = "自動附魔中...\r\n單擊 " + ToggleKeyEvent.ToString() + " 停止";
                str_times = "附魔次數：";
                str_temp2 = "替換屬性";
                str_temp3 = "選擇屬性";
            }
            else if (Hud.CurrentLanguage == Language.ruRU)
            {
                str_Header = "【МОД-Авто Зачарование】";
                str_InfoPre = "Курсор над правым окном\r\n для выбора свойства\r\n";
                str_Info = "Нажать " + ToggleKeyEvent.ToString() + " для Старта";
                str_NoItem = "Положите предмет";
                str_NoAffixListShow = "Выберите Свойство и нажмите \"?\" \r\nдля отображения дополнительного свойства";
                str_Running = "Авто Зачарование...\r\nНажать " + ToggleKeyEvent.ToString() + " для Остановки";
                str_times = "Время Зачарование】:";
                str_temp2 = "Заменить свойство";
                str_temp3 = "Выбрать Свойство";
            }
            else
            {
                str_Header = "【Auto Enchant-MOD】";
                str_InfoPre = "Cursor Over Right Window\r\nTo Select Property\r\n";
                str_Info = "Click " + ToggleKeyEvent.ToString() + " To Start";
                str_NoItem = "Place an Item";
                str_NoAffixListShow = "Select Property And Click The \"?\" \r\nTo Display Alternate Property";
                str_Running = "Auto Enchanting...\r\nClick " + ToggleKeyEvent.ToString() + " To Stop";
                str_times = "Enchant Times:";
                str_temp2 = "Replace Property";
                str_temp3 = "Select Property";
            }
            if (clipState != ClipState.Inventory) return;
            if (!Hud.Game.IsInGame || !enchantDialog.Visible || !Hud.Inventory.InventoryMainUiElement.Visible) return;
            var xDelta = 0.62f;
            var xDelta1 = 0.1f;
            var y = vendorPage.Rectangle.Y + vendorPage.Rectangle.Height * 0.037f;
            var layout = HeaderFont.GetTextLayout(str_Header);
            HeaderFont.DrawText(layout, vendorPage.Rectangle.X + (vendorPage.Rectangle.Width * xDelta - layout.Metrics.Width) / 2, y);
            y += layout.Metrics.Height * 1.3f;
            if (!Running)
            {
                if (enchantItem0.Visible) //放置附魔物品
                {
                    if (!affixListDialog.Visible || !isItemSelected())
                    {
                        layout = InfoFont.GetTextLayout(str_NoAffixListShow);
                        InfoFont.DrawText(layout, vendorPage.Rectangle.X + vendorPage.Rectangle.Width * xDelta1, y);
                    }
                    else
                    {
                        AlternateProperty = getAffixItemTextByCursor();
                        if (AlternateProperty == "")
                        {
                            layout = InfoFont.GetTextLayout(str_InfoPre);
                            InfoFont.DrawText(layout, vendorPage.Rectangle.X + vendorPage.Rectangle.Width * xDelta1, y);
                        }
                        else
                        {
                            layout = InfoFont.GetTextLayout(str_Info + "\r\n\r\n" + str_temp3 + "\r\n" + processLongString(AlternateProperty) + "\r\n\r\n" + str_times + getRemainEnchantCount().ToString());
                            InfoFont.DrawText(layout, vendorPage.Rectangle.X + vendorPage.Rectangle.Width * xDelta1, y);
                        }
                    }
                }
                else
                {
                    layout = InfoFont.GetTextLayout(str_NoItem);
                    InfoFont.DrawText(layout, vendorPage.Rectangle.X + vendorPage.Rectangle.Width * xDelta1, y);
                }
            }
            else
            {
                layout = InfoFont.GetTextLayout(str_Running + "\r\n\r\n" + str_temp2 + "\r\n" + AlternateProperty + "\r\n\r\n" + str_times + "(" + currentCount.ToString() + "/" + enchantCount.ToString() + ")");
                InfoFont.DrawText(layout, vendorPage.Rectangle.X + vendorPage.Rectangle.Width * xDelta1, y);
            }
        }
        //选取附魔词缀
        private string getAffixItemTextByCursor()
        {
            var itemText = "";
            IUiElement[] AffixListCanEnchanted =   
            {
                Hud.Render.RegisterUiElement("Root.NormalLayer.validItemAffixes_dialog.affixes._content._stackpanel._item0.stack.text", null, null),
                Hud.Render.RegisterUiElement("Root.NormalLayer.validItemAffixes_dialog.affixes._content._stackpanel._item1.stack.text", null, null),
                Hud.Render.RegisterUiElement("Root.NormalLayer.validItemAffixes_dialog.affixes._content._stackpanel._item2.stack.text", null, null),
                Hud.Render.RegisterUiElement("Root.NormalLayer.validItemAffixes_dialog.affixes._content._stackpanel._item3.stack.text", null, null),
                Hud.Render.RegisterUiElement("Root.NormalLayer.validItemAffixes_dialog.affixes._content._stackpanel._item4.stack.text", null, null),
                Hud.Render.RegisterUiElement("Root.NormalLayer.validItemAffixes_dialog.affixes._content._stackpanel._item5.stack.text", null, null),
                Hud.Render.RegisterUiElement("Root.NormalLayer.validItemAffixes_dialog.affixes._content._stackpanel._item6.stack.text", null, null),
                Hud.Render.RegisterUiElement("Root.NormalLayer.validItemAffixes_dialog.affixes._content._stackpanel._item7.stack.text", null, null),
                Hud.Render.RegisterUiElement("Root.NormalLayer.validItemAffixes_dialog.affixes._content._stackpanel._item8.stack.text", null, null),
                Hud.Render.RegisterUiElement("Root.NormalLayer.validItemAffixes_dialog.affixes._content._stackpanel._item9.stack.text", null, null),
                Hud.Render.RegisterUiElement("Root.NormalLayer.validItemAffixes_dialog.affixes._content._stackpanel._item10.stack.text", null, null),
                Hud.Render.RegisterUiElement("Root.NormalLayer.validItemAffixes_dialog.affixes._content._stackpanel._item11.stack.text", null, null),
                Hud.Render.RegisterUiElement("Root.NormalLayer.validItemAffixes_dialog.affixes._content._stackpanel._item12.stack.text", null, null),
                Hud.Render.RegisterUiElement("Root.NormalLayer.validItemAffixes_dialog.affixes._content._stackpanel._item13.stack.text", null, null),
                Hud.Render.RegisterUiElement("Root.NormalLayer.validItemAffixes_dialog.affixes._content._stackpanel._item14.stack.text", null, null),
                Hud.Render.RegisterUiElement("Root.NormalLayer.validItemAffixes_dialog.affixes._content._stackpanel._item15.stack.text", null, null),
                Hud.Render.RegisterUiElement("Root.NormalLayer.validItemAffixes_dialog.affixes._content._stackpanel._item16.stack.text", null, null),
                Hud.Render.RegisterUiElement("Root.NormalLayer.validItemAffixes_dialog.affixes._content._stackpanel._item17.stack.text", null, null),
                Hud.Render.RegisterUiElement("Root.NormalLayer.validItemAffixes_dialog.affixes._content._stackpanel._item18.stack.text", null, null),
                Hud.Render.RegisterUiElement("Root.NormalLayer.validItemAffixes_dialog.affixes._content._stackpanel._item19.stack.text", null, null),
                Hud.Render.RegisterUiElement("Root.NormalLayer.validItemAffixes_dialog.affixes._content._stackpanel._item20.stack.text", null, null),
                Hud.Render.RegisterUiElement("Root.NormalLayer.validItemAffixes_dialog.affixes._content._stackpanel._item21.stack.text", null, null),
                Hud.Render.RegisterUiElement("Root.NormalLayer.validItemAffixes_dialog.affixes._content._stackpanel._item22.stack.text", null, null),
                Hud.Render.RegisterUiElement("Root.NormalLayer.validItemAffixes_dialog.affixes._content._stackpanel._item23.stack.text", null, null),
                Hud.Render.RegisterUiElement("Root.NormalLayer.validItemAffixes_dialog.affixes._content._stackpanel._item24.stack.text", null, null),
                Hud.Render.RegisterUiElement("Root.NormalLayer.validItemAffixes_dialog.affixes._content._stackpanel._item25.stack.text", null, null),
                Hud.Render.RegisterUiElement("Root.NormalLayer.validItemAffixes_dialog.affixes._content._stackpanel._item26.stack.text", null, null),
                Hud.Render.RegisterUiElement("Root.NormalLayer.validItemAffixes_dialog.affixes._content._stackpanel._item27.stack.text", null, null),
                Hud.Render.RegisterUiElement("Root.NormalLayer.validItemAffixes_dialog.affixes._content._stackpanel._item28.stack.text", null, null),
                Hud.Render.RegisterUiElement("Root.NormalLayer.validItemAffixes_dialog.affixes._content._stackpanel._item29.stack.text", null, null),
                Hud.Render.RegisterUiElement("Root.NormalLayer.validItemAffixes_dialog.affixes._content._stackpanel._item30.stack.text", null, null),
                Hud.Render.RegisterUiElement("Root.NormalLayer.validItemAffixes_dialog.affixes._content._stackpanel._item31.stack.text", null, null),
            };

            for (int i=0; i<AffixListCanEnchanted.Length; i++)
            {
                var uiRect = AffixListCanEnchanted[i];
                if (uiRect.Visible && Hud.Window.CursorInsideRect(uiRect.Rectangle.X, uiRect.Rectangle.Y, uiRect.Rectangle.Width, uiRect.Rectangle.Height))
                {
                    var ss = uiRect.ReadText(Encoding.UTF8, true);
                    ss = Regex.Replace(ss, @"\{icon.*?\}", ""); //removes {icon...}
                    itemText = ss;
                }
            }                
            return itemText;
        }
        
        private int getTotalResource(IUiElement text)
        {
            if (text.Visible)
            {
                var ss = text.ReadText(Encoding.UTF8, true);
                var tempstr = "";
                foreach (var c in ss)
                {
                    if (c == '/' && tempstr != "")
                    {
                        if (tempstr == "*") return -1;
                        else return (int)CustomNumericParse(tempstr);
                    }
                    else 
                    {
                        tempstr += c;
                    } 
                }
            }
            return -1;
        }
        private int getUnitResource(IUiElement text)
        {
            if (text.Visible)
            {
                var ss = text.ReadText(Encoding.UTF8, true);
                var tempstr = "";
                foreach (var c in ss)
                {
                    if (c == '/')
                    {
                        tempstr = "";
                    }
                    else 
                    {
                        tempstr += c;
                    } 
                }
                return (int)CustomNumericParse(tempstr);
            }
            return -1;
        }
        private int getRemainEnchantCount()
        {
            IUiElement[] uiMaterialList =   
            {
                Hud.Render.RegisterUiElement("Root.NormalLayer.vendor_dialog_mainPage.enchant_dialog.LayoutRoot.required_materials.ingredient_0.text", null, null),  //奥数之尘
                Hud.Render.RegisterUiElement("Root.NormalLayer.vendor_dialog_mainPage.enchant_dialog.LayoutRoot.required_materials.ingredient_1.text", null, null),  //萦雾水晶
                Hud.Render.RegisterUiElement("Root.NormalLayer.vendor_dialog_mainPage.enchant_dialog.LayoutRoot.required_materials.ingredient_2.text", null, null),  //遗忘之魂
                Hud.Render.RegisterUiElement("Root.NormalLayer.vendor_dialog_mainPage.enchant_dialog.LayoutRoot.required_materials.ingredient_3.text", null, null),  //死亡之息
                Hud.Render.RegisterUiElement("Root.NormalLayer.vendor_dialog_mainPage.enchant_dialog.LayoutRoot.required_materials.ingredient_4.text", null, null),  //宝石
            };
            var uiGem = Hud.Render.RegisterUiElement("Root.NormalLayer.vendor_dialog_mainPage.enchant_dialog.LayoutRoot.required_materials.ingredient_4.icon", null, null);
            long realMaterial = 0;
            var gemid = 0;
            enchantCount = 0;
            for (int i=0; i<uiMaterialList.Length; i++)
            {
                switch (i)
                {
                    case 0:
                        realMaterial = Hud.Game.Me.Materials.ArcaneDust - ReservedArcaneDust;
                        break;
                    case 1:
                        realMaterial = Hud.Game.Me.Materials.VeiledCrystal - ReservedVeiledCrystal;
                        break;
                    case 2:
                        realMaterial = Hud.Game.Me.Materials.ForgottenSoul - ReservedForgottenSoul;
                        break;
                    case 3:
                        realMaterial = Hud.Game.Me.Materials.DeathsBreath - ReservedDeathsBreath;
                        break;
                    case 4:
                        if(uiGem.Visible)
                        {
                            gemid = uiGem.AnimState;
                            if(gemid == 53)//红宝石
                            {
                                realMaterial = CountGems(1019190640) - ReservedGem;
                            }
                            else if(gemid == 15)//紫宝石
                            {
                                realMaterial = CountGems(3446938397) - ReservedGem;
                            }
                            else if (gemid == 91)//钻石
                            {
                                realMaterial = CountGems(3256663690) - ReservedGem;
                            }
                            else if (gemid == 34)//绿宝石
                            {
                                realMaterial = CountGems(2838965544) - ReservedGem;
                            }
                            else if (gemid == 72)//黄宝石
                            {
                                realMaterial = CountGems(4267641564) - ReservedGem;
                            }
                        }
                        else
                        {
                            realMaterial = 0;
                        }
                        break;
                    default:
                        realMaterial = 0;
                        break;
                }
                var u = getUnitResource(uiMaterialList[i]);//获取材料所需数量
                var total = realMaterial;
                var unit = (u <= 0) ? 0 : u;
                if(unit > 0 && total <= 0)
                {
                    enchantCount = 0;//材料不足附魔次数为0
                }
                else if(unit > 0 && total > 0)
                {
                    var c = total / unit;
                    if (c < enchantCount || enchantCount == 0) enchantCount = (int)c; //取最少材料可附魔的次数
                }

            }
            return enchantCount;
        }
        private long CountGems(uint snoItem)//获取宝石数量
        {
            var count = 0;
            foreach (var item in Hud.Inventory.ItemsInStash)
            {
                if (item.SnoItem.Sno == snoItem) count += (int)item.Quantity;
            }

            foreach (var item in Hud.Inventory.ItemsInInventory)
            {
                if (item.SnoItem.Sno == snoItem) count += (int)item.Quantity;
            }
            return count;
        }
        private string processLongString(string str)
        {
            var ss = "";
            int count = 0;
            if (str.Length > 50)
            {
                foreach(var c in str)
                {
                    if (count == 50) ss += "\r\n";
                    ss += c;
                    count ++;
                }
                return ss;
            }
            else 
            {
                return str;
            }
        }
        private string removeValue(string text)
        {
            var ss = "";
            if (text == null) return ss;
            foreach(var c in text)
            {
                if ((c >= '0' && c <= '9') || c == '.' || c == '-' || c == ',') continue;
                ss += c;
            }
            return ss;
        }

        //获取词缀条目
        private string getItemContent(string itemText)
        {
            if (itemText == null) return "";
            var ss = itemText;
            ss = ss.Replace("点闪电伤害", "点伤害");//zhCN
            ss = ss.Replace("点火焰伤害", "点伤害");
            ss = ss.Replace("点冰霜伤害", "点伤害");
            ss = ss.Replace("点毒性伤害", "点伤害");
            ss = ss.Replace("点奥术伤害", "点伤害");
            ss = ss.Replace("点神圣伤害", "点伤害");

            ss = ss.Replace("點電擊傷害", "點傷害");//zhTW
            ss = ss.Replace("點火焰傷害", "點傷害");
            ss = ss.Replace("點冰寒傷害", "點傷害");
            ss = ss.Replace("點毒素傷害", "點傷害");
            ss = ss.Replace("點秘法傷害", "點傷害");
            ss = ss.Replace("點神聖傷害", "點傷害");

            ss = ss.Replace("Lightning Damage", "Damage");//enUS
            ss = ss.Replace("Fire Damage", "Damage");
            ss = ss.Replace("Cold Damage", "Damage");
            ss = ss.Replace("Poison Damage", "Damage");
            ss = ss.Replace("Arcane Damage", "Damage");
            ss = ss.Replace("Holy Damage", "Damage");

            ss = ss.Replace("ед. урона от молнии", "ед. урона");//ruRU
            ss = ss.Replace("ед. урона от огня", "ед. урона");
            ss = ss.Replace("ед. урона от холода", "ед. урона");
            ss = ss.Replace("ед. урона от яда", "ед. урона");
            ss = ss.Replace("ед. урона от тайной магии", "ед. урона");
            ss = ss.Replace("ед. урона от сил Света", "ед. урона");

            ss = ss.Replace("번개 무기 공격력", "무기 공격력");//koKR
            ss = ss.Replace("화염 무기 공격력", "무기 공격력");
            ss = ss.Replace("냉기 무기 공격력", "무기 공격력");
            ss = ss.Replace("독 무기 공격력", "무기 공격력");
            ss = ss.Replace("비전 무기 공격력", "무기 공격력"); 
            ss = ss.Replace("신성 무기 공격력", "무기 공격력");

            ss = ss.Replace("Blitzschaden", "Schaden");//deDE
            ss = ss.Replace("Feuerschaden", "Schaden");
            ss = ss.Replace("Kälteschaden", "Schaden");
            ss = ss.Replace("Giftschaden", "Schaden");
            ss = ss.Replace("Arkanschaden", "Schaden");
            ss = ss.Replace("Heiligschaden", "Schaden");

            ss = ss.Replace("points de dégâts de foudre", "points de dégâts");//frFR
            ss = ss.Replace("points de dégâts de feu", "points de dégâts");
            ss = ss.Replace("points de dégâts de froid", "points de dégâts");
            ss = ss.Replace("points de dégâts de poison", "points de dégâts");
            ss = ss.Replace("points de dégâts arcaniques", "points de dégâts");
            ss = ss.Replace("points de dégâts sacrés", "points de dégâts");

            ss = ss.Replace("danni da fulmine", "danni");//itIT
            ss = ss.Replace("danni da fuoco", "danni");
            ss = ss.Replace("danni da freddo", "danni");
            ss = ss.Replace("danni da veleno", "danni");
            ss = ss.Replace("danni arcani", "danni");
            ss = ss.Replace("danni sacri", "danni");

            ss = ss.Replace("p. de daño de rayos", "p.de daño");//esES
            ss = ss.Replace("p. de daño de fuego", "p.de daño");
            ss = ss.Replace("p. de daño de frío", "p.de daño");
            ss = ss.Replace("p. de daño de veneno", "p.de daño");
            ss = ss.Replace("p. de daño arcano", "p.de daño");
            ss = ss.Replace("p. de daño sagrado", "p.de daño");

            ss = ss.Replace("de daño de Rayo", "de daño");//enMX
            ss = ss.Replace("de daño de Fuego", "de daño");
            ss = ss.Replace("de daño de Frío", "de daño");
            ss = ss.Replace("de daño de Veneno", "de daño");
            ss = ss.Replace("de daño Arcano", "de daño");
            ss = ss.Replace("de daño Sacro", "de daño");

            ss = ss.Replace("obrażeń od błyskawic", "obrażeń");//plPL
            ss = ss.Replace("obrażeń od ognia", "obrażeń");
            ss = ss.Replace("obrażeń od zimna", "obrażeń");
            ss = ss.Replace("obrażeń od trucizny", "obrażeń");
            ss = ss.Replace("obrażeń od mocy tajemnej", "obrażeń");
            ss = ss.Replace("obrażeń od mocy świętej", "obrażeń");

            ss = ss.Replace("de dano Elétrico", "de dano");//ptPT
            ss = ss.Replace("de dano Ígneo", "de dano");
            ss = ss.Replace("de dano Gélido", "de dano");
            ss = ss.Replace("de dano Venenoso", "de dano");
            ss = ss.Replace("de dano Arcano", "de dano");
            ss = ss.Replace("de dano Sagrado", "de dano");

            ss = Regex.Replace(ss, @"\{.*?\}", ""); //removes {...}
            ss = Regex.Replace(ss, @"\[.*?\]", ""); //removes [...]
            ss = Regex.Replace(ss, @"\(.*?\)", ""); //removes (...)
            ss = removeValue(ss);
            return ss.Trim();//避免部分词条前后可能有空格导致不一致，例如英文版爆伤词缀后多了个空格
        }
        //获取词缀值
        private string getItemValue(string itemText)
        {
            if (itemText == null) return "";
            var ss = "";
            var find = false;
            Regex re = new Regex(@"\[(?<result>)[^[\]]+\]");  //取中括号中的内容
            MatchCollection mc = re.Matches(itemText);
            foreach (Match ma in mc)
            {
                ss = Regex.Replace(ma.Value, @"[\[\]]", ""); //removes [];
                string[] arr = ss.Split('-');
                foreach (var s in arr)
                {
                    ss = s;
                }
                arr = ss.Split('~');
                foreach (var s in arr)
                {
                    ss = s;
                }
                find = true;
            }
            /*if (!find)
            {
                re = new Regex(@"\((?<result>)[^[\]]+\)"); //取小括号中的内容，主要用于获取镶孔数量，此处存在问题会导致英文版客户端优先获取到错误内容(仅XX职业)导致无法转换数字
                mc = re.Matches(itemText);
                foreach (Match ma in mc)
                {
                    ss = Regex.Replace(ma.Value, "[()]", ""); //removes ()
                    find = true;
                }
            }*/
            if (!find)
            {
                var tempStr = "";
                var i = 0;
                var N = 0;
                foreach (var c in itemText)
                {
                    i++;
                    if ((c >= '0' && c <= '9'))
                    {
                        tempStr += c;
                        N = i;//记录数字的位置
                    }
                    if(c == '-' || c == '~')
                    {
                        tempStr += c;
                    }
                    if (c == '.' && (i - 1 == N))//记录数字之后1位的“.”避免英文句号被记录
                    {
                        tempStr += c;
                    }
                }
                find = true;
                ss = tempStr;
            } 
            if(ss.Contains("-"))//取最大伤害白字
            {
                var arr = ss.Split('-');
                ss = arr[1];
            }
            if (ss.Contains("~"))//koKR取最大伤害白字
            {
                var arr = ss.Split('~');
                ss = arr[1];
            }
            ss = Regex.Replace(ss, ",", "");
            return Regex.Replace(ss, " ","");
        }
        //判断词缀是否应被选择
        private bool isItemSelected()
        {
            IUiElement[] ItemList =   
            {
                Hud.Render.RegisterUiElement("Root.NormalLayer.vendor_dialog_mainPage.enchant_dialog.LayoutRoot.item_affix_list._content._stackpanel._item0.list_item_window.selected_background", null, null), //属性选中为true
                Hud.Render.RegisterUiElement("Root.NormalLayer.vendor_dialog_mainPage.enchant_dialog.LayoutRoot.item_affix_list._content._stackpanel._item1.list_item_window.selected_background", null, null), //属性选中为true
                Hud.Render.RegisterUiElement("Root.NormalLayer.vendor_dialog_mainPage.enchant_dialog.LayoutRoot.item_affix_list._content._stackpanel._item2.list_item_window.selected_background", null, null), //属性选中为true
                Hud.Render.RegisterUiElement("Root.NormalLayer.vendor_dialog_mainPage.enchant_dialog.LayoutRoot.item_affix_list._content._stackpanel._item3.list_item_window.selected_background", null, null), //属性选中为true
                Hud.Render.RegisterUiElement("Root.NormalLayer.vendor_dialog_mainPage.enchant_dialog.LayoutRoot.item_affix_list._content._stackpanel._item4.list_item_window.selected_background", null, null), //属性选中为true
                Hud.Render.RegisterUiElement("Root.NormalLayer.vendor_dialog_mainPage.enchant_dialog.LayoutRoot.item_affix_list._content._stackpanel._item5.list_item_window.selected_background", null, null), //属性选中为true
                Hud.Render.RegisterUiElement("Root.NormalLayer.vendor_dialog_mainPage.enchant_dialog.LayoutRoot.item_affix_list._content._stackpanel._item6.list_item_window.selected_background", null, null), //属性选中为true
                Hud.Render.RegisterUiElement("Root.NormalLayer.vendor_dialog_mainPage.enchant_dialog.LayoutRoot.item_affix_list._content._stackpanel._item7.list_item_window.selected_background", null, null), //属性选中为true
                Hud.Render.RegisterUiElement("Root.NormalLayer.vendor_dialog_mainPage.enchant_dialog.LayoutRoot.item_affix_list._content._stackpanel._item8.list_item_window.selected_background", null, null), //属性选中为true
                Hud.Render.RegisterUiElement("Root.NormalLayer.vendor_dialog_mainPage.enchant_dialog.LayoutRoot.item_affix_list._content._stackpanel._item9.list_item_window.selected_background", null, null), //属性选中为true
            };

            return ItemList.Any(a =>a.Visible);
        }
        private void AutoEnchantWork()
        {
            IUiElement[] ItemTextList =   
            {
                Hud.Render.RegisterUiElement("Root.NormalLayer.vendor_dialog_mainPage.enchant_dialog.LayoutRoot.item_affix_list._content._stackpanel._item0.list_item_window.text", null, null), //第1条属性内容
                Hud.Render.RegisterUiElement("Root.NormalLayer.vendor_dialog_mainPage.enchant_dialog.LayoutRoot.item_affix_list._content._stackpanel._item1.list_item_window.text", null, null), //第2条属性内容
                Hud.Render.RegisterUiElement("Root.NormalLayer.vendor_dialog_mainPage.enchant_dialog.LayoutRoot.item_affix_list._content._stackpanel._item2.list_item_window.text", null, null), //第3条属性内容
                Hud.Render.RegisterUiElement("Root.NormalLayer.vendor_dialog_mainPage.enchant_dialog.LayoutRoot.item_affix_list._content._stackpanel._item3.list_item_window.text", null, null), //第3条属性内容
                Hud.Render.RegisterUiElement("Root.NormalLayer.vendor_dialog_mainPage.enchant_dialog.LayoutRoot.item_affix_list._content._stackpanel._item4.list_item_window.text", null, null), //第3条属性内容
                Hud.Render.RegisterUiElement("Root.NormalLayer.vendor_dialog_mainPage.enchant_dialog.LayoutRoot.item_affix_list._content._stackpanel._item5.list_item_window.text", null, null), //第3条属性内容
                Hud.Render.RegisterUiElement("Root.NormalLayer.vendor_dialog_mainPage.enchant_dialog.LayoutRoot.item_affix_list._content._stackpanel._item6.list_item_window.text", null, null), //第3条属性内容
                Hud.Render.RegisterUiElement("Root.NormalLayer.vendor_dialog_mainPage.enchant_dialog.LayoutRoot.item_affix_list._content._stackpanel._item7.list_item_window.text", null, null), //第3条属性内容
                Hud.Render.RegisterUiElement("Root.NormalLayer.vendor_dialog_mainPage.enchant_dialog.LayoutRoot.item_affix_list._content._stackpanel._item8.list_item_window.text", null, null), //第3条属性内容
                Hud.Render.RegisterUiElement("Root.NormalLayer.vendor_dialog_mainPage.enchant_dialog.LayoutRoot.item_affix_list._content._stackpanel._item9.list_item_window.text", null, null), //第3条属性内容
            };
            
            if (EnchantItemText == ""
                || !enchantDialog.Visible 
                || !enchantButton.Visible        //保证放置附魔物品
                || PlaceanItemtoReplaceaProperty()
            ) return;

          
            //能执行到这里，表示可以附魔了
            if (isItemSelected())   //有词缀选中
            {
                if (ReplaceProperty() && enchant_state != EnchantState.generated)   //首次附魔或更新已经附魔词缀，这个按下去，会产生新词缀
                {
                    //获取附魔所需金币
                    string gold = "";
                    if (Hud.CurrentLanguage == Language.zhCN || Hud.CurrentLanguage == Language.zhTW)
                    {
                        gold = PublicClassPlugin.Between(enchantButton.ReadText(Encoding.UTF8, true), "：", "{");
                    }
                    else
                    {
                        gold = PublicClassPlugin.Between(enchantButton.ReadText(Encoding.UTF8, true), ":", "{");
                    }
                    if (gold.Contains(","))//处理千位分隔符
                    {
                        gold = gold.Replace(",", "");
                    }
                    if (gold.Contains("."))//处理deDE千位分隔符
                    {
                        gold = gold.Replace(".", "");
                    }
                    if (gold.Contains(" "))//处理frFR千位分隔符,不是普通的空格
                    {
                        gold = gold.Replace(" ", "");
                    }
                    gold = gold.Replace(" ", "");//处理可能出现的普通空格
                    if (gold == "")
                    {
                        EnchantGold = 0;
                    }
                    else
                    {
                        EnchantGold = long.Parse(gold);
                    }
                    //按下附魔按钮前的条件
                    var text0 = ItemTextList[0].ReadText(Encoding.UTF8, true);
                    if (Perfect == true || (getItemContent(text0) == getItemContent(EnchantItemText) && getItemValue(text0) == getItemValue(EnchantItemText))//附魔词缀达到最大值
                        || currentCount >= enchantCount //附魔次数完成
                        || Hud.Game.Me.Materials.Gold - EnchantGold <= ReservedGold//金币低于保留数量
                        )
                    {
                        Running = false;
                        TurnedOn = false;
                        Perfect = false;
                        currentCount = 0;
                    }
                    else
                    {
                        Hud.Interaction.ClickUiElement(MouseButtons.Left, enchantButton);
                        enchant_state = EnchantState.generated;
                        currentCount++;  //完成一次附魔
                    }
                }
                else if (SelectProperty() && enchant_state != EnchantState.changed)  //选中词缀替换现有词缀
                {
                    enchant_state = EnchantState.changed;
                    Hud.Interaction.ClickUiElement(MouseButtons.Left, enchantButton);
                }
            }
            else if (ItemTextList.Where(a =>a.Visible).Count() >= 3)//没有词缀选中，需要选择合适词缀，要等三个词缀都出现
            {
                var text0 = ItemTextList[0].ReadText(Encoding.UTF8, true);//词缀列表第一个词缀
                double Item0Value = CustomNumericParse(getItemValue(text0));
                if (getItemContent(text0) == getItemContent(EnchantItemText))   //如果当前词缀是符合要求的,选择属性更好的词缀
                {
                    var selectItem = ItemTextList[0];
                    foreach (var item in ItemTextList)
                    {
                        if (!item.Visible) continue;
                        var text = item.ReadText(Encoding.UTF8, true);
                        
                        if (getItemContent(text) == getItemContent(EnchantItemText))
                        {
                            double ItemValue = CustomNumericParse(getItemValue(text));
                            if (ItemValue >= Item0Value && ItemValue >= CustomNumericParse(getItemValue(selectItem.ReadText(Encoding.UTF8, true))))
                            {
                                selectItem = item;
                                if (getItemContent(text) == getItemContent(EnchantItemText) && getItemValue(text) == getItemValue(EnchantItemText))//附魔词缀达到最大值
                                {
                                    Perfect = true;
                                    break;
                                }
                            }
                        }
                    }
                    if (selectItem.Visible && enchant_state != EnchantState.selected && SelectProperty())
                    {
                        Hud.Interaction.ClickUiElement(MouseButtons.Left, selectItem);
                        enchant_state = EnchantState.selected;
                    }
                }
                else //选择属性正确的词缀
                {
                    var selectItem = ItemTextList[0];
                    foreach (var item in ItemTextList)
                    {
                        if (!item.Visible) continue;
                        var text = item.ReadText(Encoding.UTF8, true);
                        if (getItemContent(text) == getItemContent(EnchantItemText))
                        {
                            selectItem = item;
                            if (getItemContent(text) == getItemContent(EnchantItemText) && getItemValue(text) == getItemValue(EnchantItemText))//附魔词缀达到最大值
                            {
                                Perfect = true;
                                break;
                            }
                            break;
                        }
                    }
                    if (selectItem.Visible && enchant_state != EnchantState.selected && SelectProperty())
                    {
                        Hud.Interaction.ClickUiElement(MouseButtons.Left, selectItem);
                        enchant_state = EnchantState.selected;
                    }
                }
            }
        }
        public void AfterCollect()
        {

            if (!TestTurnedOn()) return;
            if (!Running) return;
            AutoEnchantWork();
        }
        private bool TestTurnedOn()
        {
            if (!Hud.Game.IsInGame 
                || !Hud.Inventory.InventoryMainUiElement.Visible 
                || !enchantDialog.Visible
                || !Hud.Window.IsForeground
                )
            {
                TurnedOn = false;
                Running = false;
                enchant_state = EnchantState.start;
            }
            return TurnedOn;
        }

        private double CustomNumericParse(string v)
        {
            double returnV = 0d;
            try
            {
                returnV = (double)decimal.Parse(v);
                return returnV;
            }
            catch (FormatException)
            {
                Hud.Debug(v);
                if (string.IsNullOrEmpty(v)) return 0d; //如果输入的字符串为空或NULL，则直接返回0
                if (!char.IsDigit(v[0])) return 0d; //如果输入的字符串是非数字开头，直接返回0
                string subV = string.Empty;
                for (int i = 0; i < v.Length; i++)
                {
                    if (char.IsDigit(v[i]) || (v[i].Equals('.') && !subV.Contains("."))) //从左至右，判断字符串的每位字符是否是数字或小数点，小数点只保留第一个
                        subV += v[i];
                    else
                        break;
                }
                subV.TrimEnd(new char[] { '.' }); // 如果解析后的子字符串的末位是小数点，则去掉它

                if (subV.Contains(".")) // 如果解析结果包含小数点，则根据小数点分两段求值
                {
                    string strPointRight = subV.Substring(subV.IndexOf('.') + 1); //小数点右侧部分
                    subV = subV.Substring(0, subV.IndexOf('.')); //小数点左侧部分
                                                                 //计算小数点右侧的部分
                    for (int i = 0; i < strPointRight.Length; i++)
                    {
                        returnV += ((int)strPointRight[i] - 48) / Math.Pow(10, i + 1); //(int)strPointRight[i] 是取该字符的ASCII码
                    }
                }
                //计算小数点左侧的部分
                int iLen = subV.Length; //小数点左侧部分的长度
                for (int i = 0; i < iLen; i++)
                {
                    returnV += ((int)subV[i] - 48) * Math.Pow(10, iLen - 1 - i); //按位乘以10的幂，并和小数点右侧结果相加
                }
                return returnV;
            }
            finally
            {
            }

        }
    }
}