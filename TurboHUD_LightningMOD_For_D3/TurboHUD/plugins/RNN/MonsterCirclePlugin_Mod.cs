// Mod of the plugin MonsterCirclePlugin by CB. The original can be found:
// https://www.ownedcore.com/forums/diablo-3/turbohud/turbohud-community-plugins/612786-international-cb-monstercircleplugin.html
// Changes: Draw different circles to indicate that the elites have certain abilities. Disable EliteMonsterAffixPlugin automatically if DisableEMAffix = true

using System.Collections.Generic;
using Turbo.Plugins.Default;
using System.Linq;
using System;

namespace Turbo.Plugins.CB
{
    public class MonsterCirclePlugin_Mod : BasePlugin, IInGameWorldPainter, ICustomizer
    {
        public WorldDecoratorCollection RareDecorator { get; set; }

        public WorldDecoratorCollection ShieldDecorator { get; set; }
        public WorldDecoratorCollection IlusDecorator { get; set; }
        public WorldDecoratorCollection ChainsDecorator { get; set; }
        public WorldDecoratorCollection WormDecorator { get; set; }
        public WorldDecoratorCollection MortarDecorator { get; set; }
        public WorldDecoratorCollection TeleportDecorator { get; set; }
        public WorldDecoratorCollection WallerDecorator { get; set; }
        public WorldDecoratorCollection InvisDecorator { get; set; }

        public WorldDecoratorCollection ChampionDecorator { get; set; }
        public WorldDecoratorCollection ChampionDecoratorH { get; set; }
        public WorldDecoratorCollection JuggernautDecorator { get; set; }
		public WorldDecoratorCollection GoblinDecorator { get; set; } // Goblin
		public WorldDecoratorCollection RareMinionDecorator { get; set; }   // Elite Minion
        public WorldDecoratorCollection UniqueDecorator { get; set; }   //Purple
        public WorldDecoratorCollection BossDecorator { get; set; }   //Boss

		public WorldDecoratorCollection HitBoxEliteDecorator { get; set; }
		public WorldDecoratorCollection EliteIluAcdDecorator { get; set; }

		public bool ShowHitBox { get; set; }
		public bool ShowWaller { get; set; }
		public bool ShowMortar { get; set; }
		public bool ShowTeleport { get; set; }

		public bool DisableEMAffix { get; set; }

        public MonsterCirclePlugin_Mod ()
        {
            Enabled = true;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
			ShowHitBox = true;		//Show HitBox of monster
			ShowWaller = false;		//Show circle for Waller
			ShowMortar = false;		//Show circle for Mortar
			ShowTeleport = false;   //Show circle for Teleport

			DisableEMAffix = true;  // Disable default plugin EliteMonsterAffixPlugin

            var shadowBrush = Hud.Render.CreateBrush(96, 0, 0, 0, 1);

			HitBoxEliteDecorator = new WorldDecoratorCollection(
				new GroundCircleDecorator(Hud) {
					Brush = Hud.Render.CreateBrush(20, 200, 250, 250, 0),
                    Radius = -1
                },
				new GroundCircleDecorator(Hud) {
					Brush = Hud.Render.CreateBrush(220, 0, 0, 0, 0.1f),
                    Radius = -1
                }
            );
			EliteIluAcdDecorator = new WorldDecoratorCollection(
				new GroundCircleDecorator(Hud) {
					Brush = Hud.Render.CreateBrush(10, 200, 250, 250, 0),
                    Radius = -1
                },
				new GroundCircleDecorator(Hud) {
					Brush = Hud.Render.CreateBrush(100,220, 220, 220, 0.7f),
                    Radius = -1
                }
            );

			GoblinDecorator = new WorldDecoratorCollection(
				new GroundCircleDecorator(Hud) {
                    Brush = Hud.Render.CreateBrush(255, 57, 194, 29, 3),
                    Radius = 3
                },
				new GroundCircleDecorator(Hud) {
                    Brush = Hud.Render.CreateBrush(255, 240, 213, 10, 5),
                    Radius = 2
                },
				new GroundCircleDecorator(Hud) {
                    Brush = Hud.Render.CreateBrush(180, 255, 0, 0, 6),
                    Radius = 0.3f
                }
			);
            RareDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud) {
                    Brush = Hud.Render.CreateBrush(255, 255, 148, 20, 3),
                    Radius = 3f
                },
				new GroundCircleDecorator(Hud) {
                    Brush = Hud.Render.CreateBrush(255, 255, 148, 20, 5),
                    Radius = 2f
                },
				new GroundCircleDecorator(Hud) {
                    Brush = Hud.Render.CreateBrush(180, 255, 0, 0, 6),
                    Radius = 0.3f
                },

                new MapShapeDecorator(Hud) {
                    Brush = Hud.Render.CreateBrush(255, 255, 148, 20, 0),
                    Radius = 6,
                    ShapePainter = new CircleShapePainter(Hud)
                }
            );
            ChampionDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud) {
                    Brush = Hud.Render.CreateBrush(255, 64, 128, 255, 3),
                    Radius = 3f
                },
				new GroundCircleDecorator(Hud) {
                    Brush = Hud.Render.CreateBrush(255, 64, 128, 255, 5),
                    Radius = 2f
                },
				new GroundCircleDecorator(Hud) {
                    Brush = Hud.Render.CreateBrush(180, 255, 0, 0, 6),
                    Radius = 0.3f
                },
                new MapShapeDecorator(Hud) {
                    Brush = Hud.Render.CreateBrush(255, 64, 128, 255, 0),
                   Radius = 6,
                    ShapePainter = new CircleShapePainter(Hud)
                }
            );
            ChampionDecoratorH = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud) {
                    Brush = Hud.Render.CreateBrush(255, 64, 128, 255, 3, SharpDX.Direct2D1.DashStyle.Dash),
                    Radius = 3f
                },
				new GroundCircleDecorator(Hud) {
                    Brush = Hud.Render.CreateBrush(255, 64, 128, 255, 5),
                    Radius = 2f
                },
				new GroundCircleDecorator(Hud) {
                    Brush = Hud.Render.CreateBrush(180, 255, 0, 0, 6),
                    Radius = 0.3f
                },
                new MapShapeDecorator(Hud) {
                    Brush = Hud.Render.CreateBrush(255, 64, 128, 255, 0),
                    Radius = 6,
                    ShapePainter = new CircleShapePainter(Hud)
                }
            );

            JuggernautDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud)
                {
                    Brush = Hud.Render.CreateBrush(255, 255, 50, 0, 3),
                    Radius = 3f,
                },
				new GroundCircleDecorator(Hud) {
                    Brush = Hud.Render.CreateBrush(255, 255, 148, 20, 5),
                    Radius = 2
                },
				new GroundCircleDecorator(Hud) {
                    Brush = Hud.Render.CreateBrush(180, 255, 0, 0, 6),
                    Radius = 0.3f
                },

                new MapShapeDecorator(Hud)
                {
                    Brush = Hud.Render.CreateBrush(255, 255, 50, 0, 0),
                    ShadowBrush = shadowBrush,
                    Radius = 6,
                    ShapePainter = new CircleShapePainter(Hud),
                }
             );

            MortarDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud) {
                    Brush = Hud.Render.CreateBrush(255, 180, 180, 180, 3, SharpDX.Direct2D1.DashStyle.Dash),
                    Radius = 1.75f
                }
            );

            TeleportDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud) {
                    //Brush = Hud.Render.CreateBrush(255, 200, 200, 200, 3),
					Brush = Hud.Render.CreateBrush(220, 100, 250, 250, 3),
                    Radius = 1.55f
                }
            );

            WallerDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud) {
                    Brush = Hud.Render.CreateBrush(255, 190, 95, 0, 4),
                    Radius = 1.25f
                }
            );

            IlusDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud) {
                    Brush = Hud.Render.CreateBrush(255, 57, 194, 29, 4),
                    Radius = 2.25f
                }
            );
            ChainsDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud) {
                    Brush = Hud.Render.CreateBrush(255, 255, 20, 0, 3, SharpDX.Direct2D1.DashStyle.Dash),
                    Radius = 2.5f
                }
            );
            WormDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud) {
                    Brush = Hud.Render.CreateBrush(255, 255, 170, 0, 3, SharpDX.Direct2D1.DashStyle.Dash),
                    Radius = 2.5f
                }
            );

            ShieldDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud) {
                    Brush = Hud.Render.CreateBrush(255, 255, 255, 0, 3),
                    Radius = 2.75f
                }
            );


            InvisDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud) {
                    Brush = Hud.Render.CreateBrush(255, 255, 255, 255, 6),
                    Radius = 3.40f
                }
            );

		RareMinionDecorator = new WorldDecoratorCollection(
				new GroundCircleDecorator(Hud) {
                    Brush = Hud.Render.CreateBrush(255, 255, 148, 20, 3, SharpDX.Direct2D1.DashStyle.Dash),
                    Radius = 3
                },
				new GroundCircleDecorator(Hud) {
                    Brush = Hud.Render.CreateBrush(180, 255, 0, 0, 6, SharpDX.Direct2D1.DashStyle.Dash),
                    Radius = 0
                },
                new MapShapeDecorator(Hud) {
                    Brush = Hud.Render.CreateBrush(255, 192, 92, 20, 2.0f),
                    Radius = 4,
                    ShapePainter = new CircleShapePainter(Hud)
                }
        );

		UniqueDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud) {
                    Brush = Hud.Render.CreateBrush(255,255,140,255, 3),
                    Radius = 3
                },
				new GroundCircleDecorator(Hud) {
                    Brush = Hud.Render.CreateBrush(255,255,140,255, 5),
                    Radius = 2
                },
				new GroundCircleDecorator(Hud) {
                    Brush = Hud.Render.CreateBrush(180, 255, 0, 0, 6),
                    Radius = 0.3f
                },
                new MapShapeDecorator(Hud) {
                    Brush = Hud.Render.CreateBrush(255,255,140,255, 0),
                    Radius = 6,
                    ShapePainter = new CircleShapePainter(Hud)
                }
            );

        BossDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud) {
                    Brush = Hud.Render.CreateBrush(255, 255, 96, 0, 4),
                    Radius = 4
                },
				new GroundCircleDecorator(Hud) {
                    Brush = Hud.Render.CreateBrush(255, 255, 96, 0, 5),
                    Radius = 2
                },
				new GroundCircleDecorator(Hud) {
                    Brush = Hud.Render.CreateBrush(180, 255, 0, 0, 6),
                    Radius = 0.3f
                },
                new MapShapeDecorator(Hud) {
                    Brush = Hud.Render.CreateBrush(255, 255, 96, 0, 0),
                    Radius = 6,
                    ShapePainter = new CircleShapePainter(Hud)
                }
            );
        }

        public void Customize()
        {
            if (DisableEMAffix) { Hud.TogglePlugin<EliteMonsterAffixPlugin>(false); }
        }

		private bool HasAffix(IMonster m, MonsterAffix afx)
		{
			return m.AffixSnoList.Any(a => a.Affix == afx);
		}

        public void PaintWorld(WorldLayer layer)
        {
			if (!Hud.Game.IsInGame || Hud.Game.Me.IsInTown) return;
            var monsters = Hud.Game.AliveMonsters;
			var goblins = Hud.Game.AliveMonsters.Where(x => x.SnoMonster.Priority == MonsterPriority.goblin);
			foreach (var monster in goblins)
			{
                GoblinDecorator.Paint(layer, monster, monster.FloorCoordinate, null);
            }

            List<IMonster> monstersElite = new List<IMonster>();
            foreach (var monster in monsters)
            {

                if (monster.Rarity == ActorRarity.Champion || monster.Rarity == ActorRarity.Rare)
                {
                   monstersElite.Add(monster);
                }

				else if (monster.Rarity == ActorRarity.RareMinion) {
					if (monster.SummonerAcdDynamicId != 0) EliteIluAcdDecorator.Paint(layer, monster, monster.FloorCoordinate, string.Empty);
					else RareMinionDecorator.Paint(layer, monster, monster.FloorCoordinate, monster.SnoMonster.NameLocalized);
                }

                else if (monster.Rarity == ActorRarity.Unique) {
                    UniqueDecorator.Paint(layer, monster, monster.FloorCoordinate, monster.SnoMonster.NameLocalized);
					if (ShowHitBox) EliteIluAcdDecorator.Paint(layer, monster, monster.FloorCoordinate, string.Empty);
                }

                else if (monster.Rarity == ActorRarity.Boss) {
					if (monster.SummonerAcdDynamicId != 0) EliteIluAcdDecorator.Paint(layer, monster, monster.FloorCoordinate, string.Empty);
					else {
						BossDecorator.Paint(layer, monster, monster.FloorCoordinate, monster.SnoMonster.NameLocalized);
						if (ShowHitBox) HitBoxEliteDecorator.Paint(layer, monster, monster.FloorCoordinate, string.Empty);
					}
                }
            }

			foreach (var monster in monstersElite)
            {
                if (monster.SummonerAcdDynamicId == 0)
                {
                    if (monster.Rarity == ActorRarity.Rare)
                    {
						if (HasAffix(monster,MonsterAffix.Juggernaut))	JuggernautDecorator.Paint(layer, monster, monster.FloorCoordinate, monster.SnoMonster.NameLocalized);
						else RareDecorator.Paint(layer, monster, monster.FloorCoordinate, monster.SnoMonster.NameLocalized);
						if (HasAffix(monster,MonsterAffix.Wormhole)) WormDecorator.Paint(layer, monster, monster.FloorCoordinate, monster.SnoMonster.NameLocalized);
                    }
                    else if (monster.Rarity == ActorRarity.Champion)
					{
						if (HasAffix(monster,MonsterAffix.HealthLink))  ChampionDecoratorH.Paint(layer, monster, monster.FloorCoordinate, monster.SnoMonster.NameLocalized);
						else ChampionDecorator.Paint(layer, monster, monster.FloorCoordinate, monster.SnoMonster.NameLocalized);
						if (HasAffix(monster,MonsterAffix.FireChains)) ChainsDecorator.Paint(layer, monster, monster.FloorCoordinate, monster.SnoMonster.NameLocalized);
                    }

                    if (HasAffix(monster,MonsterAffix.Shielding)) ShieldDecorator.Paint(layer, monster, monster.FloorCoordinate, monster.SnoMonster.NameLocalized);
					if (HasAffix(monster,MonsterAffix.Illusionist)) IlusDecorator.Paint(layer, monster, monster.FloorCoordinate, monster.SnoMonster.NameLocalized);

					if (monster.Invisible) InvisDecorator.Paint(layer, monster, monster.FloorCoordinate, monster.SnoMonster.NameLocalized);

					if ( ShowWaller && HasAffix(monster,MonsterAffix.Waller) ) WallerDecorator.Paint(layer, monster, monster.FloorCoordinate, monster.SnoMonster.NameLocalized);
					if ( ShowMortar && HasAffix(monster,MonsterAffix.Mortar) ) MortarDecorator.Paint(layer, monster, monster.FloorCoordinate, monster.SnoMonster.NameLocalized);
					if ( ShowTeleport && HasAffix(monster,MonsterAffix.Teleporter) ) TeleportDecorator.Paint(layer, monster, monster.FloorCoordinate, monster.SnoMonster.NameLocalized);

					if (ShowHitBox) HitBoxEliteDecorator.Paint(layer, monster, monster.FloorCoordinate, string.Empty);
				}
				else EliteIluAcdDecorator.Paint(layer, monster, monster.FloorCoordinate, string.Empty);
			}
			monstersElite.Clear();
        }
    }
}