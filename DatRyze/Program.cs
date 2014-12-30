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

namespace DatRyze {
	class Program {
		
		static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
		static Orbwalking.Orbwalker Orbwalker;
		static Spell Q, W, E, R;
		//private static Items.Item Dfg;
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
			
			Menu lastHitMenu = Menu.AddSubMenu(new Menu("Last Hit Spells (Not in AA Range)", "lastHitSpells"));
			lastHitMenu.AddItem(new MenuItem("lastHitUseQ", "Use Q").SetValue(true));
			lastHitMenu.AddItem(new MenuItem("lastHitUseE", "Use E").SetValue(true));
			
			Menu mixedMenu = Menu.AddSubMenu(new Menu("Mixed Mode Spells", "mixedSpells"));
			mixedMenu.AddItem(new MenuItem("mixedUseQ", "Use Q").SetValue(true));
			mixedMenu.AddItem(new MenuItem("mixedUseE", "Use E").SetValue(true));
			mixedMenu.AddItem(new MenuItem("mixedManaManager", "Mana Manager (%)").SetValue(new Slider(40, 1, 100)));
									
			Menu.AddToMainMenu();

			Drawing.OnDraw += Drawing_OnDraw;

			Game.OnGameUpdate += Game_OnGameUpdate;
			
		}
		
		static void Game_OnGameUpdate(EventArgs args) {
			if (Player.IsDead)
				return;
			switch (Orbwalker.ActiveMode) {
				case Orbwalking.OrbwalkingMode.Combo:
					if (Menu.Item("comboUseQ").GetValue<bool>())
						useQ(false);
					if (Menu.Item("comboUseW").GetValue<bool>())
						useW(false);
					if (Menu.Item("comboUseE").GetValue<bool>())
						useE(false);
					break;
				case Orbwalking.OrbwalkingMode.LaneClear:
					if (Menu.Item("laneClearUseQ").GetValue<bool>())
						useQ(true);
					if (Menu.Item("laneClearUseE").GetValue<bool>())
						useE(true);
					break;
				case Orbwalking.OrbwalkingMode.LastHit:
					if (Menu.Item("lastHitUseQ").GetValue<bool>())
						useQ(true);
					if (Menu.Item("lastHitUseE").GetValue<bool>())
						useE(true);
					break;
				case Orbwalking.OrbwalkingMode.Mixed:
					float manaPercentage = (Player.Mana / Player.MaxMana) * 100;
					int manaManagerValue = Menu.Item("mixedManaManager").GetValue<Slider>().Value;
					if (Menu.Item("mixedUseQ").GetValue<bool>() && manaPercentage > manaManagerValue)
						useQ(false);
					if (Menu.Item("mixedUseE").GetValue<bool>() && manaPercentage > manaManagerValue)
						useE(false);
					break;
				
			}
			
		}
		
		static void Drawing_OnDraw(EventArgs args) {
			
		}
		
		static void useQ(bool onMinion) {
			if (onMinion) {
				Obj_AI_Base minion = MinionManager.GetMinions(Player.Position, 625).FirstOrDefault();
				if (Q.IsReady() && Player.Distance(minion) > 550 && minion.IsValidTarget())
					Q.CastOnUnit(minion);
			} else {
				Obj_AI_Hero target = TargetSelector.GetTarget(625, TargetSelector.DamageType.Magical);
				if (Q.IsReady() && target != null && target.IsValidTarget())
					Q.CastOnUnit(target);
			}
		}
		
		static void useW(bool onMinion) {
			if (onMinion) {
				Obj_AI_Base minion = MinionManager.GetMinions(Player.Position, 600).FirstOrDefault();
				if (W.IsReady() && Player.Distance(minion) > 550 && minion.IsValidTarget())
					W.CastOnUnit(minion);
			} else {
				Obj_AI_Hero target = TargetSelector.GetTarget(600, TargetSelector.DamageType.Magical);
				if (W.IsReady() && target != null && target.IsValidTarget())
					W.CastOnUnit(target);
			}
		}
		
		static void useE(bool onMinion) {
			if (onMinion) {
				Obj_AI_Base minion = MinionManager.GetMinions(Player.Position, 600).FirstOrDefault();
				if (E.IsReady() && Player.Distance(minion) > 550 && minion.IsValidTarget())
					E.CastOnUnit(minion);
			} else {
				Obj_AI_Hero target = TargetSelector.GetTarget(600, TargetSelector.DamageType.Magical);
				if (E.IsReady() && target != null && target.IsValidTarget())
					E.CastOnUnit(target);
			}
		}
	}
}

//http://pastie.org/9804616