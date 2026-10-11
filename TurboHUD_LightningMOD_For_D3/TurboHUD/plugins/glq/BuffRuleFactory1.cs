namespace Turbo.Plugins.glq // Credits to Jack
{
    using System.Collections.Generic;
    using Turbo.Plugins.Default;

    public class BuffRuleFactory1
    {
        public IController Hud { get; set; }

        public BuffRuleFactory1(IController hud)
        {
            Hud = hud;
        }

        public IEnumerable<BuffRule> CreateBuffRules(uint pwr)
        {
            // ############ WIZARD ACTIVE SKILLS ##############
            // ################################################
            if (pwr == Hud.Sno.SnoPowers.Wizard_ArcaneOrb.Sno){
                yield return new BuffRule(pwr) { IconIndex = 6, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = true};        // Arcane Orb (Rune 3) - this spell stores the old buff values of Rune 3!?
                yield return new BuffRule(pwr) { IconIndex = 5, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = true};        // Arcane Orb (Rune 2)
			}
            if (pwr == Hud.Sno.SnoPowers.Wizard_ArcaneTorrent.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};     // Arcane Torrent (Active) (No Rune, Rune 1, Rune 3, Rune 4, Rune 5) - not needed (UNLESS YOU PLAY MINES!)
                //yield return new BuffRule(pwr) { IconIndex = 2, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true};       // Arcane Torrent (DMG Stack) (No Rune, Rune 1, Rune 4, Rune 5) - not needed
                //yield return new BuffRule(pwr) { IconIndex = 5, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};     // Arcane Torrent (Active) (No Rune, Rune 1, Rune 4, Rune 5) - not needed
                yield return new BuffRule(pwr) { IconIndex = 8, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Arcane Torrent (DMG Stack) (No Rune, Rune 1, Rune 2, Rune 4, Rune 5) - Actual DMG Stack
                //yield return new BuffRule(pwr) { IconIndex = 9, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};     // Arcane Torrent (Active) (Rune 1) - not needed
                //yield return new BuffRule(pwr) { IconIndex = 3, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};     // Arcane Torrent (Active) (Rune 2) - not needed
			}
            if (pwr == Hud.Sno.SnoPowers.Wizard_Archon.Sno){
                //Archon SNO
                //yield return new BuffRule(134872) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};   // Archon (Transforming) - not working
                yield return new BuffRule(pwr) { IconIndex = 2, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true};         // Archon (Stacks)
                //yield return new BuffRule(pwr) { IconIndex = 4, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true};       // Archon (Stacks) - not needed
                //yield return new BuffRule(pwr) { IconIndex = 5, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true};         // Swami (Stacks)
                //yield return new BuffRule(134872) { IconIndex = 6, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};   // Swami (Active) - works is however bugged (continues even when swami is debuffed manually)
			}
            if (pwr == Hud.Sno.SnoPowers.Wizard_ArchonArcaneBlast.Sno){
                //nothing here
            }
            if (pwr == Hud.Sno.SnoPowers.Wizard_ArchonArcaneBlastCold.Sno){
                //nothing here
            }
            if (pwr == Hud.Sno.SnoPowers.Wizard_ArchonArcaneBlastFire.Sno){
                //nothing here
            }
            if (pwr == Hud.Sno.SnoPowers.Wizard_ArchonArcaneBlastLightning.Sno){
                //nothing here
            }
            if (pwr == Hud.Sno.SnoPowers.Wizard_ArchonArcaneStrike.Sno){
                //nothing here
            }
            if (pwr == Hud.Sno.SnoPowers.Wizard_ArchonArcaneStrikeCold.Sno){
                //nothing here
            }
            if (pwr == Hud.Sno.SnoPowers.Wizard_ArchonArcaneStrikeFire.Sno){
                //nothing here
            }
            if (pwr == Hud.Sno.SnoPowers.Wizard_ArchonArcaneStrikeLightning.Sno){
                //nothing here
            }
            if (pwr == Hud.Sno.SnoPowers.Wizard_ArchonCancel.Sno){
                //nothing here
            }
            if (pwr == Hud.Sno.SnoPowers.Wizard_ArchonDisintegrationWave.Sno){ // if you use Vyr and Max Arcane (= all_elements==0) the game will jump between the three Arcane Runes (IconIndex 0, 3, 1) for Channeling Disintregration Wave       
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};     // Arcane Wave (No Rune, No Vyr), (Vyr, Max Arcane) - not needed
                yield return new BuffRule(pwr) { IconIndex = 6, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Arcane Wave (No Rune, No Vyr), (Rune 2, No Vyr), (Rune 5, No Vyr), (Vyr, Max Arcane)
                //yield return new BuffRule(pwr) { IconIndex = 3, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};     // Arcane Wave (Rune 2, No Vyr), (Vyr, Max Arcane) - not needed
                //yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};     // Arcane Wave (Rune 5, No Vyr), (Vyr, Max Arcane) - not needed
			}
            if (pwr == Hud.Sno.SnoPowers.Wizard_ArchonDisintegrationWaveCold.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 2, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};      // Cold Wave (Rune 4, No Vyr), (Vyr, Max Cold) - not needed
                yield return new BuffRule(pwr) { IconIndex = 6, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};        // Cold Wave (Rune 4, No Vyr), (Vyr, Max Cold)  
			}      
            if (pwr == Hud.Sno.SnoPowers.Wizard_ArchonDisintegrationWaveFire.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 5, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};      // Fire Wave (Rune 1, No Vyr), (Vyr, Max Fire) - not needed
                yield return new BuffRule(pwr) { IconIndex = 6, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};        // Fire Wave (Rune 1, No Vyr), (Vyr, Max Fire)
			}
            if (pwr == Hud.Sno.SnoPowers.Wizard_ArchonDisintegrationWaveLightning.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 4, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};      // Lightning Wave (Rune 3, No Vyr), (Vyr, Max Light) - not needed
                yield return new BuffRule(pwr) { IconIndex = 6, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};        // Lightning Wave (Rune 3, No Vyr), (Vyr, Max Light)
			}
            if (pwr == Hud.Sno.SnoPowers.Wizard_ArchonSlowTime.Sno){                    
                yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Slow Time (Effect 1) (Rune 3, Vyr)
                //yield return new BuffRule(pwr) { IconIndex = 2, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};     // Slow Time (Effect 2) (Rune 3, Vyr) - not needed
                //yield return new BuffRule(pwr) { IconIndex = 10, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};    // Slow Time (Effect 3) (Rune 3, Vyr) - not needed
			}
            if (pwr == Hud.Sno.SnoPowers.Wizard_ArchonTeleport.Sno){
                //nothing here
            }
            if (pwr == Hud.Sno.SnoPowers.Wizard_BlackHole.Sno){
                yield return new BuffRule(pwr) { IconIndex = 8, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true};         // Black Hole (Rune 2)
                yield return new BuffRule(pwr) { IconIndex = 5, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true};         // Black Hole (Rune 5)
			}
            if (pwr == Hud.Sno.SnoPowers.Wizard_Blizzard.Sno){
                //nothing \o/
			}
            if (pwr == Hud.Sno.SnoPowers.Wizard_DiamondSkin.Sno){
                yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Diamond Skin (No Rune, Rune 1, Rune 4, Rune 5)
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Diamond Skin (Rune 2)
                yield return new BuffRule(pwr) { IconIndex = 2, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Diamond Skin (Rune 3)
			}
            if (pwr == Hud.Sno.SnoPowers.Wizard_Disintegrate.Sno){
                yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Disintegrate (Active) (No Rune, Rune 1, Rune 2, Rune 3, Rune 4, Rune 5)
                //yield return new BuffRule(pwr) { IconIndex = 2, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};      // Disintegrate (Active) (Rune 4) - not needed
                //yield return new BuffRule(pwr) { IconIndex = 10, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};     // Disintegrate (DMG Stack) (No Rune, Rune 1, Rune 2, Rune 3, Rune 4, Rune 5) - Actual DMG Stack - not needed
			}
            if (pwr == Hud.Sno.SnoPowers.Wizard_Electrocute.Sno){
                //nothing \o/
			}
            if (pwr == Hud.Sno.SnoPowers.Wizard_EnergyArmor.Sno){
                yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Energy Armor (No Rune, Rune 4)
                yield return new BuffRule(pwr) { IconIndex = 3, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Energy Armor (Rune 1)
                yield return new BuffRule(pwr) { IconIndex = 5, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Energy Armor (Rune 2)
                yield return new BuffRule(pwr) { IconIndex = 4, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Energy Armor (Rune 3)
                //yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};      // Energy Armor (Rune 5) (first rune 5 effect) - not needed
                yield return new BuffRule(pwr) { IconIndex = 2, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Energy Armor (Rune 5) (second rune 5 effect) 
			}
            if (pwr == Hud.Sno.SnoPowers.Wizard_EnergyTwister.Sno){
                yield return new BuffRule(pwr) { IconIndex = 3, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true};         // Energy Twister - Storm Chaser
			}
            if (pwr == Hud.Sno.SnoPowers.Wizard_ExplosiveBlast.Sno){ 
                yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true};         // Explosive Blast (No Rune, Rune 1, Rune 2, Rune 3, Rune 4, Rune 5) - Stacks for Wand of Woh
			}
            if (pwr == Hud.Sno.SnoPowers.Wizard_Familiar.Sno){
                yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Familiar (No Rune, Rune 1, Rune 5)
                yield return new BuffRule(pwr) { IconIndex = 2, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Familiar (Rune 2) (first effect)
                yield return new BuffRule(pwr) { IconIndex = 3, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Familiar (Rune 2) (second effect)
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Familiar (Rune 3) 
			}
            if (pwr == Hud.Sno.SnoPowers.Wizard_FrostNova.Sno){
                yield return new BuffRule(pwr) { IconIndex = 5, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Frost Nova - Deep Freeze
			}
            if (pwr == Hud.Sno.SnoPowers.Wizard_Hydra.Sno){
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true};         // Hydra (No Rune, Rune 1, Rune 2, Rune 3, Rune 4, Rune 5)
			}
            if (pwr == Hud.Sno.SnoPowers.Wizard_IceArmor.Sno){
                yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Ice Armor (No Rune, Rune 1, Rune 3, Rune 4, Rune 5) - Base Effect
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Ice Armor (Rune 1)
                yield return new BuffRule(pwr) { IconIndex = 3, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true};         // Ice Armor (Rune 2) 
                //yield return new BuffRule(pwr) { IconIndex = 2, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};      // Ice Armor (Rune 5)  - not needed
			}
            if (pwr == Hud.Sno.SnoPowers.Wizard_MagicMissile.Sno){
                //nothing \o/
			}
            if (pwr == Hud.Sno.SnoPowers.Wizard_MagicWeapon.Sno){
                yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Magic Weapon (No Rune, Rune 1, Rune 2, Rune 3, Rune 4, Rune 5) (first effect)
                //yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};      // Magic Weapon (No Rune, Rune 1, Rune 2, Rune 3, Rune 4, Rune 5) (second effect) - not needed
                yield return new BuffRule(pwr) { IconIndex = 2, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true};         // Magic Weapon (Rune 5) (third effect)
                //yield return new BuffRule(pwr) { IconIndex = 3, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};      // Magic Weapon (Rune 5) (fourth effect) - not needed
			}
            if (pwr == Hud.Sno.SnoPowers.Wizard_Meteor.Sno){
                //nothing \o/
			}
            if (pwr == Hud.Sno.SnoPowers.Wizard_MirrorImage.Sno){
                yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Mirror Image (Rune 0, Rune 1, Rune 2, Rune 3, Rune 4, Rune 5)
			}
            if (pwr == Hud.Sno.SnoPowers.Wizard_RayOfFrost.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};      // Ray of Frost (Active) (No Rune, Rune 1, Rune 2, Rune 3, Rune 5) - not needed
                //yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};      // Ray of Frost (Active) (Rune 4) - not needed
                yield return new BuffRule(pwr) { IconIndex = 6, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true};         // Ray of Frost (DMG Stack) (No Rune, Rune 1, Rune 2, Rune 3, Rune 4, Rune 5)
			}
            if (pwr == Hud.Sno.SnoPowers.Wizard_ShockPulse.Sno){
                //nothing \o/
			}
            if (pwr == Hud.Sno.SnoPowers.Wizard_SlowTime.Sno){
                yield return new BuffRule(pwr) { IconIndex = 2, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Slow Time (No Rune, Rune 1, Rune 2, Rune 3, Rune 4, Rune 5)
                yield return new BuffRule(pwr) { IconIndex = 3, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Slow Time (Rune 5) - (Player under Slow Time Effect)
			}
            if (pwr == Hud.Sno.SnoPowers.Wizard_SpectralBlade.Sno){
                yield return new BuffRule(pwr) { IconIndex = 4, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true};         // Spectral Blade (Rune 1)
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true};         // Spectral Blade (Rune 4)
			}
            if (pwr == Hud.Sno.SnoPowers.Wizard_StormArmor.Sno){
                yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Storm Armor (No Rune, Rune 1, Rune 3, Rune 4, Rune 5)
                yield return new BuffRule(pwr) { IconIndex = 3, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Storm Armor (Rune 2)
                yield return new BuffRule(pwr) { IconIndex = 2, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Storm Armor (Rune 4) - Speed Effect
			}
            if (pwr == Hud.Sno.SnoPowers.Wizard_Teleport.Sno){
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Teleport (Rune 1)
                yield return new BuffRule(pwr) { IconIndex = 2, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Teleport (Rune 2)
                yield return new BuffRule(pwr) { IconIndex = 4, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Teleport (Rune 3)
                yield return new BuffRule(pwr) { IconIndex = 3, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Teleport (Rune 4) (second rune 4 effect)
                //yield return new BuffRule(pwr) { IconIndex = 6, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};      // Teleport (Rune 4) (first rune 4 effect) - not needed
			}
            if (pwr == Hud.Sno.SnoPowers.Wizard_WaveOfForce.Sno){
                yield return new BuffRule(pwr) { IconIndex = 2, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = true};        // Wafe of Force - Arcane Attunment
			}
            // ############ WIZARD PASSIVE SKILLS ##############
            // ################################################
            if (pwr == Hud.Sno.SnoPowers.Wizard_Passive_ArcaneDynamo.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Wizard_Passive_ArcaneDynamo (Active)
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = true};        // Wizard_Passive_ArcaneDynamo (Stacks)
			}
            if (pwr == Hud.Sno.SnoPowers.Wizard_Passive_AstralPresence.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Passive (equipped)
			}
            if (pwr == Hud.Sno.SnoPowers.Wizard_Passive_Audacity.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Passive (equipped)
			}
            if (pwr == Hud.Sno.SnoPowers.Wizard_Passive_Blur.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Passive (equipped)
			}
            if (pwr == Hud.Sno.SnoPowers.Wizard_Passive_ColdBlooded.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Passive (equipped)
			}
            if (pwr == Hud.Sno.SnoPowers.Wizard_Passive_Conflagration.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Passive (equipped)
			}
            if (pwr == Hud.Sno.SnoPowers.Wizard_Passive_Dominance.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // x1_Wizard_Passive_ArcaneAegis (Active) (a.k.a Dominance)
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true};         // x1_Wizard_Passive_ArcaneAegis (Stacks) (a.k.a Dominance)
			}
            if (pwr == Hud.Sno.SnoPowers.Wizard_Passive_ElementalExposure.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Passive (equipped)
			}
            if (pwr == Hud.Sno.SnoPowers.Wizard_Passive_Evocation.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Passive (equipped)
			}
            if (pwr == Hud.Sno.SnoPowers.Wizard_Passive_GalvanizingWard.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Wizard_Passive_GalvanizingWard (Active)
                yield return new BuffRule(pwr) { IconIndex = 2, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Wizard_Passive_GalvanizingWard (Shield Up)
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Wizard_Passive_GalvanizingWard (Shield Cooldown)
			}
            if (pwr == Hud.Sno.SnoPowers.Wizard_Passive_GlassCannon.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Passive (equipped)
			}
            if (pwr == Hud.Sno.SnoPowers.Wizard_Passive_Illusionist.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Wizard_Passive_Illusionist (Active)
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Wizard_Passive_Illusionist (Speedbuff)
			}
            if (pwr == Hud.Sno.SnoPowers.Wizard_Passive_Paralysis.Sno){                
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};     // Wizard_Passive_Paralysis (Active)
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Wizard_Passive_Paralysis (Proc)
			}
            if (pwr == Hud.Sno.SnoPowers.Wizard_Passive_PowerHungry.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Passive (equipped)
			}
            if (pwr == Hud.Sno.SnoPowers.Wizard_Passive_Prodigy.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Passive (equipped)
			}
            if (pwr == Hud.Sno.SnoPowers.Wizard_Passive_TemporalFlux.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Passive (equipped)
			}
            if (pwr == Hud.Sno.SnoPowers.Wizard_Passive_UnstableAnomaly.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Wizard_Passive_UnstableAnomaly (Active)
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Wizard_Passive_UnstableAnomaly (Cooldown)
				yield return new BuffRule(pwr) { IconIndex = 2, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Wizard_Passive_UnstableAnomaly (Cooldown)
			}
            if (pwr == Hud.Sno.SnoPowers.Wizard_Passive_UnwaveringWill.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // X1_Wizard_Passive_UnwaveringWill (Active)
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // X1_Wizard_Passive_UnwaveringWill (Buff)
			}
            // ############ MONK ACTIVE SKILLS ##############
            // ##############################################
            if (pwr == Hud.Sno.SnoPowers.Monk_BlindingFlash.Sno){ 
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};       // Rune 5
			}
            if (pwr == Hud.Sno.SnoPowers.Monk_BreathOfHeaven.Sno){
                yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};       // Rune 3
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};       // Rune 4
                yield return new BuffRule(pwr) { IconIndex = 2, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};       // Rune 5
			}
            if (pwr == Hud.Sno.SnoPowers.Monk_CripplingWave.Sno){ 
                //nothing here
			}
            if (pwr == Hud.Sno.SnoPowers.Monk_CycloneStrike.Sno){ 
                //nothing here
			}
            if (pwr == Hud.Sno.SnoPowers.Monk_DashingStrike.Sno){ 
                yield return new BuffRule(pwr) { IconIndex = 3, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Rune 1 (MS)
                yield return new BuffRule(pwr) { IconIndex = 2, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Rune 2 (Dodge)
                yield return new BuffRule(pwr) { IconIndex = 4, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Rune 4 (IAS)
			}
            if (pwr == Hud.Sno.SnoPowers.Monk_DeadlyReach.Sno){ 
                yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = true};        // Rune 4
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Rune 5    
			}
            if (pwr == Hud.Sno.SnoPowers.Monk_Epiphany.Sno){ 
                yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Rune 0, Rune 1, Rune 2, Rune 3, Rune 4, Rune 5 (base effect dash)
                //yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};      // Rune 0, Rune 1, Rune 2, Rune 3, Rune 4, Rune 5 (base effect regen) - not needed
                //yield return new BuffRule(pwr) { IconIndex = 6, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};      // Rune 1 - not needed
                //yield return new BuffRule(pwr) { IconIndex = 8, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};      // Rune 2 - not needed
                //yield return new BuffRule(pwr) { IconIndex = 12, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};       // Rune 3 (Heal Instance)
                //yield return new BuffRule(pwr) { IconIndex = 4, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};      // Rune 5 - not needed
			}
            if (pwr == Hud.Sno.SnoPowers.Monk_ExplodingPalm.Sno){ 
                //nothing here
			}
            if (pwr == Hud.Sno.SnoPowers.Monk_FistsOfThunder.Sno){ 
                //yield return new BuffRule(pwr) { IconIndex = 7, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};     // Rune 0, Rune 1, Rune 2, Rune 3, Rune 4, Rune 5 (Third Hit) - not needed
			}
            if (pwr == Hud.Sno.SnoPowers.Monk_InnerSanctuary.Sno){ 
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Rune 0, Rune 1, Rune 2, Rune 3, Rune 4, Rune 5
                //yield return new BuffRule(pwr) { IconIndex = 3, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};     // Rune 2 (LPS) - not needed
                yield return new BuffRule(pwr) { IconIndex = 6, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Rune 4 (Shield)
			}
            if (pwr == Hud.Sno.SnoPowers.Monk_LashingTailKick.Sno){ 
                //nothing here
			}
            if (pwr == Hud.Sno.SnoPowers.Monk_MantraOfConviction.Sno){
                foreach(BuffRule b in CreateBuffRules(Hud.Sno.SnoPowers.Monk_Passive_MantraOfConvictionV2.Sno))
                    yield return b;
            }
            if (pwr == Hud.Sno.SnoPowers.Monk_Passive_MantraOfConvictionV2.Sno){ 
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};     // Rune 0, Rune 1, Rune 2, Rune 3, Rune 4, Rune 5 (Equipped) - not needed
                yield return new BuffRule(pwr) { IconIndex = 2, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Rune 0, Rune 1, Rune 2, Rune 3, Rune 4, Rune 5 (Active)
                //yield return new BuffRule(pwr) { IconIndex = 3, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};     // Rune 0, Rune 1, Rune 2, Rune 3, Rune 4, Rune 5 
                //yield return new BuffRule(pwr) { IconIndex = 5, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};     // Rune 0, Rune 1, Rune 2, Rune 3, Rune 4, Rune 5
			}
            if (pwr == Hud.Sno.SnoPowers.Monk_MantraOfHealing.Sno){
                foreach(BuffRule b in CreateBuffRules(Hud.Sno.SnoPowers.Monk_Passive_MantraOfHealingV2.Sno))
                    yield return b;
            }
            if (pwr == Hud.Sno.SnoPowers.Monk_Passive_MantraOfHealingV2.Sno){ 
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};     // Rune 0, Rune 1, Rune 2, Rune 3, Rune 4, Rune 5 (Base Effect)
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Rune 0, Rune 1, Rune 2, Rune 3, Rune 4, Rune 5 (Active)
                //yield return new BuffRule(pwr) { IconIndex = 5, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};     // Rune 0, Rune 1, Rune 2, Rune 3, Rune 4, Rune 5
                //yield return new BuffRule(pwr) { IconIndex = 10, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};    // Rune 0, Rune 1, Rune 2, Rune 3, Rune 4, Rune 5 (Equipped) - not needed
                //yield return new BuffRule(pwr) { IconIndex = 11, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};    // Rune 1
                //yield return new BuffRule(pwr) { IconIndex = 12, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};    // Rune 2
                //yield return new BuffRule(pwr) { IconIndex = 13, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};    // Rune 3
                //yield return new BuffRule(pwr) { IconIndex = 14, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};    // Rune 4
                //yield return new BuffRule(pwr) { IconIndex = 15, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};    // Rune 5
			}
            if (pwr == Hud.Sno.SnoPowers.Monk_MantraOfRetribution.Sno){
                foreach(BuffRule b in CreateBuffRules(Hud.Sno.SnoPowers.Monk_Passive_MantraOfRetributionV2.Sno))
                    yield return b;
            }
            if (pwr == Hud.Sno.SnoPowers.Monk_Passive_MantraOfRetributionV2.Sno){ 
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};     // Rune 0, Rune 1, Rune 2, Rune 3, Rune 4, Rune 5 (Base Effect)
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Rune 0, Rune 1, Rune 2, Rune 3, Rune 4, Rune 5 (Active)
                //yield return new BuffRule(pwr) { IconIndex = 5, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};     // Rune 0, Rune 1, Rune 2, Rune 3, Rune 4, Rune 5
                //yield return new BuffRule(pwr) { IconIndex = 10, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};    // Rune 0, Rune 1, Rune 2, Rune 3, Rune 4, Rune 5 (Equipped) - not needed
                //yield return new BuffRule(pwr) { IconIndex = 11, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};    // Rune 1
                //yield return new BuffRule(pwr) { IconIndex = 12, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};    // Rune 3
                //yield return new BuffRule(pwr) { IconIndex = 13, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};    // Rune 4
                //yield return new BuffRule(pwr) { IconIndex = 14, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};    // Rune 2
                //yield return new BuffRule(pwr) { IconIndex = 15, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};    // Rune 5
			}
            if (pwr == Hud.Sno.SnoPowers.Monk_MantraOfSalvation.Sno){
                foreach(BuffRule b in CreateBuffRules(Hud.Sno.SnoPowers.Monk_Passive_MantraOfEvasionV2.Sno))
                    yield return b;
            }
            if (pwr == Hud.Sno.SnoPowers.Monk_Passive_MantraOfEvasionV2.Sno){ 
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};     // Rune 0, Rune 1, Rune 2, Rune 3, Rune 4, Rune 5 (Base Effect)
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Rune 0, Rune 1, Rune 2, Rune 3, Rune 4, Rune 5 (Active)
                //yield return new BuffRule(pwr) { IconIndex = 5, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};     // Rune 0, Rune 1, Rune 2, Rune 3, Rune 4, Rune 5
                yield return new BuffRule(pwr) { IconIndex = 8, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Rune 2 (proc downtime)
                //yield return new BuffRule(pwr) { IconIndex = 10, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};    // Rune 0, Rune 1, Rune 2, Rune 3, Rune 4, Rune 5 (Equipped) - not needed
                //yield return new BuffRule(pwr) { IconIndex = 11, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};    // Rune 5
                //yield return new BuffRule(pwr) { IconIndex = 12, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};    // Rune 4
                //yield return new BuffRule(pwr) { IconIndex = 13, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};    // Rune 1
                //yield return new BuffRule(pwr) { IconIndex = 14, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};    // Rune 3
                //yield return new BuffRule(pwr) { IconIndex = 15, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};    // Rune 2
			}
     
            if (pwr == Hud.Sno.SnoPowers.Monk_MysticAlly.Sno){
                //yield return new BuffRule(362118) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};  // Rune 0, Rune 1, Rune 2, Rune 3, Rune 4 (Ally)
                //yield return new BuffRule(362118) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};   // Rune 4 (on sacrifice) (Ally)
                //yield return new BuffRule(362118) { IconIndex = 5, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};   // Rune 4 (on sacrifice) (Ally)
			}
            if (pwr == Hud.Sno.SnoPowers.Monk_Serenity.Sno){ 
                yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Rune 0, Rune 1, Rune 2, Rune 3, Rune 4, Rune 5 
			}
            if (pwr == Hud.Sno.SnoPowers.Monk_SevenSidedStrike.Sno){ 
                //nothing here
			}
            if (pwr == Hud.Sno.SnoPowers.Monk_SweepingWind.Sno){ 
                yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true};         // Rune 0, Rune 1, Rune 2, Rune 3, Rune 4, Rune 5 (Base Effect)
                yield return new BuffRule(pwr) { IconIndex = 6, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Rune 2 (Spirit Regen)
			}
            if (pwr == Hud.Sno.SnoPowers.Monk_TempestRush.Sno){  
                yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Rune 0, Rune 1, Rune 2. Rune 3, Rune 4, Rune 5 (Active)
                //yield return new BuffRule(pwr) { IconIndex = 2, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};     // Rune 4 - not needed
                yield return new BuffRule(pwr) { IconIndex = 3, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = true};        // Rune 3 (Stacks)
                yield return new BuffRule(pwr) { IconIndex = 4, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Rune 0, Rune 1, Rune 2, Rune 3, Rune 4, Rune 5 (DMG Tick)
			}
            if (pwr == Hud.Sno.SnoPowers.Monk_WaveOfLight.Sno){ 
                //nothing here
			}
            if (pwr == Hud.Sno.SnoPowers.Monk_WayOfTheHundredFists.Sno){ 
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};     // Rune 0, Rune 1, Rune 2, Rune 3, Rune 4, Rune 5 (Third Hit) - not needed
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true};         // Rune 2
                //yield return new BuffRule(pwr) { IconIndex = 2, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};      // Rune 2 - not needed
                yield return new BuffRule(pwr) { IconIndex = 4, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true};         // Rune 4
			}
            // ############ MONK PASSIVE SKILLS ##############
            // ##############################################
            if (pwr == Hud.Sno.SnoPowers.Monk_Passive_Alacrity.Sno){ 
                //nothing here
			}
            if (pwr == Hud.Sno.SnoPowers.Monk_Passive_BeaconOfYtar.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Passive (equipped) 
			}
            if (pwr == Hud.Sno.SnoPowers.Monk_Passive_ChantOfResonance.Sno){ 
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Passive (equipped) 
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = true};        // Passive (stacking??) 
			}
            if (pwr == Hud.Sno.SnoPowers.Monk_Passive_CombinationStrike.Sno){ 
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Passive (equipped) 
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Passive (Fist of Thunder Stack) 
                yield return new BuffRule(pwr) { IconIndex = 2, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Passive (Deadly Reach Stack) 
                yield return new BuffRule(pwr) { IconIndex = 3, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Passive (WotHF Stack) 
                yield return new BuffRule(pwr) { IconIndex = 4, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Passive (CripplingWave Stack) 
			}
            if (pwr == Hud.Sno.SnoPowers.Monk_Passive_Determination.Sno){ //determination?
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Passive (equipped) 
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = true};        // Passive (uptime) 
            }   
            if (pwr == Hud.Sno.SnoPowers.Monk_Passive_ExaltedSoul.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Passive (equipped) 
            } 
            if (pwr == Hud.Sno.SnoPowers.Monk_Passive_FleetFooted.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Passive (equipped) 
			}
            if (pwr == Hud.Sno.SnoPowers.Monk_Passive_Harmony.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Passive (equipped) 
            }               
            if (pwr == Hud.Sno.SnoPowers.Monk_Passive_Momentum.Sno){ 
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Passive (equipped) 
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Passive (uptime) 
			}
            if (pwr == Hud.Sno.SnoPowers.Monk_Passive_MythicRhythm.Sno){ 
                //yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Passive (equipped) 
                yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Passive (up) 
                //yield return new BuffRule(pwr) { IconIndex = 2, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Passive (used) 
			}
            if (pwr == Hud.Sno.SnoPowers.Monk_Passive_NearDeathExperience.Sno){ 
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Passive (equipped) 
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Passive (downtime) 
				yield return new BuffRule(pwr) { IconIndex = 3, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Passive (downtime) 
			}
            if (pwr == Hud.Sno.SnoPowers.Monk_Passive_RelentlessAssault.Sno){ 
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Passive (equipped) 
			}
            if (pwr == Hud.Sno.SnoPowers.Monk_Passive_Resolve.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Passive (equipped) 
			}
            if (pwr == Hud.Sno.SnoPowers.Monk_Passive_SeizeTheInitiative.Sno){ 
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Passive (equipped) 
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Passive (uptime) 
			}
            if (pwr == Hud.Sno.SnoPowers.Monk_Passive_SixthSense.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Passive (equipped) 
			}
            if (pwr == Hud.Sno.SnoPowers.Monk_Passive_TheGuardiansPath.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Passive (equipped) 
			}
            if (pwr == Hud.Sno.SnoPowers.Monk_Passive_Transcendence.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Passive (equipped) 
			}
            if (pwr == Hud.Sno.SnoPowers.Monk_Passive_Unity.Sno){ 
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Passive (equipped) 
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Passive (Group-DPS) 
                yield return new BuffRule(pwr) { IconIndex = 2, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = true};        // Passive (Self-DPS) 
			}
            // ############ BARBARIAN ACITVE SKILLS ##############
            // ###################################################
            if (pwr == Hud.Sno.SnoPowers.Barbarian_AncientSpear.Sno){
                //nothing here
			}
            if (pwr == Hud.Sno.SnoPowers.Barbarian_Avalanche.Sno){ 
                //nothing here
			}
            if (pwr == Hud.Sno.SnoPowers.Barbarian_Bash.Sno){ 
                yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true};         // Rune 3
			}
            if (pwr == Hud.Sno.SnoPowers.Barbarian_BattleRage.Sno){ 
                yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Rune 0, Rune 1, Rune 2, Rune 3, Rune 4, Rune 5
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Rune 4
			}
            if (pwr == Hud.Sno.SnoPowers.Barbarian_CallOfTheAncients.Sno){ 
                yield return new BuffRule(pwr) { IconIndex = 4, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Rune 0, Rune 1, Rune 2, Rune 3, Rune 4, Rune 5 (Active)
                yield return new BuffRule(pwr) { IconIndex = 6, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Rune 5 (DMG Reduction)
			}
            if (pwr == Hud.Sno.SnoPowers.Barbarian_Cleave.Sno){ 
                //nothing here
			}
            if (pwr == Hud.Sno.SnoPowers.Barbarian_Earthquake.Sno){ 
                //nothing here
			}
            if (pwr == Hud.Sno.SnoPowers.Barbarian_Frenzy.Sno){ 
                yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true};         // Rune 0
			}
            if (pwr == Hud.Sno.SnoPowers.Barbarian_FuriousCharge.Sno){ 
                yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Rune 0, Rune 1, Rune 2, Rune 3, Rune 4, Rune 5 (Active)
			}
            if (pwr == Hud.Sno.SnoPowers.Barbarian_GroundStomp.Sno){ 
                //nothing here
			}
            if (pwr == Hud.Sno.SnoPowers.Barbarian_HammerOfTheAncients.Sno){ 
                yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Rune 4
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Rune 5
			}
            if (pwr == Hud.Sno.SnoPowers.Barbarian_IgnorePain.Sno){ 
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Rune 0, Rune 1, Rune 2, Rune 3, Rune 4, Rune 5
                yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Rune 4 (group)
			}
            if (pwr == Hud.Sno.SnoPowers.Barbarian_Leap.Sno){ 
                yield return new BuffRule(pwr) { IconIndex = 2, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Rune 1
			}
            if (pwr == Hud.Sno.SnoPowers.Barbarian_Overpower.Sno){ 
                //yield return new BuffRule(pwr) { IconIndex = 2, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};      // Rune 0, Rune 1, Rune 2, Rune 3, Rune 4, Rune 5 - not needed
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Rune 2
                yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Rune 3
			}
            if (pwr == Hud.Sno.SnoPowers.Barbarian_Rend.Sno){ 
                //nothing here
			}
            if (pwr == Hud.Sno.SnoPowers.Barbarian_Revenge.Sno){ 
                yield return new BuffRule(pwr) { IconIndex = 3, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Rune 0, Rune 1, Rune 2, Rune 3, Rune 4, Rune 5 (what base effect?)
                yield return new BuffRule(pwr) { IconIndex = 2, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Rune 2
			}
            if (pwr == Hud.Sno.SnoPowers.Barbarian_SeismicSlam.Sno){ 
                //nothing here
			}
            if (pwr == Hud.Sno.SnoPowers.Barbarian_Sprint.Sno){
                yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Rune 0, Rune 2, Rune 3, Rune 4, Rune 5
                //yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};      // Rune 2 - not needed
                yield return new BuffRule(pwr) { IconIndex = 4, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Rune 1
                yield return new BuffRule(pwr) { IconIndex = 3, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Rune 5 (group)
			}
            if (pwr == Hud.Sno.SnoPowers.Barbarian_ThreateningShout.Sno){ 
                //nothing here
			}
            if (pwr == Hud.Sno.SnoPowers.Barbarian_WarCry.Sno){
                yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Rune 0, Rune 1, Rune 2, Rune 3, Rune 4, Rune 5 (Active) 
                //yield return new BuffRule(pwr) { IconIndex = 6, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};      // Rune 0, Rune 1, Rune 2, Rune 3, Rune 4, Rune 5 (6sec Buff) - not needed
                //yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};      // Rune 1 - not working
                //yield return new BuffRule(pwr) { IconIndex = 5, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};      // Rune 3 - not working
                //yield return new BuffRule(pwr) { IconIndex = 2, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};      // Rune 4 - not working
                //yield return new BuffRule(pwr) { IconIndex = 3, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};      // Rune 5 - not working
                //yield return new BuffRule(318821) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};   // War Cry Belt - not working
			}
            if (pwr == Hud.Sno.SnoPowers.Barbarian_WeaponThrow.Sno){ 
                //nothing here
			}
            if (pwr == Hud.Sno.SnoPowers.Barbarian_Whirlwind.Sno){ 
                yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Rune 0, Rune 1, Rune 2, Rune 3, Rune 4, Rune 5 
                //yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};     // Rune 0, Rune 1, Rune 2, Rune 3, Rune 4, Rune 5 - not needed
                //yield return new BuffRule(pwr) { IconIndex = 3, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};      // Rune 1 - not needed
			}
            if (pwr == Hud.Sno.SnoPowers.Barbarian_WrathOfTheBerserker.Sno){ 
                yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Rune 0, Rune 1, Rune 2, Rune 3, Rune 4, Rune 5
			}
            // ############ BARBARIAN PASSIVE SKILLS ##############
            // ####################################################
            if (pwr == Hud.Sno.SnoPowers.Barbarian_Passive_Animosity.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Passive (equipped) 
			}
            if (pwr == Hud.Sno.SnoPowers.Barbarian_Passive_BerserkerRage.Sno){ 
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Passive (equipped) 
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Active 
			}
            if (pwr == Hud.Sno.SnoPowers.Barbarian_Passive_Bloodthirst.Sno){ 
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Passive (equipped) 
			}
            if (pwr == Hud.Sno.SnoPowers.Barbarian_Passive_BoonOfBulKathos.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Passive (equipped) 
			}
            if (pwr == Hud.Sno.SnoPowers.Barbarian_Passive_Brawler.Sno){  
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Passive (equipped) 
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Active 
			}
            if (pwr == Hud.Sno.SnoPowers.Barbarian_Passive_EarthenMight.Sno){ 
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Passive (equipped) 
			}
            if (pwr == Hud.Sno.SnoPowers.Barbarian_Passive_InspiringPresence.Sno){ 
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Passive (equipped) 
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Duration
                //yield return new BuffRule(pwr) { IconIndex = 2, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};      // Heal Instance
			}
            if (pwr == Hud.Sno.SnoPowers.Barbarian_Passive_Juggernaut.Sno){ 
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Passive (equipped) 
			}
            if (pwr == Hud.Sno.SnoPowers.Barbarian_Passive_NervesOfSteel.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};        // Passive (equipped) 
                yield return new BuffRule(pwr) { IconIndex = 2, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Cooldown
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Cooldown
			}
            if (pwr == Hud.Sno.SnoPowers.Barbarian_Passive_NoEscape.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Passive (equipped) 
			}
            if (pwr == Hud.Sno.SnoPowers.Barbarian_Passive_PoundOfFlesh.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Passive (equipped) 
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true};         // Stacks
			}
            if (pwr == Hud.Sno.SnoPowers.Barbarian_Passive_Rampage.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Passive (equipped) 
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true};         // Stacks
			}
            if (pwr == Hud.Sno.SnoPowers.Barbarian_Passive_Relentless.Sno){ 
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Passive (equipped) 
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Active 
            }
            if (pwr == Hud.Sno.SnoPowers.Barbarian_Passive_Ruthless.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Passive (equipped) 
			}
            if (pwr == Hud.Sno.SnoPowers.Barbarian_Passive_Superstition.Sno){ 
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Passive (equipped) 
			}
            if (pwr == Hud.Sno.SnoPowers.Barbarian_Passive_SwordAndBoard.Sno){ 
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Passive (equipped) 
			}
            if (pwr == Hud.Sno.SnoPowers.Barbarian_Passive_ToughAsNails.Sno){ 
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Passive (equipped) 
			}
            if (pwr == Hud.Sno.SnoPowers.Barbarian_Passive_Unforgiving.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Passive (equipped) 
			}
            if (pwr == Hud.Sno.SnoPowers.Barbarian_Passive_WeaponsMaster.Sno){ 
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Passive (equipped) 
			}
            // ############ WITCHDOCTOR ACTIVE SKILLS ##############
            // #####################################################
            if (pwr == Hud.Sno.SnoPowers.WitchDoctor_AcidCloud.Sno){ 
                //nothing here
			}
            if (pwr == Hud.Sno.SnoPowers.WitchDoctor_BigBadVoodoo.Sno){
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Rune 0, Rune 1, Rune 5 (Active) 
                yield return new BuffRule(pwr) { IconIndex = 2, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Rune 3 (Active)
                yield return new BuffRule(pwr) { IconIndex = 3, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Rune 2 (Active)
                yield return new BuffRule(pwr) { IconIndex = 4, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Rune 0, Rune 1, Rune 2, Rune 3, Rune 4, Rune 5 (Duration)
                yield return new BuffRule(pwr) { IconIndex = 5, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Rune 4 (Active)
			}
            if (pwr == Hud.Sno.SnoPowers.WitchDoctor_CorpseSpider.Sno){
                //nothing here
            }
            if (pwr == Hud.Sno.SnoPowers.WitchDoctor_FetishArmy.Sno){     
                //nothing here
            }
            if (pwr == Hud.Sno.SnoPowers.WitchDoctor_Firebats.Sno){     
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};     // Rune 0, Rune 1, Rune 2, Rune 3, Rune 4, Rune 5 (Channel)
                yield return new BuffRule(pwr) { IconIndex = 4, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = true};        // Rune 5 (DMG inc)
            }
            if (pwr == Hud.Sno.SnoPowers.WitchDoctor_Firebomb.Sno){
                //nothing here
            }
            if (pwr == Hud.Sno.SnoPowers.WitchDoctor_Gargantuan.Sno){
                yield return new BuffRule(pwr) { IconIndex = 4, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Rune 3
            }
            if (pwr == Hud.Sno.SnoPowers.WitchDoctor_GraspOfTheDead.Sno){
                yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Rune 5
            }
            if (pwr == Hud.Sno.SnoPowers.WitchDoctor_Haunt.Sno){
                yield return new BuffRule(pwr) { IconIndex = 5, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Rune 4
            }
            if (pwr == Hud.Sno.SnoPowers.WitchDoctor_Hex.Sno){ 
                //yield return new BuffRule(pwr) { IconIndex = 5, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Hex (4PC Arachyr DMG Red)
                yield return new BuffRule(pwr) { IconIndex = 2, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};      // Hex (4PC Arachyr %Life) - not needed
			}
            if (pwr == Hud.Sno.SnoPowers.WitchDoctor_Horrify.Sno){
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Rune 2
                yield return new BuffRule(pwr) { IconIndex = 2, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Rune 4
            }
            if (pwr == Hud.Sno.SnoPowers.WitchDoctor_LocustSwarm.Sno){
                yield return new BuffRule(pwr) { IconIndex = 5, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Rune 2
            }
            if (pwr == Hud.Sno.SnoPowers.WitchDoctor_MassConfusion.Sno){
                //nothing here
            }
            if (pwr == Hud.Sno.SnoPowers.WitchDoctor_Piranhas.Sno){
                //nothing here
            }
            if (pwr == Hud.Sno.SnoPowers.WitchDoctor_PlagueOfToads.Sno){
                //nothing here
            }
            if (pwr == Hud.Sno.SnoPowers.WitchDoctor_PoisonDart.Sno){
                //nothing here
            }
            if (pwr == Hud.Sno.SnoPowers.WitchDoctor_Sacrifice.Sno){
                yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true};         // Rune 5
            }
            if (pwr == Hud.Sno.SnoPowers.WitchDoctor_SoulHarvest.Sno){
                yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true};         // Rune 0
                //yield return new BuffRule(pwr) { IconIndex = 2, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true};
                yield return new BuffRule(pwr) { IconIndex = 5, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true}; 
       // Rune 1 - not needed
            }
            if (pwr == Hud.Sno.SnoPowers.WitchDoctor_SpiritBarrage.Sno){
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true};         // Rune 2
                yield return new BuffRule(pwr) { IconIndex = 3, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Rune 5
            }
            if (pwr == Hud.Sno.SnoPowers.WitchDoctor_SpiritWalk.Sno){
                yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Rune 0, Rune 1, Rune 2, Rune 3, Rune 4, Rune 5
            }
            if (pwr == Hud.Sno.SnoPowers.WitchDoctor_SummonZombieDog.Sno){
                //nothing here
            }
            if (pwr == Hud.Sno.SnoPowers.WitchDoctor_WallOfDeath.Sno){//Wall of Death
                //nothing here
            }
            if (pwr == Hud.Sno.SnoPowers.WitchDoctor_ZombieCharger.Sno){}//??
            // ############ WITCHDOCTOR PASSIVE SKILLS ##############
            // ######################################################
            if (pwr == Hud.Sno.SnoPowers.WitchDoctor_Passive_BadMedicine.Sno){ //debuff icon on IconIndex = 1
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Passive (equipped)            
			}
            if (pwr == Hud.Sno.SnoPowers.WitchDoctor_Passive_BloodRitual.Sno){ 
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Passive (equipped)            
			}
            if (pwr == Hud.Sno.SnoPowers.WitchDoctor_Passive_CircleOfLife.Sno){ 
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Passive (equipped)            
			}
            if (pwr == Hud.Sno.SnoPowers.WitchDoctor_Passive_ConfidenceRitual.Sno){ 
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Passive (equipped)            
			}
            if (pwr == Hud.Sno.SnoPowers.WitchDoctor_Passive_CreepingDeath.Sno){ 
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Passive (equipped)            
			}
            if (pwr == Hud.Sno.SnoPowers.WitchDoctor_Passive_FetishSycophants.Sno){ //has no stacks?
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Passive (equipped)            
			}
            if (pwr == Hud.Sno.SnoPowers.WitchDoctor_Passive_FierceLoyalty.Sno){ 
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Passive (equipped)
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Passive (speed)
                //yield return new BuffRule(pwr) { IconIndex = 2, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};      // Passive (additional dog?) - not needed
			}
            if (pwr == Hud.Sno.SnoPowers.WitchDoctor_Passive_GraveInjustice.Sno){ 
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Passive (equipped)
                //yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true};       // Passive (debuff?) - not needed
			}
            if (pwr == Hud.Sno.SnoPowers.WitchDoctor_Passive_GruesomeFeast.Sno){ 
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true};         // Passive (equipped/stacks)
			}
            if (pwr == Hud.Sno.SnoPowers.WitchDoctor_Passive_JungleFortitude.Sno){ 
                //yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 0, ShowTimeLeft = false, ShowStacks = false};       // Passive (equipped)
                //yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true};       // Passive (Pets?) - not working 
			}
            if (pwr == Hud.Sno.SnoPowers.WitchDoctor_Passive_MidnightFeast.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Passive (equipped)            
			}
            if (pwr == Hud.Sno.SnoPowers.WitchDoctor_Passive_PierceTheVeil.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Passive (equipped)            
			}
            if (pwr == Hud.Sno.SnoPowers.WitchDoctor_Passive_RushOfEssence.Sno){ 
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Passive (equipped)
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1 , ShowTimeLeft = true, ShowStacks = true};        // Passive (stacks)
			}
            if (pwr == Hud.Sno.SnoPowers.WitchDoctor_Passive_SpiritualAttunement.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Passive (equipped)            
			}
            if (pwr == Hud.Sno.SnoPowers.WitchDoctor_Passive_SpiritVessel.Sno){ 
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Passive (equipped)
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Passive (cooldown)
				yield return new BuffRule(pwr) { IconIndex = 2, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Passive (cooldown)
			}
            if (pwr == Hud.Sno.SnoPowers.WitchDoctor_Passive_SwamplandAttunement.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 0, ShowTimeLeft = false, ShowStacks = false};       // Passive (equipped)
                //yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true};       // Passive (Pets?) - not working 
			}
            if (pwr == Hud.Sno.SnoPowers.WitchDoctor_Passive_TraitZombieDogSpawner.Sno){} //?????
            if (pwr == Hud.Sno.SnoPowers.WitchDoctor_Passive_TribalRites.Sno){
                //nothing here - doesn't show ?
			}
            if (pwr == Hud.Sno.SnoPowers.WitchDoctor_Passive_VisionQuest.Sno){ 
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};       // Passive (equipped)
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Passive (cooldown)
			}
            if (pwr == Hud.Sno.SnoPowers.WitchDoctor_Passive_ZombieHandler.Sno){ 
                //yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 0, ShowTimeLeft = false, ShowStacks = false};       // Passive (equipped)
                //yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true};       // Passive (Pets?) - not working 
            }
            // ############ CRUSADER ACTIVE SKILLS ##############
            // ##################################################
            if (pwr == Hud.Sno.SnoPowers.Crusader_AkaratsChampion.Sno){
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};         // Rune 0, Rune 1, Rune 2, Rune 3, Rune 4, Rune 5
                yield return new BuffRule(pwr) { IconIndex = 11, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Rune 4 (proc cooldown)
            }
            if (pwr == Hud.Sno.SnoPowers.Crusader_BlessedHammer.Sno){
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};         // Rune 1 (last hammer duration)
                yield return new BuffRule(pwr) { IconIndex = 6, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true};          // Rune 1 (fire stacks)
            }
            if (pwr == Hud.Sno.SnoPowers.Crusader_BlessedShield.Sno){
                yield return new BuffRule(pwr) { IconIndex = 7, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};         // Rune 0, Rune 1, Rune 2, Rune 3, Rune 4, Rune 5 (last shield duration)
                yield return new BuffRule(pwr) { IconIndex = 2, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true};          // RUne 3 (stacks)
            }
            if (pwr == Hud.Sno.SnoPowers.Crusader_Bombardment.Sno){
                //nothing here
            }
            if (pwr == Hud.Sno.SnoPowers.Crusader_Condemn.Sno){
                yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};         // Rune 0, Rune 1, Rune 3, Rune 4, Rune 5
            }
            if (pwr == Hud.Sno.SnoPowers.Crusader_Consecration.Sno){
                yield return new BuffRule(pwr) { IconIndex = 8, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};         // Rune 0, Rune 1, Rune 2, Rune 3, Rune 4, Rune 5
            }
            if (pwr == Hud.Sno.SnoPowers.Crusader_CrushingResolve.Sno){}//???
            if (pwr == Hud.Sno.SnoPowers.Crusader_FallingSword.Sno){
                yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};         // Rune 0, Rune 1, Rune 2, Rune 3, Rune 4, Rune 5
            }
            if (pwr == Hud.Sno.SnoPowers.Crusader_FistOfTheHeavens.Sno){
                yield return new BuffRule(pwr) { IconIndex = 2, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};         // Rune 3 (last fissure duration)
            }
            if (pwr == Hud.Sno.SnoPowers.Crusader_HeavensFury.Sno){
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};         // Rune 0, Rune 1, Rune 2, Rune 3, Rune 4
            }
            if (pwr == Hud.Sno.SnoPowers.Crusader_IronSkin.Sno){
                yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};         // Rune 0, Rune 1, Rune 2, Rune 3, Rune 4, Rune 5
                //yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};       // Rune 4 - not needed
                yield return new BuffRule(pwr) { IconIndex = 3, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};         // Rune 5
            }
            if (pwr == Hud.Sno.SnoPowers.Crusader_Judgment.Sno){
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};         // Rune 1
            }
            if (pwr == Hud.Sno.SnoPowers.Crusader_Justice.Sno){
                yield return new BuffRule(pwr) { IconIndex = 7, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};         // Rune 0
                yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true};          // Rune 4
            }
 //           if (pwr == Hud.Sno.SnoPowers.Crusader_LawsOfFate.Sno){}//???
//            Removed - Nephalem Majesty, Cut - Laws of Fate
            if (pwr == 342299){
                yield return new BuffRule(342299) { IconIndex = 6, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false };//Crusader_LawsOfHope
            }
            if (pwr == 342286)
            {
                yield return new BuffRule(342286) { IconIndex = 6, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false };//Crusader_LawsOfJustice
            }
            if (pwr == 342284)
            {
                yield return new BuffRule(342284) { IconIndex = 6, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false };//Crusader_LawsOfValor
            }
            if (pwr == Hud.Sno.SnoPowers.Crusader_Phalanx.Sno){
                yield return new BuffRule(pwr) { IconIndex = 3, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};         // Rune 1, Rune 5
            }
            if (pwr == Hud.Sno.SnoPowers.Crusader_Provoke.Sno){
                yield return new BuffRule(pwr) { IconIndex = 2, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true};          // Rune 1
                yield return new BuffRule(pwr) { IconIndex = 3, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};         // Rune 4
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};         // Rune 5
            }
            if (pwr == Hud.Sno.SnoPowers.Crusader_Punish.Sno){
                yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};         // Rune 0, Rune 1, Rune 2, Rune 3, Rune 4, Rune 5
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};         // Rune 2
                yield return new BuffRule(pwr) { IconIndex = 2, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};         // Rune 3
                yield return new BuffRule(pwr) { IconIndex = 3, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};        // Rune 5

            }
            if (pwr == Hud.Sno.SnoPowers.Crusader_ShieldBash.Sno){
                yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};        // Rune 0, Rune 1, Rune 2, Rune 3, Rune 4, Rune 5
            }
            if (pwr == Hud.Sno.SnoPowers.Crusader_ShieldGlare.Sno){
                //nothing here
            }
            if (pwr == Hud.Sno.SnoPowers.Crusader_Slash.Sno){
                yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true};          // Rune 4
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true};          // Rune 5
            }
            if (pwr == Hud.Sno.SnoPowers.Crusader_Smite.Sno){
                yield return new BuffRule(pwr) { IconIndex = 14, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Rune 0
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true};          // Rune 4
            }
            if (pwr == Hud.Sno.SnoPowers.Crusader_SteedCharge.Sno){
                yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};         // Rune 0, Rune 1, Rune 2, Rune 3, Rune 4, Rune 5 (duration)
                //yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};       // Rune 0, Rune 1, Rune 2, Rune 3, Rune 4, Rune 5 (transform?)
                //yield return new BuffRule(pwr) { IconIndex = 2, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};       // Rune 1 - not needed                
                //yield return new BuffRule(pwr) { IconIndex = 3, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};      // Rune 2 - not needed                 
                //yield return new BuffRule(pwr) { IconIndex = 5, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};      // Rune 5 - not needed      
            }
            if (pwr == Hud.Sno.SnoPowers.Crusader_SweepAttack.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};       // Rune 0, Rune 1, Rune 2, Rune 3, Rune 4, Rune 5 - not needed
                yield return new BuffRule(pwr) { IconIndex = 2, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};        // Rune 3 
                yield return new BuffRule(pwr) { IconIndex = 4, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};         // Rune 5
            }
            // ############ CRUSADER PASSIVE SKILLS ##############
            // ###################################################
            if (pwr == Hud.Sno.SnoPowers.Crusader_Passive_Blunt.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};        // Passive (equipped)
            } 
            if (pwr == Hud.Sno.SnoPowers.Crusader_Passive_DivineFortress.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};        // Passive (equipped)
                //yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};      // Passive (equipped) - not needed
            }  
            if (pwr == Hud.Sno.SnoPowers.Crusader_Passive_Fanaticism.Sno){
                //nothing here
            } 
            if (pwr == Hud.Sno.SnoPowers.Crusader_Passive_Fervor.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};        // 1h equipped + passive equipped
            } 
            if (pwr == Hud.Sno.SnoPowers.Crusader_Passive_Finery.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};        // Passive (equipped)
            } 
            if (pwr == Hud.Sno.SnoPowers.Crusader_Passive_HeavenlyStrength.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};        // Passive (equipped)
            } 
            if (pwr == Hud.Sno.SnoPowers.Crusader_Passive_HoldYourGround.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};        // Passive (equipped)
            } 
            if (pwr == Hud.Sno.SnoPowers.Crusader_Passive_HolyCause.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};        // Passive (equipped)
                //yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};      // Passive (equipped) - not needed
            } 
            if (pwr == Hud.Sno.SnoPowers.Crusader_Passive_Indestructible.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};        // Passive (equipped)
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};         // Passive (cooldown)
                yield return new BuffRule(pwr) { IconIndex = 2, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};         // Passive (DMG buff)
            } 
            if (pwr == Hud.Sno.SnoPowers.Crusader_Passive_Insurmountable.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};        // Passive (equipped)
            }  
            if (pwr == Hud.Sno.SnoPowers.Crusader_Passive_IronMaiden.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};        // Passive (equipped)
            } 
            if (pwr == Hud.Sno.SnoPowers.Crusader_Passive_LongArmOfTheLaw.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};        // Passive (equipped)
            } 
            if (pwr == Hud.Sno.SnoPowers.Crusader_Passive_LordCommander.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};        // Passive (equipped)
            } 
            if (pwr == Hud.Sno.SnoPowers.Crusader_Passive_Renewal.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};        // Passive (equipped)
            } 
            if (pwr == Hud.Sno.SnoPowers.Crusader_Passive_Righteousness.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};        // Passive (equipped)
            } 
            if (pwr == Hud.Sno.SnoPowers.Crusader_Passive_ToweringShield.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};        // Passive (equipped)
            } 
            if (pwr == Hud.Sno.SnoPowers.Crusader_Passive_Vigilant.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};        // Passive (equipped)
            } 
            if (pwr == Hud.Sno.SnoPowers.Crusader_Passive_Wrathful.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};        // Passive (equipped)
            } 
            // ############ DEMON HUNTER ACTIVE SKILLS ##############
            // ######################################################
            if (pwr == Hud.Sno.SnoPowers.DemonHunter_Bolas.Sno){
                //nothing here
            }
            if (pwr == Hud.Sno.SnoPowers.DemonHunter_Caltrops.Sno){
                yield return new BuffRule(pwr) { IconIndex = 3, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};        // Rune 5
            } 
            if (pwr == Hud.Sno.SnoPowers.DemonHunter_Chakram.Sno){
                yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};         // Rune 5
            }
            if (pwr == Hud.Sno.SnoPowers.DemonHunter_ClusterArrow.Sno){
                //nothing here
            }
            if (pwr == Hud.Sno.SnoPowers.DemonHunter_Companion.Sno){
                //passive companion is on pwr+1
                yield return new BuffRule(pwr) { IconIndex = 2, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};         // Rune 5
            }
            if (pwr == Hud.Sno.SnoPowers.DemonHunter_ElementalArrow.Sno){
                yield return new BuffRule(pwr) { IconIndex = 3, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};         // Rune 4 (what the hell is this buff???)
            }
            if (pwr == Hud.Sno.SnoPowers.DemonHunter_EntanglingShot.Sno){
                //nothing here
            }
            if (pwr == Hud.Sno.SnoPowers.DemonHunter_EvasiveFire.Sno){
                yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};         // Rune 1
            }
            if (pwr == Hud.Sno.SnoPowers.DemonHunter_FanOfKnives.Sno){
                yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};         // Rune 2
            }
            if (pwr == Hud.Sno.SnoPowers.DemonHunter_Grenades.Sno){
                //nothing here
            }
            if (pwr == Hud.Sno.SnoPowers.DemonHunter_HungeringArrow.Sno){
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};         // Rune 5 (what the hell is this buff???)
            }
            if (pwr == Hud.Sno.SnoPowers.DemonHunter_Impale.Sno){
                //nothing here
            }
            if (pwr == Hud.Sno.SnoPowers.DemonHunter_MarkedForDeath.Sno){
                yield return new BuffRule(pwr) { IconIndex = 3, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};         // Rune 0, Rune 1, Rune 2, Rune 3, Rune 4, Rune 5 (what is this - it scales with custom engineering?!?!)
            }
            if (pwr == Hud.Sno.SnoPowers.DemonHunter_Multishot.Sno){
                yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};         // Rune 5 (rockets?)
            }
            if (pwr == Hud.Sno.SnoPowers.DemonHunter_Preparation.Sno){
                //passive for rune 1
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};         // Rune 4
            }
            if (pwr == Hud.Sno.SnoPowers.DemonHunter_RainOfVengeance.Sno){
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};         // Rune 1
            }
            if (pwr == Hud.Sno.SnoPowers.DemonHunter_RapidFire.Sno){
                yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};        // Rune 0, Rune 1, Rune 2, Rune 3, Rune 4, Rune 5
                //yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};      // Rune 3 (rockets) - not needed
            }
            if (pwr == Hud.Sno.SnoPowers.DemonHunter_Sentry.Sno){
                yield return new BuffRule(pwr) { IconIndex = 8, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = true};         // Rune 0, Rune 1, Rune 2, Rune 3, Rune 4, Rune 5
                yield return new BuffRule(pwr) { IconIndex = 5, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};         // Rune 3
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};        // Rune 5
            }
            if (pwr == Hud.Sno.SnoPowers.DemonHunter_ShadowPower.Sno){
                yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};         // Rune 0, Rune 1, Rune 2, Rune 3, Rune 4, Rune 5
            }
            if (pwr == Hud.Sno.SnoPowers.DemonHunter_SmokeScreen.Sno){
                yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};         // Rune 0, Rune 1, Rune 3, Rune 4, Rune 5
                yield return new BuffRule(pwr) { IconIndex = 2, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};         // Rune 2
            }
            if (pwr == Hud.Sno.SnoPowers.DemonHunter_SpikeTrap.Sno){
                yield return new BuffRule(pwr) { IconIndex = 8, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = true};         // Rune 0, Rune 1, Rune 2, Rune 3, Rune 4, Rune 5 (stacks)
                //yield return new BuffRule(pwr) { IconIndex = 9, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};      // Rune 0, Rune 1, Rune 2, Rune 3, Rune 4, Rune 5 (active) - not needed
            }
            if (pwr == Hud.Sno.SnoPowers.DemonHunter_Strafe.Sno){
                yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};         // Rune 0, Rune 1, Rune 2, Rune 3, Rune 4, Rune 5 (active)
                //yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};      // Rune 3 - not needed
                //yield return new BuffRule(pwr) { IconIndex = 2, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};      // Rune 1 - not needed
                yield return new BuffRule(pwr) { IconIndex = 3, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true};          // Rune 1 (stacks)
            }
            if (pwr == Hud.Sno.SnoPowers.DemonHunter_Vault.Sno){
                yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};        // Rune 0, Rune 1, Rune 2, Rune 3, Rune 4, Rune 5 (active)
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};         // Rune 3
                yield return new BuffRule(pwr) { IconIndex = 3, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true};          // Rune 5
            }
            if (pwr == Hud.Sno.SnoPowers.DemonHunter_Vengeance.Sno){
                yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};         // Rune 0, Rune 1, Rune 2, Rune 3, Rune 4, Rune 5
                //yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};       // Rune 0, Rune 2, Rune 4 - not needed
                //yield return new BuffRule(pwr) { IconIndex = 2, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};       // Rune 0, Rune 1, Rune 2, Rune 3, Rune 4, Rune 5 - not needed
                //yield return new BuffRule(pwr) { IconIndex = 3, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};        // Rune 5, (rockets)
                //yield return new BuffRule(pwr) { IconIndex = 4, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};      // Rune 2 - not needed
                //yield return new BuffRule(pwr) { IconIndex = 5, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};        // Rune 1, (grenades)
                //yield return new BuffRule(pwr) { IconIndex = 6, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};       // Rune 3 - not needed
                //yield return new BuffRule(pwr) { IconIndex = 7, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};       // Rune 4 - not needed
                //yield return new BuffRule(pwr) { IconIndex = 8, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};         // Rune 0, Rune 2, Rune 4 (rockets)
            }
			// Punkk add  Шестерни комплект 2 - 4
			if (pwr == 484289) { // Gears of Dreadland / 2pc
				yield return new BuffRule(pwr) { IconIndex = 10, MinimumIconCount = 1, ShowStacks = false, ShowTimeLeft = true };
			}
			if (pwr == 484290) { // Gears of Dreadland / 4pc
				yield return new BuffRule(pwr) { IconIndex = 8, MinimumIconCount = 1, ShowStacks = false, ShowTimeLeft = true };
			}			
			
            // ############ DEMON HUNTER PASSIVE SKILLS ##############
            // #######################################################
            if (pwr == Hud.Sno.SnoPowers.DemonHunter_Passive_Ambush.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};        // Passive (equipped)
            } 
            if (pwr == Hud.Sno.SnoPowers.DemonHunter_Passive_Archery.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};        // Passive (equipped and weapon used)
            }  
            if (pwr == Hud.Sno.SnoPowers.DemonHunter_Passive_Awareness.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};        // Passive (equipped)
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};         // Passive (cooldown)
                yield return new BuffRule(pwr) { IconIndex = 2, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};         // Passive (vanish, heal)
            }  
            if (pwr == Hud.Sno.SnoPowers.DemonHunter_Passive_Ballistics.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};        // Passive (equipped)
            } 
            if (pwr == Hud.Sno.SnoPowers.DemonHunter_Passive_Brooding.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};      // Passive (equipped) - not needed
                //yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};       // Passive (cooldown) - not needed
                yield return new BuffRule(pwr) { IconIndex = 2, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true};          // Passive (stationary stacking)
                yield return new BuffRule(pwr) { IconIndex = 3, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true};          // Passive (moving debuff)
            }   
            if (pwr == Hud.Sno.SnoPowers.DemonHunter_Passive_CullTheWeak.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};        // Passive (equipped)
            }
            if (pwr == Hud.Sno.SnoPowers.DemonHunter_Passive_CustomEngineering.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};        // Passive (equipped)
            } 
            if (pwr == Hud.Sno.SnoPowers.DemonHunter_Passive_Grenadier.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};        // Passive (equipped)
            } 
            if (pwr == Hud.Sno.SnoPowers.DemonHunter_Passive_HotPursuit.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};        // Passive (equipped)
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};         // Passive (hit bonus)
            } 
            if (pwr == Hud.Sno.SnoPowers.DemonHunter_Passive_Leech.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};        // Passive (equipped)
            } 
            if (pwr == Hud.Sno.SnoPowers.DemonHunter_Passive_NightStalker.Sno){
               // yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};        // Passive (equipped)
            } 
            if (pwr == Hud.Sno.SnoPowers.DemonHunter_Passive_NumbingTraps.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};        // Passive (equipped)
            } 
            if (pwr == Hud.Sno.SnoPowers.DemonHunter_Passive_Perfectionist.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};        // Passive (equipped)
            } 
            if (pwr == Hud.Sno.SnoPowers.DemonHunter_Passive_Sharpshooter.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};      // Passive (equipped) - not needed
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = true};         // Passive (equipped)
            } 
            if (pwr == Hud.Sno.SnoPowers.DemonHunter_Passive_SingleOut.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};        // Passive (equipped)
            } 
            if (pwr == Hud.Sno.SnoPowers.DemonHunter_Passive_SteadyAim.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};      // Passive (equipped) - not needed
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};        // Passive (up)
            } 
            if (pwr == Hud.Sno.SnoPowers.DemonHunter_Passive_TacticalAdvantage.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};      // Passive (equipped) - not needed
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};         // Passive (up)
            } 
            if (pwr == Hud.Sno.SnoPowers.DemonHunter_Passive_ThrillOfTheHunt.Sno){
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};        // Passive (equipped)
            }
            // ############ Necromancer ACTIVE SKILLS ##############
            // ######################################################

            if (pwr == Hud.Sno.SnoPowers.Necromancer_GrimScythe.Sno)
            {
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true };         
            }
            if (pwr == Hud.Sno.SnoPowers.Necromancer_SiphonBlood.Sno)
            {
                yield return new BuffRule(pwr) { IconIndex = 6, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true };
            }
            if (pwr == Hud.Sno.SnoPowers.Necromancer_BoneSpear.Sno)
            {
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true };
            }
            if (pwr == Hud.Sno.SnoPowers.Necromancer_SkeletalMage.Sno)
            {
                yield return new BuffRule(pwr) { IconIndex = 3, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true };
				yield return new BuffRule(pwr) { IconIndex = 6, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true, UsePowersTexture = true};
            }
            if (pwr == Hud.Sno.SnoPowers.Necromancer_DeathNova.Sno)
            {
                yield return new BuffRule(pwr) { IconIndex = 3, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true };
            }
            if (pwr == Hud.Sno.SnoPowers.Necromancer_Devour.Sno)
            {
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true };
                yield return new BuffRule(pwr) { IconIndex = 2, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true };
            }
            if (pwr == Hud.Sno.SnoPowers.Necromancer_LandOfTheDead.Sno)
            {
                yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false };
            }
            if (pwr == 471738)//衰老
            {
                yield return new BuffRule(pwr) { IconIndex = 5, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = true };
                yield return new BuffRule(pwr) { IconIndex = 6, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = true };
            }
            if (pwr == 471869)//脆弱
            {
                yield return new BuffRule(pwr) { IconIndex = 3, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = true };
                yield return new BuffRule(pwr) { IconIndex = 6, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = true };
            }
			if (pwr == 465350)//血魂双分
            {
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true };
            }
            if (pwr == Hud.Sno.SnoPowers.Necromancer_BoneArmor.Sno)
            {
                yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true };
                yield return new BuffRule(pwr) { IconIndex = 6, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true };
            }
            if (pwr == Hud.Sno.SnoPowers.Necromancer_BloodRush.Sno)
            {
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false };
            }

            // ############ Necromancer PASSIVE SKILLS ##############
            // #######################################################

            if (pwr == Hud.Sno.SnoPowers.Necromancer_Passive_FueledByDeath.Sno)
            {
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true };
            }
            if (pwr == Hud.Sno.SnoPowers.Necromancer_Passive_FinalService.Sno)
            {
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false };
                yield return new BuffRule(pwr) { IconIndex = 2, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false };
            }

            if (pwr == Hud.Sno.SnoPowers.Necromancer_Passive_RathmasShield.Sno)
            {
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false };
            }


            // ask for a request to add your item here: http://turbohud.freeforums.net/thread/4045/v7-1-english-gigi-partybuffplugin
            // ############ LEGENDARY ITEM POWERS ############## 
            // #################################################
            if (pwr == Hud.Sno.SnoPowers.AetherWalker.Sno){
                //nothing here
            } 
			if (pwr == Hud.Sno.SnoPowers.AhavarionSpearOfLycander.Sno){
                //nothing here
            } 
			if (pwr == Hud.Sno.SnoPowers.AkkhansAddendum.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.AkkhansLeniency.Sno){
				yield return new BuffRule(266951) { IconIndex = 3, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true};
			} 
			if (pwr == Hud.Sno.SnoPowers.AkkhansManacles.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.AncestorsGrace.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.AncientParthanDefenders.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.AnessaziEdge.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.AquilaCuirass.Sno){
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false};        // Active
            } 
			if (pwr == Hud.Sno.SnoPowers.ArchmagesVicalyke.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.Arcstone.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.ArmorOfTheKindRegent.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.ArreatsLaw.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.ArthefsSparkOfLife.Sno){
                //nothing here
            }
			if (pwr == Hud.Sno.SnoPowers.AshnagarrsBloodBracer.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.BakuliJungleWraps.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.Balance.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.BalefulRemnant.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.BandOfHollowWhispers.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.BandOfMight.Sno){
				yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};
			} 
			if (pwr == Hud.Sno.SnoPowers.BandOfTheRueChambers.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.BastionsRevered.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.BeckonSail.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.BeltOfTheTrove.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.BeltOfTranscendence.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.BindingOfTheLost.Sno){
				yield return new BuffRule(96694) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true}; 
			} 
			if (pwr == Hud.Sno.SnoPowers.BindingsOfTheLesserGods.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.Blackfeather.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.BladeOfProphecy.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.BladeOfTheTribes.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.BladeOfTheWarlord.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.BlessedOfHaull.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.BloodBrother.Sno){
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false, UseLegendaryItemTexture = Hud.Inventory.GetSnoItem(Hud.Sno.SnoItems.Unique_Sword_2H_103_x1.Sno)};        // Uptime
            } 
			if (pwr == Hud.Sno.SnoPowers.BovineBardiche.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.BracerOfFury.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.BracersOfDestruction.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.BracersOfTheFirstMen.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.BrokenCrown.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.BrokenPromises.Sno){
				yield return new BuffRule(pwr) { IconIndex = 2, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false, UseLegendaryItemTexture = Hud.Inventory.GetSnoItem(Hud.Sno.SnoItems.Unique_Ring_006.Sno)};
			} 
			if (pwr == Hud.Sno.SnoPowers.BulKathossWeddingBand.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.ButchersCarver.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.CamsRebuttal.Sno){
				yield return new BuffRule(239137) { IconIndex = 8, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false, UseLegendaryItemTexture = Hud.Inventory.GetSnoItem(Hud.Sno.SnoItems.Unique_Sword_2H_102_x1.Sno)};
			} 
			if (pwr == Hud.Sno.SnoPowers.CapeOfTheDarkNight.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.Carnevil.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.CesarsMemento.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.Chaingmail.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.ChainOfShadows.Sno){
				yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false}; 
			} 
			if (pwr == Hud.Sno.SnoPowers.ChanonBolter.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.ChilaniksChain.Sno){
				yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false, UseLegendaryItemTexture = Hud.Inventory.GetSnoItem(Hud.Sno.SnoItems.Unique_BarbBelt_101_x1.Sno)};
			} 
			if (pwr == Hud.Sno.SnoPowers.Cindercoat.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.CoilsOfTheFirstSpider.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.ConventionOfElements.Sno){
				yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};
				yield return new BuffRule(pwr) { IconIndex = 2, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};
				yield return new BuffRule(pwr) { IconIndex = 3, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};
				yield return new BuffRule(pwr) { IconIndex = 4, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};
				yield return new BuffRule(pwr) { IconIndex = 5, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};
				yield return new BuffRule(pwr) { IconIndex = 6, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};
				yield return new BuffRule(pwr) { IconIndex = 7, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};
			} 
			if (pwr == Hud.Sno.SnoPowers.CordOfTheSherma.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.CorruptedAshbringer.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.CountessJuliasCameo.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.CrashingRain.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.CrownOfThePrimus.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.CrystalFist.Sno){
				yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};
			} 
			if (pwr == Hud.Sno.SnoPowers.CusterianWristguards.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.Darklight.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.DarkMagesShade.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.DeadlyRebirth.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.DeathseersCowl.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.DeathWatchMantle.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.Deathwish.Sno){
                //nothing here
            } 
			if (pwr == Hud.Sno.SnoPowers.DepthDiggers.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.DishonoredLegacy.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.DovuEnergyTrap.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.DrakonsLesson.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.DreadIron.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.ElusiveRing.Sno){
				yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};
			} 
			if (pwr == Hud.Sno.SnoPowers.EnchantingFavor.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.EternalUnion.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.Eunjangdo.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.EyeOfPeshkov.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.FaithfulMemory.Sno){
				yield return new BuffRule(239137) { IconIndex = 9, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true};
			} 
			if (pwr == Hud.Sno.SnoPowers.FateOfTheFell.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.FazulasImprobableChain.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.FireWalkers.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.FlailOfTheAscended.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.Fleshrake.Sno){
				yield return new BuffRule(312736) { IconIndex = 5, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true};
			} 
			if (pwr == Hud.Sno.SnoPowers.FlyingDragon.Sno){
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false, UseLegendaryItemTexture = Hud.Inventory.GetSnoItem(Hud.Sno.SnoItems.Unique_CombatStaff_2H_009.Sno)};        // Uptime
                //yield return new BuffRule(pwr) { IconIndex = 7, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false, UseLegendaryItemTexture = Hud.Inventory.GetSnoItem(Hud.Sno.SnoItems.Unique_CombatStaff_2H_009.Sno)};        // ICD
            } 
			if (pwr == Hud.Sno.SnoPowers.FortressBallista.Sno){
				yield return new BuffRule(pwr) { IconIndex = 2, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = true};
			} 
			if (pwr == Hud.Sno.SnoPowers.FragmentOfDestiny.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.Frostburn.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.Fulminator.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.FuryOfTheAncients.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.GabrielsVambraces.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.Genzaniku.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.GestureOfOrpheus.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.GirdleOfGiants.Sno){
				yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};
			} 
			if (pwr == Hud.Sno.SnoPowers.GladiatorGauntlets.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.GoldenFlense.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.Goldwrap.Sno){
				yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true, UseLegendaryItemTexture = Hud.Inventory.GetSnoItem(Hud.Sno.SnoItems.Unique_Belt_010.Sno)};
			} 
			if (pwr == Hud.Sno.SnoPowers.GungdoGear.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.GyanaNaKashu.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.GyrfalconsFoote.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.Hack.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.HaloOfArlyse.Sno){} 
			if (pwr == 451158){
				yield return new BuffRule(74499) { IconIndex = 4, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};
			} 
			if (pwr == Hud.Sno.SnoPowers.HammerJammers.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.HandOfTheProphet.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.HarringtonWaistguard.Sno){
				yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};
			} 
			if (pwr == Hud.Sno.SnoPowers.HauntingGirdle.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.HauntOfVaxo.Sno){
				yield return new BuffRule(pwr) { IconIndex = 3, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};
			} 
			if (pwr == Hud.Sno.SnoPowers.HeartOfIron.Sno){} 
			//if (pwr == Hud.Sno.SnoPowers.HellcatWaistguard.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.HergbrashsBinding.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.HexingPantsOfMrYan.Sno){
				yield return new BuffRule(pwr) { IconIndex = 2, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false, UseLegendaryItemTexture = Hud.Inventory.GetSnoItem(Hud.Sno.SnoItems.Unique_Pants_101_x1.Sno)}; 
			} 
			if (pwr == Hud.Sno.SnoPowers.HillenbrandsTrainingSword.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.HomingPads.Sno){
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false, UseLegendaryItemTexture = Hud.Inventory.GetSnoItem(Hud.Sno.SnoItems.Unique_Shoulder_001_x1.Sno)};        // Active
            } 
			if (pwr == Hud.Sno.SnoPowers.HuntersWrath.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.HwojWrap.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.IncenseTorchOfTheGrandTemple.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.Ingeom.Sno){
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Uptime
            } 
			if (pwr == Hud.Sno.SnoPowers.InviolableFaith.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.IrontoeMudsputters.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.JacesHammerOfVigilance.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.JangsEnvelopment.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.Jawbreaker.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.JeramsBracers.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.JohannasArgument.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.JustiniansMercy.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.KarleisPoint.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.KassarsRetribution.Sno){
				yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false, UseLegendaryItemTexture = Hud.Inventory.GetSnoItem(Hud.Sno.SnoItems.Unique_Flail_1H_104_x1.Sno)}; 
			} 
			if (pwr == Hud.Sno.SnoPowers.KekegisUnbreakableSpirit.Sno){
				yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};        // Uptime
			} 
			if (pwr == Hud.Sno.SnoPowers.KhassettsCordOfRighteousness.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.KmarTenclip.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.KredesFlame.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.KrelmsBuffBelt.Sno){
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false, UseLegendaryItemTexture = Hud.Inventory.GetSnoItem(Hud.Sno.SnoItems.Unique_Belt_Set_02_x1.Sno)};         // Downtime
                yield return new BuffRule(pwr) { IconIndex = 2, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false, UseLegendaryItemTexture = Hud.Inventory.GetSnoItem(Hud.Sno.SnoItems.Unique_Belt_Set_02_x1.Sno)};        // Active
            } 
			if (pwr == Hud.Sno.SnoPowers.KrelmsBuffBracers.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.Kridershot.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.KyoshirosBlade.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.KyoshirosSoul.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.LakumbasOrnament.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.Lamentation.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.LastBreath.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.LefebvresSoliloquy.Sno){
				yield return new BuffRule(223473) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};
			} 
			if (pwr == Hud.Sno.SnoPowers.LeonineBowOfHashir.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.LiannasWings.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.LionsClaw.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.LordGreenstonesFan.Sno){
				 yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = true};
			} 
			if (pwr == Hud.Sno.SnoPowers.LutSocks.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.MadawcsSorrow.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.Madstone.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.Magefist.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.MalothsFocus.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.ManaldHeal.Sno){
                //nothing here
            } 
			if (pwr == Hud.Sno.SnoPowers.MantleOfChanneling.Sno){
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false, UseLegendaryItemTexture = Hud.Inventory.GetSnoItem(Hud.Sno.SnoItems.P4_Unique_Shoulder_103.Sno)};         // Active
            } 
			if (pwr == Hud.Sno.SnoPowers.MarasKaleidoscope.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.MaskOfJeram.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.MoonlightWard.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.MordullusPromise.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.NemesisBracers.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.NilfursBoast.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.Oathkeeper.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.OculusRing.Sno){
                yield return new BuffRule(pwr) { IconIndex = 2, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false, UseLegendaryItemTexture = Hud.Inventory.GetSnoItem(Hud.Sno.SnoItems.Unique_Ring_017.Sno)};    // Uptime
                yield return new BuffRule(pwr) { IconIndex = 3, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false, UseLegendaryItemTexture = Hud.Inventory.GetSnoItem(Hud.Sno.SnoItems.Unique_Ring_017.Sno)};     // ICD
            } 
			if (pwr == Hud.Sno.SnoPowers.OdynSon.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.OdysseysEnd.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.Omnislash.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.OmrynsChain.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.PintosPride.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.PoxFaulds.Sno){
				yield return new BuffRule(pwr) { IconIndex = 3, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};
			} 
			if (pwr == Hud.Sno.SnoPowers.PrideOfCassius.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.PromiseOfGlory.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.PuzzleRing.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.Quetzalcoatl.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.RabidStrike.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.RakoffsGlassOfLife.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.RanslorsFolly.Sno){
                //nothing here
            } 
			if (pwr == Hud.Sno.SnoPowers.RazorStrop.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.RechelsRingOfLarceny.Sno){
				yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false, UseLegendaryItemTexture = Hud.Inventory.GetSnoItem(Hud.Sno.SnoItems.Unique_Ring_104_x1.Sno)};
			} 
			if (pwr == Hud.Sno.SnoPowers.RelicOfAkarat.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.Remorseless.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.RhenhoFlayer.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.RibaldEtchings.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.Rimeheart.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.RingOfEmptiness.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.RiveraDancers.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.RogarsHugeStone.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.SacredHarness.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.SacredHarvester.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.SaffronWrap.Sno){
				yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = true};
			} 
			if (pwr == Hud.Sno.SnoPowers.SashOfKnives.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.Scarbringer.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.Scourge.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.Scrimshaw.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.SeborsNightmare.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.SerpentsSparker.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.ShardOfHate.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.ShiMizusHaori.Sno){
				yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false, UseLegendaryItemTexture = Hud.Inventory.GetSnoItem(Hud.Sno.SnoItems.Unique_Chest_101_x1.Sno)};
			} 
			if (pwr == Hud.Sno.SnoPowers.SkeletonKey.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.SkularsSalvation.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.SkullGrasp.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.SkullOfResonance.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.SkySplitter.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.Skywarden.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.SlipkasLetterOpener.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.SloraksMadness.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.SmokingThurible.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.Solanium.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.SpauldersOfZakara.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.SpiritGuards.Sno){
				yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false};
			} 
			if (pwr == Hud.Sno.SnoPowers.StaffOfChiroptera.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.StalgardsDecimator.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.Standoff.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.StArchewsGage.Sno){
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false, UseLegendaryItemTexture = Hud.Inventory.GetSnoItem(Hud.Sno.SnoItems.Unique_Gloves_101_p2.Sno)};         // Uptime
            } 
			if (pwr == Hud.Sno.SnoPowers.Starfire.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.StarmetalKukri.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.StormCrow.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.StringOfEars.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.StrongarmBracers.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.SuWongDiviner.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.SwampLandWaders.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.Swiftmount.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.SwordOfIllWill.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.TalismanOfAranoch.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.TaskerandTheo.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.TheBarber.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.TheBurningAxeOfSankis.Sno){
				yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false, UseLegendaryItemTexture = Hud.Inventory.GetSnoItem(Hud.Sno.SnoItems.Unique_Axe_1H_007.Sno)}; 
			} 
			if (pwr == Hud.Sno.SnoPowers.TheButchersSickle.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.TheCloakOfTheGarwulf.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.TheCrudestBoots.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.TheDaggerOfDarts.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.TheDemonsDemise.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.TheEssOfJohan.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.TheFistOfAzTurrasq.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.TheFlowOfEternity.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.TheFurnace.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.TheGavelOfJudgment.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.TheGidbinn.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.TheGrandVizier.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.TheGrinReaper.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.TheLawsOfSeph.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.TheMagistrate.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.TheMindsEye.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.TheMortalDrama.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.ThePaddle.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.TheShameOfDelsere.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.TheShortMansFinger.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.TheSmolderingCore.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.TheSpiderQueensGrasp.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.TheStarOfAzkaranth.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.TheSwami.Sno){
				yield return new BuffRule(134872) { IconIndex = 5, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true}; 
			} 
			if (pwr == Hud.Sno.SnoPowers.TheTallMansFinger.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.TheThreeHundredthSpear.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.TheTormentor.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.TheTwistedSword.Sno){
				yield return new BuffRule(77113) { IconIndex = 6, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true}; 
			} 
			if (pwr == Hud.Sno.SnoPowers.TheUndisputedChampion.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.ThunderfuryBlessedBladeOfTheWindseeker.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.TiklandianVisage.Sno){
				yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false}; 
			} 
			if (pwr == Hud.Sno.SnoPowers.TragOulCoils.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.TzoKrinsGaze.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.UnstableScepter.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.VadimsSurge.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.ValtheksRebuke.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.VambracesOfSescheron.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.VelvetCamaral.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.VengefulWind.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.Vigilance.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.VileWard.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.VisageOfGiyua.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.VisageOfGunes.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.VoosJuicer.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.WandOfWoh.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.WarhelmOfKassar.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.WarstaffOfGeneralQuang.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.WarzechianArmguards.Sno){
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false, UseLegendaryItemTexture = Hud.Inventory.GetSnoItem(Hud.Sno.SnoItems.Unique_Bracer_101_x1.Sno)};         // Uptime
            } 
			if (pwr == Hud.Sno.SnoPowers.Wizardspike.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.WojahnniAssaulter.Sno){
				yield return new BuffRule(397780) { IconIndex = 2, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true}; 
			} 
			if (pwr == Hud.Sno.SnoPowers.WrapsOfClarity.Sno){
				yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false}; 
			} 
			if (pwr == Hud.Sno.SnoPowers.Wyrdward.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.XephirianAmulet.Sno){} 
			if (pwr == Hud.Sno.SnoPowers.ZoeysSecret.Sno){} 
			//补充遗漏装备
			if (pwr == 434849){
				yield return new BuffRule(pwr) { IconIndex = 2, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true}; //三元宝珠Triumvirate
			} 
			if (pwr == 449037){
				yield return new BuffRule(87525) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true}; //无尽深渊法珠Orb of Infinite Depth
			} 
			if (pwr == 435016){
				yield return new BuffRule(239042) { IconIndex = 5, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = true}; //不破之钢Denial
			}
            //死灵法师装备
            if (pwr == 476684)
            {
                yield return new BuffRule(476684) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true, UseLegendaryItemTexture = Hud.Inventory.GetSnoItem(Hud.Sno.SnoItems.P6_Unique_Phylactery_01.Sno)}; //失时
            }
            if (pwr == 476587)
            {
                yield return new BuffRule(476587) { IconIndex = 7, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true}; //纳伊尔的黑镰
            }
            if (pwr == 475248)
            {
                yield return new BuffRule(454174) { IconIndex = 2, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true }; //精魂魔掌
            }
            if (pwr == 476689)
            {
                yield return new BuffRule(476689) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false, UseLegendaryItemTexture = Hud.Inventory.GetSnoItem(Hud.Sno.SnoItems.P6_Unique_Belt_01.Sno)}; //但提的束缚
            }
            if (pwr == 476588)
            {
                yield return new BuffRule(476588) { IconIndex = 2, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false, UseLegendaryItemTexture = Hud.Inventory.GetSnoItem(Hud.Sno.SnoItems.P6_Necro_Unique_Pants_21.Sno)}; //傀儡大师的马裤
            }
            if (pwr == 476580)
            {
                yield return new BuffRule(476580) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true}; //拜尸者的护肩
            }
            if (pwr == 389601)
            {
                yield return new BuffRule(475243) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true, UseLegendaryItemTexture = Hud.Inventory.GetSnoItem(Hud.Sno.SnoItems.P6_Necro_Unique_Boots_21.Sno)}; //斯图亚特的护胫
            }
            if (pwr == 476583)
            {
                yield return new BuffRule(476583) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = true}; //约翰之石
            }
            //悬赏物品
            if (pwr == 322977){
				yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false}; //骄矜必败Pride Fall
			} 
			if (pwr == 322975){
				yield return new BuffRule(pwr) { IconIndex = 7, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true, UseLegendaryItemTexture = Hud.Inventory.GetSnoItem(Hud.Sno.SnoItems.Unique_Ring_108_x1.Sno)}; //贪婪之戒Avarice Band
			} 
			
			if (pwr == 322980){
				yield return new BuffRule(pwr) { IconIndex = 7, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true, UseLegendaryItemTexture = Hud.Inventory.GetSnoItem(Hud.Sno.SnoItems.Unique_Belt_103_x1.Sno)}; //贪婪腰带Insatiable Belt
			}
            if (pwr == 334883)
            {
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false, UseLegendaryItemTexture = Hud.Inventory.GetSnoItem(Hud.Sno.SnoItems.Unique_Shoulder_103_x1.Sno) }; //骷髅王肩铠Pauldrons of the Skeleton King
            }
            //锻造物品
            if (pwr == 247585){
				yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false, UseLegendaryItemTexture = Hud.Inventory.GetSnoItem(Hud.Sno.SnoItems.Unique_Shield_011.Sno)}; //尸墙 Wall of Bone
			} 
			
			//套装
			if (pwr == 447541){
				yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true}; //罗盘玫瑰The Compass Rose
				yield return new BuffRule(pwr) { IconIndex = 2, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true}; //旅人祈愿The Traveler's Pledge
			} 
			if (pwr == 359583){
				yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false}; //守心restraint
				yield return new BuffRule(pwr) { IconIndex = 2, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false}; //克己focus
			} 
			if (pwr == 359582){
				yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true}; //小流氓对剑Set_Istvan's Paired Blades(Little Rogue,The Slanderer)
			} 
			if (pwr == 440235){
				yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = true}; //迦陀朵的决心(Chantodo's Force,Chantodo's Will)
			} 
			if (pwr == 440569){
				yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false}; //神龙之魂(Shenlong's Fist of Legend,Shenlong's Relentless Assault)
			} 
			
			//职业套装

			//法师
			if (pwr == 429855){
				yield return new BuffRule(pwr) { IconIndex = 5, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true}; //塔拉夏6件套Tal Rasha's Elements
			} 
			if (pwr == 450294){
				yield return new BuffRule(1769) { IconIndex = 9, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false}; //德尔西尼4件Set_Delsere's Magnum Opus
			}
			if (pwr == 359580){
				yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false}; //不死鸟2件Firebird's Finery
			}
			if (pwr == 445814){
				yield return new BuffRule(359581) { IconIndex = 5, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = true}; //不死鸟6件Firebird's Finery
			}
            //武僧
            if (pwr == 430228)
            {
                yield return new BuffRule(pwr) { IconIndex = 2, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false }; //千飓6件Raiment of a Thousand Storms
            }
            //野蛮人
            if (pwr == 429673){
				yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = true}; //蕾寇6件The Legacy of Raekor
			}
			//巫医
			if (pwr == 437711){
				yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false}; //魔牙6件Helltooth Harness
			}
			if (pwr == 439308){
				yield return new BuffRule(30631) { IconIndex = 6, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false}; //亚拉基尔4件Spirit of Arachyr
			}
			
			//圣教军
			if (pwr == 436426){
				yield return new BuffRule(239137) { IconIndex = 4, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false}; //圣光4件The Seeker of the Light
			}
			if (pwr == 445829){
				yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true}; //幻魔师2件Thorns of the Invoker
			}
			if (pwr == 445639){
				yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false}; //幻魔师4件Thorns of the Invoker
			}
			if (pwr == 409428){
				yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true}; //罗兰6件Roland's Legacy
			}
			//狩魔猎人
			if (pwr == 434964){
				yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false}; //娜塔亚6件Natalya's Vengeance
			}
			if (pwr == 423244){
				yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false}; //邪秽之精4件Unhallowed Essence
			}
            //死灵法师
            if (pwr == 467463) 
            {
                yield return new BuffRule(pwr) { IconIndex = 2, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true, UseLegendaryItemTexture = Hud.Inventory.GetSnoItem(Hud.Sno.SnoItems.P6_Necro_Set_1_Helm.Sno)}; //拉斯玛的骨甲4件
            }
            
            if (pwr == 472273)
            {
                yield return new BuffRule(472273) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true}; //死疫使者的裹布4件
            }
            if (pwr == 472274) 
            {
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true, UseLegendaryItemTexture = Hud.Inventory.GetSnoItem(Hud.Sno.SnoItems.P6_Necro_Set_4_Helm.Sno)}; //死疫使者的裹布6件
            }
            

            // ############ LEGENDARY GEM POWERS ##############
            // ################################################
            if (pwr == Hud.Sno.SnoPowers.BaneOfThePowerfulPrimary.Sno){
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false}; 
            }
            if (pwr == Hud.Sno.SnoPowers.BaneOfThePowerfulSecondary.Sno){
                //nothing here
            } 
            if (pwr == Hud.Sno.SnoPowers.BaneOfTheStrickenPrimary.Sno){
                yield return new BuffRule(pwr) { IconIndex = 2, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false, UseLegendaryItemTexture = Hud.Inventory.GetSnoItem(Hud.Sno.SnoItems.Unique_Gem_018_x1.Sno)};     // ICD
                //yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 0, ShowTimeLeft = false, ShowStacks = false, UseLegendaryItemTexture = Hud.Inventory.GetSnoItem(Hud.Sno.SnoItems.Unique_Gem_018_x1.Sno)};  // Equipped - not needed
            }
            if (pwr == Hud.Sno.SnoPowers.BaneOfTheStrickenSecondary.Sno){
                //nothing here
            } 
            if (pwr == Hud.Sno.SnoPowers.BaneOfTheTrappedPrimary.Sno){
                //nothingh ere
            } 
            if (pwr == Hud.Sno.SnoPowers.BaneOfTheTrappedSecondary.Sno){
                //nothing here
            } 
            if (pwr == Hud.Sno.SnoPowers.BoonOfTheHoarderPrimary.Sno){
                //nothing here
            }
            if (pwr == Hud.Sno.SnoPowers.BoonOfTheHoarderSecondary.Sno){
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false, UseLegendaryItemTexture = Hud.Inventory.GetSnoItem(Hud.Sno.SnoItems.Unique_Gem_014_x1.Sno)};     // Active
            }  
            if (pwr == Hud.Sno.SnoPowers.BoyarskysChipPrimary.Sno){
                //nothing here
            } 
            if (pwr == Hud.Sno.SnoPowers.BoyarskysChipSecondary.Sno){
                //nothing here
            } 
            if (pwr == Hud.Sno.SnoPowers.EnforcerPrimary.Sno){} 
            if (pwr == Hud.Sno.SnoPowers.EnforcerSecondary.Sno){} 
            if (pwr == Hud.Sno.SnoPowers.EsotericAlterationPrimary.Sno){
                //nothing here
            } 
            if (pwr == Hud.Sno.SnoPowers.EsotericAlterationSecondary.Sno){
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false, UseLegendaryItemTexture = Hud.Inventory.GetSnoItem(Hud.Sno.SnoItems.Unique_Gem_016_x1.Sno)};    
            } 
            if (pwr == Hud.Sno.SnoPowers.GemOfEasePrimary.Sno){
                //nothing here
            } 
            if (pwr == Hud.Sno.SnoPowers.GemOfEaseSecondary.Sno){
                //nothing here
            } 
            if (pwr == Hud.Sno.SnoPowers.GemOfEfficaciousToxinPrimary.Sno){
                //nothing here
            } 
            if (pwr == Hud.Sno.SnoPowers.GemOfEfficaciousToxinSecondary.Sno){
                //nothing here
            } 
            if (pwr == Hud.Sno.SnoPowers.GogokOfSwiftnessPrimary.Sno){
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true}; 
                //yield return new BuffRule(pwr) { IconIndex = 2, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false,  UseLegendaryItemTexture = Hud.Inventory.GetSnoItem(Hud.Sno.SnoItems.Unique_Gem_008_x1.Sno)}; - not needed 
                //yield return new BuffRule(pwr) { IconIndex = 3, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false,  UseLegendaryItemTexture = Hud.Inventory.GetSnoItem(Hud.Sno.SnoItems.Unique_Gem_008_x1.Sno)}; - not needed
                //yield return new BuffRule(pwr) { IconIndex = 4, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false,  UseLegendaryItemTexture = Hud.Inventory.GetSnoItem(Hud.Sno.SnoItems.Unique_Gem_008_x1.Sno)}; - not needed
            } 
            if (pwr == Hud.Sno.SnoPowers.GogokOfSwiftnessSecondary.Sno){
                //nothing here
           } 
            if (pwr == Hud.Sno.SnoPowers.IceblinkPrimary.Sno){
                //nothing here
            } 
            if (pwr == Hud.Sno.SnoPowers.IceblinkSecondary.Sno){
                //nothing here
            } 
            if (pwr == Hud.Sno.SnoPowers.InvigoratingGemstonePrimary.Sno){
				yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true, UseLegendaryItemTexture = Hud.Inventory.GetSnoItem(Hud.Sno.SnoItems.Unique_Gem_009_x1.Sno)}; 
			} 
                
            if (pwr == Hud.Sno.SnoPowers.InvigoratingGemstoneSecondary.Sno){} 
            if (pwr == Hud.Sno.SnoPowers.MirinaeTeardropOfTheStarweaverPrimary.Sno){
                //nothing here
            } 
            if (pwr == Hud.Sno.SnoPowers.MirinaeTeardropOfTheStarweaverSecondary.Sno){
                //nothing here
            } 
            if (pwr == Hud.Sno.SnoPowers.MoltenWildebeestsGizzardPrimary.Sno){
                //nothing here
            } 
            if (pwr == Hud.Sno.SnoPowers.MoltenWildebeestsGizzardSecondary.Sno){
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false, UseLegendaryItemTexture = Hud.Inventory.GetSnoItem(Hud.Sno.SnoItems.Unique_Gem_017_x1.Sno)};         // Downtime
                yield return new BuffRule(pwr) { IconIndex = 2, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false, UseLegendaryItemTexture = Hud.Inventory.GetSnoItem(Hud.Sno.SnoItems.Unique_Gem_017_x1.Sno)};         // Shield is up
            } 
            if (pwr == Hud.Sno.SnoPowers.MoratoriumPrimary.Sno){
                //nothing here
            } 
            if (pwr == Hud.Sno.SnoPowers.MoratoriumSecondary.Sno){
                //nothing here
            } 
            if (pwr == Hud.Sno.SnoPowers.MutilationGuardPrimary.Sno){
                //nothing here
            } 
            if (pwr == Hud.Sno.SnoPowers.MutilationGuardSecondary.Sno){
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false, UseLegendaryItemTexture = Hud.Inventory.GetSnoItem(Hud.Sno.SnoItems.Unique_Gem_019_x1.Sno)};        // Active
            } 
            if (pwr == Hud.Sno.SnoPowers.PainEnhancerPrimary.Sno){
                //nothing here
            } 
            if (pwr == Hud.Sno.SnoPowers.PainEnhancerSecondary.Sno){
                //nothing here
            } 
            if (pwr == Hud.Sno.SnoPowers.RedSoulShardPrimary.Sno){} 
            if (pwr == Hud.Sno.SnoPowers.RedSoulShardSecondary.Sno){} 
            if (pwr == Hud.Sno.SnoPowers.SimplicitysStrengthPrimary.Sno){} 
            if (pwr == Hud.Sno.SnoPowers.SimplicitysStrengthSecondary.Sno){} 
            if (pwr == Hud.Sno.SnoPowers.TaegukPrimary.Sno){                    
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true};         // Stacks
                //yield return new BuffRule(pwr) { IconIndex = 2, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false, UseLegendaryItemTexture = Hud.Inventory.GetSnoItem(Hud.Sno.SnoItems.Unique_Gem_015_x1.Sno)};       // Channeling - not needed
            } 
            if (pwr == Hud.Sno.SnoPowers.TaegukSecondary.Sno){
                //nothing here
            }
            if (pwr == Hud.Sno.SnoPowers.WreathOfLightningPrimary.Sno){
                yield return new BuffRule(pwr) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false, UseLegendaryItemTexture = Hud.Inventory.GetSnoItem(Hud.Sno.SnoItems.Unique_Gem_004_x1.Sno)};         // Active
            } 
            if (pwr == Hud.Sno.SnoPowers.WreathOfLightningSecondary.Sno){
                //nothing here
            } 
            if (pwr == Hud.Sno.SnoPowers.ZeisStoneOfVengeancePrimary.Sno){
                //nothing here
            } 
            if (pwr == Hud.Sno.SnoPowers.ZeisStoneOfVengeanceSecondary.Sno){
                //nothing here
			} 
			//圣坛效果
            if (pwr == Hud.Sno.SnoPowers.Generic_PagesBuffElectrified.Sno){
                yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false}; //电击塔
            }
            if (pwr == Hud.Sno.SnoPowers.Generic_PagesBuffElectrifiedTieredRift.Sno){
                yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false}; //电击塔 大秘
            }
            if (pwr == Hud.Sno.SnoPowers.Generic_PagesBuffInfiniteCasting.Sno){
                yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false}; //减耗塔
            }
            if (pwr == Hud.Sno.SnoPowers.Generic_PagesBuffInvulnerable.Sno){
                yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false}; //护盾塔
            }
            if (pwr == Hud.Sno.SnoPowers.Generic_PagesBuffDamage.Sno){
                yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false}; //威能塔
            }
            if (pwr == Hud.Sno.SnoPowers.Generic_PagesBuffRunSpeed.Sno){
                yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false}; //速度塔
            }
            if (pwr == Hud.Sno.SnoPowers.Generic_ShrinePowerEnlightened.Sno){
                yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false}; //启迪圣坛
            }
            if (pwr == 030477){
                yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false}; //启迪圣坛2
            }
            if (pwr == Hud.Sno.SnoPowers.Generic_ShrinePowerFrenzied.Sno){
                yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false}; //狂怒圣坛
            }
            if (pwr == 030479){
                yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false}; //狂怒圣坛2
            }
            if (pwr == Hud.Sno.SnoPowers.Generic_ShrinePowerFortune.Sno){
                yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false}; //幸运圣坛
            }
            if (pwr == 030478){
                yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false}; //幸运圣坛2
            }
            if (pwr == 260349){
                yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false}; //增效圣坛
            }
            if (pwr == Hud.Sno.SnoPowers.Generic_ShrinePowerBlessed.Sno){
                yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false}; //祝福圣坛
            }
            if (pwr == 030476){
                yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false}; //祝福圣坛2
            }
            if (pwr == Hud.Sno.SnoPowers.Generic_ShrineDesecratedHoarder.Sno){
                yield return new BuffRule(pwr) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false}; //飞驰圣坛
            }
        }

        private ITexture GetIconTexture(BuffPaintInfo info)
        {
            uint textureId = 0;
            if (info.Rule != null)
            {
                if (info.Rule.UseLegendaryItemTexture != null)
                {
                    return Hud.Texture.GetItemTexture(info.Rule.UseLegendaryItemTexture);
                }

                textureId = info.SnoPower.NormalIconTextureId;
                if (!info.Rule.UsePowersTexture && info.Icons[0].Exists && (info.Icons[0].TextureId != 0)) textureId = info.Icons[0].TextureId;
            }
            else
            {
                textureId = info.Icons[0].TextureId;
            }
            if (textureId <= 0) return null;

            return Hud.Texture.GetTexture(textureId);
        }
    }
}