/*
 * User: GoldenGates
 * Date: 29/12/2014
 * Time: 5:33 PM
 */
 
//TODO: Add Auto-W if Tower aggros enemy

/*
 Features:
 -Full combo with togglable QWER
 -Slider to adjust health to Ulti at
 -Seraph activator at slider adjustable health
 -Lane clear with togglable QE with Mana Manager
 -Last hit with togglable QE (if out of AA range) with Mana Manager
 -Mixed mode (harass/last hit) with togglable QE with Mana Manager
 -Drawing Q+WE+AA ranges
 -Auto pot at adjustable health/mana
 -Auto cage under tower when possible
 
 */

using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace DatRyze {
	class Program {
		
		static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
		
		static Orbwalking.Orbwalker Orbwalker;
		static Spell Q, W, E, R;
		static Items.Item Seraph;
		static Items.Item HealthPot;
		static Items.Item ManaPot;
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
			R = new Spell(SpellSlot.R);
			Seraph = new Items.Item(3040, 0);
			HealthPot = new Items.Item(2003, 0);
			ManaPot = new Items.Item(2004, 0);
			
			Menu = new Menu("Dat Ryze", Player.ChampionName, true);
			
			Menu orbwalkerMenu = Menu.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
			Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
			
			Menu tsMenu = Menu.AddSubMenu(new Menu("Target Selector", "TS"));
			TargetSelector.AddToMenu(tsMenu);
			
			Menu spellsMenu = Menu.AddSubMenu(new Menu("Spells", "spellsMenu"));
			
			Menu comboMenu = spellsMenu.AddSubMenu(new Menu("Combo Spells", "comboSpells"));
			comboMenu.AddItem(new MenuItem("comboUseQ", "Use Q").SetValue(true));
			comboMenu.AddItem(new MenuItem("comboUseW", "Use W").SetValue(true));
			comboMenu.AddItem(new MenuItem("comboUseE", "Use E").SetValue(true));
			comboMenu.AddItem(new MenuItem("comboUseR", "Use R").SetValue(true));
			comboMenu.AddItem(new MenuItem("comboSliderR", "Use R at Health (%)").SetValue(new Slider(80, 1, 100)));
			
			Menu laneClearMenu = spellsMenu.AddSubMenu(new Menu("Lane Clear Spells", "laneClearSpells"));
			laneClearMenu.AddItem(new MenuItem("laneClearUseQ", "Use Q").SetValue(true));
			laneClearMenu.AddItem(new MenuItem("laneClearUseE", "Use E").SetValue(true));
			laneClearMenu.AddItem(new MenuItem("laneClearManaManager", "Mana Manager (%)").SetValue(new Slider(40, 1, 100)));
			
			Menu lastHitMenu = spellsMenu.AddSubMenu(new Menu("Last Hit Spells", "lastHitSpells"));
			lastHitMenu.AddItem(new MenuItem("lastHitUseQ", "Use Q").SetValue(false));
			lastHitMenu.AddItem(new MenuItem("lastHitUseE", "Use E").SetValue(false));
			lastHitMenu.AddItem(new MenuItem("lastHitManaManager", "Mana Manager (%)").SetValue(new Slider(40, 1, 100)));
			
			Menu mixedMenu = spellsMenu.AddSubMenu(new Menu("Mixed Mode Spells", "mixedSpells"));
			mixedMenu.AddItem(new MenuItem("mixedUseQ", "Use Q").SetValue(true));
			mixedMenu.AddItem(new MenuItem("mixedUseE", "Use E").SetValue(true));
			mixedMenu.AddItem(new MenuItem("mixedManaManager", "Mana Manager (%)").SetValue(new Slider(40, 1, 100)));
						
			Menu itemsMenu = Menu.AddSubMenu(new Menu("Items", "items"));
			itemsMenu.AddItem(new MenuItem("useSeraphs", "Use Seraph's Active").SetValue(true));
			itemsMenu.AddItem(new MenuItem("seraphHealth", "Activate at Health (%)").SetValue(new Slider(25, 1, 100)));
			itemsMenu.AddItem(new MenuItem("useHealthPot", "Use Health Potion").SetValue(true));
			itemsMenu.AddItem(new MenuItem("healthPotHealth", "Activate at Health (%)").SetValue(new Slider(30, 1, 100)));
			itemsMenu.AddItem(new MenuItem("useManaPot", "Use Mana Potion").SetValue(true));
			itemsMenu.AddItem(new MenuItem("manaPotMana", "Activate at Mana (%)").SetValue(new Slider(30, 1, 100)));
									
			Menu drawMenu = Menu.AddSubMenu(new Menu("Drawing", "drawing"));
			drawMenu.AddItem(new MenuItem("drawAA", "Draw AA Range").SetValue(true));
			drawMenu.AddItem(new MenuItem("drawQ", "Draw Q Range").SetValue(true));
			drawMenu.AddItem(new MenuItem("drawWE", "Draw W/E Range").SetValue(true));
			
			Menu.AddToMainMenu();
			Drawing.OnDraw += Drawing_OnDraw;
			Game.OnGameUpdate += Game_OnGameUpdate;
			Game.OnGameProcessPacket += Game_OnGameProcessPacket;
			Game.PrintChat("<font color ='#33FFFF'>Dat Ryze</font> by GoldenGates loaded. Enjoy!");
		}
		
		static void Game_OnGameUpdate(EventArgs args) {
			if (Player.IsDead)
				return;
			Checks();
			float manaPercentage = (Player.Mana / Player.MaxMana) * 100;
			float healthPercentage = (Player.Health / Player.MaxHealth) * 100;
			switch (Orbwalker.ActiveMode) {
				case Orbwalking.OrbwalkingMode.Combo:
					int comboRHealth = Menu.Item("comboSliderR").GetValue<Slider>().Value;
					if (Menu.Item("comboUseQ").GetValue<bool>())
						useQ(false, false);
					if (Menu.Item("comboUseW").GetValue<bool>())
						useW();
					if (Menu.Item("comboUseE").GetValue<bool>())
						useE(false, false);
					if (Menu.Item("comboUseR").GetValue<bool>() && R.IsReady() && healthPercentage < comboRHealth)
						R.Cast();		
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
			if (Menu.Item("drawAA").GetValue<bool>())
				Utility.DrawCircle(Player.Position, 550, Color.Blue);
			if (Menu.Item("drawQ").GetValue<bool>())
				Utility.DrawCircle(Player.Position, 625, Color.Orange);
			if (Menu.Item("drawWE").GetValue<bool>())
				Utility.DrawCircle(Player.Position, 600, Color.HotPink);
		}
		
		static void Checks() {
			float healthPercentage = (Player.Health / Player.MaxHealth) * 100;
			float manaPercentage = (Player.Mana / Player.MaxMana) * 100;
			if (Menu.Item("useSeraphs").GetValue<bool>() && Items.HasItem(Seraph.Id) && Seraph.IsReady() && !InFountain() && healthPercentage < Menu.Item("seraphHealth").GetValue<Slider>().Value && Player.CountEnemysInRange(600) > 0)
				Seraph.Cast();			
			if (Menu.Item("useHealthPot").GetValue<bool>() && Items.HasItem(HealthPot.Id) && HealthPot.IsReady() && !InFountain() && !Player.HasBuff("RegenerationPotion", true) && healthPercentage < Menu.Item("healthPotHealth").GetValue<Slider>().Value)
				HealthPot.Cast();
			if (Menu.Item("useManaPot").GetValue<bool>() && Items.HasItem(ManaPot.Id) && ManaPot.IsReady() && !InFountain() && manaPercentage < Menu.Item("manaPotMana").GetValue<Slider>().Value)
				ManaPot.Cast();					
		}
				
		static void useQ(bool onMinion, bool laneClear) {
			if (!Q.IsReady())
				return;
			if (onMinion) {
				Obj_AI_Base minion = MinionManager.GetMinions(Player.Position, 625).FirstOrDefault();
				if (minion != null && laneClear && minion.IsValidTarget())
					Q.CastOnUnit(minion);
				else if (minion != null && Player.Distance(minion) > 550 && minion.IsValidTarget())
					Q.CastOnUnit(minion);
			} else {
				Obj_AI_Hero target = TargetSelector.GetTarget(625, TargetSelector.DamageType.Magical);
				if (target != null && target.IsValidTarget())
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
				if (minion != null && laneClear && minion.IsValidTarget())
					E.CastOnUnit(minion);
				else if (minion != null && Player.Distance(minion) > 550 && minion.IsValidTarget())
					E.CastOnUnit(minion);
			} else {
				Obj_AI_Hero target = TargetSelector.GetTarget(600, TargetSelector.DamageType.Magical);
				if (target != null && target.IsValidTarget())
					E.CastOnUnit(target);
			}
		}
		
		//Credits to sebastiank1
		static bool InFountain() {
			float fountainRange = 750;
			if (Utility.Map.GetMap()._MapType == Utility.Map.MapType.SummonersRift)
				fountainRange = 1050;
			return ObjectManager.Get<Obj_SpawnPoint>()
                    .Where(spawnPoint => spawnPoint.IsAlly)
                    .Any(spawnPoint => Vector2.Distance(ObjectManager.Player.Position.To2D(), spawnPoint.Position.To2D()) < fountainRange);
		}
		
		//Credits to FluxySenpai
		static void Game_OnGameProcessPacket(GamePacketEventArgs args) {
			if (args.PacketData[0] == Packet.S2C.TowerAggro.Header) {
				var p = Packet.S2C.TowerAggro.Decoded(args.PacketData);
				var target = ObjectManager.GetUnitByNetworkId<Obj_AI_Hero>(p.TargetNetworkId);
				if (target != null && target.IsValidTarget() && Player.Distance(target) <= W.Range) {
					Game.PrintChat("Target Found Under Tower, Auto Caging");
					W.CastOnUnit(target);
				}
			}
		}
	}
}

//http://pastie.org/9804616