using System;
using LeagueSharp.Common;
using System.Linq;
using Color = System.Drawing.Color;

namespace PentakillZed {

	public class MenuHelper {
		
		public Menu menu;
		
		public MenuHelper() {
			menu = new Menu("Pentakill Zed", "PentakillZed", true);
			Menu tsMenu = menu.AddSubMenu(new Menu("Target Selector", "TS"));
			TargetSelector.AddToMenu(tsMenu);
			AddCombo();
			AddDrawing();
			AddHarass();
			//AddItems();
			AddLaneClear();
			AddLastHit();
		}
		
		public Menu getOrbwalkerMenu() {
			return menu.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
		}
		
		public void AddCombo() {
			Menu comboMenu = menu.AddSubMenu(new Menu("Combo", "Combo"));
			comboMenu.AddItem(new MenuItem("comboUseQ", "Use Razor Shuriken (Q)").SetValue(true));
			comboMenu.AddItem(new MenuItem("comboUseW", "Use Living Shadow (W)").SetValue(true));
			comboMenu.AddItem(new MenuItem("comboUseE", "Use Shadow Slash (E)").SetValue(true));
			comboMenu.AddItem(new MenuItem("comboUseR", "Use Death Mark (R)").SetValue(true));
			comboMenu.AddItem(new MenuItem("comboIgn", "Use Ignite if Killable").SetValue(true));
			comboMenu.AddItem(new MenuItem("comboLine", "Line Combo (Different from Combo Key)").SetValue(new KeyBind("X".ToCharArray()[0], KeyBindType.Press)));
		}
		
		public void AddDrawing() {			
			Menu drawingMenu = menu.AddSubMenu(new Menu("Drawing", "drawing"));
			drawingMenu.AddItem(new MenuItem("drawQ", "Draw Q Range").SetValue(true));
			drawingMenu.AddItem(new MenuItem("drawW", "Draw W Range").SetValue(true));
			drawingMenu.AddItem(new MenuItem("drawE", "Draw E Range").SetValue(true));
			drawingMenu.AddItem(new MenuItem("drawR", "Draw R Range").SetValue(true));
			drawingMenu.AddItem(new MenuItem("drawDmg", "Draw Combo Damage").SetValue(true));
			drawingMenu.AddItem(new MenuItem("drawShadow", "Draw Shadow Positions").SetValue(true));
		}
		
		public void AddHarass() {
			Menu harassMenu = menu.AddSubMenu(new Menu("Harass", "harass"));
			harassMenu.AddItem(new MenuItem("harassUseQ", "Use Razor Shuriken (Q)").SetValue(true));
			harassMenu.AddItem(new MenuItem("harassUseW", "Use Living Shadow (W)").SetValue(true));
			harassMenu.AddItem(new MenuItem("harassUseE", "Use Use Shadow Slash (E)").SetValue(true));
		}
		
		public void AddItems() {
			Menu itemMenu = menu.AddSubMenu(new Menu("Items", "items"));
			itemMenu.AddItem(new MenuItem("temp", "Coming Soon..."));
		}
		
		public void AddLaneClear() {
			Menu lcMenu = menu.AddSubMenu(new Menu("Lane Clear", "laneClear"));
			lcMenu.AddItem(new MenuItem("lcUseQ", "Use Razor Shuriken (Q)").SetValue(true));
			lcMenu.AddItem(new MenuItem("lcUseE", "Use Shadow Slash (E)").SetValue(true));
			lcMenu.AddItem(new MenuItem("lcEnergyManager", "Energy Manager (%)").SetValue(new Slider(30, 1, 100)));			
		}
		
		public void AddLastHit() {
			Menu lastHitMenu = menu.AddSubMenu(new Menu("Last Hit", "lastHit"));
			lastHitMenu.AddItem(new MenuItem("lastHitUseQ", "Use Razor Shuriken (Q)").SetValue(true));			
		}
	}
}
