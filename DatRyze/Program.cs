/*
 * User: GoldenGates
 * Date: 29/12/2014
 * Time: 5:33 PM
 */
 
//TODO: Fix Lane Clear farming with Spells 

using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using System.Drawing;

namespace DatRyze {
	class Program {
		
		static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
		static Orbwalking.Orbwalker Orbwalker;
		static Spell Q, W, E, R;
		static Items.Item Seraph;
		static Menu Menu;
		
		public static void Main(string[] args) {
			CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
		}
		
		static void Game_OnGameLoad(EventArgs args) {
			if (Player.ChampionName != "Ryze")
				return;
			Q = new Spell(SpellSlot.Q, 625);
			W = new Spell(SpellSlot.W, 600);
			E = new Spell(SpellSlot.E, 600);
			R = new Spell(SpellSlot.R, 600);
			Seraph = new Items.Item(3040, 550);
			
			Menu = new Menu("Dat Ryze Menu", Player.ChampionName, true);
			
			Menu orbwalkerMenu = Menu.AddSubMenu(new Menu("DatRyze Orbwalker", "Orbwalker"));
			Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
			
			Menu tsMenu = Menu.AddSubMenu(new Menu("DatRyze TS", "Target Selector"));
			TargetSelector.AddToMenu(tsMenu);
			
			Menu comboMenu = Menu.AddSubMenu(new Menu("Combo Spells", "comboSpells"));
			comboMenu.AddItem(new MenuItem("comboUseQ", "Use Q").SetValue(true));
			comboMenu.AddItem(new MenuItem("comboUseW", "Use W").SetValue(true));
			comboMenu.AddItem(new MenuItem("comboUseE", "Use E").SetValue(true));
			comboMenu.AddItem(new MenuItem("comboUseR", "Use R").SetValue(true));
			
			Menu laneClearMenu = Menu.AddSubMenu(new Menu("Lane Clear Spells", "laneClearSpells"));
			laneClearMenu.AddItem(new MenuItem("laneClearUseQ", "Use Q").SetValue(true));
			laneClearMenu.AddItem(new MenuItem("laneClearUseE", "Use E").SetValue(true));
			laneClearMenu.AddItem(new MenuItem("laneClearManaManager", "Mana Manager (%)").SetValue(new Slider(40, 1, 100)));
			
			Menu lastHitMenu = Menu.AddSubMenu(new Menu("Last Hit Spells (Not in AA Range)", "lastHitSpells"));
			lastHitMenu.AddItem(new MenuItem("lastHitUseQ", "Use Q").SetValue(true));
			lastHitMenu.AddItem(new MenuItem("lastHitUseE", "Use E").SetValue(true));
			lastHitMenu.AddItem(new MenuItem("lastHitManaManager", "Mana Manager (%)").SetValue(new Slider(40, 1, 100)));
			
			Menu mixedMenu = Menu.AddSubMenu(new Menu("Mixed Mode Spells", "mixedSpells"));
			mixedMenu.AddItem(new MenuItem("mixedUseQ", "Use Q").SetValue(true));
			mixedMenu.AddItem(new MenuItem("mixedUseE", "Use E").SetValue(true));
			mixedMenu.AddItem(new MenuItem("mixedManaManager", "Mana Manager (%)").SetValue(new Slider(40, 1, 100)));
			
			Menu itemsMenu = Menu.AddSubMenu(new Menu("Items", "items"));
			itemsMenu.AddItem(new MenuItem("useSeraphs", "Use Seraph's Active").SetValue(true));
			itemsMenu.AddItem(new MenuItem("seraphHealth", "Activate at Health (%)").SetValue(new Slider(20, 1, 100)));
									
			Menu drawMenu = Menu.AddSubMenu(new Menu("Drawing", "drawing"));
			drawMenu.AddItem(new MenuItem("drawAA", "Draw AA Range").SetValue(true));
			drawMenu.AddItem(new MenuItem("drawQ", "Draw Q Range").SetValue(true));
			drawMenu.AddItem(new MenuItem("drawWE", "Draw W/E Range").SetValue(true));
			
			Menu.AddToMainMenu();

			Drawing.OnDraw += Drawing_OnDraw;

			Game.OnGameUpdate += Game_OnGameUpdate;
			
			Game.PrintChat("Dat Ryze by GoldenGates loaded. Enjoy!");
			
		}
		
		static void Game_OnGameUpdate(EventArgs args) {
			if (Player.IsDead)
				return;
			Checks();
			float manaPercentage = (Player.Mana / Player.MaxMana) * 100;
			switch (Orbwalker.ActiveMode) {
				case Orbwalking.OrbwalkingMode.Combo:
					if (Menu.Item("comboUseQ").GetValue<bool>())
						useQ(false, false);
					if (Menu.Item("comboUseW").GetValue<bool>())
						useW();
					if (Menu.Item("comboUseE").GetValue<bool>())
						useE(false, false);
					break;
				case Orbwalking.OrbwalkingMode.LaneClear:
					int laneClearMana = Menu.Item("laneClearManaManager").GetValue<Slider>().Value;
					if (Menu.Item("laneClearUseQ").GetValue<bool>() && manaPercentage > laneClearMana)
						useQ(true, true);
					if (Menu.Item("laneClearUseE").GetValue<bool>() && manaPercentage > laneClearMana)
						useE(true, true);
					break;
				case Orbwalking.OrbwalkingMode.LastHit:
					int lastHitMana = Menu.Item("lastHitManaManager").GetValue<Slider>().Value;
					if (Menu.Item("lastHitUseQ").GetValue<bool>() && manaPercentage > lastHitMana)
						useQ(true, false);
					if (Menu.Item("lastHitUseE").GetValue<bool>() && manaPercentage > lastHitMana)
						useE(true, false);
					break;
				case Orbwalking.OrbwalkingMode.Mixed:
					int mixedMana = Menu.Item("mixedManaManager").GetValue<Slider>().Value;
					if (Menu.Item("mixedUseQ").GetValue<bool>() && manaPercentage > mixedMana)
						useQ(false, false);
					if (Menu.Item("mixedUseE").GetValue<bool>() && manaPercentage > mixedMana)
						useE(false, false);
					break;
				
			}
			
		}
		
		static void Drawing_OnDraw(EventArgs args) {
			if (Menu.Item("drawAA").GetValue<bool>)
				Utility.DrawCircle(Player.Position, 550, Color.Blue);
			if (Menu.Item("drawQ").GetValue<bool>)
				Utility.DrawCircle(Player.Position, 625, Color.Orange);
			if (Menu.Item("drawWE").GetValue<bool>)
				Utility.DrawCircle(Player.Position, 600, Color.LimeGreen);
		}
		
		static void Checks() {
			float healthPercentage = (Player.Health / Player.MaxHealth) * 100;
			if (Items.HasItem(Seraph.Id) && Seraph.IsReady() && healthPercentage < Menu.Item("seraphHealth").GetValue<Slider>().Value && Player.CountEnemysInRange(600) > 0) {
				Seraph.Cast();
			}
		}
		
		static void useQ(bool onMinion, bool laneClear) {
			if (!Q.IsReady())
				return;
			if (onMinion) {
				Obj_AI_Base minion = MinionManager.GetMinions(Player.Position, 625).FirstOrDefault();
				if (laneClear && minion.IsValidTarget())
					Q.CastOnUnit(minion);
				else if (Player.Distance(minion) > 550 && minion.IsValidTarget())
					Q.CastOnUnit(minion);
			} else {
				Obj_AI_Hero target = TargetSelector.GetTarget(625, TargetSelector.DamageType.Magical);
				if (target.IsValidTarget())
					Q.CastOnUnit(target);
			}
		}
		
		static void useW() {
			if (!W.IsReady())
				return;
			Obj_AI_Hero target = TargetSelector.GetTarget(600, TargetSelector.DamageType.Magical);
			if (target != null && target.IsValidTarget())
				W.CastOnUnit(target);
			
		}
		
		static void useE(bool onMinion, bool laneClear) {
			if (!E.IsReady())
				return;
			if (onMinion) {
				Obj_AI_Base minion = MinionManager.GetMinions(Player.Position, 600).FirstOrDefault();
				if (laneClear && minion.IsValidTarget())
					E.CastOnUnit(minion);
				else if (Player.Distance(minion) > 550 && minion.IsValidTarget())
					E.CastOnUnit(minion);
			} else {
				Obj_AI_Hero target = TargetSelector.GetTarget(600, TargetSelector.DamageType.Magical);
				if (target.IsValidTarget())
					E.CastOnUnit(target);
			}
		}
	}
}

//http://pastie.org/9804616