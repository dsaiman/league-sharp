/*
 * User: GoldenGates
 * Date: 2015-01-19
 * Time: 8:27 PM
 */
using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace DatRenekton {
	class Program {
		static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
		
		static Orbwalking.Orbwalker Orbwalker;
		static Spell Q, W, E, R;
		static Items.Item Tiamat;
		static Items.Item Hydra;
		static Menu Menu;
		const string rageBuffName = "renektonrageready";
		const string wBuffName = "renektonpreexecute";
		const string e2BuffName = "renektonsliceanddicedelay";
		const string rBuffName = "renektonreignofthetyrant";
		
		public static void Main(string[] args) {
			CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
		}
		 
		static void Game_OnGameLoad(EventArgs args) {
			if (Player.ChampionName != "Renekton")
				return;
			Q = new Spell(SpellSlot.Q, 225);
			W = new Spell(SpellSlot.W);
			E = new Spell(SpellSlot.E, 450);
			R = new Spell(SpellSlot.R);
			Tiamat = new Items.Item((int)ItemId.Tiamat_Melee_Only, 420);
			Hydra = new Items.Item((int)ItemId.Ravenous_Hydra_Melee_Only, 420);
			
			Menu = new Menu("Dat Renekton", Player.ChampionName, true);
			
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
			comboMenu.AddItem(new MenuItem("comboSliderR", "Use R at Health (%)").SetValue(new Slider(30, 1, 100)));
			
			Menu laneClearMenu = spellsMenu.AddSubMenu(new Menu("Lane Clear Spells", "laneClearSpells"));
			laneClearMenu.AddItem(new MenuItem("laneClearUseQ", "Use Q").SetValue(true));
			
			Menu mixedMenu = spellsMenu.AddSubMenu(new Menu("Mixed Mode Spells", "mixedSpells"));
			mixedMenu.AddItem(new MenuItem("mixedUseQ", "Use Q").SetValue(true));
			mixedMenu.AddItem(new MenuItem("mixedUseW", "Use W").SetValue(true));
															
			Menu drawMenu = Menu.AddSubMenu(new Menu("Drawing", "drawing"));
			drawMenu.AddItem(new MenuItem("drawQ", "Draw Q Range").SetValue(true));
			drawMenu.AddItem(new MenuItem("drawIt", "Draw Balls (Risky Click of the Day)").SetValue(false));
			
			Menu.AddToMainMenu();
			Drawing.OnDraw += Drawing_OnDraw;
			Orbwalking.AfterAttack += Orbwalking_AfterAttack;
			Game.OnGameUpdate += Game_OnGameUpdate;
			Game.PrintChat("<font color ='#33FFFF'>Dat Renekton</font> by GoldenGates loaded, enjoy! Best used with an activator and evader!");
		}
		
		static void Game_OnGameUpdate(EventArgs args) {
			if (Player.IsDead)
				return;
			Checks();
			Obj_AI_Hero target = TargetSelector.GetTarget(450, TargetSelector.DamageType.Physical);
			switch (Orbwalker.ActiveMode) {
				case Orbwalking.OrbwalkingMode.Combo:	
					if (target != null) {
						if (Player.HealthPercentage() < Menu.Item("comboSliderR").GetValue<Slider>().Value && R.IsReady())
							R.Cast();						
						if (Menu.Item("comboUseQ").GetValue<bool>())
							useQ(target);
						if (Menu.Item("comboUseE").GetValue<bool>()) {
							if (Player.Distance(target) > 225)
								useE(target);
						}
					}
					break;
				case Orbwalking.OrbwalkingMode.LaneClear:
					Obj_AI_Base minion = MinionManager.GetMinions(Player.Position, 225).FirstOrDefault();
					useQ(minion);
					break;
				case Orbwalking.OrbwalkingMode.Mixed:
					if (Menu.Item("comboUseQ").GetValue<bool>())
						useQ(target);
					break;
			}
			
		}
		
		static void Drawing_OnDraw(EventArgs args) {
			if (Menu.Item("drawQ").GetValue<bool>())
				
				Render.Circle.DrawCircle(Player.Position, Player.BoundingRadius + 225, Color.Orange);
			if (Menu.Item("drawIt").GetValue<bool>()) {
				Render.Circle.DrawCircle(new Vector3(Player.Position.X - 125, Player.Position.Y, Player.Position.Z), 100, Color.Red);
				Render.Circle.DrawCircle(new Vector3(Player.Position.X + 75, Player.Position.Y, Player.Position.Z), 100, Color.Red);
				
			}
		}
		
		static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target) {
			if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo) {
				if (Menu.Item("comboUseW").GetValue<bool>() && W.IsReady() && target.IsEnemy)
					W.Cast();
				if (Tiamat.IsOwned() && Player.Distance(target) < Tiamat.Range && Tiamat.IsReady())
					Tiamat.Cast();
				if (Hydra.IsOwned() && Player.Distance(target) < Hydra.Range && Hydra.IsReady())
					Hydra.Cast();
			}
		}
		
		static void Checks() {
		}
				
		static void useQ(Obj_AI_Base unit) {
			if (!Q.IsReady())
				return;
			if (unit != null && Player.Distance(unit) < 225) {
				Q.Cast();
			}			
			
		}
		
		
		static void useE(Obj_AI_Base target) {
			Obj_AI_Base minion = MinionManager.GetMinions(Player.Position, 225).FirstOrDefault();
			if (!E.IsReady())
				return;
			if (target != null && Player.Distance(target) < E.Range) {
				E.Cast(target.Position);
			} else if (target != null && Player.Distance(target) < 800 && minion != null && Player.Distance(minion) < E.Range && target.Distance(minion) < E.Range) {
				E.Cast(minion.Position);
				if (Player.HasBuff(e2BuffName, true, true))
					E.Cast(target.Position);
			}
		}
		
	}
}