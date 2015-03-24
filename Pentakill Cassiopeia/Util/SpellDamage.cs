using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace Pentakill_Cassiopeia.Util
{
    class SpellDamage
    {
        public static float getComboDamage(Obj_AI_Hero target)
        {
            float damage = 0f;
            if (Program.menuController.getMenu().Item("comboUseQ").GetValue<bool>())
            {
                if (Program.q.IsReady())
                {
                    //Multipled by 1.5 since Q will likely hit at least twice in a combo
                    damage += Program.q.GetDamage(target) * 1.5f;
                }
            }
            if (Program.menuController.getMenu().Item("comboUseW").GetValue<bool>())
            {
                if (Program.w.IsReady())
                {
                    damage += Program.w.GetDamage(target);
                }
            }
            if (Program.menuController.getMenu().Item("comboUseE").GetValue<bool>())
            {
                if (Program.e.IsReady())
                {
                    //Multipled by 3 since E will likely hit at least thrice in a combo
                    damage += Program.e.GetDamage(target) * 3f;
                }
            }
            if (Program.menuController.getMenu().Item("comboUseR").GetValue<bool>())
            {
                if (Program.r.IsReady())
                {
                    damage += Program.r.GetDamage(target);
                }
            }
            if (Program.menuController.getMenu().Item("useIgnite").GetValue<bool>())
            {
                if (Program.ignite.IsReady())
                {
                    damage += (float)Program.player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
                }
            }
            return damage;
        }

        //TODO: More Accurate Methods
    }
}
