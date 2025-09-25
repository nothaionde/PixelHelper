namespace Turbo.Plugins.LM
{
    using Turbo.Plugins.glq;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Forms;
    using Turbo.Plugins.Default;

    public class OpenChestPlugin1 : BasePlugin, IAfterCollectHandler, INewAreaHandler
    {

        public bool WhenForceMoveInvalid { get; set; } = true;
        private IActor selectedActor { get; set; }
        private IUiElement uiInv { get; set; }
        public int IntervalMilliseconds { get; set; } = 10;
        private IWatch _timer;
        public bool Clicking = false;
        public bool Rack { get; set; }
        public bool ChestNormal { get; set; }
        public bool Chest { get; set; }
        public bool DeadBody { get; set; }
        public bool Stone { get; set; }
        public bool Pool { get; set; }
        public bool Shrine { get; set; }
        public bool door { get; set; }
        public int ClickPylonInGRRank { get; set; } = 140;
        private readonly HashSet<uint> _clickedAnnIds = new HashSet<uint>();
        public readonly HashSet<ActorSnoEnum> SwitchIds = new HashSet<ActorSnoEnum>
        {
            ActorSnoEnum._a3dun_keep_bridge_switch,
            ActorSnoEnum._a3dun_keep_bridge_switch_b
        };
        public readonly HashSet<ActorSnoEnum> DoorsIdsBlackList = new HashSet<ActorSnoEnum>() {
            ActorSnoEnum._cald_merchant_cart, // A2 to belial
			ActorSnoEnum._a2dun_cald_exit_gate, // A2 to belial
			ActorSnoEnum._a2dunswr_gates_causeway_gates_non_op, // A2 to belial
			ActorSnoEnum._a2dun_cald_belial_acid_attack, // A2 to belial
			ActorSnoEnum._a2dun_cald_belial_room_gate_a, // A2 to belial
            ActorSnoEnum._trout_cultists_summoning_portal_b, // A2 Alcarnus
            ActorSnoEnum._caout_target_dummy, // A2 City
			ActorSnoEnum._start_location_team_0, // A2 City
            ActorSnoEnum._a3dun_crater_st_demon_chainpylon_fire_azmodan, // A3 rakkis crossing
			ActorSnoEnum._a3dun_keep_bridge, // A3 rakkis crossing
            ActorSnoEnum._a3dun_rmpt_frozendoor_a, // A3 stonefort
            ActorSnoEnum._catapult_a3dunkeep_warmachines_snow_firing, // A3 battlefields
            ActorSnoEnum._x1_crusader_trebuchet_pending_tar,
            ActorSnoEnum._event_1000monster_portal,
            ActorSnoEnum._a2dun_zolt_sandbridgebase_bossfight,
            ActorSnoEnum._px_highlands_camp_resurgentcult_portal,
            ActorSnoEnum._x1_bog_catacombsportal_beaconloc,
            ActorSnoEnum._x1_malthael_boss_orb_collapse, // malthael fight
            ActorSnoEnum._caout_oasis_mine_entrance_a, // check this one, maybe a bounty
            ActorSnoEnum._trout_leor_painting, // leoric manor
            ActorSnoEnum._a4dun_sigil_room_platform_a, // Holy Sanctum
            ActorSnoEnum._a3dun_rmpt_catapult_follower_event_gate, // a3 catapult event
            ActorSnoEnum._a1dun_leor_jail_door_superlocked_a_fake,
            ActorSnoEnum._cos_pet_mimic_01,
            ActorSnoEnum._shoulderpads_norm_base_flippy, // ???
            ActorSnoEnum._x1_abattoir_barricade_solid,
            ActorSnoEnum._x1_fortress_floatrubble_a,
            ActorSnoEnum._a3dun_keep_barrel_snow_no_skirt, // Sturdy Barrel
            ActorSnoEnum._x1_fortress_crystal_prison_shield,
            ActorSnoEnum._x1_westm_railing_a_01_piece1,
            ActorSnoEnum._x1_pand_hexmaze_corpse, // Corpse
            ActorSnoEnum._dh_companion_runec,
            ActorSnoEnum._loottype2_tristramvillager_male_c_corpse_01, // Dead Villager
            ActorSnoEnum._uber_bossworld3_st_demon_chainpylon_fire_azmodan, // uber realm
            ActorSnoEnum._trdun_crypt_skeleton_king_throne_parts, // uber realm
            ActorSnoEnum._double_crane_a_caout_miningevent_chest_minievent, // A2 howling plateau event
            ActorSnoEnum._p6_church_bloodchannel_a, // A2 Temple of Unborn 1
            ActorSnoEnum._a4dun_sigil_tile_invis_wall, // A4 Bounty "Watch Your Step"
            ActorSnoEnum._p1_tgoblin_gate, // Greed door
            ActorSnoEnum._p1_tgoblin_vault_door, // Vault door
            ActorSnoEnum._x1_urzael_soundspawner, // Urzael fight
            ActorSnoEnum._x1_urzael_soundspawner_02, // Urzael fight
            ActorSnoEnum._x1_urzael_soundspawner_03, // Urzael fight
            ActorSnoEnum._x1_urzael_soundspawner_04, // Urzael fight
            ActorSnoEnum._x1_westm_ex,
            ActorSnoEnum._trout_cath_entrance_door,
            ActorSnoEnum._trout_highlands_goatmen_chokepoint_gate,//南北高地连接处
            ActorSnoEnum._p6_church_ironmaiden,//初民神殿铁处女
            ActorSnoEnum._caout_cage,//A2奥斯卡那牢笼
            ActorSnoEnum._x1_fortress_portal_switch,//A5混沌要塞死亡之门传送门
            ActorSnoEnum._a1dun_leor_spike_spawner_switch,//A1苦痛刑牢尖钉陷阱
            ActorSnoEnum._a4dungarden_corruption_gate,//梦魇回响
            ActorSnoEnum._px_bounty_ramparts_camp_catapultfiring,//石垒的投石机
            ActorSnoEnum._px_bounty_ramparts_camp_catapultidle,//石垒的投石机
            ActorSnoEnum._px_bounty_ramparts_camp_switch,//石垒的投石机转盘
            ActorSnoEnum._px_bounty_camp_pinger,//石垒的投石机转盘
            ActorSnoEnum._p76_trout_oldtristram_exit_gate,//敌意幻境旧崔斯特姆
            ActorSnoEnum._trout_oldtristram_exit_gate,//敌意幻境旧崔斯特姆
            ActorSnoEnum._x1_westm_door_gate,//敌意幻境维斯特玛城
            ActorSnoEnum._p76_garden_corruption_1000monsterfight,//敌意幻境地狱裂隙
            ActorSnoEnum._p4_ruins_frost_brazier_coals_a,//长者圣殿的火盆
            ActorSnoEnum._p43_ad_a1dun_leor_gate_a,//旧崔斯特姆16层
            ActorSnoEnum._p43_ad_a1dun_leor_jail_door_superlocked_a,//旧崔斯特姆16层
            ActorSnoEnum._p43_ad_catacombs_door_a,//旧崔斯特姆6层
            ActorSnoEnum._p43_ad_catacombs_door_a_hallsoftheblind,//旧崔斯特姆7层
            ActorSnoEnum._p43_ad_a1dun_leor_gate_a_locked,//旧崔斯特姆13层
            ActorSnoEnum._p43_ad_valor_pedestal_locked,//旧崔斯特姆
            ActorSnoEnum._p43_ad_trdun_cath_wooddoor_lazarus,//旧崔斯特姆不洁祭坛
            ActorSnoEnum._p76_x1_bog_wickerman_barricade,//敌意幻象
            ActorSnoEnum._px_bounty_camp_pinger_450,//苦痛刑牢2层
            ActorSnoEnum._x1_global_chest_cursedchest,//诅咒宝箱
            ActorSnoEnum._x1_global_chest_cursedchest_b,//诅咒宝箱
            ActorSnoEnum._x1_global_chest_cursedchest_b_mutantevent,//诅咒宝箱
            ActorSnoEnum._ghosttotem,//A2悬赏任务

        };
        public OpenChestPlugin1()
        {
            Enabled = true;
            Rack = true;
            ChestNormal = true;
            Chest = true;
            DeadBody = true;
            Stone = true;
            Shrine = true;
            door = true;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            uiInv = Hud.Inventory.InventoryMainUiElement;
            _timer = Hud.Time.CreateAndStartWatch();
        }


        public void OnNewArea(bool newGame, ISnoArea area)
        {
            _clickedAnnIds.Clear();
        }
        public void AfterCollect()
        {
            if (!Hud.Game.IsInGame)
                return;
            if (Hud.Game.IsInTown)
                return;
            if (Hud.Game.IsPaused)
                return;
            if (Hud.Game.IsLoading)
                return;
            if (Hud.Game.Me.IsDead)
                return;
            if (Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.Generic_ActorGhostedBuff.Sno))
                return;
            if (PublicClassPlugin.isCasting(Hud))
                return;
            if (!Hud.Window.IsForeground)
                return;
            if (!Hud.Render.MinimapUiElement.Visible)
                return;
            if (Hud.Render.IsAnyBlockingUiElementVisible)
                return;
            if (!_timer.TimerTest(IntervalMilliseconds))
                return;
            if (Clicking)
                return;
            if (uiInv.Visible)
            {
                return;
            }

            var Actors = Hud.Game.Actors.Where(x =>
            {
                if (WhenForceMoveInvalid && Hud.Interaction.IsHotKeySet(ActionKey.Move) && Hud.Interaction.IsContinuousActionStarted(ActionKey.Move))
                    return false;
                if (!x.IsOnScreen || x.NormalizedXyDistanceToMe > 10 )
                {
                    if(x.IsOnScreen && _clickedAnnIds.Any(a => a == x.AnnId))
                    {
                        _clickedAnnIds.Remove(x.AnnId);
                    }
                    
                    return false;
                }
                    
                if (_clickedAnnIds.Contains(x.AnnId))
                    return false;
                if (!x.IsDisabled && !x.IsOperated
                && ((
                (x.GizmoType == GizmoType.Chest && //所有触发哈林顿的道具
                ((Rack && (x.SnoActor.Kind == ActorKind.ArmorRack || x.SnoActor.Kind == ActorKind.WeaponRack))//防具架子武器架子
                || (DeadBody && x.SnoActor.Kind == ActorKind.DeadBody)//尸体
                || (Chest && x.SnoActor.Kind == ActorKind.Chest)//华丽宝箱
                || (ChestNormal && x.SnoActor.Kind == ActorKind.ChestNormal)//普通宝箱
                || (Stone && x.SnoActor.Kind == ActorKind.None)//石板等交互物
                && x.SnoActor.NameEnglish != "Chandelier Chain"//大教堂吊灯
                && x.SnoActor.Kind != ActorKind.CursedEvent //诅咒宝箱
                && x.SnoActor.Kind != ActorKind.QuestActivate)//任务
                ))
                || (Shrine && (Hud.Game.Me.GetSetItemCount(749637) >= 6 && Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.Barbarian_Passive_NoEscape.Sno) && x.SnoActor.Sno == ActorSnoEnum._x1_lr_shrine_invulnerable) == false //蕾寇+无处可逃不自动护盾开塔
                && (x.SnoActor.Kind == ActorKind.Shrine && x.GizmoType != GizmoType.HealingWell && x.GizmoType != GizmoType.PoolOfReflection && (Hud.Game.Me.InGreaterRiftRank == 0 || (Hud.Game.Me.InGreaterRiftRank <= ClickPylonInGRRank))
                && 
                ((Hud.Game.Players.Where(p => p.CoordinateKnown).All(p =>  ((Hud.Game.NumberOfPlayersInGame == 1 || (p.IsMe && p.Powers.BuffIsActive(Hud.Sno.SnoPowers.NemesisBracers.Sno))) ? true : !p.Powers.BuffIsActive(Hud.Sno.SnoPowers.NemesisBracers.Sno)))))))//圣坛和塔，如果是塔判断附近队友是否有开怪手，没有直接开（单人模式直接开）
                || (Pool && x.GizmoType == GizmoType.PoolOfReflection && (Hud.Game.NumberOfPlayersInGame == 1 ? true : false))//经验池，单人模式直接开，有队友的话不开
                || (door && ((x.GizmoType == GizmoType.Door || SwitchIds.Contains(x.SnoActor.Sno)) && !DoorsIdsBlackList.Contains(x.SnoActor.Sno)))//门和桥
                ))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }).OrderBy(x => x.NormalizedXyDistanceToMe);
            if (Actors.Count() <= 0) return;
            selectedActor = Actors.FirstOrDefault();
            if (selectedActor == null || uiInv.Visible)
            {
                return;
            }
            var tempX = Hud.Window.CursorX;
            var tempY = Hud.Window.CursorY;
            Clicking = true;
            try
            {
                _clickedAnnIds.Add(selectedActor.AnnId);
                for (var i = 0; i < 100; i++)
                {
                    Hud.Interaction.MouseMove(selectedActor.ScreenCoordinate.X, selectedActor.ScreenCoordinate.Y, 1, 1);
                    Hud.Interaction.MouseDown(MouseButtons.Left);
                    Hud.Interaction.MouseUp(MouseButtons.Left);
                    if (!Hud.Game.Actors.Any(x => x.AnnId == selectedActor.AnnId && !x.IsDisabled && !x.IsOperated && x.NormalizedXyDistanceToMe <= 10))
                        break;
                    Hud.Wait(5);
                }
                _timer.Restart();
            }
            finally
            {
                Clicking = false;
            }
            Hud.Interaction.MouseMove(tempX, tempY, 1, 1);
            Hud.Interaction.MouseUp(MouseButtons.Left);
        }

    }
}