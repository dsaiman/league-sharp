using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using Pentakill_Cassiopeia.Util;

namespace Pentakill_Cassiopeia
{
    class GameLogic
    {

        static int lastECast = 0;

        public static void Checks()
        {
            //Updates the auto-leveling sequence each time (redundant)
            List<SpellSlot> SKILL_SEQUENCE = new List<SpellSlot>() { SpellSlot.Q, SpellSlot.E, SpellSlot.E, SpellSlot.W, SpellSlot.E, SpellSlot.R, SpellSlot.E, SpellSlot.Q, SpellSlot.E, SpellSlot.Q, SpellSlot.R, SpellSlot.Q, SpellSlot.Q, SpellSlot.W, SpellSlot.W, SpellSlot.R, SpellSlot.W, SpellSlot.W };
            AutoLevel.UpdateSequence(SKILL_SEQUENCE);
            //Enables if user selects yes else disables
            if (Program.menuController.getMenu().Item("autoLevel").GetValue<bool>())
                AutoLevel.Enable();
            else
                AutoLevel.Disable();
        }

        public static void performCombo()
        {
            //Gets best target in Q range
            Obj_AI_Hero target = TargetSelector.GetTarget(Program.q.Range, TargetSelector.DamageType.Magical);

            //Ignite handler
            if (Program.menuController.getMenu().Item("useIgnite").GetValue<bool>())
            {
                if (SpellDamage.getComboDamage(target) > target.Health && target.Distance(Program.player.Position) < 500)
                {
                    Program.player.Spellbook.CastSpell(Program.ignite);
                }
            }
            //Ulti handler in combo
            if (Program.menuController.getMenu().Item("comboUseR").GetValue<bool>())
            {
                List<Obj_AI_Hero> targets;
                /* if (Program.menuController.getMenu().Item("faceOnlyR").GetValue<bool>())
                 {
                     //Gets all facing enemies who can get hit by R
                     targets = HeroManager.Enemies.Where(o => Program.r.WillHit(o, target.Position) && o.IsFacing(Program.player)).ToList<Obj_AI_Hero>();
                 }
                 else
                 {*/
                //Gets all enemies who can get hit by R
                targets = HeroManager.Enemies.Where(o => Program.r.WillHit(o, target.Position) && o.Distance(Program.player.Position) < 500).ToList<Obj_AI_Hero>();

                // }
                if (targets.Count >= Program.menuController.getMenu().Item("minEnemies").GetValue<Slider>().Value)
                {
                    Program.r.Cast(target.Position);
                }
            }
            //Casts Q if selected in menu
            if (Program.menuController.getMenu().Item("comboUseQ").GetValue<bool>())
            {
                if (target != null)
                {
                    Program.q.CastIfHitchanceEquals(target, HitChance.High);
                }
            }
            //Casts E if selected in menu and in E range and target is poisoned
            if (Program.menuController.getMenu().Item("comboUseE").GetValue<bool>())
            {
                if (target != null && target.Distance(Program.player.Position) < Program.e.Range && (Environment.TickCount - lastECast) > Program.menuController.getMenu().Item("eDelay").GetValue<Slider>().Value)
                {
                    if (target.HasBuffOfType(BuffType.Poison))
                    {
                        Program.e.CastOnUnit(target);
                        lastECast = Environment.TickCount;

                    }
                }
            }
            //Casts W if selected in menu
            if (Program.menuController.getMenu().Item("comboUseW").GetValue<bool>())
            {
                if (target != null)
                {
                    Program.w.CastIfHitchanceEquals(target, HitChance.High);
                }
            }
        }

        public static void performHarass()
        {
            //Gets best target in Q range
            Obj_AI_Hero target = TargetSelector.GetTarget(Program.q.Range, TargetSelector.DamageType.Magical);
            //Casts Q if selected in menu
            if (Program.menuController.getMenu().Item("harassUseQ").GetValue<bool>())
            {
                if (target != null)
                {
                    Program.q.CastIfHitchanceEquals(target, HitChance.High);
                }
            }
            //Casts E if selected in menu and in E range and target is poisoned
            if (Program.menuController.getMenu().Item("harassUseE").GetValue<bool>())
            {
                if (target != null && target.Distance(Program.player.Position) < Program.e.Range && (Environment.TickCount - lastECast) > Program.menuController.getMenu().Item("eDelay").GetValue<Slider>().Value)
                {
                    if (target.HasBuffOfType(BuffType.Poison))
                    {
                        Program.e.CastOnUnit(target);
                        lastECast = Environment.TickCount;
                    }
                }
            }
            //Casts W if selected in menu
            if (Program.menuController.getMenu().Item("harassUseW").GetValue<bool>())
            {
                if (target != null)
                {
                    Program.w.CastIfHitchanceEquals(target, HitChance.High);
                }
            }
        }

        public static void performLastHit()
        {
            //Gets minion that can be killed with Q + E
            Obj_AI_Base minion = MinionManager.GetMinions(Program.player.Position, Program.e.Range).FirstOrDefault(o => (o.Health < Program.q.GetDamage(o) + Program.e.GetDamage(o)));
            if (Program.menuController.getMenu().Item("lastHitUseQ").GetValue<bool>())
            {
                //Checks if minion will NOT die from Q
                if (minion != null && minion.Health > Program.e.GetDamage(minion))
                {
                    Program.q.CastIfHitchanceEquals(minion, HitChance.High);
                }
            }
            if (Program.menuController.getMenu().Item("lastHitUseE").GetValue<bool>())
            {
                //Checks if minion will die from E
                if (minion != null && minion.Health < Program.e.GetDamage(minion) && (Environment.TickCount - lastECast) > Program.menuController.getMenu().Item("eDelay").GetValue<Slider>().Value)
                {
                    //Is the minion poisoned so E doesn't go on CD?
                    if (minion.HasBuffOfType(BuffType.Poison))
                    {
                        Program.e.CastOnUnit(minion);
                        lastECast = Environment.TickCount;
                    }
                }
            }
        }

        public static void performLaneClear()
        {
            if (Program.menuController.getMenu().Item("laneClearUseW").GetValue<bool>())
            {
                //Finds the best location to cast W in hitting maximum minions
                List<Obj_AI_Base> minionList = MinionManager.GetMinions(Program.e.Range);
                var wCast = MinionManager.GetBestCircularFarmLocation(minionList.Select(minion => minion.Position.To2D()).ToList(), Program.w.Width, Program.w.Range);
                if (wCast.MinionsHit >= 2)
                {
                    Program.w.Cast(wCast.Position);
                }
            }
            if (Program.menuController.getMenu().Item("laneClearUseQ").GetValue<bool>())
            {
                //Gets minion and casts Q on it
                Obj_AI_Base minion = MinionManager.GetMinions(Program.player.Position, Program.e.Range).FirstOrDefault();
                if (minion != null)
                {
                    Program.q.CastIfHitchanceEquals(minion, HitChance.High);
                }
            }
            if (Program.menuController.getMenu().Item("laneClearUseE").GetValue<bool>())
            {
                //Gets minion that is poisoned and casts E on it
                Obj_AI_Base minion = MinionManager.GetMinions(Program.player.Position, Program.e.Range).FirstOrDefault(o => o.HasBuffOfType(BuffType.Poison));
                if (minion != null && (Environment.TickCount - lastECast) > Program.menuController.getMenu().Item("eDelay").GetValue<Slider>().Value)
                {
                    Program.e.CastOnUnit(minion);
                    lastECast = Environment.TickCount;
                }
            }
        }
    }
}
