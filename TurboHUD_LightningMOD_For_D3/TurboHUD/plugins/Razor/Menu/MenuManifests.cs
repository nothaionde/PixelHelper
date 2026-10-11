/*
	This file contains manifest data to describe plugins that don't implement IPluginManifest
*/

namespace Turbo.Plugins.Razor.Menu
{
	using System.Collections.Generic;
	
	using Turbo.Plugins.Default;
	using Turbo.Plugins.Razor.Plugin;

	public class MenuManifests : BasePlugin, ICustomizer, IPluginManifest
	{
		//IPluginManifest
		public string Name { get; set; } //optional display name
        public string Description { get; set; } = "Descriptions for plugins in the Menu Plugin System";
		public string Version { get; set; } = "24-Sep-2021";
		public List<string> Dependencies { get; set; } //= new List<string>() {"Turbo.Plugins.Razor.Menu.MenuPlugin"};
		
		public MenuManifests() { Enabled = true; }

		public override void Load(IController hud) { base.Load(hud); }

		public void Customize()
		{
			Hud.RunOnPlugin<MenuTogglePlugins>(plugin =>
			{
				plugin.Manifests.AddRange(new IPluginManifest[] {
					//General
					/*new PluginManifest()
					{
						Path = "Turbo.Plugins.Razor.Click.ClickEventHandler",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Notifies click handler plugins of mouse clicks and figures out whether or not they should be propagated to the game client",
						Version = "24-Jul-2021",
						//Dependencies = new List<string>() {} //list the full namespace path of plugins that this plugin requires
					},*/
					new PluginManifest()
					{
						Path = "Turbo.Plugins.Razor.Hotkey.HotkeyEventHandler",
						//Name = string.Empty, //leave blank if using class name as the display name
						//Description = "Notifies hotkey handler plugins of key events that occur only when the game client is in the foreground and the chat prompt is not being used",
						Description = "Уведомляет плагины обработчиков горячих клавиш о ключевых событиях, которые происходят только тогда, когда игровой клиент находится на переднем плане и приглашение чата не используется.",
						Version = "19-Apr-2021",
						//Dependencies = new List<string>() {} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.Razor.Label.LabelController",
						//Name = string.Empty, //leave blank if using class name as the display name
						//Description = "Handles all of the click events and hover tooltips for Labels",
						Description = "Обрабатывает все события кликов и всплывающие подсказки для меток",
						Version = "5-Sep-2021",
						//Dependencies = new List<string>() {} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.Razor.Log.TextLogger",
						//Name = string.Empty, //leave blank if using class name as the display name
						//Description = "Manages delayed file logging calls for text writing plugins",
						Description = "Управляет вызовами отложенной регистрации файлов для плагинов для написания текста.",
						Version = "5-Sep-2021",
						//Dependencies = new List<string>() {} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.Razor.Util.LocationInfo",
						//Name = string.Empty, //leave blank if using class name as the display name
						//Description = "Computes additional details and applies corrections to your current game world location data that can be used by other plugins",
						Description = "Вычисляет дополнительные данные и применяет исправления к вашим текущим данным о местоположении в игровом мире, которые могут использоваться другими плагинами.",
						Version = "26-Jun-2021",
						//Dependencies = new List<string>() {} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.Razor.Util.UIOverlapHelper",
						//Name = string.Empty, //leave blank if using class name as the display name
						//Description = "Keeps track of the visibility of user interface elements that should block custom plugin displays from rendering or being interacted with",
						Description = "Отслеживает видимость элементов пользовательского интерфейса, которые должны блокировать отображение пользовательских плагинов или взаимодействие с ними.",
						Version = "15-Jul-2021",
						//Dependencies = new List<string>() {} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.Razor.Util.UIOverlapHelper",
						//Name = string.Empty, //leave blank if using class name as the display name
						//Description = "Keeps track of the visibility of user interface elements that should block custom plugin displays from rendering or being interacted with",
						Description = "Отслеживает видимость элементов пользовательского интерфейса, которые должны блокировать отображение пользовательских плагинов или взаимодействие с ними.",
						Version = "15-Jul-2021",
						//Dependencies = new List<string>() {} //list the full namespace path of plugins that this plugin requires
					},
					
					//supplemental plugins
					new PluginManifest()
					{
						Path = "Turbo.Plugins.Razor.SpiritBarrageHelper",
						//Name = string.Empty, //leave blank if using class name as the display name
						//Description = "Shows countdown bars and ground indicators for spirit barrage phantasms and keep tracks of snapshot/buff status for each phantasm",
						Description = "Показывает полосы обратного отсчета и наземные индикаторы для призрачных призраков и отслеживает статус snapshot/buff для каждого призрака.",
						Version = "15-Jul-2021",
						Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.Razor.RunStats.MenuSpiritBarrage",
						Name = "Phantasm Inspector", //leave blank if using class name as the display name
						//Description = "Shows a table of recent phantasms cast and their buff statuses",
						Description = "Показывает таблицу последних использованных фантомов и статусов их положительных эффектов.",
						Version = "11-Jul-2021",
						Dependencies = new List<string>() {
							"Turbo.Plugins.Razor.Menu.MenuPlugin", 
							"Turbo.Plugins.Razor.SpiritBarrageHelper", 
						} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.Razor.Proc.PartyProcTracker",
						//Name = string.Empty, //leave blank if using class name as the display name
						//Description = "Shows countdown bars for cheat death effects and announces them with audio or TTS cues",
						Description = "Показывает полосы обратного отсчета для эффектов смерти от читов и объявляет их звуковыми сигналами или репликами TTS.",
						Version = "31-Jul-2021",
						Dependencies = new List<string>() {"Turbo.Plugins.Razor.Movable.MovableController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.Razor.Proc.PartyProcScreenshots",
						//Name = string.Empty, //leave blank if using class name as the display name
						//Description = "Takes a screenshot whenever you or a teammate triggers a cheat death effect",
						Description = "Делает снимок экрана всякий раз, когда вы или ваш товарищ по команде запускаете эффект чит-смерти.",
						Version = "26-Jul-2020",
						Dependencies = new List<string>() {"Turbo.Plugins.Razor.Proc.PartyProcTracker"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.Razor.ItemMapMarkers",
						//Name = string.Empty, //leave blank if using class name as the display name
						//Description = "Remembers (and marks on the minimap) items that match customizable rules.",
						Description = "Запоминает (и отмечает на миникарте) элементы, которые соответствуют настраиваемому правилу.",
						Version = "23-Sep-2021",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Movable.MovableController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.Razor.BountyDropTracker",
						//Name = string.Empty, //leave blank if using class name as the display name
						//Description = "Shows countdown bars for cheat death effects and announces them with audio or TTS cues",
						Description = "Показывает полосы обратного отсчета для эффектов смерти от читов и объявляет их звуковыми сигналами или репликами TTS.",
						Version = "25-Jun-2021",
						Dependencies = new List<string>() {
							"Turbo.Plugins.Razor.Movable.MovableController",
							"Turbo.Plugins.Razor.Util.LocationInfo",
						} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.Razor.ImmunityHelper",
						//Name = string.Empty, //leave blank if using class name as the display name
						//Description = "Checks a variety of statuses to show you whether or not your character is currently immune to damage",
						Description = "Проверяет различные статусы, чтобы показать, иммунен ли ваш персонаж в настоящее время к повреждениям.",
						Version = "5-Sep-2021",
						Dependencies = new List<string>() {
							"Turbo.Plugins.Razor.Label.LabelController",
							"Turbo.Plugins.Razor.Movable.MovableController",
						} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.Razor.GreaterRiftHints",
						//Name = string.Empty, //leave blank if using class name as the display name
						//Description = "Shows gem upgrade status for the party under Urshi's and Orek's feet, and shows Greater Rift levels \nunlocked and shard caps for each party member in the obelisk menu, marks monsters that are still alive",
						Description = "Показывает статус улучшения драгоценных камней для группы под ногами Урши и Орека, а также показывает \nразблокированные уровни Великого Портала и кровавых осколков для каждого члена группы в меню обелиска, помечает монстров, которые еще живы.",
						Version = "1-Aug-2021",
						Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					
					//Config files
					new PluginManifest()
					{
						Path = "Turbo.Plugins.User.CustomPluginEnablerOrDisabler",
						//Name = string.Empty, //leave blank if using class name as the display name
						//Description = "Plugin toggle states for custom plugins (auto-generated by Menu Plugin System)",
						Description = "Состояние плагинов для Custom плагинов (автоматически генерируются системой плагинов меню)",
						//Version = "24-Sep-2021",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.User.DefaultPluginEnablerOrDisabler",
						//Name = string.Empty, //leave blank if using class name as the display name
						//Description = "Plugin toggle states for default plugins (auto-generated by Menu Plugin System)",
						Description = "Состояния плагинов для плагинов Default (автоматически генерируются системой плагинов меню)",
						//Version = "24-Sep-2021",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.User.LightningModPluginEnablerOrDisabler",
						//Name = string.Empty, //leave blank if using class name as the display name
						//Description = "Plugin toggle states for LightningMod plugins (auto-generated by Menu Plugin System)",
						Description = "Состояния плагинов для плагинов LightningMod (автоматически генерируются системой плагинов меню)",
						//Version = "24-Sep-2021",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.User.MenuPluginConfig",
						//Name = string.Empty, //leave blank if using class name as the display name
						//Description = "Menu addon toggle states and configuration settings (auto-generated by Menu Plugin System)",
						Description = "Запоменает какое меню включено а какое нет (автоматически сгенерированные системой плагинов меню)",
						//Version = "24-Sep-2021",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.User.MovablePluginConfig",
						//Name = string.Empty, //leave blank if using class name as the display name
						//Description = "Movable plugin positions and enabled/disabled state settings (auto-generated by Movable Plugin System)",
						Description = "Позиции подвижных плагинов и настройки включенного/отключенного (автоматически генерируются Movable Plugin System)",
						//Version = "24-Sep-2021",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					
					//RunStats
					new PluginManifest()
					{
						Path = "Turbo.Plugins.Razor.RunStats.MenuAttackSpeed",
						//Name = string.Empty, //leave blank if using class name as the display name
						//Description = "Shows your mainhand, offhand, pet and bonus attack speeds, as well as pain enhancer stack count",
						Description = "Показывает вашу основную и вторую руки, скорость атаки питомца дополнительную скорость атаки, а также количество стеков усилителей боли.",
						Version = "7-Jul-2021",
						Dependencies = new List<string>() {"Turbo.Plugins.Razor.Menu.MenuPlugin"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.Razor.RunStats.MenuBounties",
						//Name = string.Empty, //leave blank if using class name as the display name
						//Description = "Tracks the progress of bounty quests and rewards in the current game",
						Description = "Отслеживает ход выполнения квестов и наград в текущей игре.",
						Version = "12-Sep-2021",
						Dependencies = new List<string>() {
							"Turbo.Plugins.Razor.Menu.MenuPlugin",
							"Turbo.Plugins.Razor.BountyDropTracker",
						} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.Razor.RunStats.MenuCrowdControl",
						//Name = string.Empty, //leave blank if using class name as the display name
						//Description = "Shows icons and running duration of crowd control effects applied on you, elite monsters and bosses",
						Description = "Показывает значки и продолжительность действия эффектов контроля, примененных к вам, элитным монстрам и боссам.",
						Version = "7-Jul-2021",
						Dependencies = new List<string>() {"Turbo.Plugins.Razor.Menu.MenuPlugin"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.Razor.RunStats.MenuDamageReduction",
						//Name = string.Empty, //leave blank if using class name as the display name
						//Description = "Graphs your current damage reduction value history",
						Description = "Отображает вашу текущую историю значений снижения урона",
						Version = "7-Jul-2021",
						Dependencies = new List<string>() {
							"Turbo.Plugins.Razor.ImmunityHelper",
							"Turbo.Plugins.Razor.Proc.PartyProcTracker",
							"Turbo.Plugins.Razor.Menu.MenuPlugin",
						} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.Razor.RunStats.MenuHealth",
						//Name = string.Empty, //leave blank if using class name as the display name
						//Description = "Graphs your health and shield value history",
						Description = "Графики вашего здоровья и истории щита",
						Version = "1-Jul-2021",
						Dependencies = new List<string>() {
							"Turbo.Plugins.Razor.ImmunityHelper",
							"Turbo.Plugins.Razor.Proc.PartyProcTracker",
							"Turbo.Plugins.Razor.Menu.MenuPlugin",
						} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.Razor.RunStats.MenuDamageDone",
						//Name = string.Empty, //leave blank if using class name as the display name
						//Description = "Records and graphs the values of various damage statistics",
						Description = "Записывает и отображает в виде графиков значения различных статистических данных о повреждениях.",
						Version = "10-Jul-2021",
						Dependencies = new List<string>() {"Turbo.Plugins.Razor.Menu.MenuPlugin"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.Razor.RunStats.MenuPools",
						//Name = string.Empty, //leave blank if using class name as the display name
						//Description = "Shows pool count of your characters, your party, and history of pools found in the current game",
						Description = "Показывает количество пулов ваших персонажей, вашей группы и историю пулов, найденных в текущей игре.",
						Version = "25-Sep-2021",
						Dependencies = new List<string>() {
							"Turbo.Plugins.Razor.Menu.MenuPlugin",
							"Turbo.Plugins.Razor.Util.LocationInfo",
						} //list the full namespace path of plugins that this plugin requires
						
					},
					// Добаляем описание Дополнительных плагинов кастом
					new PluginManifest()
					{
						Path = "Turbo.Plugins.Dark.ShrineAlertPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Оповещение пилонов и чаш опыта. Кастомизация PluginEnablerOrDisablerPlugin.",
						Version = "17-Nov-2017",
						Dependencies = new List<string>() {
							"Turbo.Plugins.Dark.ShrineAlertConfig",
						}
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.DAV.DAV_BossCDPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Показывает откат умений боссов в ВП.",
						Version = "23-Aug-2019",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.DavUtil.DAV_MapPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Показывает название локации в ВП на минекарте.",
						Version = "23-Aug-2019",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.DAV.DAV_ShrineTaker",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Показывает кто взял пилон.",
						Version = "24-Dec-2020",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},					
					new PluginManifest()
					{
						Path = "Turbo.Plugins.DAV.DAV_UrshiPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Показывает кто не улучшил камни в ВП (У Razora есть похожий).",
						Version = "14-Jan-2020",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},					
					new PluginManifest()
					{
						Path = "Turbo.Plugins.DAV.DAV_WallPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Показывает стены возводимые элитой.",
						Version = "13-Nov-2020",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.DAVMonster.DAV_EliteInfoPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Вверху с лево от мини карты показывает афиксы и здоровье элиты.",
						Version = "01-Sep-2019",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.glq.GLQ_GRProgressionToTime",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "В ВП на шкале прогресса добавляет время.",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},					
					new PluginManifest()
					{
						Path = "Turbo.Plugins.glq.MonsterRiftProgressionPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "На всех монстрах показывает колличество прогресса. Кастомизация PluginEnablerOrDisablerPlugin.",
						Version = "13-Feb-2019",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.glq.GLQ_BeenToRecordPlugin1",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Показывает на карте кто был в какой локации (Помогает при поиске чаш опыта, видно в поручениях кто не пошел выполнять).",
						Version = "N/A", 
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.Jack.RiftInfoPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Показывает время и прогресс рифтов. Кастомизация PluginEnablerOrDisablerPlugin.",
						Version = "14-Feb-2019",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},	
					new PluginManifest()
					{
						Path = "Turbo.Plugins.Jack.Actors.DoorsPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Показывает на минекарте двери. Кастомизация PluginEnablerOrDisablerPlugin.",
						Version = "14-Feb-2019",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.Jack.Items.WeaponDamageRerollCalculatorPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "На оружие показывает максимальные значения для рола. Кастомизация PluginEnablerOrDisablerPlugin.",
						Version = "3-May-2021",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.Jack.Monsters.DangerousAffixMonsterPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Показывает элиты с опасным афиксом. Включен только джагернаут (натиск) для включения остальных редактируйте файл DangerousAffixMonsterConfig",
						Version = "14-Feb-2019",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.Jack.DifficultiesInfoPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Отображает данные обзора сложности Diablo 3, когда вы наводите курсор мыши на сложность (только в городе).",
						Version = "14-Feb-2019",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.johnbl.MinimapCursorPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Показывает на минекарте курсор мышки вне города.",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.User.MobsOnMapColorsPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Показывает на минекарте обычных монстров (треш) разными цветами.",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.Prrovoss.PartyInspector",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Показывает во что одеты игроки что у нех в кубе и т.п. в игре нажмите F7 чтобы увидеть его работу",
						Version = "17-Nov-2017",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.Resu.BattleTagAboveBanner",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Показывает на флаге Имя с Иконкой персонажа когда игрок не в городе.",
						Version = "15-Feb-2019",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.Resu.DangerPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Показывает опасности на карте горнила, взрыв монстров и т.п. Кастомизация PluginEnablerOrDisablerPlugin.",
						Version = "15-Feb-2019",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.Resu.HotEnablerDisablerPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Можно включить или отключить любой плагин для определенного режима игры софткор, хардкор, сезон. (подробнее в plugins.User.PluginEnablerOrDisablerPlugin.cs)",
						Version = "15-Feb-2019",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},					
					new PluginManifest()
					{
						Path = "Turbo.Plugins.Resu.OtherPlayersHeadsPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Показывает на минекарте ник и иконку персонажа.",
						Version = "15-Feb-2019",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.Resu.PotionPerfectionPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Показывает процент совершенства зелий в тайнике и инвентаре. Не отображается, когда зелье идеально.",
						Version = "15-Feb-2019",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},					
					new PluginManifest()
					{
						Path = "Turbo.Plugins.Resu.PrimalAncientProbabilityPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Указывает на вероятность выпадения древних и первозданных, берет статистику из папки stat_tracker, но там неучитываются предметы от Кадалы и из Куба. (Сомнительный плагин не тестил но пусть будет)",
						Version = "15-Feb-2019",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.Resu.TimeEverywherePlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Показывает часы при открытой вкладке инвентаря.",
						Version = "15-Feb-2019",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.Resu.UrshisGiftPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Показывает процент шанса улучшить легендарные самоцветы на камнях пишет какой портал открыть для 100% шанса если камень на вас пишет рядом с камнем.",
						Version = "15-Feb-2019",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.RNN.AreaPoolsDeaths",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Показывает сколько вы взяли чаш опыта. И смерти игрока. (Отключен есть в сборке Razor если ненравится Razor его можно отключить или использовать оба).",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.RNN.DeathInMap",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Показывает на пилонах и чаша отражений сколько игроков находится рядом (Все тут).",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},					
					new PluginManifest()
					{
						Path = "Turbo.Plugins.RNN.GRPylonMarker",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "На шкале опыта маркер появления пилонов в виде синеи полосы. Ниже показана иконка пилона название и процент на котором он появился.",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.RNN.Oculus_Inside",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "ПЕРСТЕНЬ-ОКО в круге показывает таймер и если в него войти немного изменяет цвет.",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},					
					new PluginManifest()
					{
						Path = "Turbo.Plugins.RNN.OtherClassMarkers",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Этот плагин показывает круги (внешние и внутренние), метки (имена) и аватары других игроков (отключен. Можно включить в командной игре для удобства).",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},					
					new PluginManifest()
					{
						Path = "Turbo.Plugins.RNN.OtherShrinePlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Показывает приблизительные места появления столпов на мини карте. Так же чаш опыта и колодцев исцеления.",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},					
					new PluginManifest()
					{
						Path = "Turbo.Plugins.RNN.TimeLeftSkillBar",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Показывает оставшееся время (длительность) баффа, связанного с каждым из умений. На иконках кнопок.",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},					
					new PluginManifest()
					{
						Path = "Turbo.Plugins.RNN.TimersRiftBar",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Дополнительное окно закрытия ВП с возможностью перемещения по F12.",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},					
					new PluginManifest()
					{
						Path = "Turbo.Plugins.Razor.SkillCooldowns",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Показывает откат умений в числовом варианте под кнопками. Смена отображения на штатное TH F10.",
						Version = "3-May-2021",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					// Добаляем описание LightningMod плагинов
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.AutoSkillPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Автопрожатие кнопок по откату (работает вне города)",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},		
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.DrinkHealthPotionPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Автолечение",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.ConvertMaterialPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "МОД-Преобразовать материал",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.EnchantPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "МОД-Авто Зачарование",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.KadalaPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "МОД - Быстрая Покупка",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.PickUpPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "МОД - Автоподбор",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.SalvagePlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "МОД - Распыл",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.ThrowItemPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "МОД - Сброс на Землю",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.UpgradeRareToLengendaryPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "МОД - Улучшить Редкий Предмет",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.UpgradeToAncientPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "МОД - Перековывать в древний, первозданный",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.CloseDialogPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Закрытие всех диалогов",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.CloseQuestDialogPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Закрытие всех диалогов в приключениях",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.OpenChestPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Авто клик предметов сундуки, пилоны, и т.п.",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.OpenGreatRiftPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "МОД-Поиск ВП",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},					
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.UpgradeLegendaryGemPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "МОД-Улучшение Легендарного Самоцвета",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},

					
					// Стандартные плагины TH
					new PluginManifest()
					{
						Path = "Turbo.Plugins.Default.CheatDeathBuffFeederPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Миникарта краснеет после срабатывания пасивки уклонение от смерти.",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.Default.TopMonsterHealthBarPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Над строкой здоровья босса в ВП показывает какой эффект к нему применяется (ослепление и т.п.).",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},					
					new PluginManifest()
					{
						Path = "Turbo.Plugins.Default.OriginalSkillBarPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Показывает откат умений в числовом варианте на кнопках. Пока активен Razor.SkillCooldowns управление сменой отображение F10.",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					
					// Автобой ДХ					
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.DemonHunterBolasPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Плаг под кидание боласа.",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.DemonHunterChakramShurikenCloudPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Шакрам - Облако Cюрикенов : Эффективно",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},					
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.DemonHunterCompanionPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Питомец: Эффективно когда КДР больше 69% или есть кольцо Зодиака,для рун Волк, Вепрь ,\nПаук только каст если Элита и Босс рядом, каст если руна Хорьки и здоровья меньше 60% и сфера \nздоровья рядом, Каст если руна Летучая Мышь и Ненависть меньше 15%, если в группе есть COE, \nто выполняется Волк во время эффекта COE (Дает вам преимущество).",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.DemonHunterEvasiveFirePlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Сила тьмы: Каст в опасности, Активируется если есть 4п сета Мантия Тени, \nЭффективно в сете Шестерни Мертвых Земель 6п или Сущность порока 6п",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.DemonHunterFanofKnivesPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Веер клинков - Клинковая броня: Эффективно когда КДР более 40%, иначе с другой стороны Каст если монстры в 40 ярдах.",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},					
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.DemonHunterImpalePlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Бросок кинжала: Есть эффект от Цепи Теней и Кульбит.",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.DemonHunterPreparationPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "На изготовку - другая руна: Каст если Концентрация израсходована, Каст если  руна Боевые Шрамы и Здоровье менее 40%.",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.DemonHunterPreparationPunishmentPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "На изготовку - Наказание : Каст при недостатке Ненависти",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.DemonHunterRainofVengeancePlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Ливень возмездия: Есть эффект от 6п Месть Наталии.",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.DemonHunterShadowPowerPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Сила тьмы: Каст в опасности, Активируется если есть 4п сета Мантия Тени, Эффективно в \nсете Шестерни Мертвых Земель 6п или Сущность порока 6п",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.DemonHunterSmokeScreenPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Дымовая Завеса: Каст в опасности, Активируется если есть Кольцо Уклонения, Каст в \nсете Шестерни Мертвых Земель 6п и монстры вокруг, Каст с руной Сдвиг и РКР больше 40%.",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.DemonHunterStrafeHungeringArrowPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "ШМЗ макрос: Есть эффект от 2п Шестерен Мертвых Земель и умение Обстрел, увеличена частота \nОбстрела и основных умений.позволяет поддерживать только Обстрел, \nв случае когда нет полного сета GoD",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.DemonHunterVengeancePlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Возмездие: Каст если руна Мрачная Душа,руна Поясные Пушки,наполнение здоровья менее 60%, \nКаст если Утренняя Заря или Кольцо Зодиака и Элита рядом, Каст когда КДР \nвыше 29% и монстры вокруг, Эффективно если КДР больше 36%,Каст если руна Бешенство и Ненависть менее 30%.",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},

					// Автобой Вар					
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.BarbarianAncientSpearPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Копье Древних - Бросок Валуна: Активировать когда есть сет Рекор (6 шт.), \nАрреатский закон и используется бросок оружия для поддержания макс.  ярости.",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.BarbarianBandofMightPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Кольцо Силы: Каст Яростный рывок, Удар в прыжке, Топот,(Плагин не поддерживается на Саппе)",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.BarbarianBattleRagePlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Боевая Ярость: Эффективно",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.BarbarianCallOfTheAncientsPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Зов Древних: Поддержка баффа в сете Призыв Бессмертного Короля 2п,Иначе если Элита Босс рядом и здоровье менее 50%.",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.BarbarianIgnorePainPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Стойкость к боли: Каст в опасной зоне, Каст с руной Законы Толпы и члены \nГруппы в радиусе 50 ярдов,Вынужденный каст если у члена Группы есть дебафф от Каменных Рукавиц,\nЭффективно с другими рунами и есть Гордость Кассия  или Зодиак или Мессершмидт или Гнев Пустошей из 6п",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.BarbarianOverPowerPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Сверхсила: Эффективно с руной Череда Убийств или Сокрушительное Преимущество,Каст с руной Импульс и Ярость менее 30 и \nвокруг более 5 монстров,С другими рунами Каст если монстры вокруг.",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.BarbarianRendPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Рваные Раны: Каст каждые 3-4 сек если более 5 монстров в радиусе умения",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.BarbarianSeismicSlamPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Сейсмический удар - Грохотание: в сете Призыв Бессмертного Короля 6п + Рекор 4п и Гнев Берсерка в процессе восстановления \nи Ярость более 98% для эффекта Зодиака, Эффективно в сете Оплот Воли.",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.BarbarianSprintPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Спринт: Каст с руной Форсированный Марш и члены группы в пределах 50 ярдов,\nЭффективно с другими рунами во время бега.",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.BarbarianThreateningShoutPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Угрожающий Крик: Каст если Элита Босс или 10 монстров в радиусе 25 ярдов, \nКаст с руной Мрачный Урожай и в 15 ярдах от группы и монстров(10 монстров или Элита Босс)",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.BarbarianWarCryPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Воинственный клич: Эффективно, Каст если надета Цепь Чиланика или члены \nГруппы в пределах 100 ярдов, Увеличивает эффект группы.",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.BarbarianWhirlwindPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Чудо-Страна:Авто Вихрь или пилит самого сильного монстра в зоне курсора(Исключая Прислужников)",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.BarbarianWrathOfTheBerserkerPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Гнев Берсерка: Эффективно в сете Призыв Бессмертного Короля 4п или с пассивкой \nМилость Бул-Катоса и КДР выше 75% или с кольцом Зодиака или с Мессершмидтом,\nИначе только Каст если Элита Босс рядом или Здоровье 30%.",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},

					// Автобой Монах						
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.MonkBlindingFlashPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Ослепляющая Вспышка: Каст в атаке и 5 монстров или Элита в радиусе 20 ярдов, \nНе используется только рядом с Боссом.",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.MonkBreathOfHeavenPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Дыхание Небес: Каст если в радиусе 12 ярдов у члена группы здоровья менее 70%, \nКаст с руной Пламенный Гнев и Элита Босс в радиусе 40 ярдов, Каст с руной Ветерок и \nКДР более 65% и с Зодиаком или КДР выше 74%, Каст с руной Наполненность Светом и \nКДР больше 65% и с Зодиаком или КДР выше 74% и Дух менее 50%.",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.MonkCycloneStrikePlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Ураганный Удар: Каст с руной Успокаивающее Дыхание и монстры в радиусе 50 ярдов\n(Не действует на Возрожденном Боссе), Эффективно с другими рунами м с наплеч.Монолог Лефевр. \nИспользовать, когда вокруг 5 огненных колоколов (только для сезона 27).",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.MonkEpiphanyPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Прозрение: Эффективно в простое и монстры вокруг.",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.MonkInnerSanctuaryPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Святая Святых:Каст в атаке и Босс или 5 монстров в радиусе 50 ярдов.",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.MonkMantraOfConvictionPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Мантра Осуждения: Эффективно если активно Прозрение и в движении, \nЭффективно если Элита Босс рядом и достаточно ресурсов.",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.MonkMantraOfHealingPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Мантра Исцеления:Эффективно если в движении, вокруг монстры и Дух более 90%.\n(Приоритет эффект Кирасы Акилы) Каст если у члена группы спал эффект Щита \nи Дух больше 50%, ДПС задача защиты если Дух менее 30%.",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.MonkMantraOfSalvationPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Мантра Спасения: Эффективно в движении и вокруг монстры.",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.MonkMysticAllyPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Таинственный Союзник: Каст с руной Воздушный Союзник и Дух менее 50, Каст с руной \nВыносливый Союзник и здоровье менее 30%. Активировать когда используется руна \nВодный или Земляной союзник и рядом более 1 монстра, или элита или бос.",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.MonkSerenityPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Покой: Каст если Здоровье менее 60% или порция здоровья на откате и Здоровья менее 80%, \nКаст с руной Спокойствие и КДР выше 70% и в приоритете саппортит Колдуна, Некроманта и Крестоносца.",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.MonkSweepingWindPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Хлещущий Ветер: Эффективно при достаточном количестве ресурсов.",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.MonkTempestRushPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Стремительность Урагана: Авто удержание Стремительность Урагана или блок монстра с самым \nбольшим здоровьем в области курсора (Не касается Прислужников) \n(Запуск когда руна Шквал, стаков более 101 и монстры в радиусе 15 ярдов)",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.CloseToDPSPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Монах. Авто перемещение в выгодную позицию вокруг Босса и сражается \n(каст касание смерти затем волна увечий)",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},					

					// Автобой Некромант
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.NecArmyoftheDeadPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Армия Мертвецов: Каст если рядом с курсором элита или босс или много жирных мобов",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.NecBloodRushPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Кровавый Рывок: Каст в Похоронном Убранстве, Пожирание-Аура Пожирания, Костяное копьё, \nи 15 ярдов от курсора и Эссенция меньше  80% только на БОССЕ",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.NecBoneArmorPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Костяная броня: Кроме руны Огран.Неуязвимость, Каст если мобы в пределах 30 ярдов, с \nруной Да Окрепнет Плоть твоя и не колдует если ХП < 50%, не колдует в сете \nМилость Инария. С Косой Цикла колдует.С руной Смятение и с кольцами Крисбина и COE, \nколдует в момент старта COE.",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.NecCommandSkeletonsPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Бонус Повелит.Скелетов (Кост.Колокол, Доспех Поющ.Крови)",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.NecDevourPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Пожирание: Каст если вокруг труппы или активно Царство Мертвых и вокруг монстры или \nЗдоровья и Эссенции недостаточно.",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.NecLandOfTheDeadPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "После  %прогр.ВП(Некр) 'Царство мертвых' ост.времени (Более точного описания нет)",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.NecNayrsBlackDeath_BoneArmorPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Костяная броня (Более точного описания нет)",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.NecNayrsBlackDeath_BoneSpearPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Костяное копье (Более точного описания нет)",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.NecNayrsBlackDeath_CommandSkeletonsPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Повелитель скелетов: Эффективно на БОССЕ, в Доспехе Поющей Крови, во время \nЦарство мертвых (искл.Чумные земли),в сете Йессет и выбранным мобом, \nс руной Коллекционер Костей  и Зодиака постоянный Клик.",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.NecNayrsBlackDeath",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Черная Смерть Найра: Поддержка бонуса отравления Костяное копье, Костяная броня, \nСкелет-маг, Вампиризм, Кольцо смерти, Повелитель скелетов.",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.NecNayrsBlackDeath_RevivePlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Когда чёрная коса БАФФ не пропала, кастуйте её при мобах и боссах или оверлордах, \nиначе тип монстра не будет оцениваться (Более точного описания нет)",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.NecNayrsBlackDeath_SiphonBloodPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Экипируйте черную косу (Более точного описания нет)",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.NecNayrsBlackDeath_SkeletalMagePlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Скелет-маг (Более точного описания нет)",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.NecRevivePlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Оживление: Каст в сете Ратмы 2п, оживших миньонов меньше 10 и труп рядом с курсором",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.NecSimulacrumPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Симулякр: Каст если: 1.В амуле Призрачные Видения. 2.Активно умение Царство Мертвых 3.\nВремя отката меньше чем время Баффа + 6сек.",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.NecSkeletalMagePlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Скелет-маг: С руной Сингулярность или в Воле Разета, вначале обычные скелеты затем \nподнимает мощного скелета им на замену, поднимает мощного скелета когда курсор на элите.С руной \nСкелет-Лучник и без Воли Разета и Кирасе Акилы активна эффект \nскорости атаки в режиме БОССА.",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},

					// Автобой Крестоносец
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.CrusaderAkaratsChampionPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Поборник Акарата: Каст если ресурсов меньше 20% или Элита Босс в 40 ярдах, Эффективно \nесли КДР больше 75% или (50% и сет Доспехи Аккана 4п или есть Зодиаком или Поборник Акарата востанавливается). \nКаст в Эгиде Доблести с руной Разделенная Ярость и эффект COE от Сил Света.",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.CrusaderBombardmentPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Бомбардировка:Каст с руной Бочки с Шипами,сэт Акана 6п, Призыватель 2п, COE，мобы в \nрадиусе 30 ярдов,физурон COE старт меннее 1сек. продолжать использовать, когда выбран Поборник Акарата - Сосредоточение \nи используется сет Аккана (6 предметов)",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.CrusaderCondemnPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Порицание: Каст когда достаточно ресурсов и монстры вокруг.",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.CrusaderFistoftheHeavensPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Кулак Небес: Каст в Эгида Доблести с эффектом от 2п и 4п.",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.CrusaderIronSkinPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Железная Кожа: Каст в опасности, Эффективно если КДР больше 50% или с кольцом Зодиака. \nКаст с руной Отражающая Кожа  и еольцом COE, монстры в радиусе 30 ярдов и до эффекта Физ Урона COE менее 3 сек. \nпродолжать использовать, когда выбран Поборник Акарата - \nСосредоточение и используется сет Аккана (6 предметов)",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.CrusaderJudgmentPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Суд: Авто каст если Элита в радиусе 20 ярдов от курсора и умение активно продолжает \nиспользовать Суд когда используется сет Аккана в режиме атаки",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.CrusaderLawsOfHopePlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Принцип Надежды: Каст если кто-либо из группы в опасности. \nЭффективно с пассивкой Длинная Рука Закона или КДР больше 50% или с Зодиаком.",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.CrusaderLawsOfJusticePlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Принцип Справедливости: Каст если кто-либо из группы в опасности. \nЭффективно с пассивкой Длинная Рука Закона или КДР больше 50% или с Зодиаком.",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.CrusaderLawsOfValorPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Принцип Доблести: Эффективно с пассивкой Длинная Рука Закона или КДР больше 50% или с Зодиаком.",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.CrusaderPhalanxPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Фаланга: используется при атаке врагов с сетом Аккана.",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.CrusaderProvokePlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Насмешка: Каст в опасности или ресурсы меньше ресурсов монстра, \nКаст если КДР больше 50%, Каст если Бомбардировка активируется руной Отражающая кожа.",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.CrusaderSteedChargePlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Скакун: Эффективно в сете Рвение Норвальда 2п и недобитые мобы в радиусе 15 ярдов.",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},

					// Автобой Колдун
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.WitchDoctorBigBadVoodooPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Большое Страшное Вуду: Каст с Кукри и фетиш рядом или с пас.Неспр.\nСмерть и Элита Босс рядом, или эффективно в сете Облачения Мундунугу 2п с пас.Несправедливая Смерть",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.WitchDoctorCorpseSpidersPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Трупные Пауки - Паучья королева: Каст когда Паучья королева исчезает.",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.WitchDoctorFetishArmyPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Армия Фетишей: Каст в сете Призрак Зунимассы 2п и мало Фетишей.",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.WitchDoctorGargantuanPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Громадень: Каст если Элита Босс в радиусе 40 ярдов или здоровья менее 50%.",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.WitchDoctorHexPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Сглаз: Эффективно в сете Дух Арахира 4п.",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.WitchDoctorHorrifyPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Ужас: Каст когда 5 монстров или Элита Босс в пределах навыкаe,Каст с \nЗодиаком и вокруг монстры,Каст с руной Пугающий Вид и монстры в радиусе 50 ярдов",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.WitchDoctorMassConfusionPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Всеобщее Помешательство: Каст если Элита Босс или 3 монстра в радиусе 18 ярдов.",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.WitchDoctorPiranhasPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Пираньи-Пираньядо : Каст если Элита Босс или 10 монстр в радиусе 15 ярдов вокруг курсора",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.WitchDoctorSoulHarvestJadeHarvesterPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Жатва Душ - Одеяние Нефритового Жнеца: Каст в сете Нефритового \nЖнеца 6п и монстры в радиусе 18 ярдов поражёны Мстит.Духом, Каст с Кольцом \nПустоты и рядом монстр поражённый Мстит.Духом и эффект Рой Саранчи от сета Нефритового Жнеца.",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.WitchDoctorSoulHarvestPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Жатва Душ: Каст если Элита Босс или 3 монстра в радиусе 18 ярдов.",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.WitchDoctorSpiritBarragePhantasmPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Призрачный Шквал - Фантазм: Каст если количество Фантазма  меньше 3 в \nрадиусе 60 ярдов и Принудительное перемещение не нажато.",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.WitchDoctorSpiritWalkPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Поступь Духа: Эффективно в сете Облачения Мундунугу из 4п.",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.WitchDoctorZombieDogsPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Призыв Псов-Зомби: Каст если Псов-Зомби меньше 3 или с кольцом \nПалец Высокого Человека и Псы-Зомби исчезают.",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},

					// Автобой Чародей
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.WizardArcaneOrbPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Магическая Сфера - Хоровод Сфер: Каст после исчезновения Хоровода Сфер.",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.WizardArchonArcaneBlastColdPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Архонт - (Более точного описания нет): Каст когда вокруг монстры",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.WizardArchonArcaneBlastFirePlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Архонт - (Более точного описания нет): Каст когда вокруг монстры",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.WizardArchonArcaneBlastLightningPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Архонт - (Более точного описания нет): Каст когда вокруг монстры",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.WizardArchonArcaneBlastFirePlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Архонт - (Более точного описания нет): Каст когда вокруг монстры",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.WizardArchonArcaneBlastPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Архонт - Удар Тайной Магии: Каст когда вокруг монстры",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.WizardArchonPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Архонт: Каст в сете Чантодо и 20 стаков.Если возможно сначала будет \nкаст заклинания Черная Дыра.",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.WizardArchonSlowTimePlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Архонт - Замедление времени : Автоактивация",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.WizardBlackHolePlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Черная Дыра: Каст с руной Абс. Ноль или с Кража Магии и с кольцом COE и монстры в \nрадиусе 15 ярдов от курсора и COE менее 1 сек от самого высокого элемента.",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.WizardBlizzardPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Снежная Буря: Каст со сферой Зимний Ветер и монстр не поражён Снежной Бурей и \nхотябы 1 Гидра в зоне курсора.",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.WizardDiamondSkinPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Алмазная Кожа: Эффективно с руной Толстая Кожа и КДР 60%, для других рун \nобязательны кольцо Зодиака или Мессершмидт. Всегда эффективно с Ин-Гиом",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.WizardDisintegratePlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Расщепление:Каст Каст в Убранстве Огненной Птицы 6п и не в движении. \nКоличестао горящих > 1 и меньше: 44. Каст если заканчивается еффект Тхэгыка и Столп или Элита Босс рядом",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.WizardEnergyArmorPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Энергетический Доспех: Эффективно.",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.WizardExplosiveBlastPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Энергетический Взрыв: Каст если монстры вокруг.",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.WizardFamiliarPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Фамилиар: Эффективно.",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.WizardFrostNovaPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Кольцо Льда: Каст ели монстры в радиусе 16 ярдов (не кастуется в мультиплее бой с Боссом) \nКаст с руной Глубокая Заморозка и рядом 5 монстров.",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.WizardHydraPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Гидра: Каст с Змеиным Жезлом и в сете Одеяния Тифона 4п и 5 вспышек Маг.\nГенератора в зоне курсора возле монстра, находящемся в зоне Снежной Бури.",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.WizardIceArmorPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Ледяной Доспех: Эффективно.",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.WizardMagicWeaponPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Магическое Оружие: Эффективно.",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.WizardMirrorImagePlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Зеркальное отражение: Каст в Убранстве Огненной Птицы 6п и Элита Босс или \nбольше 10 монстров в радиусе 15 ярдов, Каст в соотв.момент цикла кольца Стихий",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.WizardStormArmorPlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Доспех Бури: Эффективно.",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.WizardTriumvirate_ElectrocutePlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Триумвират: Каст Базового Заклинания для получения эффекта от сферы Триумвират",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.WizardTriumvirate_MagicMissilePlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Триумвират: (Более точного описания нет)",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.WizardTriumvirate_ShockPulsePlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Триумвират: (Более точного описания нет)",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},
					new PluginManifest()
					{
						Path = "Turbo.Plugins.LightningMod.WizardTriumvirate_SpectralBladePlugin",
						//Name = string.Empty, //leave blank if using class name as the display name
						Description = "Триумвират: (Более точного описания нет)",
						Version = "N/A",
						//Dependencies = new List<string>() {"Turbo.Plugins.Razor.Label.LabelController"} //list the full namespace path of plugins that this plugin requires
					},					

					
					
				});
			});
		}
	}
}