using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Pentakill_Cassiopeia.Controller;
using Pentakill_Cassiopeia.Util;

namespace Pentakill_Cassiopeia
{
    class Program
    {
        public static MenuController menuController;
        public static Orbwalking.Orbwalker orbwalker;
        public static Obj_AI_Hero player;
        public static Spell q;
        public static Spell w;
        public static Spell e;
        public static Spell r;
        public static SpellSlot ignite;

        static void OnGameLoad(EventArgs args)
        {
            //Assigning objects used in later parts
            menuController = new MenuController();
            orbwalker = new Orbwalking.Orbwalker(menuController.getOrbwalkingMenu());
            player = ObjectManager.Player;

            //Check if our Champion is Cassiopeia
            if (player.ChampionName != "Cassiopeia")
            {
                return;
            }

            //Initiating spells
            q = new Spell(SpellSlot.Q, 850f);
            q.SetSkillshot(0.4f, 40f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            w = new Spell(SpellSlot.W, 850f);
            w.SetSkillshot(0.5f, 90f, 2500, false, SkillshotType.SkillshotCircle);
            e = new Spell(SpellSlot.E, 700f);
            e.SetTargetted(0.2f, float.MaxValue);
            r = new Spell(SpellSlot.R, 825f);
            r.SetSkillshot(0.6f, (float)(80 * Math.PI / 180), float.MaxValue, false, SkillshotType.SkillshotCone);
            ignite = player.GetSpellSlot("summonerdot");

            //Add menu to main menu
            menuController.addToMainMenu();

            //Subscribe to OnGameUpdate and OnDraw method
            Game.OnUpdate += OnGameUpdate;
            Drawing.OnDraw += OnDraw;
            Game.PrintChat("<font color ='#33FFFF'>Pentakill Cassiopeia</font> by <font color = '#FFFF00'>GoldenGates</font> loaded, enjoy!");
        }

        static void OnGameUpdate(EventArgs args)
        {
            //If we're dead do nothing
            if (player.IsDead)
                return;
            //Performs checks before orbwalking
            GameLogic.Checks();
            //Orbwalking handling with modes
            switch (orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    GameLogic.performCombo();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    if (menuController.getMenu().Item("harassManager").GetValue<Slider>().Value < player.ManaPercentage())
                    {
                        GameLogic.performHarass();
                    }
                    break;
                case Orbwalking.OrbwalkingMode.LastHit:
                    if (menuController.getMenu().Item("lastHitManager").GetValue<Slider>().Value < player.ManaPercentage())
                    {
                        GameLogic.performLastHit();
                    }
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    if (menuController.getMenu().Item("laneClearManager").GetValue<Slider>().Value < player.ManaPercentage())
                    {
                        GameLogic.performLaneClear();
                    }
                    break;
            }
        }

        #region Drawing
        static readonly Render.Text text = new Render.Text(
                                               0, 0, "", 11, new ColorBGRA(255, 0, 0, 255), "monospace");

        public static void OnDraw(EventArgs args)
        {
            if (Program.menuController.getMenu().Item("drawQW").GetValue<bool>())
                Render.Circle.DrawCircle(Program.player.Position, Program.q.Range, System.Drawing.Color.Yellow);
            if (Program.menuController.getMenu().Item("drawE").GetValue<bool>())
                Render.Circle.DrawCircle(Program.player.Position, Program.e.Range, System.Drawing.Color.Green);
            if (Program.menuController.getMenu().Item("drawR").GetValue<bool>())
                Render.Circle.DrawCircle(Program.player.Position, Program.r.Range, System.Drawing.Color.IndianRed);
            if (Program.menuController.getMenu().Item("drawDmg").GetValue<bool>())
                DrawHPBarDamage();

        }

        static void DrawHPBarDamage()
        {
            const int XOffset = 10;
            const int YOffset = 20;
            const int Width = 103;
            const int Height = 8;
            foreach (var unit in ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsValid && h.IsHPBarRendered && h.IsEnemy))
            {
                var barPos = unit.HPBarPosition;
                float damage = SpellDamage.getComboDamage(unit);
                float percentHealthAfterDamage = Math.Max(0, unit.Health - damage) / unit.MaxHealth;
                float yPos = barPos.Y + YOffset;
                float xPosDamage = barPos.X + XOffset + Width * percentHealthAfterDamage;
                float xPosCurrentHp = barPos.X + XOffset + Width * unit.Health / unit.MaxHealth;

                if (damage > unit.Health)
                {
                    text.X = (int)barPos.X + XOffset;
                    text.Y = (int)barPos.Y + YOffset - 13;
                    text.text = ((int)(unit.Health - damage)).ToString();
                    text.OnEndScene();
                }
                Drawing.DrawLine(xPosDamage, yPos, xPosDamage, yPos + Height, 2, System.Drawing.Color.Yellow);
            }
        }
        #endregion Drawing

        static void Main(string[] args)
        {
            //Clears console
            Console.Clear();
            Console.WriteLine("Pentakill Cassiopeia Loading...");
            //Subscribes to OnGameLoad method
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }
    }
}
