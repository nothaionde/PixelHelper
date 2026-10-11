using Turbo.Plugins.Default;
namespace Turbo.Plugins.glq
{
    public class GLQ_CursorOnMonster1 : BasePlugin, IInGameWorldPainter
    {
        public WorldDecoratorCollection TrashDecorator { get; set; }
        public WorldDecoratorCollection ChampionDecorator { get; set; }
        public WorldDecoratorCollection RareMinionDecorator { get; set; }
        public WorldDecoratorCollection RareDecorator { get; set; }
        public WorldDecoratorCollection UniqueDecorator { get; set; }
        public WorldDecoratorCollection BossDecorator { get; set; }

        private float _radius = -1;
        public float Radius
        {
            get { return _radius; }
            set
            {
                if (value == _radius || value < -1)
                    return;

                _radius = value;
                InitDecorators();
            }
        }

        public GLQ_CursorOnMonster1()
        {
            Enabled = true;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            InitDecorators();
        }

        public void PaintWorld(WorldLayer layer)
        {
            var monster = Hud.Game.SelectedMonster2 ?? Hud.Game.SelectedMonster1;
            if (monster == null) return;
            if (monster.SummonerAcdDynamicId != 0 && monster.IsElite) return;
            if (!monster.Invisible && !monster.IsElite && (monster.SnoMonster.Priority != MonsterPriority.goblin) && (monster.SnoMonster.Priority != MonsterPriority.boss) && (monster.SnoMonster.Priority != MonsterPriority.keywarden))
            {
                TrashDecorator.Paint(layer, monster, monster.FloorCoordinate, monster.SnoMonster.NameLocalized);
            }
            if (monster.Rarity == ActorRarity.Unique)
            {
                UniqueDecorator.Paint(layer, monster, monster.FloorCoordinate, monster.SnoMonster.NameLocalized);
            }
            if (monster.SnoMonster.Priority == MonsterPriority.boss)
            {
                BossDecorator.Paint(layer, monster, monster.FloorCoordinate, monster.SnoMonster.NameLocalized);
            }
            if (monster.Rarity == ActorRarity.Champion)
            {
                ChampionDecorator.Paint(layer, monster, monster.FloorCoordinate, monster.SnoMonster.NameLocalized);
            }
            if (monster.Rarity == ActorRarity.RareMinion)
            {
                RareMinionDecorator.Paint(layer, monster, monster.FloorCoordinate, monster.SnoMonster.NameLocalized);
            }
            if (monster.Rarity == ActorRarity.Rare)
            {
                RareDecorator.Paint(layer, monster, monster.FloorCoordinate, monster.SnoMonster.NameLocalized);
            }
            if ((monster.Rarity == ActorRarity.Unique) && (monster.SnoMonster.Priority < MonsterPriority.keywarden))
            {
                UniqueDecorator.Paint(layer, monster, monster.FloorCoordinate, monster.SnoMonster.NameLocalized);
            }
            if (monster.SnoMonster.Priority == MonsterPriority.boss)
            {
                BossDecorator.Paint(layer, monster, monster.FloorCoordinate, monster.SnoMonster.NameLocalized);
            }
        }

        private void InitDecorators()
        {
            int stroke = (Radius == -1) ? 0 : 5;

            var TrashGroundBrush = Hud.Render.CreateBrush(255, 200, 200, 200, stroke, SharpDX.Direct2D1.DashStyle.Dash);
            TrashDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud)
                {
                    Brush = TrashGroundBrush,
                    Radius = Radius,
                }
                );

            var EliteChampionGroundBrush = Hud.Render.CreateBrush(255, 64, 128, 255, stroke, SharpDX.Direct2D1.DashStyle.Dash);
            ChampionDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud)
                {
                    Brush = EliteChampionGroundBrush,
                    Radius = Radius,
                }
                );
            var EliteMinionGroundBrush = Hud.Render.CreateBrush(255, 255, 216, 160, stroke, SharpDX.Direct2D1.DashStyle.Dash);
            RareMinionDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud)
                {
                    Brush = EliteMinionGroundBrush,
                    Radius = Radius,
                }
                );
            var EliteLeaderGroundBrush = Hud.Render.CreateBrush(255, 255, 148, 20, stroke, SharpDX.Direct2D1.DashStyle.Dash);
            RareDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud)
                {
                    Brush = EliteLeaderGroundBrush,
                    Radius = Radius,
                }
                );
            var EliteUniqueGroundBrush = Hud.Render.CreateBrush(255, 255, 140, 255, stroke, SharpDX.Direct2D1.DashStyle.Dash);
            UniqueDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud)
                {
                    Brush = EliteUniqueGroundBrush,
                    Radius = Radius,
                }
                );
            var BossGroundBrush = Hud.Render.CreateBrush(255, 255, 96, 0, stroke, SharpDX.Direct2D1.DashStyle.Dash);
            BossDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud)
                {
                    Brush = BossGroundBrush,
                    Radius = Radius,
                }
                );
        }
    }
}