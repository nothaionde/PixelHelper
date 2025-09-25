namespace Turbo.Plugins.LightningMod
{
    using System.Linq;
    using System.Text;
    using System.Windows.Forms;
    public class BaseKanaiPlugin : BaseInventoryManagementPlugin
    {


        protected IUiElement vendorPage;
        protected IUiElement transmuteDialog;
        protected IUiElement pageNumber;
        protected IUiElement transmuteButton;
        protected IUiElement reciepeButton;
        protected IUiElement fillButton;
        protected IUiElement nextButton;
        protected IUiElement prevButton;
        protected IUiElement item1;
        protected IUiElement item2;
        protected int[] PageIndexes { get; }

        public bool ExtremeSpeedMode { get; private set; }
        public BaseKanaiPlugin(params int[] pageIndexes)
        {
            PageIndexes = pageIndexes;
            ExtremeSpeedMode = false;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);

            vendorPage = Hud.Render.RegisterUiElement("Root.NormalLayer.vendor_dialog_mainPage", Hud.Inventory.InventoryMainUiElement, null);
            transmuteDialog = Hud.Render.RegisterUiElement("Root.NormalLayer.vendor_dialog_mainPage.transmute_dialog", vendorPage, null);
            pageNumber = Hud.Render.RegisterUiElement("Root.NormalLayer.Kanais_Recipes_main.LayoutRoot.PageControls.page_number", transmuteDialog, null);
            transmuteButton = Hud.Render.RegisterUiElement("Root.NormalLayer.vendor_dialog_mainPage.transmute_dialog.LayoutRoot.transmute_button", transmuteDialog, null);
            reciepeButton = Hud.Render.RegisterUiElement("Root.NormalLayer.vendor_dialog_mainPage.transmute_dialog.LayoutRoot.recipe_button", transmuteDialog, null);
            fillButton = Hud.Render.RegisterUiElement("Root.NormalLayer.Kanais_Recipes_main.LayoutRoot.button_fill_currencies", transmuteDialog, null);
            nextButton = Hud.Render.RegisterUiElement("Root.NormalLayer.Kanais_Recipes_main.LayoutRoot.PageControls.page_next", transmuteDialog, null);
            prevButton = Hud.Render.RegisterUiElement("Root.NormalLayer.Kanais_Recipes_main.LayoutRoot.PageControls.page_previous", transmuteDialog, null);
            item1 = Hud.Render.RegisterUiElement("Root.TopLayer.item 1.stack", transmuteDialog, null);
            item2 = Hud.Render.RegisterUiElement("Root.TopLayer.item 2.stack", transmuteDialog, null);
        }
        
        protected int GetPageNum()
        {
            //Hud.ReCollect(true);
            var pageText = pageNumber.ReadText(Encoding.UTF8, true);
            var pageleft = Between(pageText, null, "，");
            if (string.IsNullOrEmpty(pageleft)) pageleft = Between(pageText, null, ",");//处理zhCN分页符
            if (string.IsNullOrEmpty(pageleft)) pageleft = Between(pageText, null, "/");//处理大部分语言分页符
            if (string.IsNullOrEmpty(pageleft)) pageleft = Between(pageText, null, "из");//处理ruRU的分页符
            if (string.IsNullOrEmpty(pageleft)) return -1;
            if (pageleft.Contains("10")) return 10;
            else if (pageleft.Contains("1")) return 1;
            else if (pageleft.Contains("2")) return 2;
            else if (pageleft.Contains("3")) return 3;
            else if (pageleft.Contains("4")) return 4;
            else if (pageleft.Contains("5")) return 5;
            else if (pageleft.Contains("6")) return 6;
            else if (pageleft.Contains("7")) return 7;
            else if (pageleft.Contains("8")) return 8;
            else if (pageleft.Contains("9")) return 9;
            else return -1;
        }
        public static string Between(string str, string strLeft, string strRight) //取文本中间
        {
            if (str == null || str.Length == 0) return "";
            if (strLeft != null)
            {
                int indexLeft = str.IndexOf(strLeft);//左边字符串位置
                if (indexLeft < 0) return "";
                indexLeft = indexLeft + strLeft.Length;//左边字符串长度
                if (strRight != null)
                {
                    int indexRight = str.IndexOf(strRight, indexLeft);//右边字符串位置
                    if (indexRight < 0) return "";
                    return str.Substring(indexLeft, indexRight - indexLeft);//indexRight - indexLeft是取中间字符串长度
                }
                else return str.Substring(indexLeft, str.Length - indexLeft);//取字符串右边
            }
            else//取字符串左边
            {
                if (strRight == null) return "";
                int indexRight = str.IndexOf(strRight);
                if (indexRight <= 0) return "";
                else return str.Substring(0, indexRight);
            }
        }
        protected bool OpenKanaiCube(int pageNum)
        {
            for (int i = 0; i < 50; i++)
            {
                int _delay = (int)Hud.Game.CurrentLatency + 60 - (int)Hud.Stat.RenderPerfCounter.LastCount;
                if (_delay < 100) _delay = 100;
                //往前一页
                Hud.Interaction.MoveMouseOverUiElement(prevButton);
                Hud.Interaction.MouseDown(MouseButtons.Left);
                Hud.Interaction.MouseUp(MouseButtons.Left);
                if (!Hud.WaitFor(_delay, 10, 10, () => GetPageNum() == pageNum))
                {
                    break;
                }
                if (!Hud.Window.IsForeground) return false;
            }
            for (int x = 0; x < 50; x++)//50次纠错
            {
                int _delay = (int)Hud.Game.CurrentLatency + 60 - (int)Hud.Stat.RenderPerfCounter.LastCount;
                if (_delay < 100) _delay = 100;
                //翻页到指定页
                if (!Hud.Window.IsForeground || !transmuteDialog.Visible) return false;
                if (GetPageNum() != pageNum)
                {
                    if (GetPageNum() < pageNum)
                    {
                        //向后翻页
                        //Hud.Interaction.ClickUiElement(MouseButtons.Left, nextButton);
                        Hud.Interaction.MoveMouseOverUiElement(nextButton);
                        Hud.WaitFor(_delay, 10, 10, () => !item1.Visible && !item2.Visible);
                        Hud.Interaction.MouseDown(MouseButtons.Left);
                        Hud.Interaction.MouseUp(MouseButtons.Left);
                        if (Hud.WaitFor(_delay, 10, 10, () => GetPageNum() == pageNum))
                        {
                            continue;
                        }
                    }
                    else
                    {
                        //向前翻页
                        //Hud.Interaction.ClickUiElement(MouseButtons.Left, prevButton);
                        Hud.Interaction.MoveMouseOverUiElement(prevButton);
                        Hud.WaitFor(_delay, 10, 10, () => !item1.Visible && !item2.Visible);
                        Hud.Interaction.MouseDown(MouseButtons.Left);
                        Hud.Interaction.MouseUp(MouseButtons.Left);
                        if (Hud.WaitFor(_delay, 10, 10, () => GetPageNum() == pageNum))
                        {
                            continue;
                        }
                    }
                }
                else
                {
                    return true;
                }
            }
            return false;
        }

        protected bool TransmuteOne(IItem item)
        {
            for (int i = 0; i < 50; i++)
            {
                int _delay = (int)Hud.Game.CurrentLatency + 60 - (int)Hud.Stat.RenderPerfCounter.LastCount;
                if (_delay < 100) _delay = 100;
                if (!Hud.Window.IsForeground || !transmuteDialog.Visible)
                {
                    Hud.Interaction.MouseUp(MouseButtons.Left);
                    return false;
                }
                if (Hud.Inventory.ItemsInInventory.Any(x => x.ItemUniqueId == item.ItemUniqueId))
                {
                    //将物品放入魔盒
                    for (int j = 0; j < 50; j++)
                    {
                        if (!Hud.Window.IsForeground || !transmuteDialog.Visible)
                        {
                            Hud.Interaction.MouseUp(MouseButtons.Left);
                            return false;
                        }
                        //Hud.Interaction.ClickInventoryItem(MouseButtons.Right, item);
                        Hud.Interaction.MoveMouseOverInventoryItem(item);
                        Hud.Wait(10);
                        Hud.Interaction.MouseDown(MouseButtons.Left);
                        Hud.Wait(10);
                        Hud.Interaction.MouseMove(transmuteDialog.Rectangle.Width / 2 + Hud.Inventory.GetItemRect(item).Width, transmuteDialog.Rectangle.Height / 2 - Hud.Inventory.GetItemRect(item).Height);
                        Hud.Wait(10);
                        Hud.Interaction.MouseUp(MouseButtons.Left);
                        Hud.Wait(_delay);//转换按钮变亮色
                        if (transmuteButton.AnimState == 54)
                        {
                            break;
                        }
                        if(j == 50) return false;
                    }
                    //Hud.WaitFor(_delay, 10, 10, () => transmuteButton.AnimState == 54);//转换按钮变亮色
                    
                    //放入材料
                    Hud.Interaction.MoveMouseOverUiElement(fillButton);
                    Hud.Interaction.MouseDown(MouseButtons.Right);
                    Hud.Interaction.MouseUp(MouseButtons.Right);
                    Hud.WaitFor(_delay, 10, 10, () => !item1.Visible && !item2.Visible);
                    Hud.Interaction.ClickUiElement(MouseButtons.Left, fillButton);
                    Hud.Wait(10);
                    Hud.Interaction.ClickUiElement(MouseButtons.Left, fillButton);
                    Hud.Wait((int)Hud.Game.CurrentLatency);
                    Hud.Interaction.MouseUp(MouseButtons.Right);

                    for (int j = 0; j < 10; j++)
                    {
                        //点击转换
                        Hud.Interaction.ClickUiElement(MouseButtons.Left, transmuteButton);
                        Hud.WaitFor(_delay, 10, 10, () => transmuteButton.AnimState == 51);//转换按钮变灰色
                        Hud.ReCollect();
                        if (Hud.WaitFor(_delay, 10, 10, () => !Hud.Inventory.ItemsInInventory.Any(x => x.ItemUniqueId == item.ItemUniqueId)))
                        {
                            return true;
                        }
                    }   
                }
                else
                {
                    return true;
                }
            }
            return false;
                
        }

        protected bool ValidateTransmuteTurnedOn(bool UseInventoryLockArea)
        {
            if (!Hud.Game.IsInGame || !transmuteDialog.Visible || !pageNumber.Visible || !Hud.Window.IsForeground || (UseInventoryLockArea && (Hud.Inventory.InventoryLockArea.Width <= 0 || Hud.Inventory.InventoryLockArea.Height <= 0)))
            {
                TurnedOn = false;
            }
            return TurnedOn;
        }
    }
}