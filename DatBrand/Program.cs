using System;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using System.Linq;
using System.Collections.Generic;
using Color = System.Drawing.Color;

namespace DatBrand {
	class Program {
		
		const string champName = "Brand";
		static Obj_AI_Hero player { get { return ObjectManager.Player; } }
		static Spell q, w, e, r;
		static SpellSlot ignite;
		static Orbwalking.Orbwalker orbwalker;
		static Menu menu;
		static readonly Render.Text Text = new Render.Text(
			                                   0, 0, "", 11, new ColorBGRA(255, 0, 0, 255), "monospace");
		
		public static void Main(string[] args) {
			CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
		}
		
		static void Game_OnGameLoad(EventArgs args) {
			if (player.ChampionName != champName)
				return;
			/*Spell Initialization and Setting*/
			q = new Spell(SpellSlot.Q, 1050);
			q.SetSkillshot(0.25f, 60, 1600, true, SkillshotType.SkillshotLine);
			w = new Spell(SpellSlot.W, 900);
			w.SetSkillshot(1, 240, float.MaxValue, false, SkillshotType.SkillshotCircle);
			e = new Spell(SpellSlot.E, 625);
			e.SetTargetted(0.25f, float.MaxValue);
			r = new Spell(SpellSlot.R, 750); 
			r.SetTargetted(0.25f, 1000);
			ignite = player.GetSpellSlot("summonerdot");
			
			/*Menu*/
			InitializeMenu();
			
			menu.AddToMainMenu();
			Game.OnGameUpdate += Game_OnGameUpdate;
			Drawing.OnDraw += Draw_OnDraw;
			Game.PrintChat("<font color ='#33FFFF'>Dat Brand</font> by GoldenGates loaded, enjoy!");
		}
		
		static void Game_OnGameUpdate(EventArgs args) {
			if (player.IsDead)
				return;
			switch (orbwalker.ActiveMode) {
				case Orbwalking.OrbwalkingMode.Combo:
					DoCombo(menu.Item("comboUseQ").GetValue<bool>(), menu.Item("comboUseW").GetValue<bool>(), menu.Item("comboUseE").GetValue<bool>(), menu.Item("comboUseR").GetValue<bool>(), menu.Item("comboUseIgn").GetValue<bool>());
					break;
				case Orbwalking.OrbwalkingMode.Mixed:
					if (player.ManaPercentage() > menu.Item("harassManaManager").GetValue<Slider>().Value) {
						DoHarass(menu.Item("harassUseQ").GetValue<bool>(), menu.Item("harassUseW").GetValue<bool>(), menu.Item("harassUseE").GetValue<bool>());
					}
					break;
				case Orbwalking.OrbwalkingMode.LaneClear:
					if (player.ManaPercentage() > menu.Item("lcManaManager").GetValue<Slider>().Value) {
						DoLaneClear(menu.Item("lcUseW").GetValue<bool>(), menu.Item("lcUseE").GetValue<bool>());
					}
					break;
					
			}
		}
		
		static void DoCombo(bool useQ, bool useW, bool useE, bool useR, bool useIgn) {
			Obj_AI_Hero target = TargetSelector.GetTarget(w.Range - 50, TargetSelector.DamageType.Magical);
			if (target != null && target.IsValidTarget()) {
				if (player.Distance(target) < e.Range) {
					if (e.IsReady() && useE)
						e.Cast(target, true);
					if (q.IsReady() && IsAblazed(target) && useQ)
						q.CastIfHitchanceEquals(target, HitChance.High, true);
					if (w.IsReady() && IsAblazed(target) && useW)
						w.CastIfHitchanceEquals(target, HitChance.High, true);
					if (r.IsReady() && IsAblazed(target) && useR)
						r.Cast(target, true);
					if (getComboDamage(target) > target.Health - 100 && ignite.IsReady() && useIgn)
						player.Spellbook.CastSpell(ignite, target);
				} else {
					if (w.IsReady() && useW)
						w.CastIfHitchanceEquals(target, HitChance.High, true);
					if (q.IsReady() && IsAblazed(target) && useQ)
						q.CastIfHitchanceEquals(target, HitChance.High, true);
					if (e.IsReady() && IsAblazed(target) && useE)
						e.Cast(target, true);
					if (r.IsReady() && IsAblazed(target) && useR)
						r.Cast(target, true);
					if (getComboDamage(target) > target.Health - 100 && ignite.IsReady() && useIgn)
						player.Spellbook.CastSpell(ignite, target);
				}
			}
		}
		
		static void DoHarass(bool useQ, bool useW, bool useE) {
			Obj_AI_Hero target = TargetSelector.GetTarget(w.Range - 50, TargetSelector.DamageType.Magical);
			if (target != null && target.IsValidTarget()) {
				if (player.Distance(target) < e.Range) {
					if (e.IsReady() && useE)
						e.Cast(target, true);
					if (q.IsReady() && useQ)
						q.CastIfHitchanceEquals(target, HitChance.High, true);
					if (w.IsReady() && useW)
						w.CastIfHitchanceEquals(target, HitChance.High, true);
				} else {
					if (w.IsReady() && useW)
						w.CastIfHitchanceEquals(target, HitChance.High, true);
					if (q.IsReady() && useQ)
						q.CastIfHitchanceEquals(target, HitChance.High, true);
					if (e.IsReady() && useE)
						e.Cast(target, true);
				}
			}
		}
		
		static void DoLaneClear(bool useW, bool useE) {
			//W>E
			List<Obj_AI_Base> minions = MinionManager.GetMinions(player.Position, w.Range);
			var wCastLocation = MinionManager.GetBestCircularFarmLocation(minions.Select(minion => minion.Position.To2D()).ToList(), w.Width, w.Range);
			if (w.IsReady() && wCastLocation.MinionsHit > 2 && useW) {
				w.Cast(wCastLocation.Position);
			}
			if (e.IsReady() && useE) {
				foreach (Obj_AI_Base minion in minions) {
					if (IsAblazed(minion) && player.Distance(minion) < e.Range) {
						e.Cast(minion);
						break;
					}
				}
			}
		}
		
		static void Draw_OnDraw(EventArgs args) {
			if (menu.Item("drawQ").GetValue<bool>()) {
				if (q.IsReady()) {
					Render.Circle.DrawCircle(player.Position, player.BoundingRadius + q.Range, Color.LightGreen);
				} else {
					Render.Circle.DrawCircle(player.Position, player.BoundingRadius + q.Range, Color.Red);
				}
			}
			if (menu.Item("drawW").GetValue<bool>()) {
				if (w.IsReady()) {
					Render.Circle.DrawCircle(player.Position, player.BoundingRadius + w.Range, Color.LightGreen);
				} else {
					Render.Circle.DrawCircle(player.Position, player.BoundingRadius + w.Range, Color.Red);
				}
			}
			if (menu.Item("drawE").GetValue<bool>()) {
				if (e.IsReady()) {
					Render.Circle.DrawCircle(player.Position, player.BoundingRadius + e.Range, Color.LightGreen);
				} else {
					Render.Circle.DrawCircle(player.Position, player.BoundingRadius + e.Range, Color.Red);
				}
			}
			if (menu.Item("drawR").GetValue<bool>()) {
				if (r.IsReady()) {
					Render.Circle.DrawCircle(player.Position, player.BoundingRadius + r.Range, Color.LightGreen);
				} else {
					Render.Circle.DrawCircle(player.Position, player.BoundingRadius + r.Range, Color.Red);
				}
			}
			
			/*HP Bar Damage Indicator --- All credits to xSalice*/
			if (menu.Item("drawDmg").GetValue<bool>())
				DrawHPBarDamage();
		}
		
		static void InitializeMenu() {
			menu = new Menu("Dat Brand", "DatBrand", true);
			
			Menu orbwalkerMenu = menu.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
			orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
			
			Menu tsMenu = menu.AddSubMenu(new Menu("Target Selector", "TS"));
			TargetSelector.AddToMenu(tsMenu);
			
			Menu comboMenu = menu.AddSubMenu(new Menu("Combo", "combo"));
			comboMenu.AddItem(new MenuItem("comboUseQ", "Use Q").SetValue(true));
			comboMenu.AddItem(new MenuItem("comboUseW", "Use W").SetValue(true));
			comboMenu.AddItem(new MenuItem("comboUseE", "Use E").SetValue(true));
			comboMenu.AddItem(new MenuItem("comboUseR", "Use R").SetValue(true));
			comboMenu.AddItem(new MenuItem("comboUseIgn", "Use Ignite").SetValue(true));
			               			
			Menu harassMenu = menu.AddSubMenu(new Menu("Harass", "harass"));
			harassMenu.AddItem(new MenuItem("harassUseQ", "Use Q").SetValue(true));
			harassMenu.AddItem(new MenuItem("harassUseW", "Use W").SetValue(true));
			harassMenu.AddItem(new MenuItem("harassUseE", "Use E").SetValue(true));
			harassMenu.AddItem(new MenuItem("harassManaManager", "Mana Manager (%)").SetValue(new Slider(50, 1, 100)));
			
			Menu lcMenu = menu.AddSubMenu(new Menu("Lane Clear", "laneClear"));
			lcMenu.AddItem(new MenuItem("lcUseW", "Use W").SetValue(true));
			lcMenu.AddItem(new MenuItem("lcUseE", "Use E").SetValue(true));
			lcMenu.AddItem(new MenuItem("lcManaManager", "Mana Manager (%)").SetValue(new Slider(30, 1, 100)));
						
			Menu drawingMenu = menu.AddSubMenu(new Menu("Drawing", "drawing"));
			drawingMenu.AddItem(new MenuItem("drawQ", "Draw Q Range").SetValue(true));
			drawingMenu.AddItem(new MenuItem("drawW", "Draw W Range").SetValue(true));
			drawingMenu.AddItem(new MenuItem("drawE", "Draw E Range").SetValue(true));
			drawingMenu.AddItem(new MenuItem("drawR", "Draw R Range").SetValue(true));
			drawingMenu.AddItem(new MenuItem("drawDmg", "Draw Combo Damage").SetValue(true));
			
		}
		
		static bool IsAblazed(Obj_AI_Base unit) {
			return unit.HasBuff("brandablaze", true);
		}
		
		static double getComboDamage(Obj_AI_Base target) {
			double damage = player.GetAutoAttackDamage(target);
			if (q.IsReady() && menu.Item("comboUseQ").GetValue<bool>())
				damage += player.GetSpellDamage(target, SpellSlot.Q);
			if (w.IsReady() && menu.Item("comboUseW").GetValue<bool>())
				damage += player.GetSpellDamage(target, SpellSlot.W);
			if (e.IsReady() && menu.Item("comboUseE").GetValue<bool>())
				damage += player.GetSpellDamage(target, SpellSlot.E);
			if (r.IsReady() && menu.Item("comboUseR").GetValue<bool>())
				damage += player.GetSpellDamage(target, SpellSlot.R);
			if (ignite.IsReady())
				damage += player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
			return damage;
		}
		
		static void DrawHPBarDamage() {
			const int XOffset = 10;
			const int YOffset = 20;
			const int Width = 103;
			const int Height = 8;
			foreach (var unit in ObjectManager.Get<Obj_AI_Hero>().Where(h =>h.IsValid && h.IsHPBarRendered && h.IsEnemy)) {
				var barPos = unit.HPBarPosition;
				var damage = getComboDamage(unit);
				var percentHealthAfterDamage = Math.Max(0, unit.Health - damage) / unit.MaxHealth;
				var yPos = barPos.Y + YOffset;
				var xPosDamage = barPos.X + XOffset + Width * percentHealthAfterDamage;
				var xPosCurrentHp = barPos.X + XOffset + Width * unit.Health / unit.MaxHealth;

				if (damage > unit.Health) {					
					Text.X = (int)barPos.X + XOffset;
					Text.Y = (int)barPos.Y + YOffset - 13;
					Text.text = ((int)(unit.Health - damage)).ToString();
					Text.OnEndScene();
				}

				Drawing.DrawLine((float)xPosDamage, yPos, (float)xPosDamage, yPos + Height, 2, Color.Yellow);
			}
		}
	}
}