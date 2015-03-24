using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace Pentakill_Cassiopeia.Controller
{
    public class MenuController
    {
        private Menu menu;

        public MenuController()
        {
            menu = new Menu("Pentakill Cassiopeia", "menu", true);
            Combo();
            Harass();
            LastHit();
            LaneClear();
            Drawings();
            Misc();
        }

        private void Combo()
        {
            Menu comboMenu = menu.AddSubMenu(new Menu("Combo", "combo"));
            comboMenu.AddItem(new MenuItem("comboUseQ", "Use Q")).SetValue(true);
            comboMenu.AddItem(new MenuItem("comboUseW", "Use W")).SetValue(true);
            comboMenu.AddItem(new MenuItem("comboUseE", "Use E")).SetValue(true);
            comboMenu.AddItem(new MenuItem("comboUseR", "Use R")).SetValue(true);
         //   comboMenu.AddItem(new MenuItem("faceOnlyR", "Only R If Can Stun")).SetValue(true);
            comboMenu.AddItem(new MenuItem("minEnemies", "Minimum Enemies for R").SetValue(new Slider(2, 1, 5)));
            comboMenu.AddItem(new MenuItem("useIgnite", "Smart Ignite").SetValue(true));
        }

        private void Harass()
        {
            Menu harassMenu = menu.AddSubMenu(new Menu("Harass", "harass"));
            harassMenu.AddItem(new MenuItem("harassUseQ", "Use Q")).SetValue(true);
            harassMenu.AddItem(new MenuItem("harassUseW", "Use W")).SetValue(false);
            harassMenu.AddItem(new MenuItem("harassUseE", "Use E")).SetValue(true);
            harassMenu.AddItem(new MenuItem("harassManager", "Minimum Mana %").SetValue(new Slider(60, 1, 100)));
        }

        private void LastHit()
        {
            Menu lastHitMenu = menu.AddSubMenu(new Menu("Last Hit", "lastHit"));
            lastHitMenu.AddItem(new MenuItem("lastHitUseQ", "Use Q")).SetValue(true);
            lastHitMenu.AddItem(new MenuItem("lastHitUseE", "Use E")).SetValue(true);
            lastHitMenu.AddItem(new MenuItem("lastHitManager", "Minimum Mana %").SetValue(new Slider(50, 1, 100)));
        }

        private void LaneClear()
        {
            Menu laneClearMenu = menu.AddSubMenu(new Menu("Lane Clear", "laneClear"));
            laneClearMenu.AddItem(new MenuItem("laneClearUseQ", "Use Q")).SetValue(true);
            laneClearMenu.AddItem(new MenuItem("laneClearUseW", "Use W")).SetValue(true);
            laneClearMenu.AddItem(new MenuItem("laneClearUseE", "Use E")).SetValue(true);
            laneClearMenu.AddItem(new MenuItem("laneClearManager", "Minimum Mana %").SetValue(new Slider(25, 1, 100)));
        }

        private void Drawings()
        {
            Menu drawingsMenu = menu.AddSubMenu(new Menu("Drawings", "drawings"));
            drawingsMenu.AddItem(new MenuItem("drawQW", "Draw Q/W")).SetValue(true);
            drawingsMenu.AddItem(new MenuItem("drawE", "Draw E")).SetValue(true);
            drawingsMenu.AddItem(new MenuItem("drawR", "Draw R")).SetValue(true);
            drawingsMenu.AddItem(new MenuItem("drawDmg", "Draw Damage")).SetValue(true);
        }

        private void Misc()
        {
            menu.AddItem(new MenuItem("autoLevel", "Auto Level Spells")).SetValue(true);
        }

        public Menu getOrbwalkingMenu()
        {
            return menu.AddSubMenu(new Menu("PC Orbwalker", "orbwalkerMenu"));
        }

        public Menu getMenu()
        {
            return menu;
        }

        public void addToMainMenu()
        {
            menu.AddToMainMenu();
        }

    }
}
