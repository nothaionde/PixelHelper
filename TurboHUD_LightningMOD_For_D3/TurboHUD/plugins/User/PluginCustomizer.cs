using Turbo.Plugins.Default;
using System.Linq;
using System.Windows.Forms;
using Turbo.Plugins.LightningMod;
using Turbo.Plugins.glq;
using Turbo.Plugins.Prrovoss;
using Turbo.Plugins.Razor;
using Turbo.Plugins.Razor.Plugin;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Globalization;
using Turbo.Plugins.Default;

namespace Turbo.Plugins.User
{
    public class PluginCustomizer : BasePlugin, ICustomizer
    {
        public PluginCustomizer()
        {
        }

        public void Customize()
        {
            Hud.RunOnPlugin<glq.MonstersCountPlugin>(plugin =>
            {
                plugin.ShowCircle = false;//Draw a circle on minimap
                plugin.ToggleEnable = false;//Enable/Disable hotkey switching
                plugin.ToggleKeyEvent = Hud.Input.CreateKeyEvent(true, Key.LeftControl, true, false, false);// Переключить базовый диапазон и максимальный диапазон LeftControl
                plugin.yard = 40;   //Basic range of Statistics
                plugin.MaxYard = 120;//The maximum range of Statistics (120 is maximum)
                plugin.ShowMonstersCount = false;
                plugin.ShowTotalProgression = false;
                plugin.ShowTrashProgression = false;
                plugin.ShowEliteProgression = false;
                plugin.ShowRareMinionProgression = false;
                plugin.ShowRiftGlobeProgression = false;
                plugin.ShowTime = false;//Progress to time value
                plugin.ShowHauntedCount = false;
                plugin.ShowLlocustCount = false;
                plugin.ShowPalmedCount = false;
                plugin.ShowPiranhasCount = false;
                plugin.ShowCursedCount = false;
                plugin.ShowKrysbinCount = false;
                plugin.ShowPainEnhancerCount = false;
                plugin.ShowEntanglingShotCount = false;
                plugin.XWidth = 0.84f;  //Влево -  Вправо
                plugin.YHeight = 0.66f; // Вниз - Вверх
            });

            Hud.RunOnPlugin<RNN.EnmityVisionPlugin>(plugin =>
            {
                plugin.ShowWaypoint = true;
                plugin.SoundEnabled = true;         // Alert sound for a new Vision
                plugin.FileSound = "notification_9.wav";    // File to be played. It must be in the Sounds\ folder

                plugin.EnableSpeak = true;  // false to disable all speak
                plugin.SpeakOpenPortalUI = " ";     // null or empty not to speak, vision is detected via UI
                plugin.SpeakOpenPortalArea = "Видения вражды";      // null or empty not to speak, vision is detected via portal. A nearby portal is typically detected before the UI notification
                plugin.SpeakClosingPortal = null;       // null or empty not to speak
                plugin.SpeakClosedPortal = null;        // null or empty not to speak
                plugin.SpeakChestFound = null;          // null or empty not to speak	
                plugin.SpeakGoblinFound = null;         // null or empty not to speak
                plugin.SpeakFirstPortal = "Вход найден";        // Speak when the entry portal is found after the vision is detected via UI
                plugin.SpeakNextPortalOrder = null;     // Speak portals , {0} = Level number
                plugin.SpeakNextPortal = "Следующий уровень";           // Speak portals. Show When the number of levels cannot be set

                plugin.LabelFirstPortal = "Видения вражды";         // Show when the entry Portal has been found
                plugin.LabelNextPortalOrder = "Уровень {0}";        // {0} = Level number
                plugin.LabelNextPortal = "Следующий уровень";           // Show when the number of levels cannot be set

                plugin.EnableTextScreen = true;             // Show TextScreen
                plugin.TextScreen = "{5}. {6}"; // {0}=Act, {1}=Area NameLocalized, {2}=time, {3}=Levels, {4}=TextMoreLeveles, {5}=TextLevelMe/TextNoLevelMe, {6}=TextGoblin/TextChest/TextChestOpen/TextClosing
                plugin.TextLocUnKnown = "\"Локация неизвесна\"";    // Portal opened but not detected.
                plugin.TextNoPortal = " ";      // No active Vision.
                plugin.TextMoreLeveles = " или больше";         // shown if there is a possibility of more levels
                plugin.TextClosing = "Закрытие ... {0}с";       // show if the portal is closing. {0}=time in seconds
                plugin.TextGoblin = "Найден гоблин";
                plugin.TextChest = "Найден сундук";
                plugin.TextChestOpen = "Сундук открыт";
                plugin.TextLevelMe = "Видения вражды: Уровень {0}";             // Shows the level you are at within the vision. {0}=level	// В видениях вражды показывает уровни
                plugin.TextNoLevelMe = " ";     // default: "out" - "Вы вне портала Видения вражды !!!"		// You are not in an area of Vision  // Видения вражды появляются и если ты не в них

                plugin.TextTimeFormat = "{0:0}м{1:0}с"; // Format for Time since vision was detected // {0} = minutes, {1} = seconds

                plugin.OffsetX = 0.055f;    // 0f...1f Set the x coordinate for TextScreen
                plugin.OffsetY = 0.001f;    // 0f...1f Set the y coordinate for TextScreen
            });

            Hud.RunOnPlugin<RNN.OtherShrinePlugin>(plugin =>
            {
                plugin.LabelPylonExchange = false; // Если true то название будет отображаться только на миникарте	// Exchange labels on map and mini map. If it is false they will always be shown      			

                plugin.NotifyInTown = false;        // Озвучка в городе		// Enable or disable TTS/Popup in Town		
                plugin.TTSViewPylon = false;        // Оповещайте голосом (TTS) Пилоны и святилища	// Notify with voice (TTS) the Pylons and Sanctuaries
                plugin.TTSBuffPylon = false;        // Озвучка	о взятие и потере пилонов	// Notify with voice (TTS) When you receive a buff (and lose) from a pylon/Sanctuary
                plugin.TTSViewPoolText = " ";       // default: "Чаша отражений" Озвучка чаш отражений	// Notify with voice (TTS) the pools of Reflection, will read the indicated text. Set to "" (or null) for not TTS

                plugin.HiddenPylonUsed = false;     // Hide or show (in gray) decorators for used pylons
                plugin.LineToPylon = true;      // Линия до пилонов	// Draw a yellow line on the minimap to the PoolOfReflection/Pylon/Sanctuary detected	
                plugin.LineToPylonWidth = 2f;       // Line width
                plugin.LineToPylonRange = 700;      // Проводите линию до пилонов в том случае, если вы находитесь на расстоянии меньше этого расстояния// Only draw the line if you are less than this distance
                plugin.LineToPoolRange = 1;     // Проводите линию до Чаши отражений, если ваше расстояние меньше указанного. Для неограниченного расстояния установите значение 0 // Only draw the line if you are less than this distance. For unlimited distance set to 0

                plugin.ShowPylonSpawn = true;       // Hide or not the decorators of the possible Pylons. In case someone uses another complement for it		
                plugin.ShowPopup = false;       // Всплывающее сообщение о взятие и потери пилонов // Popup when you receive (or lose) a buff from a Pylon/Sanctuary

                plugin.xPopup = 0.5f;       // 0f ... 1f  Popup Coordinate x
                plugin.yPopup = 0.81f;      // 0f ... 1f  Popup Coordinate y
                plugin.FontSizePopup = 9f;  // Size font for popup text

                plugin.Opacity = 0.75f;     // 0f..1f opacity for all textures(of pylons/shrines/pools/wells, other markers) on the map when they're not on screen

                plugin.ShowMarkers = true;  // Show markers other than Pylons/Shrines/Pools/Wells. Assign the value "false" if you want to use another plugin for this
                plugin.LineToMarkerRange = 700; // Only draw the line if you are less than this distance

                plugin.NemesisAlert = true;
            });

            Hud.RunOnPlugin<GLQ_PlayerSkillPlugin1>(plugin =>
            {
                plugin.Sentry = false;
                plugin.SentryTimer = false;
                plugin.SlowTime = false;
                plugin.SlowTimeTimer = false;
                plugin.BlackHole = false;
                plugin.BlackHoleTimer = false;
                plugin.Piranhas = false;
                plugin.PiranhasTimer = false;
                plugin.SpiritWalkTimer = false;
                plugin.SpiritBarragePhantasm = false;
                plugin.SpiritBarragePhantasmTimer = false;
                plugin.BigBadVoodoo = false;
                plugin.BigBadVoodooTimer = false;
                plugin.GraspoftheDead = false;
                plugin.GraspoftheDeadTimer = false;
                plugin.Wizard_Meteor = false;
                plugin.Wizard_MeteorTimer = false;
            });

            Hud.RunOnPlugin<LM.EnchantPlugin1>(plugin =>
            {
                plugin.ToggleKeyEvent = Hud.Input.CreateKeyEvent(true, Key.R, false, false, false); // Кнопка Зачарование у гадалки
                plugin.enchantCount = 1000; // Максимальное время колдовства
                plugin.ReservedGold = 0; // Запас золота
                plugin.ReservedArcaneDust = 0; // Запас чародейской пыли
                plugin.ReservedVeiledCrystal = 0; // Запас Затуманеных кристалов
                plugin.ReservedForgottenSoul = 0; // Запас забытых душ
                plugin.ReservedDeathsBreath = 0;  // Запас Дыханий смерти
                plugin.ReservedGem = 0; // Запас Самоцветов
            });


        }
    }
}
