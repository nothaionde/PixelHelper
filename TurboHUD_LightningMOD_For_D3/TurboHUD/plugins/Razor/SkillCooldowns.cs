/*

This plugin shows the potential cooldown duration for all equipped skills that have cooldowns, after all current cooldown effects are applied (buffs, cdr, passives, legendary powers). Skills that have charges also have their regeneration rate affected and are displayed.

*/

namespace Turbo.Plugins.Razor
{
	using SharpDX.DirectInput; //Key
	using SharpDX.DirectWrite;
	using System;
    using System.Collections.Generic;
    using System.Linq;
	using System.Media;
	using System.Threading;

    using Turbo.Plugins.Default;
    using Turbo.Plugins.Razor.Hotkey;
	
    public class SkillCooldowns : BasePlugin, IInGameTopPainter, ICustomizer, IAfterCollectHandler, INewAreaHandler, IHotkeyEventHandler
    {
		public bool ShowCooldownLabels { get; set; } = true; //start hud with cooldown labels shown instead of damage labels
		public IKeyEvent ToggleHotkey { get; set; }
		
		public Dictionary<uint, Func<IPlayer, double, double>> CooldownModifiers; //code to handle individual skill cdr passives and item effects
		public Dictionary<uint, double> ChargeCooldowns; //cooldowns for skill charges that regenerate over time
		
		public IFont LocationFont { get; set; }
		public IBrush HintBorderBrush { get; set; }
		public IBrush HintBgBrush { get; set; }
		
		private Dictionary<uint, float> Data = new Dictionary<uint, float>();
		private int TickDataCollected = 0;
		//private WorldDecoratorCollection DebugDecorator;
		private bool? ToggleDamageLabels;
		private bool LastState; //for tracking state changes

        public SkillCooldowns()
        {
            Enabled = true;
            Order = 60004;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
			
			ToggleHotkey = Hud.Input.CreateKeyEvent(true, Key.F10, false, false, false);
			
			LocationFont = Hud.Render.CreateFont("tahoma", 7, 225, 255, 255, 255, false, false, 255, 0, 0, 0, true);
			HintBorderBrush = Hud.Render.CreateBrush(255, 195, 125, 0, 2);
			HintBgBrush = Hud.Render.CreateBrush(255, 0, 0, 0, 0); //175
			
			ChargeCooldowns = new Dictionary<uint, double>() {
				{ Hud.Sno.SnoPowers.Monk_DashingStrike.Sno, 8 },
				{ Hud.Sno.SnoPowers.Necromancer_BloodRush.Sno, 5 },
				{ Hud.Sno.SnoPowers.Barbarian_FuriousCharge.Sno, 10 },
				{ Hud.Sno.SnoPowers.Necromancer_BoneSpirit.Sno, 15 },
				{ Hud.Sno.SnoPowers.DemonHunter_Sentry.Sno, 8 },				
			};
			
			//special cases
			CooldownModifiers = new Dictionary<uint, Func<IPlayer, double, double>>() {
				//Crusader
				{
					Hud.Sno.SnoPowers.Crusader_AkaratsChampion.Sno, 
					delegate (IPlayer player, double baseCooldown) {
						//reduce base cooldown by 50% if 4pc Akkhan's set bonus is active
						return (player.Powers.BuffIsActive(359585) ? baseCooldown * 0.5d : baseCooldown);
					}
				},
				{
					Hud.Sno.SnoPowers.Crusader_ShieldGlare.Sno, 
					delegate (IPlayer player, double baseCooldown) {
						//reduce base cooldown by 30% if Towering Shield passive is used
						return (player.Powers.BuffIsActive(356052) ? baseCooldown * 0.7d : baseCooldown);
					}
				},
				{
					Hud.Sno.SnoPowers.Crusader_SteedCharge.Sno, 
					delegate (IPlayer player, double baseCooldown) {
						//reduce base cooldown by 25% if Lord Commander passive is used
						return (player.Powers.BuffIsActive(348741) ? baseCooldown * 0.75d : baseCooldown);
					}
				},
				{
					Hud.Sno.SnoPowers.Crusader_Bombardment.Sno, 
					delegate (IPlayer player, double baseCooldown) {
						//reduce base cooldown by 30% if Lord Commander passive is used
						return (player.Powers.BuffIsActive(348741) ? baseCooldown * 0.7d : baseCooldown);
					}
				},
				{
					Hud.Sno.SnoPowers.Crusader_Condemn.Sno, 
					delegate (IPlayer player, double baseCooldown) {
						//Frydehr's Wrath removes Condemn cooldown
						return (player.Powers.BuffIsActive(478477) ? 0 : baseCooldown);
					}
				},
				{
					Hud.Sno.SnoPowers.Crusader_HeavensFury.Sno, 
					delegate (IPlayer player, double baseCooldown) {
						if (baseCooldown <= 0) //shotgun
							return 0;
						
						//Eberli Charo reduces the cooldown by 45-50%
						if (!player.Powers.BuffIsActive(318853))
							return baseCooldown;
						
						double reduction = 0.45; //min
						if (HasCubedItem(player, Hud.Sno.SnoItems.Unique_Shield_102_x1.Sno)) //player.CubeSnoItem1?.Sno == Hud.Sno.SnoItems.Unique_Shield_102_x1.Sno)
							reduction = 0.5; //max
						else {
							var mainhand = Hud.Game.Items.Where(i => i.Location == ItemLocation.LeftHand && i.SnoItem.Sno == Hud.Sno.SnoItems.Unique_Shield_102_x1.Sno);
							if (mainhand.Count() > 0) {
								IItemPerfection goldStat = mainhand.First().Perfections.FirstOrDefault(p => p.Attribute == Hud.Sno.Attributes.Item_Power_Passive);
								if (goldStat is object)
									reduction = goldStat.Cur;
							}
						}
						
						return baseCooldown * (1 - reduction);
					}
				},
				
				//Barbarian
				{
					Hud.Sno.SnoPowers.Barbarian_WrathOfTheBerserker.Sno, 
					delegate (IPlayer player, double baseCooldown) {
						//reduce base cooldown by 30s if Boon of Bul-Kathos passive is used (does not show up on buff list)
						return (player.Powers.UsedPassives.Any(s => s.Sno == Hud.Sno.SnoPowers.Barbarian_Passive_BoonOfBulKathos.Sno) ? baseCooldown - 30 : baseCooldown);
					}
				},
				{
					Hud.Sno.SnoPowers.Barbarian_Earthquake.Sno, 
					delegate (IPlayer player, double baseCooldown) {
						//reduce base cooldown by 15s if Boon of Bul-Kathos passive is used (does not show up on buff list)
						return (player.Powers.UsedPassives.Any(s => s.Sno == Hud.Sno.SnoPowers.Barbarian_Passive_BoonOfBulKathos.Sno) ? baseCooldown - 15 : baseCooldown);
					}
				},
				{
					Hud.Sno.SnoPowers.Barbarian_CallOfTheAncients.Sno, 
					delegate (IPlayer player, double baseCooldown) {
						//reduce base cooldown by 30s if Boon of Bul-Kathos passive is used (does not show up on buff list)
						return (player.Powers.UsedPassives.Any(s => s.Sno == Hud.Sno.SnoPowers.Barbarian_Passive_BoonOfBulKathos.Sno) ? baseCooldown - 30 : baseCooldown);
					}
				},
				
				//Wizard
				{
					Hud.Sno.SnoPowers.Wizard_Archon.Sno, 
					delegate (IPlayer player, double baseCooldown) {
						//2pc vyr gives any Archon skill rune the benefit of the lowest rune's cooldown
						return (player.Powers.BuffIsActive(Hud.Sno.SnoPowers.Generic_ItemPassiveUniqueRing727x1.Sno) ? 
							Hud.Sno.SnoPowers.Wizard_Archon.BaseCoolDownByRune.Min() :
							baseCooldown
						);
					}
				},
				{
					Hud.Sno.SnoPowers.Wizard_SlowTime.Sno, 
					delegate (IPlayer player, double baseCooldown) {
						//crown of primus gives Slow Time the benefit of the lowest rune's cooldown
						baseCooldown = (player.Powers.BuffIsActive(Hud.Sno.SnoPowers.CrownOfThePrimus.Sno) ? 
							Hud.Sno.SnoPowers.Wizard_SlowTime.BaseCoolDownByRune.Min() :
							baseCooldown
						);
						
						//gesture of orpheus reduces the cooldown by 30-40%
						if (!player.Powers.BuffIsActive(Hud.Sno.SnoPowers.GestureOfOrpheus.Sno))
							return baseCooldown;
						
						double reduction = 0.3; //min
						if (HasCubedItem(player, Hud.Sno.SnoItems.P2_Unique_Wand_002.Sno)) //player.CubeSnoItem1?.Sno == Hud.Sno.SnoItems.P2_Unique_Wand_002.Sno)
							reduction = 0.4; //max
						else {
							var mainhand = Hud.Game.Items.Where(i => i.Location == ItemLocation.RightHand && i.SnoItem.Sno == Hud.Sno.SnoItems.P2_Unique_Wand_002.Sno);
							if (mainhand.Count() > 0) {
								IItemPerfection goldStat = mainhand.First().Perfections.FirstOrDefault(p => p.Attribute == Hud.Sno.Attributes.Item_Power_Passive);
								if (goldStat is object)
									reduction = goldStat.Cur;
							}
						}
						
						return baseCooldown * (1 - reduction);
					}
				},
				{
					Hud.Sno.SnoPowers.Wizard_Teleport.Sno, 
					delegate (IPlayer player, double baseCooldown) {
						//Aether Walker removes Teleport cooldown
						if (player.Powers.BuffIsActive(397788))
							return 0;
						
						//The Oculus lowers Teleport cooldown by 1-4s
						var wpns = Hud.Game.Items.Where(i => (i.Location == ItemLocation.LeftHand) && i.SnoItem.Sno == Hud.Sno.SnoItems.Unique_Orb_001_x1.Sno);
						if (wpns.Count() > 0) {
							IItemPerfection stat = wpns.First().Perfections.First(p => p.Attribute == Hud.Sno.Attributes.Power_Cooldown_Reduction);
							if (stat is object)
								baseCooldown -= stat.Cur;
						}
						return baseCooldown;
					}
				},
				
				//Demon Hunter
				{
					Hud.Sno.SnoPowers.DemonHunter_Vengeance.Sno, 
					delegate (IPlayer player, double baseCooldown) {
						//dawn reduces cooldown by 50-65%
						if (!player.Powers.BuffIsActive(446146))
							return baseCooldown;
						
						double reduction = 0.5; //min
						if (HasCubedItem(player, Hud.Sno.SnoItems.P4_Unique_HandXBow_001.Sno)) //player.CubeSnoItem1?.Sno == Hud.Sno.SnoItems.P4_Unique_HandXBow_001.Sno)
							reduction = 0.65; //max
						else {
							var wpns = Hud.Game.Items.Where(i => (i.Location == ItemLocation.RightHand || i.Location == ItemLocation.LeftHand) && i.SnoItem.Sno == Hud.Sno.SnoItems.P4_Unique_HandXBow_001.Sno);
							if (wpns.Count() > 0) {
								IItemPerfection goldStat = wpns.First().Perfections.First(p => p.Attribute == Hud.Sno.Attributes.Item_Power_Passive);
								if (goldStat is object)
									reduction = goldStat.Cur;
							}
						}
						
						return baseCooldown * (1 - reduction);
					}
				},
				
				//Monk
				{
					Hud.Sno.SnoPowers.Monk_BreathOfHeaven.Sno, 
					delegate (IPlayer player, double baseCooldown) {
						//eye of peshkov reduces cooldown by 38-50%
						if (!player.Powers.BuffIsActive(Hud.Sno.SnoPowers.EyeOfPeshkov.Sno))
							return baseCooldown;
						
						double reduction = 0.38; //min
						if (HasCubedItem(player, Hud.Sno.SnoItems.Unique_SpiritStone_103_x1.Sno)) //player.CubeSnoItem2?.Sno == Hud.Sno.SnoItems.Unique_SpiritStone_103_x1.Sno)
							reduction = 0.5; //max
						else {
							var hats = Hud.Game.Items.Where(i => i.Location == ItemLocation.Head && i.SnoItem.Sno == Hud.Sno.SnoItems.Unique_SpiritStone_103_x1.Sno);
							if (hats.Count() > 0) {
								IItemPerfection goldStat = hats.First().Perfections.FirstOrDefault(p => p.Attribute == Hud.Sno.Attributes.Item_Power_Passive);
								if (goldStat is object)
									reduction = goldStat.Cur;
							}
						}
						
						return baseCooldown * (1 - reduction);
					}
				},
				
				//Necromancer
				{
					Hud.Sno.SnoPowers.Necromancer_CommandGolem.Sno, 
					delegate (IPlayer player, double baseCooldown) {
						//Golemskin Breeches reduces cooldown by 20-25s
						if (player.Powers.BuffIsActive(Hud.Sno.SnoPowers.GolemskinBreeches.Sno)) {
							double reduction = 20; //min
							if (HasCubedItem(player, Hud.Sno.SnoItems.P61_Necro_Unique_Pants_21.Sno)) //player.CubeSnoItem2?.Sno == Hud.Sno.SnoItems.P61_Necro_Unique_Pants_21.Sno)
								reduction = 25; //max
							else {
								var pants = Hud.Game.Items.Where(i => i.Location == ItemLocation.Legs && i.SnoItem.Sno == Hud.Sno.SnoItems.P61_Necro_Unique_Pants_21.Sno);
								if (pants.Count() > 0) {
									IItemPerfection goldStat = pants.First().Perfections.FirstOrDefault(p => p.Attribute == Hud.Sno.Attributes.Item_Power_Passive);
									if (goldStat is object)
										reduction = goldStat.Cur;
								}
							}
						
							baseCooldown -= reduction;
						}
						
						//Commander of the Risen Dead passive reduces cooldown by 30%
						if (player.Powers.BuffIsActive(Hud.Sno.SnoPowers.Necromancer_Passive_CommanderOfTheRisenDead.Sno))
							baseCooldown *= 0.7;

						return baseCooldown;
					}
				},
				
				//Witch Doctor
				{
					Hud.Sno.SnoPowers.WitchDoctor_Hex.Sno, 
					delegate (IPlayer player, double baseCooldown) {
						//reduce base cooldown by 25% if Tribal Rites passive is used (does not show up on buff list)
						return (player.Powers.UsedPassives.Any(s => s.Sno == Hud.Sno.SnoPowers.WitchDoctor_Passive_TribalRites.Sno) ? baseCooldown * 0.75d : baseCooldown);
					}
				},
				{
					Hud.Sno.SnoPowers.WitchDoctor_Gargantuan.Sno, 
					delegate (IPlayer player, double baseCooldown) {
						//reduce base cooldown by 25% if Tribal Rites passive is used (does not show up on buff list)
						return (player.Powers.UsedPassives.Any(s => s.Sno == Hud.Sno.SnoPowers.WitchDoctor_Passive_TribalRites.Sno) ? baseCooldown * 0.75d : baseCooldown);
					}
				},
				{
					Hud.Sno.SnoPowers.WitchDoctor_FetishArmy.Sno, 
					delegate (IPlayer player, double baseCooldown) {
						//reduce base cooldown by 25% if Tribal Rites passive is used (does not show up on buff list)
						return (player.Powers.UsedPassives.Any(s => s.Sno == Hud.Sno.SnoPowers.WitchDoctor_Passive_TribalRites.Sno) ? baseCooldown * 0.75d : baseCooldown);
					}
				},
				{
					Hud.Sno.SnoPowers.WitchDoctor_SummonZombieDog.Sno, 
					delegate (IPlayer player, double baseCooldown) {
						//reduce base cooldown by 25% if Tribal Rites passive is used (does not show up on buff list)
						return (player.Powers.UsedPassives.Any(s => s.Sno == Hud.Sno.SnoPowers.WitchDoctor_Passive_TribalRites.Sno) ? baseCooldown * 0.75d : baseCooldown);
					}
				},
				{
					Hud.Sno.SnoPowers.WitchDoctor_BigBadVoodoo.Sno, 
					delegate (IPlayer player, double baseCooldown) {
						//reduce base cooldown by 25% if Tribal Rites passive is used (does not show up on buff list)
						return (player.Powers.UsedPassives.Any(s => s.Sno == Hud.Sno.SnoPowers.WitchDoctor_Passive_TribalRites.Sno) ? baseCooldown * 0.75d : baseCooldown);
					}
				},
				{
					Hud.Sno.SnoPowers.WitchDoctor_MassConfusion.Sno, 
					delegate (IPlayer player, double baseCooldown) {
						//Last Breath reduces cooldown by 15-20s
						if (player.Powers.BuffIsActive(Hud.Sno.SnoPowers.LastBreath.Sno)) {
							double reduction = 15; //min
							if (HasCubedItem(player, Hud.Sno.SnoItems.P4_Unique_CeremonialDagger_008.Sno)) //player.CubeSnoItem1?.Sno == Hud.Sno.SnoItems.P4_Unique_CeremonialDagger_008.Sno)
								reduction = 20; //max
							else {
								var pants = Hud.Game.Items.Where(i => i.Location == ItemLocation.RightHand && i.SnoItem.Sno == Hud.Sno.SnoItems.P4_Unique_CeremonialDagger_008.Sno);
								if (pants.Count() > 0) {
									IItemPerfection goldStat = pants.First().Perfections.FirstOrDefault(p => p.Attribute == Hud.Sno.Attributes.Item_Power_Passive);
									if (goldStat is object)
										reduction = goldStat.Cur;
								}
							}
						
							baseCooldown -= reduction;
						}
						
						//reduce base cooldown by 25% if Tribal Rites passive is used (does not show up on buff list)
						if (player.Powers.UsedPassives.Any(s => s.Sno == Hud.Sno.SnoPowers.WitchDoctor_Passive_TribalRites.Sno))
							baseCooldown *= 0.75;
						
						return baseCooldown;
					}
				},
			};
			
			/*DebugDecorator = new WorldDecoratorCollection(
				new GroundLabelDecorator(Hud)
				{
					BackgroundBrush = Hud.Render.CreateBrush(100, 20, 20, 20, 0),
					TextFont = Hud.Render.CreateFont("tahoma", 6.5f, 255, 255, 255, 255, false, false, false),
				}
			);*/
        }
		
		public void Customize()
		{
			/*if (HideDamageLabels)
				Hud.TogglePlugin<OriginalSkillBarPlugin>(false);*/ 
		}
		
		public void OnNewArea(bool newGame, ISnoArea area)
		{
			if (newGame) {
				Data.Clear();
			}
		}
		
		public void OnHotkeyEvent(IKeyEvent keyEvent)
		{
			if (ToggleHotkey is object && keyEvent.Matches(ToggleHotkey))
			{
				if (ToggleDamageLabels.HasValue && ToggleDamageLabels.Value)
					Hud.TogglePlugin<OriginalSkillBarPlugin>(ShowCooldownLabels);
				
				ShowCooldownLabels = !ShowCooldownLabels;
			}
		}
		
		public void AfterCollect()
		{
			if (!Hud.Game.IsInGame)
				return;
			
			//rate limiting calls to CalculateCooldown
			if (ShowCooldownLabels)
			{
				int diff = Hud.Game.CurrentGameTick - TickDataCollected;
				if (diff < 0 || diff > 30)
				{
					TickDataCollected = Hud.Game.CurrentGameTick;
					
					foreach (var skill in Hud.Game.Me.Powers.CurrentSkills)
						Data[skill.SnoPower.Sno] = CalculateCooldown(skill);
				}
			}
		}
		
        public void PaintTopInGame(ClipState clipState)
        {
			//if (!ShowCooldownLabels) return;
			if (clipState != ClipState.BeforeClip)
				return;
			
			if (ShowCooldownLabels) // && Hud.Game.MapMode == MapMode.Minimap
			{
				if (!ToggleDamageLabels.HasValue || !LastState) //check at start and at every state change
				{
					var plugin = Hud.GetPlugin<OriginalSkillBarPlugin>();
					ToggleDamageLabels = plugin.Enabled;
					
					LastState = true;
				}
				
				if (ToggleDamageLabels.Value)
					Hud.TogglePlugin<OriginalSkillBarPlugin>(false); // Чтобы показывало кудаун  Razor и оригинальный поставте значение true При этом может быть наложение одного на другое.
				
				//draw
				foreach (var skill in Hud.Game.Me.Powers.CurrentSkills)
				{
					if (!Data.ContainsKey(skill.SnoPower.Sno))
						continue;
					
					//calculate cooldown
					float cooldown = Data[skill.SnoPower.Sno]; //CalculateCooldown(skill);
					
					//draw cooldown under skill
					if (cooldown > 0) {
						//get skill icon position
						IUiElement ui = Hud.Render.GetPlayerSkillUiElement(skill.Key);
						TextLayout layout = LocationFont.GetTextLayout(cooldown.ToString(cooldown < 100 ? "F2" : "F1") + "с"); // + skill.GetResourceRequirement(skill.ResourceCost) // + skill.Rune //skill.WeaponDamageMultiplier //"s";
						float hintHeight = ui.Rectangle.Height*0.35f;
						
						HintBgBrush.Opacity = 125f/255f;
						HintBgBrush.StrokeWidth = 0;
						HintBgBrush.DrawRectangle(ui.Rectangle.Left, ui.Rectangle.Bottom, ui.Rectangle.Width, hintHeight);
						
						HintBgBrush.Opacity = 175f/255f;
						HintBgBrush.StrokeWidth = 1;
						HintBgBrush.DrawRectangle(ui.Rectangle.Left, ui.Rectangle.Bottom, ui.Rectangle.Width, hintHeight);
						
						LocationFont.DrawText(layout, ui.Rectangle.Left + ui.Rectangle.Width*0.5f - layout.Metrics.Width*0.5f, ui.Rectangle.Bottom + hintHeight*0.5f - layout.Metrics.Height*0.5f);
					}

				}
				

			}
			else if (LastState)
			{
				var plugin = Hud.GetPlugin<OriginalSkillBarPlugin>();
				ToggleDamageLabels = !plugin.Enabled;

				LastState = false;
			}
		}
		
		public float CalculateCooldown(IPlayerSkill skill)
		{
			int index = (int)skill.Rune+1;
			if (index >= skill.SnoPower.BaseCoolDownByRune.Length) index = 0; //unruned skills have a value of 255
			
			double baseCooldown = skill.SnoPower.BaseCoolDownByRune[index];
			
			if (baseCooldown < 0) { //skills that have no cooldown have a base cooldown of -1
				if (!ChargeCooldowns.TryGetValue(skill.SnoPower.Sno, out baseCooldown)) //check if it uses charges (and if so, look up how long it takes to regenerate a charge)
					return 0;
			}
			
			Func<IPlayer, double, double> modifier;
			return skill.CalculateCooldown((float)(CooldownModifiers.TryGetValue(skill.SnoPower.Sno, out modifier) ? 
				modifier(skill.Player, baseCooldown) :
				baseCooldown)
			);
		}
		
		public bool HasCubedItem(IPlayer player, uint sno) //uint = item sno
		{	
			return (player.CubeSnoItem1?.Sno == sno) || (player.CubeSnoItem2?.Sno == sno) || (player.CubeSnoItem3?.Sno == sno) || (player.CubeSnoItem4?.Sno == sno);
		}
    }
}