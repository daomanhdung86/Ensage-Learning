using System;
using System.Linq;

using Ensage;
using SharpDX;
using Ensage.Common.Extensions;
using Ensage.Common;
using Ensage.Common.Menu;
using SharpDX.Direct3D9;
using System.Windows.Input;

namespace Control_Creeps
{
    internal class Program
    {
        // Declare a Menu
        private static readonly Menu Menu = new Menu("Control Creeps", "Control Creeps", true);
        private static Hero TargetNow = new Hero();
        private static Hero TargetLock = new Hero();

        private static string test_meDistant;
        private static string test_allyDistant;

        private static bool IsTeamHaveDisable = false;

        private static Item midas, abyssal, mjollnir, boots, medall, mom;
        private static Font txt;
        private static Font not;
        //private static Key KeyControl = Key.T;


        static void Main(string[] args)
        {
            // Create sub Menu
            var mn_KeySetting = new Menu("Keys Setting", "Keys Setting");
            mn_KeySetting.AddItem(new MenuItem("Toogle Key", "Toogle Key").SetValue(new KeyBind('T', KeyBindType.Toggle)));
            mn_KeySetting.AddItem(new MenuItem("Press Key", "Press Key").SetValue(new KeyBind('F', KeyBindType.Press)));
            mn_KeySetting.AddItem(new MenuItem("Lock target Key", "Lock target Key").SetValue(new KeyBind('G', KeyBindType.Press)).SetTooltip("Lock a target closest mouse."));
            Menu.AddSubMenu(mn_KeySetting);

            var mn_GlobalSetting = new Menu("Global Setting", "Global Setting");
            //mn_GlobalSetting.AddItem(new MenuItem("Control mode", "Control mode").SetValue(new StringList(new[] { "Combat mode", "Push mode", "Protect mode"})));
            mn_GlobalSetting.AddItem(new MenuItem("Target find range", "Target find range").SetValue(new Slider(1550, 0, 2000)).SetTooltip("Range from mouse to find TargetNow Hero."));
            mn_GlobalSetting.AddItem(new MenuItem("Target mode", "Target mode").SetValue(new StringList(new[] { "ClosesFindSource", "LowestHealth" })));
            mn_GlobalSetting.AddItem(new MenuItem("Target find source", "Target find source").SetValue(new StringList(new[] { "Me", "Mouse" })));
            mn_GlobalSetting.AddItem(new MenuItem("Combat range", "Combat range").SetValue(new Slider(2000, 0, 5000)).SetTooltip("Unit will move to attack Target in Combat range from itself."));

            Menu.AddSubMenu(mn_GlobalSetting);

            //var mn_CombatSetting = new Menu("Combat setting", "Combat setting");
            //mn_CombatSetting.AddItem(new MenuItem("Combat mode", "Combat mode").SetValue(new StringList(new[] { "MaxDisableOneTarget", "DisableMultiTarget" })));
            //Menu.AddSubMenu(mn_CombatSetting);

            //var mn_PushSetting = new Menu("Push setting", "Push setting");
            //Menu.AddSubMenu(mn_PushSetting);


            //var mn_ProtectSetting = new Menu("Protect setting", "Protect setting");
            //Menu.AddSubMenu(mn_ProtectSetting);

            Menu.AddToMainMenu();

            Game.OnUpdate += Game_OnUpdate;
            Console.WriteLine("> ControlCreep By Vick# loaded!");

            //Game.OnWndProc += Game_OnWndProc;
            
            txt = new Font(
               Drawing.Direct3DDevice9,
               new FontDescription
               {
                   FaceName = "Tahoma",
                   Height = 11,
                   OutputPrecision = FontPrecision.Default,
                   Quality = FontQuality.Default
               });

            not = new Font(
               Drawing.Direct3DDevice9,
               new FontDescription
               {
                   FaceName = "Tahoma",
                   Height = 12,
                   OutputPrecision = FontPrecision.Default,
                   Quality = FontQuality.Default
               });

            Drawing.OnPreReset += Drawing_OnPreReset;
            Drawing.OnPostReset += Drawing_OnPostReset;
            Drawing.OnEndScene += Drawing_OnEndScene;
            AppDomain.CurrentDomain.DomainUnload += CurrentDomain_DomainUnload;
        }

        public static void Game_OnUpdate(EventArgs args)
        {
            var me = ObjectManager.LocalHero;
			if (!Game.IsInGame || me == null)
			{
				return;
			}
            // Pick TargetNow
            var holdKey = Menu.Item("Press Key").GetValue<KeyBind>().Active;
            var toggleKey = Menu.Item("Toogle Key").GetValue<KeyBind>().Active;
            var lockTargetKey = Menu.Item("Lock target Key").GetValue<KeyBind>().Active;

            var TargetMode = Menu.Item("Target mode").GetValue<StringList>().SelectedIndex;
            var MaxAttackRange = Menu.Item("Combat range").GetValue<Slider>().Value;
            var TargetFindRange = Menu.Item("Target find range").GetValue<Slider>().Value;
            var TargetFindSource = Menu.Item("Target find source").GetValue<StringList>().SelectedIndex;

            //var CombatMode = Menu.Item("Combat mode").GetValue<StringList>().SelectedIndex;

            if (lockTargetKey)
            {
                TargetLock = TargetSelector.ClosestToMouse(me, TargetFindRange);
            }

            if (TargetLock != null)
            {
                if (TargetLock.IsAlive)
                {
                    TargetNow = TargetLock;
                }
                else
                {
                    switch (TargetMode)
                    {
                        case 0:
                            switch (TargetFindSource)
                            {
                                case 0:
                                    var EnemyHero0 = ObjectManager.GetEntities<Hero>().Where(enemy => enemy.Team == me.GetEnemyTeam() && enemy.IsAlive && !enemy.IsIllusion && enemy.Distance2D(Game.MousePosition) <= TargetFindRange).ToList();
                                    TargetNow = EnemyHero0.MinOrDefault(x => x.Distance2D(me.Position));
                                    break;
                                case 1:
                                    TargetNow = TargetSelector.ClosestToMouse(me, TargetFindRange);
                                    break;
                            }
                            break;
                        case 1:
                            switch (TargetFindSource)
                            {
                                case 0:
                                    var EnemyHero0 = ObjectManager.GetEntities<Hero>().Where(enemy => enemy.Team == me.GetEnemyTeam() && enemy.IsAlive && !enemy.IsIllusion && enemy.Distance2D(Game.MousePosition) <= TargetFindRange).ToList();
                                    TargetNow = EnemyHero0.MinOrDefault(x => x.Health);
                                    break;
                                case 1:
                                    var EnemyHero1 = ObjectManager.GetEntities<Hero>().Where(enemy => enemy.Team == me.GetEnemyTeam() && enemy.IsAlive && !enemy.IsIllusion && enemy.Distance2D(me.Position) <= TargetFindRange).ToList();
                                    TargetNow = EnemyHero1.MinOrDefault(x => x.Health);
                                    break;
                            }
                            break;
                    }
                }
            }
            else
            {
                switch (TargetMode)
                {
                    case 0:
                        TargetNow = TargetSelector.ClosestToMouse(me, TargetFindRange);
                        break;
                    case 1:
                        var EnemyHero = ObjectManager.GetEntities<Hero>().Where(enemy => enemy.Team == me.GetEnemyTeam() && enemy.IsAlive && !enemy.IsIllusion && enemy.Distance2D(Game.MousePosition) <= TargetFindRange).ToList();
                        TargetNow = EnemyHero.MinOrDefault(x => x.Health);
                        break;
                }
            }
            
            if ((holdKey || toggleKey) && me.IsAlive)
			{
                var AllControlableUnit = ObjectManager.GetEntities<Unit>().Where(unit => unit.IsAlive && unit.IsControllable && unit.Team == me.Team).ToList();
                if (AllControlableUnit == null)
                {
                    return;
                }
                else
                {
                    foreach (var v in AllControlableUnit)
                    {
                        // 
                        // Control Juggernaut_Ward follow him
                        if (me.ClassID == ClassID.CDOTA_Unit_Hero_Juggernaut)
                        {
                            if (v.ClassID == ClassID.CDOTA_BaseNPC_Additive)
                            {
                                if (me.Position.Distance2D(v.Position) > 5 && Utils.SleepCheck(v.Handle.ToString() + "M"))
                                {
                                    v.Move(me.Position);
                                    Utils.Sleep(100, v.Handle.ToString() + "M");
                                }
                            }
                        }
                        
                        // Control Ogre Frostmage buff Ice Armor
                        if (v.Name == "npc_dota_neutral_ogre_magi")
                        {
                            var allyHero = ObjectManager.GetEntities<Hero>().Where(ally => ally.Team == me.Team && ally.IsAlive && !ally.IsIllusion && v.Distance2D(ally) <= 1500).ToList();
                            var mycreep = ObjectManager.GetEntities<Unit>().Where(creep => creep.ClassID == ClassID.CDOTA_BaseNPC_Creep_Neutral && creep.IsAlive && creep.IsControllable);
                            var armor = v.Spellbook.SpellQ;

                            if (armor.CanBeCasted())
                            {
                                foreach (var ah in allyHero)
                                {
                                    if (!ah.Modifiers.Any(y => y.Name == "modifier_ogre_magi_frost_armor") && ah.Position.Distance2D(v.Position) < armor.CastRange + 300 && Utils.SleepCheck(v.Handle.ToString() + "Q"))
                                    {
                                        armor.UseAbility(ah);
                                        Utils.Sleep(armor.CooldownLength*1000, v.Handle.ToString()+"Q");
                                    }
                                }
                                foreach (var mc in mycreep)
                                {
                                    if (!mc.Modifiers.Any(y => y.Name == "modifier_ogre_magi_frost_armor") && mc.Position.Distance2D(v.Position) <  armor.CastRange + 300 && Utils.SleepCheck(v.Handle.ToString() + "Q"))
                                    {
                                        armor.UseAbility(mc);
                                        Utils.Sleep(armor.CooldownLength*1000, v.Handle.ToString()+"Q");
                                    }
                                }
                            }
                        }

                        if (TargetNow == null)
                        {
                            return;
                        }
                        else
                        {
                            var CheckStomp = TargetNow.Modifiers.Any(y => y.Name == "modifier_centaur_hoof_stomp");
                            var CheckEnsnare = TargetNow.Modifiers.Any(y => y.Name == "modifier_dark_troll_warlord_ensnare");
                            var CheckPurge = TargetNow.Modifiers.Any(y => y.Name == "modifier_satyr_trickster_purge");
                            var CheckSlam = TargetNow.Modifiers.Any(y => y.Name == "modifier_big_thunder_lizard_slam");

                            var Neutrals = ObjectManager.GetEntities<Creep>().Where(creep => (creep.ClassID == ClassID.CDOTA_BaseNPC_Creep_Lane || creep.ClassID == ClassID.CDOTA_BaseNPC_Creep_Siege || creep.ClassID == ClassID.CDOTA_BaseNPC_Creep_Neutral || creep.ClassID == ClassID.CDOTA_BaseNPC_Invoker_Forged_Spirit || creep.ClassID == ClassID.CDOTA_BaseNPC_Creep &&
                                creep.IsAlive && creep.IsVisible && creep.IsSpawned) && creep.Team == me.GetEnemyTeam()).ToList();
                            var Neutral = ObjectManager.GetEntities<Creep>().Where(creep => (creep.ClassID == ClassID.CDOTA_BaseNPC_Creep_Neutral &&
                                creep.IsAlive ) && creep.Team == me.GetEnemyTeam()).ToList();

                            switch (v.ClassID)
                            {
                                case ClassID.CDOTA_BaseNPC_Tusk_Sigil:
                                    if (TargetNow.Position.Distance2D(v.Position) < MaxAttackRange && Utils.SleepCheck(v.Handle.ToString()))
                                    {
                                        v.Follow(TargetNow);
                                        Utils.Sleep(100, v.Handle.ToString());
                                    }
                                    break;
                                case ClassID.CDOTA_Unit_SpiritBear:
                                    if ((!me.AghanimState() && me.Position.Distance2D(v) <= 1200) || me.AghanimState())
	                                {
						                if (abyssal == null)
							                abyssal = v.FindItem("item_abyssal_blade");

						                if (mjollnir == null)
							                mjollnir = v.FindItem("item_mjollnir");

						                if (boots == null)
							                boots = v.FindItem("item_phase_boots");

						                if (midas == null)
							                midas = v.FindItem("item_hand_of_midas");

						                if (mom == null)
							                mom = v.FindItem("item_mask_of_madness");

						                if (medall == null)
							                medall = v.FindItem("item_medallion_of_courage");

                                        if (boots != null && TargetNow.Position.Distance2D(v.Position) < MaxAttackRange && boots.CanBeCasted() &&
                                            Utils.SleepCheck(v.Handle.ToString()))
                                        {
                                            v.FindItem("item_phase_boots").UseAbility();
                                            Utils.Sleep(v.FindItem("item_phase_boots").CooldownLength*1000, v.Handle.ToString());
                                        }
                                        if (mjollnir != null && TargetNow.Position.Distance2D(v.Position) < 525 && mjollnir.CanBeCasted() &&
                                            Utils.SleepCheck(v.Handle.ToString()))
                                        {
                                            v.FindItem("item_mjollnir").UseAbility(v);
                                            Utils.Sleep(350, v.Handle.ToString());
                                        }

						                if (medall != null && TargetNow.Position.Distance2D(v.Position) < 525 && medall.CanBeCasted()  &&
						                   Utils.SleepCheck(v.Handle.ToString()))
						                {
							                v.FindItem("item_medallion_of_courage").UseAbility(TargetNow);
							                Utils.Sleep(350, v.Handle.ToString());
						                }

						                if (mom != null && TargetNow.Position.Distance2D(v.Position) < 525 && mom.CanBeCasted() &&
						                   Utils.SleepCheck(v.Handle.ToString()))
						                {
							                v.FindItem("item_mask_of_madness").UseAbility();
							                Utils.Sleep(350, v.Handle.ToString());
						                }
						                if (abyssal != null && TargetNow.Position.Distance2D(v.Position) < 170 && abyssal.CanBeCasted() &&
                                            Utils.SleepCheck(v.Handle.ToString()))
                                        {
                                            v.FindItem("item_abyssal_blade").UseAbility(TargetNow);
                                            Utils.Sleep(350, v.Handle.ToString());
                                        }
						                if (midas != null)
						                {
							                foreach (var f in Neutrals)
							                {
								                if (TargetNow.Position.Distance2D(f.Position) < 650 && midas.CanBeCasted() &&
									                Utils.SleepCheck(f.Handle.ToString()))
								                {
									                v.FindItem("item_hand_of_midas").UseAbility(f);
									                Utils.Sleep(350, f.Handle.ToString());
								                }
							                }
						                }
                                    }
                                    break;
                                case ClassID.CDOTA_Unit_VisageFamiliar:
                                    var damageModif = v.Modifiers.FirstOrDefault(x => x.Name == "modifier_visage_summon_familiars_damage_charge");
                                    if (v.Health < 5 && v.Spellbook.Spell1.CanBeCasted() && Utils.SleepCheck(v.Handle.ToString() + "Q"))
                                    {
                                        v.Spellbook.Spell1.UseAbility();
                                        Utils.Sleep(v.Spellbook.Spell1.CooldownLength*1000, v.Handle.ToString()+"Q");
                                    }
                                    if (TargetNow.Position.Distance2D(v.Position) < v.Spellbook.Spell1.GetRadius()-50 && ((damageModif.StackCount < 2) && !TargetNow.IsStunned()) && v.Spellbook.Spell1.CanBeCasted() && Utils.SleepCheck(v.Handle.ToString() + "Q"))
                                    {
                                        v.Spellbook.Spell1.UseAbility();
                                        Utils.Sleep(v.Spellbook.Spell1.CooldownLength * 1000, v.Handle.ToString() + "Q");
                                    }
                                    break;
                                case ClassID.CDOTA_Unit_Brewmaster_PrimalFire:
                                    break;
                                case ClassID.CDOTA_Unit_Brewmaster_PrimalEarth:
                                    // Stun 2s
                                    if (TargetNow.Position.Distance2D(v.Position) < v.Spellbook.SpellQ.CastRange && v.Spellbook.SpellQ.CanBeCasted() && Utils.SleepCheck(v.Handle.ToString()+"Q"))
                                    {
                                        v.Spellbook.SpellQ.UseAbility(TargetNow);
                                        Utils.Sleep(v.Spellbook.SpellQ.CooldownLength*1000, v.Handle.ToString()+"Q");
                                    }
                                    // Slam AOE
                                    if (TargetNow.Position.Distance2D(v.Position) < v.Spellbook.SpellR.GetRadius() - 50 && v.Spellbook.SpellR.CanBeCasted() && Utils.SleepCheck(v.Handle.ToString()+"R"))
                                    {
                                        v.Spellbook.SpellR.UseAbility();
                                        Utils.Sleep(v.Spellbook.SpellR.CooldownLength * 1000, v.Handle.ToString() + "R");
                                    }
                                    break;
                                case ClassID.CDOTA_Unit_Brewmaster_PrimalStorm:
                                    // Dispel Magic - Dame ilu, purge; 
                                    if (TargetNow.Position.Distance2D(v.Position) < v.Spellbook.SpellQ.CastRange && v.Spellbook.SpellQ.CanBeCasted() && Utils.SleepCheck(v.Handle.ToString() + "Q"))
                                    {
                                        v.Spellbook.SpellQ.UseAbility(TargetNow.Position);
                                        Utils.Sleep(v.Spellbook.SpellQ.CooldownLength*1000 , v.Handle.ToString()+"Q");
                                    }
                                    // Cyclone - EUL 6s; for run, break channeling
                                    if (TargetNow.Position.Distance2D(v.Position) < v.Spellbook.SpellW.CastRange && v.Spellbook.SpellW.CanBeCasted() && Utils.SleepCheck(v.Handle.ToString() + "W"))
                                    {
                                        if (v.Health<200 || TargetNow.IsChanneling())
                                        {
                                            v.Spellbook.SpellW.UseAbility(TargetNow.Position);
                                            Utils.Sleep(v.Spellbook.SpellW.CooldownLength*1000, v.Handle.ToString() + "W");
                                        }
                                    }
                                    // Wind walk
                                    if (TargetNow.Position.Distance2D(v.Position) < 900 && v.Spellbook.SpellE.CanBeCasted() && Utils.SleepCheck(v.Handle.ToString() + "E"))
                                    {
                                        v.Spellbook.SpellE.UseAbility();
                                        Utils.Sleep(v.Spellbook.SpellE.CooldownLength*1000, v.Handle.ToString() + "E");
                                    }
                                    // Drunken Haze - make missing. 
                                    if (TargetNow.Position.Distance2D(v.Position) < v.Spellbook.SpellR.CastRange && v.Spellbook.SpellR.CanBeCasted() && Utils.SleepCheck(v.Handle.ToString() + "R"))
                                    {
                                        v.Spellbook.SpellR.UseAbility(TargetNow);
                                        Utils.Sleep(v.Spellbook.SpellR.CooldownLength*1000, v.Handle.ToString() + "R");
                                    }
                                    break;
                                case ClassID.CDOTA_BaseNPC_Invoker_Forged_Spirit:
                                    break;
                                case ClassID.CDOTA_BaseNPC_Warlock_Golem:
                                    break;
                                case ClassID.CDOTA_NPC_WitchDoctor_Ward:
                                    break;
                                case ClassID.CDOTA_BaseNPC_ShadowShaman_SerpentWard:
                                    break;
                                case ClassID.CDOTA_Unit_Hero_Beastmaster_Boar:
                                    break;
                                case ClassID.CDOTA_Unit_Hero_Beastmaster_Beasts:
                                    break;
                                case ClassID.CDOTA_Unit_Hero_Beastmaster_Hawk:
                                    continue;
                                case ClassID.CDOTA_BaseNPC_Creep_Neutral:
                                    switch (v.Name)
                                    {
                                        case "npc_dota_neutral_dark_troll_warlord":
                                            if (v.Spellbook.SpellQ.CanBeCasted() && v.Mana >= v.Spellbook.SpellQ.ManaCost)
                                            {
                                                if (TargetNow.Position.Distance2D(v.Position) < v.Spellbook.SpellQ.CastRange && (!CheckStomp && !CheckEnsnare && !CheckPurge && !TargetNow.IsRooted() && !TargetNow.IsHexed() && !TargetNow.IsStunned()) && Utils.SleepCheck(v.Handle.ToString() + "Q"))
                                                {
                                                    v.Spellbook.SpellQ.UseAbility(TargetNow);
                                                    Utils.Sleep(v.Spellbook.SpellQ.CooldownLength * 1000, v.Handle.ToString() + "Q");
                                                }
                                            }
                                            if (v.Spellbook.SpellW.CanBeCasted() && v.Mana >= v.Spellbook.SpellW.ManaCost)
                                            {
                                                if (TargetNow.Position.Distance2D(v.Position) < 550 && Utils.SleepCheck(v.Handle.ToString() + "W"))
                                                {
                                                    v.Spellbook.SpellW.UseAbility();
                                                    Utils.Sleep(v.Spellbook.SpellW.CooldownLength * 1000, v.Handle.ToString() + "W");
                                                }
                                            }
                                            break;
                                        case "npc_dota_neutral_centaur_khan":
                                            if (v.Spellbook.SpellQ.CanBeCasted() && v.Mana >= v.Spellbook.SpellQ.ManaCost)
                                            {
                                                if (TargetNow.Position.Distance2D(v.Position) < v.Spellbook.SpellQ.GetRadius() - 50 && (!CheckStomp && !CheckEnsnare && !TargetNow.IsRooted() && !TargetNow.IsHexed() && !TargetNow.IsStunned()) && Utils.SleepCheck(v.Handle.ToString() + "Q"))
                                                {
                                                    v.Spellbook.SpellQ.UseAbility();
                                                    Utils.Sleep(v.Spellbook.SpellQ.CooldownLength * 1000, v.Handle.ToString() + "Q");
                                                }
                                            }
                                            break;
                                        case "npc_dota_neutral_satyr_hellcaller":
                                            if (v.Spellbook.SpellQ.CanBeCasted() && v.Mana >= v.Spellbook.SpellQ.ManaCost)
                                            {
                                                if (TargetNow.Position.Distance2D(v.Position) < 850 && Utils.SleepCheck(v.Handle.ToString() + "Q"))
                                                {
                                                     if (TargetNow.Position.Distance2D(v.Position) < 550)
                                                     {
                                                        v.Spellbook.SpellQ.UseAbility(TargetNow.Position);
                                                        Utils.Sleep(v.Spellbook.SpellQ.CooldownLength * 1000, v.Handle.ToString() + "Q");
                                                     }
                                                     else
                                                     {
                                                         if (CheckStomp || CheckEnsnare || TargetNow.IsRooted() || TargetNow.IsHexed() || TargetNow.IsStunned())
                                                         {
                                                            v.Spellbook.SpellQ.UseAbility(TargetNow.Position);
                                                            Utils.Sleep(v.Spellbook.SpellQ.CooldownLength * 1000, v.Handle.ToString() + "Q");
                                                         }
                                                     }
                                                }
                                            }
                                            break;
                                        case "npc_dota_neutral_satyr_soulstealer":
                                            if (v.Spellbook.SpellQ.CanBeCasted() && v.Mana >= v.Spellbook.SpellQ.ManaCost)
                                            {
                                                if (TargetNow.Position.Distance2D(v.Position) < v.Spellbook.SpellQ.CastRange && Utils.SleepCheck(v.Handle.ToString() + "Q"))
                                                {
                                                    v.Spellbook.SpellQ.UseAbility(TargetNow);
                                                    Utils.Sleep(v.Spellbook.SpellQ.CooldownLength * 1000, v.Handle.ToString() + "Q");
                                                }
                                            }
                                            break;
                                        case "npc_dota_neutral_satyr_trickster":
                                            if (v.Spellbook.SpellQ.CanBeCasted() && v.Mana >= v.Spellbook.SpellQ.ManaCost)
                                            {
                                                if (TargetNow.Position.Distance2D(v.Position) < v.Spellbook.SpellQ.CastRange && (!CheckStomp && !CheckEnsnare && !CheckPurge && !TargetNow.IsRooted() && !TargetNow.IsHexed() && !TargetNow.IsStunned()) && Utils.SleepCheck(v.Handle.ToString() + "Q"))
                                                {
                                                    v.Spellbook.SpellQ.UseAbility(TargetNow);
                                                    Utils.Sleep(v.Spellbook.SpellQ.CooldownLength * 1000, v.Handle.ToString() + "Q");
                                                }
                                            }
                                            break;
                                        case "npc_dota_neutral_polar_furbolg_ursa_warrior":
                                            if (v.Spellbook.SpellQ.CanBeCasted() && v.Mana >= v.Spellbook.SpellQ.ManaCost)
                                            {
                                                if (TargetNow.Position.Distance2D(v.Position) < v.Spellbook.SpellQ.GetRadius() - 50 && Utils.SleepCheck(v.Handle.ToString() + "Q"))
                                                {
                                                    v.Spellbook.SpellQ.UseAbility();
                                                    Utils.Sleep(v.Spellbook.SpellQ.CooldownLength * 1000, v.Handle.ToString() + "Q");
                                                }
                                            }
                                            break;
                                        case "npc_dota_neutral_mud_golem":
                                            if (v.Spellbook.SpellQ.CanBeCasted() && v.Mana >= v.Spellbook.SpellQ.ManaCost)
                                            {
                                                if (TargetNow.Position.Distance2D(v.Position) < v.Spellbook.SpellQ.CastRange && TargetNow.IsChanneling() && Utils.SleepCheck(v.Handle.ToString() + "Q"))
                                                {
                                                    v.Spellbook.SpellQ.UseAbility(TargetNow);
                                                    Utils.Sleep(v.Spellbook.SpellQ.CooldownLength * 1000, v.Handle.ToString() + "Q");
                                                }
                                                if (TargetNow.Position.Distance2D(v.Position) < v.Spellbook.SpellQ.CastRange && (!CheckStomp && !CheckEnsnare && !CheckPurge && !TargetNow.IsRooted() && !TargetNow.IsHexed() && !TargetNow.IsStunned()) && Utils.SleepCheck(v.Handle.ToString() + "Q"))
                                                {
                                                    v.Spellbook.SpellQ.UseAbility(TargetNow);
                                                    Utils.Sleep(v.Spellbook.SpellQ.CooldownLength * 1000, v.Handle.ToString() + "Q");
                                                }
                                            }
                                            break;
                                        case "npc_dota_neutral_harpy_storm":
                                            if (v.Spellbook.SpellQ.CanBeCasted() && v.Mana >= v.Spellbook.SpellQ.ManaCost)
                                            {
                                                if (TargetNow.Position.Distance2D(v.Position) < v.Spellbook.SpellQ.CastRange && Utils.SleepCheck(v.Handle.ToString() + "Q"))
                                                {
                                                    v.Spellbook.SpellQ.UseAbility(TargetNow);
                                                    Utils.Sleep(v.Spellbook.SpellQ.CooldownLength * 1000, v.Handle.ToString() + "Q");
                                                }
                                            }
                                            break;
                                        case "npc_dota_neutral_big_thunder_lizard":
                                            if (v.Spellbook.SpellQ.CanBeCasted() && v.Mana >= v.Spellbook.SpellQ.ManaCost)
                                            {
                                                if (TargetNow.Position.Distance2D(v.Position) < v.Spellbook.SpellQ.GetRadius()-50 && Utils.SleepCheck(v.Handle.ToString() + "Q"))
                                                {
                                                    v.Spellbook.SpellQ.UseAbility();
                                                    Utils.Sleep(v.Spellbook.SpellQ.CooldownLength * 1000, v.Handle.ToString() + "Q");
                                                }
                                            }
                                            if (v.Spellbook.SpellW.CanBeCasted() && v.Mana >= v.Spellbook.SpellW.ManaCost)
                                            {
                                                var allyHero = ObjectManager.GetEntities<Hero>().Where(ally => ally.Team == me.Team && ally.IsAlive && !ally.IsIllusion && v.Distance2D(ally) <= 1000).ToList();
                                                var mycreep = ObjectManager.GetEntities<Unit>().Where(creep => creep.ClassID == ClassID.CDOTA_BaseNPC_Creep_Neutral && creep.IsAlive && creep.IsControllable && v.Distance2D(creep) <= 1000);

                                                if (Utils.SleepCheck(v.Handle.ToString() + "W"))
                                                {
                                                    var BestHero = allyHero.MaxOrDefault(x => x.DamageAverage);
                                                    v.Spellbook.SpellW.UseAbility(BestHero);
                                                    Utils.Sleep(v.Spellbook.SpellW.CooldownLength * 1000, v.Handle.ToString() + "W");
                                                }
                                                if (Utils.SleepCheck(v.Handle.ToString() + "W"))
                                                {
                                                    var BestUnit = mycreep.MaxOrDefault(x => x.DamageAverage);
                                                    v.Spellbook.SpellW.UseAbility(BestUnit);
                                                    Utils.Sleep(v.Spellbook.SpellW.CooldownLength * 1000, v.Handle.ToString() + "W");
                                                }
                                            }

                                            break;
                                        case "npc_dota_neutral_black_dragon":
                                            if (v.Spellbook.SpellQ.CanBeCasted() && v.Mana >= v.Spellbook.SpellQ.ManaCost)
                                            {
                                                if (TargetNow.Position.Distance2D(v.Position) < v.Spellbook.SpellQ.CastRange && Utils.SleepCheck(v.Handle.ToString() + "Q"))
                                                {
                                                    v.Spellbook.SpellQ.UseAbility(TargetNow.Predict(600));
                                                    Utils.Sleep(v.Spellbook.SpellQ.CooldownLength * 1000, v.Handle.ToString() + "Q");
                                                }
                                            }
                                            break;
                                    }
                                    break;
                                case ClassID.CDOTA_BaseNPC_Creep:
                                    switch (v.Name)
                                    {
                                        case "npc_dota_necronomicon_archer_1":
                                            if (v.Spellbook.SpellQ.CanBeCasted() && v.Mana >= v.Spellbook.SpellQ.ManaCost)
                                            {
                                                if (TargetNow.Position.Distance2D(v.Position) <= v.Spellbook.SpellQ.CastRange && Utils.SleepCheck(v.Handle.ToString() + "Q"))
                                                {
                                                    v.Spellbook.SpellQ.UseAbility(TargetNow);
                                                    Utils.Sleep(v.Spellbook.SpellQ.CooldownLength * 1000, v.Handle.ToString() + "Q");
                                                }
                                            }
                                            break;
                                        case "npc_dota_necronomicon_archer_2":
                                            if (v.Spellbook.SpellQ.CanBeCasted() && v.Mana >= v.Spellbook.SpellQ.ManaCost)
                                            {
                                                if (TargetNow.Position.Distance2D(v.Position) <= v.Spellbook.SpellQ.CastRange && Utils.SleepCheck(v.Handle.ToString() + "Q"))
                                                {
                                                    v.Spellbook.SpellQ.UseAbility(TargetNow);
                                                    Utils.Sleep(v.Spellbook.SpellQ.CooldownLength * 1000, v.Handle.ToString() + "Q");
                                                }
                                            }
                                            break;
                                        case "npc_dota_necronomicon_archer_3":
                                            if (v.Spellbook.SpellQ.CanBeCasted() && v.Mana >= v.Spellbook.SpellQ.ManaCost)
                                            {
                                                if (TargetNow.Position.Distance2D(v.Position) <= v.Spellbook.SpellQ.CastRange && Utils.SleepCheck(v.Handle.ToString() + "Q"))
                                                {
                                                    v.Spellbook.SpellQ.UseAbility(TargetNow);
                                                    Utils.Sleep(v.Spellbook.SpellQ.CooldownLength * 1000, v.Handle.ToString() + "Q");
                                                }
                                            }
                                            break;
                                        case "npc_dota_eidolon":
                                        if (v.Spellbook.SpellQ.CanBeCasted() && v.Mana >= v.Spellbook.SpellQ.ManaCost)
                                        {
                                            if (TargetNow.Position.Distance2D(v.Position) <= v.Spellbook.SpellQ.CastRange && Utils.SleepCheck(v.Handle.ToString() + "Q"))
                                            {
                                                v.Spellbook.SpellQ.UseAbility(TargetNow);
                                                Utils.Sleep(v.Spellbook.SpellQ.CooldownLength * 1000, v.Handle.ToString() + "Q");
                                            }
                                        }
                                        break;
                                    }
                                    break;
                            }
                            if (v.ClassID != me.ClassID && TargetNow.Position.Distance2D(v.Position) < MaxAttackRange && Utils.SleepCheck(v.Handle.ToString() + "A"))
                            {
                                v.Attack(TargetNow);
                                Utils.Sleep(v.SecondsPerAttack*1000, v.Handle.ToString() + "A");
                            }
                        }
                    }
                }
			}
		}
        static void CurrentDomain_DomainUnload(object sender, EventArgs e)
        {
            txt.Dispose();
            not.Dispose();
        }

        static void Drawing_OnEndScene(EventArgs args)
        {
            if (Drawing.Direct3DDevice9 == null || Drawing.Direct3DDevice9.IsDisposed || !Game.IsInGame)
                return;

            var player = ObjectManager.LocalPlayer;

            var holdKey = Menu.Item("Press Key").GetValue<KeyBind>().Active;
            var toggleKey = Menu.Item("Toogle Key").GetValue<KeyBind>().Active;

            if (holdKey || toggleKey)
            {
                txt.DrawText(null, "Creeps Control ON", 1200, 27, Color.Aqua);
                if (TargetNow != null && TargetNow.IsAlive)
                {
                    txt.DrawText(null, "TargetNow = " + TargetNow.Name, 1200, 47, Color.Aqua);
                }
                else
                {
                    txt.DrawText(null, "TargetNow = None", 1200, 47, Color.Aqua);
                }
                if (TargetLock != null)
                {
                    txt.DrawText(null, "TargetLock = " + TargetLock.Name, 1200, 67, Color.Aqua);
                }
                else
                {
                    txt.DrawText(null, "TargetLock = None", 1200, 67, Color.Aqua);
                }
            }
            else
            {
                txt.DrawText(null, "Creeps Control OFF", 1200, 27, Color.Aqua);
            }
        }

		static void Drawing_OnPostReset(EventArgs args)
        {
            txt.OnResetDevice();
            not.OnResetDevice();
        }

        static void Drawing_OnPreReset(EventArgs args)
        {
            txt.OnLostDevice();
            not.OnLostDevice();
        }
    }
}
