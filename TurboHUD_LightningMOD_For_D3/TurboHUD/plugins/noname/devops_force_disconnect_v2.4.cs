//DevOps Force Disconnect (DOFD) plugin for any TurboHud
//This plugin is intended for manual play and botting.
//Thank you Rock & Evan for the help getting started.
//Use of this plugin is your own risk!

//Plugin updates can be found here:
//https://www.ros-bot.com/custom-script/devops-force-disconnect-turbohud-7257675

//About:
// This plugin abuses server side mechanics to immediately disconnect to main menu.
// This disconnect method is faster than teleporting to town and then leaving the game.
// Let's you instantly leave a game even in places where it is not allowed.
// It avoids the 10 second wait timer that is usually seen.

//Puretools settings:
// "Close unwanted D3 Windows" must be DISABLED.
// "Prevent interrupt of teleports in regular rifts" must be DISABLED.
// This is important as both features prevent the proper fuctionality of this plugin.

//Options:
// Search for "Plugin options" section within this script.
// There you can configure certain options of this plugin.
// Some options are disabled by default you may want to enable them.

//Features implemented:
// - Automated create new game after a forced disconnect
// - Automated disconnect when "Leave Game" window is detected
// - Automated disconnect when the second pool is collected within the cow lovel
// - Automated disconnect when the Gate is activated in "The (Ancient) Vault" to skip boss fight
// - Automated disconnect in nephalem rifts after keys have been picked up and teleport is casted
// - Automated disconnect in nephalem rifts if the Guardian is not killed within a specified time

//DOFD Version:
// v1.0: Initial release [Basic proof of concept]
// v1.1: Mitigate potential freeze while disconnecting [Fix provided by @Hello_o]
// v1.2: Code refactoring, cleanup and added more comments [Tidying up]
// v1.3: Replaced freeze fix with a cleaner approach [Should be more stable]
// v1.4: Now quick starts games after disconnect [Convenience]
// v1.5: Immediately disconnect when "Leave Game" is detected [Manual play]
// v1.6: Complete restructure of the code layout [Tidying up]
// v1.7: Now disables the plugin in multiplayer to avoid issues [Configurable]
// v1.8: Immediately disconnect when the Vault boss gate is activated [Skip boss fight]
// v1.9: Immediately disconnect when the second pool is collected [Not the cow level]
// v2.0: Added function to write log entries [For debugging purposes only]
// v2.1: Added funcion to move the mouse cursor [For debugging purposes only]
// v2.2: When player is dead do not cast or wait for teleport [Fix]
// v2.3: Disconnect when Nephalem Rift Guardian is not killed in time [Configurable]
// v2.4: Some cleanup and minor improvements [Tidying up]

//Name space:
namespace Turbo.Plugins.User.Devops
{

    //Import turbohud:
    using Turbo.Plugins.Default;

    //Import modules:
    using System;
    using System.Linq;
    using System.Drawing;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using Turbo.Plugins.PixelDrama;

    //Main class:
    public class main_class2 : BasePlugin, IAfterCollectHandler
    {

        //Toggle plugin:
        public main_class2()
        {

            Enabled = true;

        }

        //Initialize controller:
        public override void Load(IController hud)
        {

            //Load controller:
            base.Load(hud);
            
        }
        
        //Import external functions:
        [DllImport("user32.dll")] //Find window handle by name:
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")] //Get window client area screen coordinates:
        static extern bool ClientToScreen(IntPtr hwnd, ref Point lpPoint);

        [DllImport("user32.dll")] //Move mouse cursor to screen position:
        public static extern long SetCursorPos(int x, int y);

        [DllImport("user32.dll")] //Send synchronous message to window:
        public static extern uint SendMessage(IntPtr hWnd, uint nMessage, uint wParam, uint lParam);

        [DllImport("user32.dll")] //Post asynchronous message to window:
        public static extern uint PostMessage(IntPtr hWnd, uint nMessage, uint wParam, uint lParam);

        //Global variables:
        public static class Globals
        {

            // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

            //Plugin options, configure as needed...

            //Disable all plugin actions in parties:
            //Note: This plugin can cause issues in multiplayer!
            public static bool disable_plugin_within_party = true;

            // - - - - - - - - - -

            //Disconnect immediately when the "Leave Game" window is detected:
            //Note: This will abort normal "Leave Game" and use disconnect method instead.
            public static bool leave_game_disconnect = true;

            //Automatically create a new game after "leave_game_disconnect":
            //Note: To access main menu, teleport to town and leave from there.
            public static bool leave_game_disconnect_create_new_game = true;

            // - - - - - - - - - -

            //Disconnect in nephalem rifts after picking the greater rift keys:
            //Note: Activated by casting town teleport.
            public static bool nephalem_rift_disconnect = true;

            //Automatically create a new game after "nephalem_rift_disconnect":
            public static bool nephalem_rift_disconnect_create_new_game = true;

            // - - - - - - - - - -

            //Disconnect if nephalem rift guardian is not killed within x seconds
            //while the "Kill the Nephalem Rift Guardian" quest step is active:
            //Note: Only useful for botting when guardian is on another map.
            public static bool rift_guardian_kill_timeout_disconnect = false;

            //Kill timeout in seconds before disconnect is performed:
            public static int rift_guardian_kill_timeout_seconds = 25;

            //Reset timeout if Guardian is within x yards distance:
            public static int rift_guardian_kill_timeout_reset_yards = 100;

            //Automatically create a new game after "rift_guardian_kill_timeout_disconnect":
            public static bool rift_guardian_kill_timeout_disconnect_create_new_game = true;

            // - - - - - - - - - -

            //Disconnect in cow level after second pool is collected:
            //Note: Rosbot/Pure Numbers will report wrong numbers!
            // Probably slower with bot but works good for manual play.
            public static bool cow_level_disconnect = false;

            //Automatically create a new game after "cow_level_disconnect":
            public static bool cow_level_disconnect_create_new_game = true;

            // - - - - - - - - - -

            //Disconnect before entering the Vaults boss fight:
            //Note: The vaults boss fight is a waste of time.
            public static bool goblin_vault_disconnect = false;

            //Automatically create a new game after "goblin_vault_disconnect":
            public static bool goblin_vault_disconnect_create_new_game = true;

            // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

            //The following variables should not be changed!

            //Get diablo 3 process window handle:
            public static IntPtr d3_window_handle = FindWindow(null, "Diablo III");

            //Define windows messages and virtual keys:
            public const uint virtual_key_left_mouse_button = 0x01; //VK_LBUTTON
            public const uint window_message_left_mouse_button_down = 0x0201; //WM_LBUTTONDOWN
            public const uint window_message_left_mouse_button_up = 0x0202; //WM_LBUTTONUP

            //Define interface elements (Party disable)
            public static string ui_element_player_portrait_frame_1 = "Root.NormalLayer.portraits.stack.party_stack.portrait_1.Frame"; //Player 2
            public static string ui_element_player_portrait_frame_2 = "Root.NormalLayer.portraits.stack.party_stack.portrait_2.Frame"; //Player 3
            public static string ui_element_player_portrait_frame_3 = "Root.NormalLayer.portraits.stack.party_stack.portrait_3.Frame"; //Player 4

            //Define interface elements (New game):
            public static string ui_element_start_new_game_button = "Root.NormalLayer.BattleNetCampaign_main.LayoutRoot.Menu.PlayGameButton";
            public const string ui_element_switch_hero_menu_button = "Root.NormalLayer.BattleNetCampaign_main.LayoutRoot.Slot1.LayoutRoot.SwitchHero";
            public const string ui_element_is_entering_game_tooltip = "Root.TopLayer.tooltip_dialog_background.tooltip_2";

            //Define interface elements ("Leave Game"):
            public const string ui_element_leave_game_window_cancel_button = "Root.TopLayer.BattleNetNotifications_main.LayoutRoot.LeaveGameWindow.LogoutContainer.LogoutCancelButton";
            public static string ui_element_town_portal_start_button = "Root.NormalLayer.game_dialog_backgroundScreenPC.button_recall";
            public static string ui_element_battlenet_error_notification_text = "Root.TopLayer.error_notify.error_text";
            public static List<string> ability_forbidden_error_text = new List<string> {
                "該技能無法在此使用。",                                   //Chinese
                "This ability cannot be used here.",                   //English
                "Cette compétence ne peut pas être utilisée ici.",     //French
                "Diese Fertigkeit kann hier nicht eingesetzt werden.", //German
                "L'abilità non si può usare qui.",                     //Italian
                "여기서는 이 능력을 사용할 수 없습니다.",                   //Korean
                "Tutaj nie można użyć tej zdolności.",                 //Polish
                "Esta habilidade não pode ser usada aqui.",            //Portuguese
                "Это умение нельзя использовать здесь.",               //Russian
                "No puedes usar esta habilidad aquí.",                 //Spanish (EU)
                "Esta habilidad no puede usarse aquí."                 //Spanish (AL)
            };

            //Define interface elements (Force disconnect):
            public static string ui_element_collection_shop_open_button = "Root.NormalLayer.BattleNetFooter_main.LayoutRoot.ButtonContainer.PersonalizationButton";
            public static string ui_element_collection_shop_main_window = "Root.NormalLayer.BattleNetStore_main.LayoutRoot.OverlayContainer";
            public static string ui_element_collection_shop_wings_button = "Root.NormalLayer.BattleNetStore_main.LayoutRoot.OverlayContainer.Menu._content._stackpanel._item2.NavigationMenuListItemContainer.MenuListItem.MenuSubItem.MenuSubList._content._stackpanel._item0";
            public static string ui_element_collection_shop_equip_button = "Root.NormalLayer.BattleNetStore_main.LayoutRoot.OverlayContainer.GridList.LayoutRoot.Content.PreviewTemplate.PreviewItemDescription.StoreButtons";
            public static string ui_element_collection_shop_cancel_button = "Root.NormalLayer.BattleNetStore_main.LayoutRoot.OverlayContainer.PageHeader.CloseButton";
            public static string ui_element_battlenet_notification_ok_button = "Root.TopLayer.BattleNetModalNotifications_main.ModalNotification.Buttons.ButtonList.OkButton";

            //Define disconnect variables:
            public static long posts_per_loop = 50;
            public static long loops_to_skip = 3;
            public static long loops_skipped = Globals.loops_to_skip; //Don't change.
            public static bool is_disconnecting = false; //Don't change.

            //Define generic variables:
            public static bool create_new_game = false; //Don't change.
            public static bool leave_game_cancelled = false; //Don't change.
            public static bool town_teleport_casted = false; //Don't change.
            public static bool key_count_initialized = false; //Don't change.
            public static bool key_count_increased = false; //Don't change.
            public static long start_key_count = 0; //Don't change.
            public static List<string> xp_pool_markers = new List<string>(); //Don't change.
            public static bool pool_markers_initialized = false; //Don't change.
            public static bool both_pools_collected = false; //Don't change.
            public static bool vault_gate_operated = false; //Don't change.
            public static bool rift_guardian_kill_quest_started = false; //Don't change.
            public static long rift_guardian_kill_quest_started_time = 0; //Don't change.
            public static bool rift_guardian_kill_timeout_exceeded = false; //Don't change.

            //Debug mouse cursor:
            public static bool move_mouse_cursor = false;
            public static int x_coordinate_screen_old = 0; //Don't change.
            public static int y_coordinate_screen_old = 0; //Don't change.

            //Debug log variables:
            public static bool enable_debug_log = false;
            public static bool delete_old_log = true;
            public static bool new_log_started = false; //Don't change.
            public static string debug_log_name = "dofd_debug_log"; //.txt
            public static List<string> debug_log_once_list = new List<string>(); //Don't change.

        }

        //Utilities class:
        public static class Utilities
        {

            //Write debug log:
            public static void write_debug_log(IController Hud, string message, bool log_once)
            {

                //Check logging is enabled:
                if (
                    Globals.enable_debug_log == true
                )
                {

                    //Delete old log:
                    if (
                        Globals.delete_old_log == true
                    )
                    {

                        Globals.delete_old_log = false;
                        Hud.TextLog.Delete(Globals.debug_log_name);

                    }

                    //Write log devider:
                    if (
                        Globals.new_log_started == false
                    )
                    {

                        Globals.new_log_started = true;
                        Hud.TextLog.Log(Globals.debug_log_name, "\nDOFD-LOG:", false, true);

                    }

                    //Check if "log once" is enabled:
                    if (
                        log_once == true
                    )
                    {

                        //Check message not yet logged: 
                        if (
                            !Globals.debug_log_once_list.Contains(message)
                        )
                        {

                            //Add message to list:
                            Globals.debug_log_once_list.Add(message);

                        }
                        else
                        {

                            //Skip logging message:
                            return;

                        }

                    }

                    //Write message to log:
                    Hud.TextLog.Log(Globals.debug_log_name, message, true, true);

                }

            }

            //Get user interface element information function:
            public static IUiElement get_ui_element(IRenderController renderController, string ui_element_path)
            {

                var ui_element = renderController.GetUiElement(ui_element_path) ??
                renderController.RegisterUiElement(ui_element_path, null, null);
                return ui_element;

            }

            //Is user interface element visible function:
            public static bool ui_element_visible(IRenderController renderController, string ui_element_path)
            {

                var ui_element = Utilities.get_ui_element(renderController, ui_element_path);
                return !(ui_element is null) && ui_element.Visible;

            }

            //Click user interface element function:
            public static void click_ui_element(IRenderController renderController, string message_type, string ui_element_path)
            {

                //Get UI element attributes:
                var ui_element_attributes = Utilities.get_ui_element(renderController, ui_element_path);

                //Calculate x-coordinate center:
                var x_coordinate = Convert.ToUInt16(ui_element_attributes.Rectangle.X + ui_element_attributes.Rectangle.Width / 2);

                //Calculate y-coordinate center:
                var y_coordinate = Convert.ToUInt16(ui_element_attributes.Rectangle.Y + ui_element_attributes.Rectangle.Height / 2);

                //Create long center parameter:
                var ui_element_center = (uint)((y_coordinate << 16) | (x_coordinate & 0xFFFF));

                //Check if mouse debug is enabled:
                if (
                    Globals.move_mouse_cursor == true
                )
                {

                    //Get client area to screen coordinates:
                    Point window_screen_coordinates = new Point();
                    ClientToScreen(Globals.d3_window_handle, ref window_screen_coordinates);

                    //Calculate final screen area coordinates:
                    int x_coordinate_screen = window_screen_coordinates.X + x_coordinate;
                    int y_coordinate_screen = window_screen_coordinates.Y + y_coordinate;

                    //Prevent movement to the same coordinates:
                    //Note: Otherwise it would spam the cursor position!
                    if (
                        Globals.x_coordinate_screen_old != x_coordinate_screen
                        && Globals.y_coordinate_screen_old != y_coordinate_screen
                    )
                    {

                        //Move mouse to screen area coordinates:
                        Globals.x_coordinate_screen_old = x_coordinate_screen;
                        Globals.y_coordinate_screen_old = y_coordinate_screen;
                        SetCursorPos(x_coordinate_screen, y_coordinate_screen);

                    }

                }

                //Left click center coordinates:
                switch (message_type)
                {

                    //Synchronous send:
                    case "SEND":
                        SendMessage(
                            Globals.d3_window_handle,
                            Globals.window_message_left_mouse_button_down,
                            Globals.virtual_key_left_mouse_button,
                            ui_element_center);
                        SendMessage(
                            Globals.d3_window_handle,
                            Globals.window_message_left_mouse_button_up,
                            Globals.virtual_key_left_mouse_button,
                            ui_element_center);
                        break;

                    //Asynchronous post:
                    case "POST":
                        PostMessage(
                            Globals.d3_window_handle,
                            Globals.window_message_left_mouse_button_down,
                            Globals.virtual_key_left_mouse_button,
                            ui_element_center);
                        PostMessage(
                            Globals.d3_window_handle,
                            Globals.window_message_left_mouse_button_up,
                            Globals.virtual_key_left_mouse_button,
                            ui_element_center);
                        break;

                }

            }

            //Disconnect function:
            public static void force_disconnect(IRenderController renderController)
            {

                //Note: Disconnect should be done while teleport to town is active.
                // The teleport should not be interupted till disconnect message appears.
                // This is necessary to mitigate long "Servers are busy" message when creating new games.
                // Also disconnecting too early after collecting the keys will confuse Rosbot/Purenumbers.
                // Which otherwise will lead to wrong values being reported for rifts and keys.
                // However the disconnect will still work even if these rquirements are not met.

                //Set disconnection status:
                Globals.is_disconnecting = true;

                //Accept network disconnect message:
                if (
                    Utilities.ui_element_visible(renderController, Globals.ui_element_battlenet_notification_ok_button)
                )
                {

                    Utilities.click_ui_element(renderController, "SEND", Globals.ui_element_battlenet_notification_ok_button);

                }

                //Force network disconnect:
                else if (
                    Utilities.ui_element_visible(renderController, Globals.ui_element_collection_shop_equip_button)
                )
                {

                    //Check skipped loops count:
                    if (
                        Globals.loops_to_skip == Globals.loops_skipped
                    )
                    {

                        //Reset skipped loops count:
                        Globals.loops_skipped = 0;

                        //Spam button:
                        for (
                            var i = 0; i < Globals.posts_per_loop; i++
                        )
                        {

                            //Click "Equip/Unequip" button:
                            Utilities.click_ui_element(renderController, "POST", Globals.ui_element_collection_shop_equip_button);

                        }

                    }
                    else
                    {

                        //Increase skipped loops count:
                        Globals.loops_skipped = Globals.loops_skipped + 1;

                    }

                }

                //Select wings entry:
                else if (
                    Utilities.ui_element_visible(renderController, Globals.ui_element_collection_shop_wings_button)
                )
                {

                    Utilities.click_ui_element(renderController, "SEND", Globals.ui_element_collection_shop_wings_button);

                }

                //Open collections menu:
                else if (
                    Utilities.ui_element_visible(renderController, Globals.ui_element_collection_shop_open_button)
                    && !Utilities.ui_element_visible(renderController, Globals.ui_element_collection_shop_main_window)
                )
                {

                    Utilities.click_ui_element(renderController, "SEND", Globals.ui_element_collection_shop_open_button);

                }

            }

            //Resets all switches to default:
            public static void reset_the_switches()
            {

                //Reset create new game:
                Globals.create_new_game = false;

                //Reset town teleport casted:
                Globals.town_teleport_casted = false;

                //Reset leave game cancelled:
                Globals.leave_game_cancelled = false;

                //Reset guardian time exceeded:
                Globals.rift_guardian_kill_timeout_exceeded = false;

                //Reset key count initialized:
                Globals.key_count_initialized = false;

                //Reset key count increased:
                Globals.key_count_increased = false;

                //Reset pool markers initialized:
                Globals.pool_markers_initialized = false;

                //Reset both pools collected:
                Globals.both_pools_collected = false;

                //Reset vault gate operated:
                Globals.vault_gate_operated = false;

                //Reset old cursor positions:
                Globals.x_coordinate_screen_old = 0;
                Globals.y_coordinate_screen_old = 0;

            }

        }

        //Each time after Turbohud refreshes data:
        public void AfterCollect()
        {
            Utilities.write_debug_log(Hud, "Plugin started...", true); //DEBUG-LOG

            //Check player is in menu:
            if (
                !Hud.Game.IsInGame
                && Utilities.ui_element_visible(Hud.Render, Globals.ui_element_start_new_game_button)
            )
            {

                //Close collection menu:
                if (
                    Globals.is_disconnecting == true
                    && Utilities.ui_element_visible(Hud.Render, Globals.ui_element_collection_shop_cancel_button)
                )
                {


                    Utilities.click_ui_element(Hud.Render, "SEND", Globals.ui_element_collection_shop_cancel_button);

                }
                else
                {

                    //Reset disconnecting:
                    Globals.is_disconnecting = false;

                    // If any "*_create_new_game" option is enabled
                    // and player is not in game then create a new game:
                    if (
                        Globals.create_new_game == true //Only executed after disconnecting!
                        && Utilities.ui_element_visible(Hud.Render, Globals.ui_element_switch_hero_menu_button)
                        && !Utilities.ui_element_visible(Hud.Render, Globals.ui_element_is_entering_game_tooltip)
                    )
                    {

                        Utilities.write_debug_log(Hud, "Start new game triggered.", false); //DEBUG-LOG
                        Utilities.click_ui_element(Hud.Render, "SEND", Globals.ui_element_start_new_game_button);

                    }

                }

            }

            //Ensure game is in an operatable state:
            if (
                !Hud.Game.IsPaused
                && !Hud.Game.IsLoading
                && !Hud.Render.UiHidden
            )
            {

                //Check player is in game:
                if (
                    Hud.Game.IsInGame
                )
                {

                    //Ensure the area has a valid sno number:
                    //Note: Prevents specific null pointer exceptions.
                    try {var check_area_is_valid = Hud.Game.Me.Scene.SnoArea.Sno;}
                    catch (System.NullReferenceException) {return;}

                    // If "disable_plugin_within_party" option is enabled
                    // and player is in game and there is more than one player:
                    if (
                        Globals.disable_plugin_within_party == true
                        && (Hud.Game.NumberOfPlayersInGame > 1
                        || Utilities.ui_element_visible(Hud.Render, Globals.ui_element_player_portrait_frame_1)  //Player 2
                        || Utilities.ui_element_visible(Hud.Render, Globals.ui_element_player_portrait_frame_2)  //Player 3
                        || Utilities.ui_element_visible(Hud.Render, Globals.ui_element_player_portrait_frame_3)) //Player 4
                    )
                    {

                        //Reset all switches:
                        Utilities.reset_the_switches();

                        //Skip execution:
                        return;

                    }

                    // Reset switches back to default
                    // when the town area is detected and player is in game:
                    if (
                        Hud.Game.IsInTown
                    )
                    {

                        if (
                            Globals.is_disconnecting == true
                        )
                        {

                            //Keep disconnecting:
                            Utilities.force_disconnect(Hud.Render);

                        }
                        else
                        {

                            //Reset the switches:
                            Utilities.reset_the_switches();

                        }

                    }

                    // If "leave_game_disconnect" option is enabled
                    // and player is in game and the town area is not detected:
                    if (
                        Globals.leave_game_disconnect == true
                        && !Hud.Game.IsInTown
                    )
                    {

                        //Cancel leave game window:
                        if (
                            Globals.leave_game_cancelled == false
                            && Utilities.ui_element_visible(Hud.Render, Globals.ui_element_leave_game_window_cancel_button)
                        )
                        {

                            Globals.leave_game_cancelled = true;
                            Utilities.write_debug_log(Hud, "Leave game disconnect triggered.", false); //DEBUG-LOG
                            Utilities.click_ui_element(Hud.Render, "SEND", Globals.ui_element_leave_game_window_cancel_button);

                        }

                        //Start casting town portal:
                        if (
                            Globals.leave_game_cancelled == true
                            && Globals.town_teleport_casted == false
                            && !Utilities.ui_element_visible(Hud.Render, Globals.ui_element_leave_game_window_cancel_button)
                            && !Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.Generic_UseStoneOfRecall.Sno)
                            && !Hud.Game.Me.IsDead
                        )
                        {
                            
                            Utilities.click_ui_element(Hud.Render, "SEND", Globals.ui_element_town_portal_start_button);

                        }

                        //Detect town portal cast:
                        if (
                            Globals.leave_game_cancelled == true
                            && Globals.town_teleport_casted == false
                            && (Hud.Game.Me.IsDead
                            || (Hud.Game.Me.AnimationState == AcdAnimationState.CastingPortal
                            || Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.Generic_UseStoneOfRecall.Sno))
                            || (Utilities.ui_element_visible(Hud.Render, Globals.ui_element_battlenet_error_notification_text)
                            && Globals.ability_forbidden_error_text.Contains(Hud.Render.GetUiElement(
                            Globals.ui_element_battlenet_error_notification_text).ReadText(System.Text.Encoding.UTF8, removeColors: true))))
                        )
                        {

                            Globals.town_teleport_casted = true;

                            if (
                                Globals.leave_game_disconnect_create_new_game == true
                            )
                            {

                                Globals.create_new_game = true;

                            }

                        }

                        //Disconnect from the game:
                        if (
                            Globals.leave_game_cancelled == true
                            && Globals.town_teleport_casted == true
                        )
                        {

                            Utilities.force_disconnect(Hud.Render);

                        }

                    }

                    // If "nephalem_rift_disconnect" option is enabled
                    // and the corresponding special area is detected:
                    if (
                        Globals.nephalem_rift_disconnect == true
                        && Hud.Game.SpecialArea == SpecialArea.Rift
                    )
                    {

                        //Remember old key count:
                        if (
                            Globals.key_count_initialized == false
                        )
                        {

                            Globals.start_key_count = Hud.Game.Me.Materials.GreaterRiftKeystone;
                            Globals.key_count_initialized = true;

                        }

                        //Detect key count increase:
                        if (
                            Globals.key_count_initialized == true
                            && Globals.key_count_increased == false
                            && Hud.Game.Me.Materials.GreaterRiftKeystone > Globals.start_key_count
                        )
                        {

                            Utilities.write_debug_log(Hud, "Nephalem rift disconnect triggered.", false); //DEBUG-LOG
                            Globals.key_count_increased = true;

                        }

                        //Detect town portal cast:
                        if (
                            Globals.key_count_increased == true
                            && Globals.town_teleport_casted == false
                            && (Hud.Game.Me.AnimationState == AcdAnimationState.CastingPortal
                            || Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.Generic_UseStoneOfRecall.Sno)
                            || Hud.Game.Me.IsDead)
                        )
                        {

                            Globals.town_teleport_casted = true;

                            if (
                                Globals.nephalem_rift_disconnect_create_new_game == true
                            )
                            {

                                Globals.create_new_game = true;

                            }

                        }

                        //Disconnect from the game:
                        if (
                            Globals.key_count_increased == true
                        )
                        {
                            Globals.create_new_game = true;
                            Utilities.force_disconnect(Hud.Render);

                        }

                    }

                    // If "rift_guardian_kill_timeout_disconnect" option is enabled
                    // and the corresponding special area is detected:
                    if (
                        Globals.rift_guardian_kill_timeout_disconnect == true
                        && Hud.Game.SpecialArea == SpecialArea.Rift
                    )
                    {

                        //Detect kill guardian quest step:
                        if (
                            Hud.Game.Quests.FirstOrDefault(q => q.SnoQuest.Sno == 337492).QuestStepId == 3
                        )
                        {

                            //Scan for boss enemy in range:
                            var boss_marker = Hud.Game.Markers.FirstOrDefault(
                                x =>
                                    x.SnoActor.Code.Contains("_Boss_")
                                    && x.FloorCoordinate.XYDistanceTo(Hud.Game.Me.FloorCoordinate)
                                    <= Globals.rift_guardian_kill_timeout_reset_yards
                            );

                            //Check whether a boss enemy is detected:
                            bool boss_enemy_is_detected = false;
                            if (
                                boss_marker != null
                            )
                            {

                                boss_enemy_is_detected = true;

                            }

                            //Set or reset starting time:
                            if (
                                Globals.rift_guardian_kill_quest_started == false
                                || boss_enemy_is_detected == true
                            )
                            {

                                //Log quest step starting time:
                                Globals.rift_guardian_kill_quest_started = true;
                                Globals.rift_guardian_kill_quest_started_time = Hud.Game.CurrentRealTimeMilliseconds;

                            }

                        }
                        else
                        {

                            //Reset when quest step changes:
                            if (
                                Globals.rift_guardian_kill_quest_started == true
                            )
                            {

                                Globals.rift_guardian_kill_quest_started = false;
                                Globals.rift_guardian_kill_quest_started_time = 0;

                            }

                        }

                        //Check if duration exceeded time limit:
                        if (
                            Globals.rift_guardian_kill_quest_started == true
                            && Globals.rift_guardian_kill_timeout_exceeded == false
                            && (Hud.Game.CurrentRealTimeMilliseconds >
                            (Globals.rift_guardian_kill_quest_started_time +
                            (Globals.rift_guardian_kill_timeout_seconds * 1000)))
                        )
                        {
                        
                            Utilities.write_debug_log(Hud, "Nephalem rift guardian timeout disconnect triggered.", false); //DEBUG-LOG
                            Globals.rift_guardian_kill_timeout_exceeded = true;

                        }

                        //Disconnect from game:
                        if (
                            Globals.rift_guardian_kill_quest_started == true
                            && Globals.rift_guardian_kill_timeout_exceeded == true
                        )
                        {

                            //Start casting town portal:
                            if (
                                Globals.town_teleport_casted == false
                                && !Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.Generic_UseStoneOfRecall.Sno)
                                && !Hud.Game.Me.IsDead
                            )
                            {

                                Utilities.click_ui_element(Hud.Render, "SEND", Globals.ui_element_town_portal_start_button);

                            }

                            //Detect town portal cast:
                            if (
                                Globals.town_teleport_casted == false
                                && (Hud.Game.Me.AnimationState == AcdAnimationState.CastingPortal
                                || Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.Generic_UseStoneOfRecall.Sno)
                                || Hud.Game.Me.IsDead)
                            )
                            {

                                Globals.town_teleport_casted = true;

                                if (
                                    Globals.rift_guardian_kill_timeout_disconnect_create_new_game == true
                                )
                                {

                                    Globals.create_new_game = true;

                                }

                            }

                            //Disconnect from the game:
                            if (
                                Globals.town_teleport_casted == true
                            )
                            {

                                Utilities.force_disconnect(Hud.Render);

                            }

                        }

                    }
                     
                    // If "cow_level_disconnect" option is enabled
                    // and "Not The Cow Level" area is detected:
                    if (
                        Globals.cow_level_disconnect == true
                        && Hud.Game.Me.Scene.SnoArea.Sno == 434650 //"Not the Cow Level"
                    )
                    {

                        //Reset used pools list:
                        if (
                            Globals.pool_markers_initialized == false
                        )
                        {

                            Globals.xp_pool_markers.Clear();
                            Globals.pool_markers_initialized = true;

                        }

                        //Track used xp pools:
                        if (
                            Globals.pool_markers_initialized == true
                            && Globals.both_pools_collected == false
                        )
                        {

                            var markers = Hud.Game.Markers.Where(
                                x =>
                                    x.IsPoolOfReflection
                                    && x.IsUsed
                            );

                            foreach (var marker in markers)
                            {

                                if (
                                    !Globals.xp_pool_markers.Contains(marker.Id)
                                )
                                {

                                    Globals.xp_pool_markers.Add(marker.Id);

                                }

                            }

                            if (
                                Globals.xp_pool_markers.Count >= 2
                            )
                            {

                                Utilities.write_debug_log(Hud, "Cow level disconnect triggered.", false); //DEBUG-LOG
                                Globals.both_pools_collected = true;

                            }

                        }

                        //Start casting town portal:
                        if (
                            Globals.both_pools_collected == true
                            && Globals.town_teleport_casted == false
                            && !Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.Generic_UseStoneOfRecall.Sno)
                            && !Hud.Game.Me.IsDead
                        )
                        {

                            Utilities.click_ui_element(Hud.Render, "SEND", Globals.ui_element_town_portal_start_button);

                        }

                        //Detect town portal cast:
                        if (
                            Globals.both_pools_collected == true
                            && Globals.town_teleport_casted == false
                            && (Hud.Game.Me.AnimationState == AcdAnimationState.CastingPortal
                            || Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.Generic_UseStoneOfRecall.Sno)
                            || Hud.Game.Me.IsDead)
                        )
                        {

                            Globals.town_teleport_casted = true;

                            if (
                                Globals.cow_level_disconnect_create_new_game == true
                            )
                            {

                                Globals.create_new_game = true;

                            }

                        }

                        //Disconnect from the game:
                        if (
                            Globals.both_pools_collected == true
                            && Globals.town_teleport_casted == true
                        )
                        {

                            Utilities.force_disconnect(Hud.Render);

                        }

                    }

                    // If "goblin_vault_disconnect" option is enabled
                    // and "The Vault" or "The Ancient Vault" area is detected:
                    if (
                        Globals.goblin_vault_disconnect == true
                        && (Hud.Game.Me.Scene.SnoArea.Sno == 380773 //Goblin Vault
                        || Hud.Game.Me.Scene.SnoArea.Sno == 483058) //Ancient Goblin Vault
                    )
                    {

                        //Detect goblin gate activation:
                        if (
                            Globals.vault_gate_operated == false
                        )
                        {

                            var goblin_vault_gates = Hud.Game.Actors.Where(
                                x =>
                                    x.SnoActor.Code == "p1_TGoblin_Gate"
                                    && x.IsOperated
                            );

                            foreach (var actor in goblin_vault_gates)
                            {

                                Utilities.write_debug_log(Hud, "Goblin vault disconnect triggered.", false); //DEBUG-LOG
                                Globals.vault_gate_operated = true;
                                break;

                            }

                        }

                        //Start casting town portal:
                        if (
                            Globals.vault_gate_operated == true
                            && Globals.town_teleport_casted == false
                            && Hud.Game.Me.AnimationState == AcdAnimationState.Idle
                            && !Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.Generic_UseStoneOfRecall.Sno)
                            && !Hud.Game.Me.IsDead
                        )
                        {

                            Utilities.click_ui_element(Hud.Render, "SEND", Globals.ui_element_town_portal_start_button);

                        }

                        //Detect town portal cast:
                        if (
                            Globals.vault_gate_operated == true
                            && Globals.town_teleport_casted == false
                            && (Hud.Game.Me.AnimationState == AcdAnimationState.CastingPortal
                            || Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.Generic_UseStoneOfRecall.Sno)
                            || Hud.Game.Me.IsDead)
                        )
                        {

                            Globals.town_teleport_casted = true;

                            if (
                                Globals.goblin_vault_disconnect_create_new_game == true
                            )
                            {

                                Globals.create_new_game = true;

                            }

                        }

                        //Disconnect from the game:
                        if (
                            Globals.vault_gate_operated == true
                            && Globals.town_teleport_casted == true
                        )
                        {

                            Utilities.force_disconnect(Hud.Render);

                        }

                    }

                }

            }

        }

    }

}