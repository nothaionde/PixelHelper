/*
	This file contains monster marker definitions for the MonsterMarkers plugin.
*/

namespace Turbo.Plugins.Razor.Monster
{
	using System.Linq;
	using Turbo.Plugins.Default;
	using Turbo.Plugins.Razor.Util; //IPlayer.HasCubedItem (UtilExtensions)

	public class MonsterMarkersConfig : BasePlugin, ICustomizer
	{
		public MonsterMarkersConfig()
		{ 
			Enabled = true; // Для включения плагина измените false на true
			Order = 60005;
		}

		public void Customize()
		{
			Hud.RunOnPlugin<MonsterMarkers>(plugin =>
			{
				//mark monsters that are frozen
				plugin.Markers.Add(
					new MonsterMarker() {
						IsRelevant = () => true, //Hud.Game.Items.Any(i => i.Location == ItemLocation.RightHand && i.SnoItem.Sno == Hud.Sno.SnoItems.P71_Ethereal_09.Sno), //if Buriza ethereal is equipped //Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.KrysbinsSentence.Sno) || Hud.Game.Players.Any(p => p.HeroClassDefinition.HeroClass == HeroClass.Necromancer && p.Powers.UsedSkills.Any(s => s.SnoPower.Sno == Hud.Sno.SnoPowers.Necromancer_LandOfTheDead.Sno)), //Hud.Game.Me.HeroClassDefinition.HeroClass == HeroClass.Necromancer && Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.KrysbinsSentence.Sno),
						IsMarked = (m) => m.Frozen, //!m.Frozen && !m.Stunned, //Land of the Dead freeze missing
						Font = Hud.Render.CreateFont("tahoma", 12f, 255, 255, 0, 0, false, false, 255, 0, 0, 0, true),
						Symbol = "❆",
						Decorator = new WorldDecoratorCollection(
							new MapShapeDecorator(Hud)
							{
								Brush = Hud.Render.CreateBrush(150, 255, 0, 0, 2),
								ShadowBrush = Hud.Render.CreateBrush(150, 0, 0, 0, 4),
								ShapePainter = new CrossShapePainter(Hud),
								Radius = 4,
							}
						),
					}
				);
				
				//mark monsters whose health is in Execute range if someone in the party has an execute skill or power equipped
				int ExecuteRange = 0;
				plugin.Markers.Add(
					new MonsterMarker() {
						IsRelevant = () => {
							ExecuteRange = 0;

							foreach (IPlayer player in Hud.Game.Players)
							{
								if (player.HeroClassDefinition.HeroClass == HeroClass.Necromancer)
								{
									//IPlayerSkill skill = player.Powers.UsedSkills.FirstOrDefault(x => x.SnoPower.Sno == Hud.Sno.SnoPowers.Necromancer_Frailty.Sno);
									//if (skill is object)
									if (player.Powers.UsedNecromancerPowers.Frailty is object)
									{
										ExecuteRange = player.Powers.UsedNecromancerPowers.Frailty.Rune == 4 ? 18 : 15; //skill.Rune == 4 ? 18 : 15; //? Early Grave
										return true;
									}
									
									if (player.Powers.UsedNecromancerPowers.GrimScythe is object && (player.Powers.UsedNecromancerPowers.GrimScythe.Rune == 4 || player.Powers.BuffIsActive(Hud.Sno.SnoPowers.TragOulsCorrodedFang.Sno))) //player.Powers.UsedSkills.Any(x => x.SnoPower.Sno == Hud.Sno.SnoPowers.Necromancer_GrimScythe.Sno && (x.Rune == 4 || player.Powers.BuffIsActive(Hud.Sno.SnoPowers.TragOulsCorrodedFang.Sno))))
									{
										ExecuteRange = 15;
										return true;
									}
								}
								
								if (ExecuteRange < 10 && player.Powers.BuffIsActive(Hud.Sno.SnoPowers.TheExecutioner.Sno))
								{
									//check if equipped, and if so, what is the actual execute value
									if (player.HasCubedItem(Hud.Sno.SnoItems.P66_Unique_Axe_2H_003.Sno)) //HasCubedItem(player, Hud.Sno.SnoItems.P66_Unique_Axe_2H_003.Sno)) //player.CubeSnoItem2?.Sno == Hud.Sno.SnoItems.P61_Necro_Unique_Pants_21.Sno)
										ExecuteRange = 10; //max
									else
									{
										var wpn = Hud.Game.Items.FirstOrDefault(i => i.Location == ItemLocation.RightHand && i.SnoItem.Sno == Hud.Sno.SnoItems.P66_Unique_Axe_2H_003.Sno);
										if (wpn is object)
										{
											IItemPerfection goldStat = wpn.Perfections.FirstOrDefault(p => p.Attribute == Hud.Sno.Attributes.Item_Power_Passive);
											if (goldStat is object)
											{
												int range = (int)(goldStat.Cur * 100d);
												if (ExecuteRange < range)
													ExecuteRange = range;
											}
										}
									}
								}
							}
							
							return ExecuteRange > 0;
						}, //Hud.Game.Players.Any(p => p.Powers.BuffIsActive(Hud.Sno.SnoPowers.TheExecutioner.Sno)),
						IsMarked = (m) => (m.CurHealth / m.MaxHealth)*100 < ExecuteRange,
						Font = Hud.Render.CreateFont("tahoma", 11f, 255, 255, 0, 0, false, false, 255, 0, 0, 0, true),
						Symbol = "🕱", //💀❌
						Decorator = new WorldDecoratorCollection(
							new MapLabelDecorator (Hud) {
								LabelFont = Hud.Render.CreateFont ("tahoma", 7, 255, 51, 255, 248, true, false, false),
							},
							new GroundCircleDecorator(Hud) {
								Brush = Hud.Render.CreateBrush(225, 51, 255, 248, 3, SharpDX.Direct2D1.DashStyle.Dash), //255, 0, 0
								Radius = -1,
							}
						),
					}
				);
				
				//mark monsters that can trigger Karini
				plugin.Markers.Add(
					new MonsterMarker() {
						IsRelevant = () => Hud.Game.Me.HeroClassDefinition.HeroClass == HeroClass.Wizard && Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.HaloOfKarini.Sno),
						IsMarked = (m) => m.CentralXyDistanceToMe > 15 && m.CentralXyDistanceToMe <= 50,
						Font = Hud.Render.CreateFont("tahoma", 10f, 255, 150, 199, 246, false, false, 255, 0, 0, 0, true),
						Symbol = "🗲",
					}
				);
				
				//mark the target of Command Skeletons
				plugin.Markers.Add(
					new MonsterMarker() {
						IsRelevant = () => Hud.Game.Me.Powers.UsedNecromancerPowers.CommandSkeletons is object,
						IsMarked = (m) => m.GetAttributeValueAsInt(Hud.Sno.Attributes.Power_Buff_4_Visual_Effect_None, Hud.Sno.SnoPowers.Necromancer_CommandSkeletons.Sno, 0) == 1 || //453801
							m.GetAttributeValueAsInt(Hud.Sno.Attributes.Power_Buff_4_Visual_Effect_A, Hud.Sno.SnoPowers.Necromancer_CommandSkeletons.Sno, 0) == 1 ||
							m.GetAttributeValueAsInt(Hud.Sno.Attributes.Power_Buff_4_Visual_Effect_B, Hud.Sno.SnoPowers.Necromancer_CommandSkeletons.Sno, 0) == 1 ||
							m.GetAttributeValueAsInt(Hud.Sno.Attributes.Power_Buff_4_Visual_Effect_C, Hud.Sno.SnoPowers.Necromancer_CommandSkeletons.Sno, 0) == 1 ||
							m.GetAttributeValueAsInt(Hud.Sno.Attributes.Power_Buff_4_Visual_Effect_D, Hud.Sno.SnoPowers.Necromancer_CommandSkeletons.Sno, 0) == 1 ||
							m.GetAttributeValueAsInt(Hud.Sno.Attributes.Power_Buff_4_Visual_Effect_E, Hud.Sno.SnoPowers.Necromancer_CommandSkeletons.Sno, 0) == 1,
						Font = Hud.Render.CreateFont("tahoma", 12f, 255, 23, 212, 48, true, false, 255, 0, 0, 0, true), //150, 199, 246
						Symbol = "⌖", //⌖
					}
				);
				
				/*
				//mark monsters that are missing Strongarms if someone has strongarms equipped in the party
				plugin.Markers.Add(
					new MonsterMarker() {
						IsRelevant = () => Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.StrongarmBracers.Sno),
						IsMarked = (m) => m.IsElite && m.GetAttributeValueAsInt(Hud.Sno.Attributes.Power_Buff_2_Visual_Effect_None, 318772) != 1,
						Font = Hud.Render.CreateFont("tahoma", 10f, 255, 150, 199, 246, false, false, 255, 0, 0, 0, true),
						Symbol = "💪",
					}
				);
				*/
			});

		}
	}
}