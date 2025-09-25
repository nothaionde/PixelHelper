using Turbo.Plugins.Default;
using System.Linq;
using System;
using System.Collections.Generic;

namespace Turbo.Plugins.Miqui
{
    public class LegendaryItemAffixDataPlugin : BasePlugin, ICustomizer
    {
        public LegendaryItemAffixDataPlugin()
        {
            Enabled = true;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
        }

        public void Customize()
        {
            Dictionary<uint, ItemAffixInfo> itemAffixInfos = new Dictionary<uint, ItemAffixInfo>();

            this.addInfosCommonClasses(itemAffixInfos);
            this.addInfosBarb(itemAffixInfos);
            this.addInfosCrusader(itemAffixInfos);
            this.addInfosDemonHunter(itemAffixInfos);
            this.addInfosMonk(itemAffixInfos);
            this.addInfosNecromancer(itemAffixInfos);
            this.addInfosWitchDoctor(itemAffixInfos);
            this.addInfosWizard(itemAffixInfos);

            Hud.RunOnPlugin<LegendaryItemAffixPlugin>(plugin => plugin.ItemAffixInfos = itemAffixInfos);
        }

        private void addInfosCommonClasses(Dictionary<uint, ItemAffixInfo> itemAffixInfos)
        {
            itemAffixInfos.Add(197717, ItemAffixInfo.PercentageFromSno(197717)); // Schaefer hammer
            itemAffixInfos.Add(188185, ItemAffixInfo.PercentageFromSno(188185)); // Odin son
            itemAffixInfos.Add(192511, ItemAffixInfo.PercentageFromSno(192511)); // Azure wrath
            itemAffixInfos.Add(181511, ItemAffixInfo.PercentageFromSno(181511)); // Scourge
            itemAffixInfos.Add(116389, ItemAffixInfo.PercentageFromSno(116389)); // Sky Splitter
            itemAffixInfos.Add(186560, ItemAffixInfo.PercentageFromSno(186560)); // The Executioner
            itemAffixInfos.Add(59633, ItemAffixInfo.PercentageFromSno(59633)); // Arthef's Spark of Life
            itemAffixInfos.Add(196024, new ItemAffixInfo(196024, AffixType.PERCENTAGE, "Gem_Attributes_Multiplier")); // Leoric's crown
            itemAffixInfos.Add(197206, ItemAffixInfo.PercentageFromSno(197206)); // Magefist
            itemAffixInfos.Add(197205, ItemAffixInfo.PercentageFromSno(197205)); // Frostburn
            itemAffixInfos.Add(205642, ItemAffixInfo.PercentageFromSno(205642)); // Taskers
            itemAffixInfos.Add(205624, ItemAffixInfo.PercentageFromSno(205624)); // Fire Walkers
            itemAffixInfos.Add(193670, new ItemAffixInfo(193670, AffixType.PERCENTAGE, "Crit_Damage_Percent")); // Witching Hour, shows crit dmg
            itemAffixInfos.Add(193669, ItemAffixInfo.PercentageFromSno(193669)); // String of Ears
            itemAffixInfos.Add(193692, ItemAffixInfo.PercentageFromSno(193692)); // Strongarms
            itemAffixInfos.Add(197203, ItemAffixInfo.PercentageFromSno(197203)); // Aquila
            itemAffixInfos.Add(212590, ItemAffixInfo.PercentageFromSno(212590)); // Justice Lantern
            itemAffixInfos.Add(298089, ItemAffixInfo.PercentageFromSno(298089)); // Wyrdward
            itemAffixInfos.Add(410960, ItemAffixInfo.PercentageFromSno(410960)); // Eun-jang-do
            itemAffixInfos.Add(205607, ItemAffixInfo.PercentageFromSno(205607)); // Heart of Iron
            itemAffixInfos.Add(222455, ItemAffixInfo.PercentageFromSno(222455)); // Cindercoat
            itemAffixInfos.Add(197216, ItemAffixInfo.PercentageFromSno(197216)); // Depth Diggers
            itemAffixInfos.Add(198573, ItemAffixInfo.PercentageFromSno(198573)); // Homing Pads
            // itemAffixInfos.Add(212590, ItemAffixInfo.PercentageFromSno(212590)); // Justice Lantern - Doesn't have the value for some reason
            itemAffixInfos.Add(212648, ItemAffixInfo.PercentageFromSno(212648)); // Oculus Ring
            itemAffixInfos.Add(433496, ItemAffixInfo.PercentageFromSno(433496)); // CoE
            itemAffixInfos.Add(229716, ItemAffixInfo.PercentageFromSno(229716)); // Thunderfury
            itemAffixInfos.Add(410946, ItemAffixInfo.SecondFromSno(410946)); // In-geom
            itemAffixInfos.Add(271666, ItemAffixInfo.PercentageFromSno(271666)); // The Furnace
            itemAffixInfos.Add(429681, ItemAffixInfo.PercentageFromSno(429681)); // Mantle of Channeling
            itemAffixInfos.Add(332172, ItemAffixInfo.PercentageFromSno(332172)); // St Archew
            itemAffixInfos.Add(298129, ItemAffixInfo.PercentageFromSno(298129)); // Harrington Waistguard
            itemAffixInfos.Add(298116, ItemAffixInfo.PercentageFromSno(298116)); // APDs
            itemAffixInfos.Add(296118, ItemAffixInfo.PercentageFromSno(296118)); // Reaper's Wraps
            itemAffixInfos.Add(332204, ItemAffixInfo.PercentageFromSno(332204)); // Hexing pants
            itemAffixInfos.Add(298091, ItemAffixInfo.PercentageFromSno(298091)); // Rechel's ring of Larceny

            // Doesn't work but oh well
            itemAffixInfos.Add(341333, ItemAffixInfo.PercentageFromSno(341333)); // Armor potion
            itemAffixInfos.Add(341342, ItemAffixInfo.PercentageFromSno(341342)); // All rez potion
            itemAffixInfos.Add(483316, ItemAffixInfo.SecondFromSno(483316)); // CC Immune potion

        }

        private void addInfosBarb(Dictionary<uint, ItemAffixInfo> itemAffixInfos)
        {
            itemAffixInfos.Add(191570, ItemAffixInfo.PercentageFromSno(191570)); // Standoff
            itemAffixInfos.Add(196638, ItemAffixInfo.PercentageFromSno(196638)); // 300th spear
            itemAffixInfos.Add(212232, ItemAffixInfo.PercentageFromSno(212232)); // Girdle of Giants
            itemAffixInfos.Add(192105, ItemAffixInfo.PercentageFromSno(192105)); // Fjord Cutter
            itemAffixInfos.Add(193611, ItemAffixInfo.PercentageFromSno(193611)); // Blade of the warlord
            itemAffixInfos.Add(193657, ItemAffixInfo.PercentageFromSno(193657)); // Gavel of Judgement
            itemAffixInfos.Add(195138, ItemAffixInfo.PercentageFromSno(195138)); // Fury of the vanished peak
            itemAffixInfos.Add(193664, ItemAffixInfo.PercentageFromSno(193664)); // Saffron Wrap
            itemAffixInfos.Add(201325, ItemAffixInfo.PercentageFromSno(201325)); // Vile Wards
            itemAffixInfos.Add(193676, ItemAffixInfo.PercentageFromSno(193676)); // Undisputed Champion
            itemAffixInfos.Add(193673, ItemAffixInfo.SecondFromSno(193673)); // Pride of Cassius
            itemAffixInfos.Add(197839, ItemAffixInfo.PercentageFromSno(197839)); // Band of Might
            itemAffixInfos.Add(212618, ItemAffixInfo.PercentageFromSno(212618)); // Skull Grasp
            itemAffixInfos.Add(440429, ItemAffixInfo.PercentageFromSno(440429)); // Bracers of Destruction
            itemAffixInfos.Add(440430, ItemAffixInfo.PercentageFromSno(440430)); // Bracers of the First Men
            itemAffixInfos.Add(444928, ItemAffixInfo.PercentageFromSno(444928)); // Skular's Salvation
            itemAffixInfos.Add(447838, ItemAffixInfo.PercentageFromSno(447838)); // Vambraces of Sescheron
            itemAffixInfos.Add(271979, ItemAffixInfo.PercentageFromSno(271979)); // Remorseless
            itemAffixInfos.Add(272008, ItemAffixInfo.PercentageFromSno(272008)); // Dishonored Legacy
            itemAffixInfos.Add(272009, ItemAffixInfo.PercentageFromSno(272009)); // Oathkeeper
            itemAffixInfos.Add(322776, ItemAffixInfo.PercentageFromSno(322776)); // Blade of the Tribes
            itemAffixInfos.Add(298133, ItemAffixInfo.PercentageFromSno(298133)); // Chilanik's Chain
        }

        private void addInfosCrusader(Dictionary<uint, ItemAffixInfo> itemAffixInfos)
        {
            itemAffixInfos.Add(198960, ItemAffixInfo.PercentageFromSno(198960)); // Faithful memory
            itemAffixInfos.Add(184184, ItemAffixInfo.PercentageFromSno(184184)); // Blade of prophecy
            itemAffixInfos.Add(152666, ItemAffixInfo.PercentageFromSno(152666)); // Denial
            itemAffixInfos.Add(209059, ItemAffixInfo.PercentageFromSno(209059)); // Hammer Jammers
            itemAffixInfos.Add(193675, ItemAffixInfo.PercentageFromSno(193675)); // Belt of the trove
            itemAffixInfos.Add(212234, ItemAffixInfo.PercentageFromSno(212234)); // Lamentation
            itemAffixInfos.Add(298187, ItemAffixInfo.PercentageFromSno(298187)); // Guard of Johanna
            itemAffixInfos.Add(298190, ItemAffixInfo.PercentageFromSno(298190)); // Shield of Fury
            itemAffixInfos.Add(299381, ItemAffixInfo.PercentageFromSno(299381)); // Khassett's Cord of Righteousness
            itemAffixInfos.Add(432833, ItemAffixInfo.PercentageFromSno(432833)); // Drakon's Lesson
            itemAffixInfos.Add(436469, ItemAffixInfo.PercentageFromSno(436469)); // Gabriel's Vambraces
            itemAffixInfos.Add(446057, ItemAffixInfo.PercentageFromSno(446057)); // Akkhan's Manacles
            itemAffixInfos.Add(446161, ItemAffixInfo.PercentageFromSno(446161)); // Bracer of Fury
            itemAffixInfos.Add(299411, ItemAffixInfo.PercentageFromSno(299411)); // Piro marella
            itemAffixInfos.Add(299412, ItemAffixInfo.PercentageFromSno(299412)); // Jekangbord
            itemAffixInfos.Add(299414, ItemAffixInfo.PercentageFromSno(299414)); // Akarat's Awakening
            itemAffixInfos.Add(405429, ItemAffixInfo.PercentageFromSno(405429)); // Frydehr's Wrath
            itemAffixInfos.Add(229427, ItemAffixInfo.PercentageFromSno(229427)); // Gyrfalcon's Foote
            itemAffixInfos.Add(229428, ItemAffixInfo.PercentageFromSno(229428)); // Darklight
            itemAffixInfos.Add(299431, ItemAffixInfo.PercentageFromSno(299431)); // The Mortal Drama
            itemAffixInfos.Add(299436, ItemAffixInfo.PercentageFromSno(299436)); // Fate of the Fell
            itemAffixInfos.Add(299437, ItemAffixInfo.PercentageFromSno(299437)); // Golden Flense
            itemAffixInfos.Add(403846, ItemAffixInfo.PercentageFromSno(403846)); // Akkhan's Leniency
        }

        private void addInfosDemonHunter(Dictionary<uint, ItemAffixInfo> itemAffixInfos)
        {
            itemAffixInfos.Add(196409, ItemAffixInfo.PercentageFromSno(196409)); // Dawn
            itemAffixInfos.Add(271889, ItemAffixInfo.PercentageFromSno(271889)); // Wojahnni Assaulter
            itemAffixInfos.Add(298171, ItemAffixInfo.PercentageFromSno(298171)); // Bombardier's Rucksack
            itemAffixInfos.Add(298170, ItemAffixInfo.PercentageFromSno(298170)); // The Ninth Cirri Satchel
            itemAffixInfos.Add(204874, ItemAffixInfo.PercentageFromSno(204874)); // Pus Spitter
            itemAffixInfos.Add(298172, ItemAffixInfo.PercentageFromSno(298172)); // Emimei's Duffel
            itemAffixInfos.Add(221760, ItemAffixInfo.PercentageFromSno(221760)); // Manticore
            itemAffixInfos.Add(197625, ItemAffixInfo.PercentageFromSno(197625)); // Sin Seekers
            itemAffixInfos.Add(197628, ItemAffixInfo.NoUnitFromSno(197628)); // Spines of Seething Hatred
            itemAffixInfos.Add(197627, ItemAffixInfo.PercentageFromSno(197627)); // Holy Point Shot
            itemAffixInfos.Add(193668, ItemAffixInfo.NoUnitFromSno(193668)); // Hellcat Waistguard
            itemAffixInfos.Add(440742, ItemAffixInfo.PercentageFromSno(440742)); // Hunter's Wrath
            itemAffixInfos.Add(446188, ItemAffixInfo.PercentageFromSno(446188)); // Elusive ring
            itemAffixInfos.Add(271728, ItemAffixInfo.PercentageFromSno(271728)); // Karlei's point
            itemAffixInfos.Add(271731, ItemAffixInfo.PercentageFromSno(271731)); // Lord Greenstone's Fan
            itemAffixInfos.Add(328591, ItemAffixInfo.PercentageFromSno(328591)); // Sword of Ill Will
            itemAffixInfos.Add(423247, ItemAffixInfo.PercentageFromSno(423247)); // Crashing Rain  
            itemAffixInfos.Add(298137, ItemAffixInfo.PercentageFromSno(298137)); // Zoey's Secret
            itemAffixInfos.Add(440428, ItemAffixInfo.PercentageFromSno(440428)); // Wraps of Clarity
            itemAffixInfos.Add(271875, ItemAffixInfo.NoUnitFromSno(271875)); // Kridershot
            itemAffixInfos.Add(271880, ItemAffixInfo.PercentageFromSno(271880)); // Oddissey's end
            itemAffixInfos.Add(271882, ItemAffixInfo.PercentageFromSno(271882)); // Leonine Bow of Hashir
            itemAffixInfos.Add(319407, ItemAffixInfo.PercentageFromSno(319407)); // Yang's recurve
            itemAffixInfos.Add(395304, ItemAffixInfo.PercentageFromSno(395304)); // Fortress Ballista
        }

        private void addInfosMonk(Dictionary<uint, ItemAffixInfo> itemAffixInfos)
        {
            itemAffixInfos.Add(145850, ItemAffixInfo.PercentageFromSno(145850)); // Fleshrake
            itemAffixInfos.Add(205620, ItemAffixInfo.PercentageFromSno(205620)); // The Crudest Boots
            itemAffixInfos.Add(130557, ItemAffixInfo.PercentageFromSno(130557)); // Scarbringer
            itemAffixInfos.Add(145851, ItemAffixInfo.PercentageFromSno(145851)); // Won Khim Lau
            itemAffixInfos.Add(175937, ItemAffixInfo.PercentageFromSno(175937)); // Fist of Az'Turrasq
            itemAffixInfos.Add(195145, ItemAffixInfo.PercentageFromSno(195145)); // Balance
            itemAffixInfos.Add(197072, ItemAffixInfo.PercentageFromSno(197072)); // Flow of eternity
            itemAffixInfos.Add(192342, ItemAffixInfo.PercentageFromSno(192342)); // Incense Torch of the Grand Temple
            itemAffixInfos.Add(197224, ItemAffixInfo.PercentageFromSno(197224)); // Rivera Dancers
            itemAffixInfos.Add(193688, ItemAffixInfo.PercentageFromSno(193688)); // Gungdo Gear
            itemAffixInfos.Add(222169, ItemAffixInfo.PercentageFromSno(222169)); // Gyana Na Kashu
            itemAffixInfos.Add(222305, new ItemAffixInfo(222305, AffixType.PERCENTAGE, "Power_Damage_Percent_Bonus"));// Tzo Krin's gaze
            itemAffixInfos.Add(298158, ItemAffixInfo.PercentageFromSno(298158)); // Lefebvre's Soliloquy
            itemAffixInfos.Add(440425, ItemAffixInfo.PercentageFromSno(440425)); // Binding of the Lost --------
            itemAffixInfos.Add(430290, ItemAffixInfo.PercentageFromSno(430290)); // Spirit Guards
            itemAffixInfos.Add(440427, ItemAffixInfo.PercentageFromSno(440427)); // Binding of the lesser gods
            itemAffixInfos.Add(447294, ItemAffixInfo.PercentageFromSno(447294)); // Pinto's Pride
            itemAffixInfos.Add(449038, ItemAffixInfo.PercentageFromSno(449038)); // Cesar's Memento
            itemAffixInfos.Add(298093, ItemAffixInfo.PercentageFromSno(298093)); // Band of the Rue Chambers
            itemAffixInfos.Add(299454, ItemAffixInfo.NoUnitFromSno(299454)); // The Laws of seph
            itemAffixInfos.Add(299464, ItemAffixInfo.PercentageFromSno(299464)); // Eye of Peshkov
            itemAffixInfos.Add(271963, ItemAffixInfo.PercentageFromSno(271963)); // Kyoshiro's Blade
            itemAffixInfos.Add(403775, ItemAffixInfo.PercentageFromSno(403775)); // Vengeful Wind
        }

        private void addInfosNecromancer(Dictionary<uint, ItemAffixInfo> itemAffixInfos)
        {
            itemAffixInfos.Add(467394, ItemAffixInfo.PercentageFromSno(467394)); // Trag'Ouls Corroded Fang
            itemAffixInfos.Add(467370, ItemAffixInfo.PercentageFromSno(467370)); // Funerary Pick
            itemAffixInfos.Add(485500, ItemAffixInfo.PercentageFromSno(485500)); // Bonds of C'Lena
            itemAffixInfos.Add(467581, ItemAffixInfo.PercentageFromSno(467581)); // Iron Rose
            itemAffixInfos.Add(467579, ItemAffixInfo.PercentageFromSno(467579)); // Scythe of the Cycle
            itemAffixInfos.Add(470273, ItemAffixInfo.PercentageFromSno(470273)); // Spear of Jairo
            itemAffixInfos.Add(497598, ItemAffixInfo.PercentageFromSno(497598)); // Maltorius' Petrified Spike
            itemAffixInfos.Add(497599, ItemAffixInfo.PercentageFromSno(497599)); // Bloodtide Blade
            itemAffixInfos.Add(467600, ItemAffixInfo.NoUnitFromSno(467600)); // Reilena's Shadowhook
            itemAffixInfos.Add(467594, ItemAffixInfo.PercentageFromSno(467594)); // Nayr's Black Death
            itemAffixInfos.Add(467582, ItemAffixInfo.PercentageFromSno(467582)); // Lost Time
            itemAffixInfos.Add(462249, ItemAffixInfo.PercentageFromSno(462249)); // Bone Ringer
            itemAffixInfos.Add(462250, ItemAffixInfo.PercentageFromSno(462250)); // Leger's Disdain
            itemAffixInfos.Add(467604, ItemAffixInfo.PercentageFromSno(467604)); // Mask of Scarlet Death
            itemAffixInfos.Add(467609, ItemAffixInfo.PercentageFromSno(467609)); // Corpsewhisper pauldrons
            itemAffixInfos.Add(467568, ItemAffixInfo.PercentageFromSno(467568)); // Bloodsong Mail
            itemAffixInfos.Add(467564, ItemAffixInfo.PercentageFromSno(467564)); // Steuart Greaves
            itemAffixInfos.Add(467610, ItemAffixInfo.PercentageFromSno(467610)); // Razeth's volition
            itemAffixInfos.Add(467569, ItemAffixInfo.PercentageFromSno(467569)); // Requiem Cereplate
            itemAffixInfos.Add(467577, ItemAffixInfo.PercentageFromSno(467577)); // Defiler cuisses
            itemAffixInfos.Add(467565, ItemAffixInfo.PercentageFromSno(467565)); // Bryner's Journey
            itemAffixInfos.Add(467573, ItemAffixInfo.PercentageFromSno(467573)); // Grasps of Essence
            itemAffixInfos.Add(476592, ItemAffixInfo.SecondFromSno(476592)); // Circle of Nailuj Evol
            itemAffixInfos.Add(476594, ItemAffixInfo.PercentageFromSno(476594)); // Krysbin Sentence
            itemAffixInfos.Add(476595, ItemAffixInfo.PercentageFromSno(476595)); // Lornelle's Sunstone
            itemAffixInfos.Add(476716, ItemAffixInfo.PercentageFromSno(476716)); // The Johnstone
            itemAffixInfos.Add(476720, ItemAffixInfo.PercentageFromSno(476720)); // Dayntee's Binding
            itemAffixInfos.Add(484595, ItemAffixInfo.PercentageFromSno(484595)); // Gelmindor's Marrow Guards
        }

        private void addInfosWitchDoctor(Dictionary<uint, ItemAffixInfo> itemAffixInfos)
        {
            itemAffixInfos.Add(195174, ItemAffixInfo.PercentageFromSno(195174)); // The Barber
            itemAffixInfos.Add(445265, ItemAffixInfo.PercentageFromSno(445265)); // Lakumba's Ornament
            itemAffixInfos.Add(222978, ItemAffixInfo.PercentageFromSno(222978)); // Spider queen's grasp
            itemAffixInfos.Add(195370, ItemAffixInfo.SecondFromSno(195370)); // Last breath
            itemAffixInfos.Add(197095, ItemAffixInfo.NoUnitFromSno(197095)); // Scrimshaw, No Unit post S22
            itemAffixInfos.Add(184228, ItemAffixInfo.PercentageFromSno(184228)); // Staff of Chiroptera
            itemAffixInfos.Add(194995, ItemAffixInfo.PercentageFromSno(194995)); // Gazing Demise
            itemAffixInfos.Add(191278, ItemAffixInfo.PercentageFromSno(191278)); // Uhkapian Serpent
            itemAffixInfos.Add(197624, ItemAffixInfo.PercentageFromSno(197624)); // Augustine's Panacea
            itemAffixInfos.Add(197630, ItemAffixInfo.PercentageFromSno(197630)); // Dead Man Legacy
            itemAffixInfos.Add(209057, ItemAffixInfo.PercentageFromSno(209057)); // Swamp Land Waders
            itemAffixInfos.Add(221382, ItemAffixInfo.SecondFromSno(221382)); // Tiklandian Visage
            itemAffixInfos.Add(193674, ItemAffixInfo.PercentageFromSno(193674)); // Bakuli Jungle Wraps
            itemAffixInfos.Add(445697, ItemAffixInfo.PercentageFromSno(445697)); // Ring of Emptiness
            itemAffixInfos.Add(440431, ItemAffixInfo.PercentageFromSno(440431)); // Jeram's Bracers
            itemAffixInfos.Add(440432, ItemAffixInfo.NoUnitFromSno(440432)); // Coils of the First Spider
            itemAffixInfos.Add(432666, ItemAffixInfo.PercentageFromSno(432666)); // The Short Man's Finger
            itemAffixInfos.Add(220326, ItemAffixInfo.PercentageFromSno(220326)); // Vile Hive
            itemAffixInfos.Add(395199, ItemAffixInfo.PercentageFromSno(395199)); // Henri's Perquisition
            itemAffixInfos.Add(299442, ItemAffixInfo.PercentageFromSno(299442)); // Carnevil
            itemAffixInfos.Add(299443, ItemAffixInfo.PercentageFromSno(299443)); // Mask of Jeram
            itemAffixInfos.Add(403767, ItemAffixInfo.PercentageFromSno(403767)); // The Dagger of Darts
        }

        private void addInfosWizard(Dictionary<uint, ItemAffixInfo> itemAffixInfos)
        {
            itemAffixInfos.Add(182074, ItemAffixInfo.PercentageFromSno(182074)); // Starfire
            itemAffixInfos.Add(271774, ItemAffixInfo.PercentageFromSno(271774)); // The Smoldering Core
            itemAffixInfos.Add(223577, ItemAffixInfo.PercentageFromSno(223577)); // Mempo of Twillight
            itemAffixInfos.Add(415050, ItemAffixInfo.PercentageFromSno(415050)); // Nilfur's Boast
            itemAffixInfos.Add(298130, ItemAffixInfo.PercentageFromSno(298130)); // Jang's Envelopment           
            itemAffixInfos.Add(272022, ItemAffixInfo.PercentageFromSno(272022)); // Mirrorball
            itemAffixInfos.Add(271773, ItemAffixInfo.PercentageFromSno(271773)); // Valthek's Rebuke
            itemAffixInfos.Add(325579, ItemAffixInfo.PercentageFromSno(325579)); // The Magistrate
            itemAffixInfos.Add(219329, ItemAffixInfo.PercentageFromSno(219329)); // Wizardspike
            itemAffixInfos.Add(192167, ItemAffixInfo.PercentageFromSno(192167)); // Grand Vizier
            itemAffixInfos.Add(182071, ItemAffixInfo.PercentageFromSno(182071)); // Gesture of Orpheus
            itemAffixInfos.Add(181995, ItemAffixInfo.PercentageFromSno(181995)); // Fragment of Destiny
            itemAffixInfos.Add(184199, ItemAffixInfo.PercentageFromSno(184199)); // Winter Flurry
            itemAffixInfos.Add(195325, ItemAffixInfo.PercentageFromSno(195325)); // Triumvirate
            itemAffixInfos.Add(192320, ItemAffixInfo.PercentageFromSno(192320)); // The Oculus
            itemAffixInfos.Add(193686, ItemAffixInfo.PercentageFromSno(193686)); // Ashnagarr's Blood Bracer
            itemAffixInfos.Add(218681, ItemAffixInfo.SecondFromSno(218681)); // The Swami
            itemAffixInfos.Add(224908, ItemAffixInfo.SecondFromSno(224908)); // Dark Mage's Shade
            itemAffixInfos.Add(220694, ItemAffixInfo.PercentageFromSno(220694)); // Storm Crow
            itemAffixInfos.Add(212546, ItemAffixInfo.PercentageFromSno(212546)); // Manald Heal
            itemAffixInfos.Add(212602, ItemAffixInfo.PercentageFromSno(212602)); // Halo Of Arlyse
            itemAffixInfos.Add(271634, ItemAffixInfo.PercentageFromSno(271634)); // Twisted Sword
            itemAffixInfos.Add(331908, ItemAffixInfo.PercentageFromSno(331908)); // Deathwish
            itemAffixInfos.Add(440424, ItemAffixInfo.NoUnitFromSno(440424)); // Fazula's Improbable Chain
            itemAffixInfos.Add(298123, ItemAffixInfo.PercentageFromSno(298123)); // Ranslor's folly
            itemAffixInfos.Add(449039, ItemAffixInfo.PercentageFromSno(449039)); // Halo of Karini
            itemAffixInfos.Add(272086, ItemAffixInfo.PercentageFromSno(272086)); // Wand of Woh
            itemAffixInfos.Add(272084, ItemAffixInfo.PercentageFromSno(272084)); // Serpent's Sparker
            itemAffixInfos.Add(380733, ItemAffixInfo.PercentageFromSno(380733)); // Unstable Scepter
            itemAffixInfos.Add(399318, ItemAffixInfo.PercentageFromSno(399318)); // Etched Sigil
            itemAffixInfos.Add(399319, ItemAffixInfo.PercentageFromSno(399319)); // Orb of Infinite Depth
            itemAffixInfos.Add(449047, ItemAffixInfo.PercentageFromSno(449047)); // Hergbrash's Binding
            itemAffixInfos.Add(440426, ItemAffixInfo.NoUnitFromSno(440426)); // Shame of Delsere
        }
    }
}