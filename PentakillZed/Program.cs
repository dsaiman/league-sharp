
using System;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using System.Linq;
using System.Collections.Generic;
using Color = System.Drawing.Color;

namespace PentakillZed {
	
	class Program {
		
		public const string champName = "Zed";
		public const string ultBuffName = "zedulttargetmark";
		public static Obj_AI_Hero player { get { return ObjectManager.Player; } }
		public static Spell q, w, e, r;
		public static SpellSlot ignite;
		public static Orbwalking.Orbwalker orbwalker;
		public static Menu menu;
		public static readonly Render.Text Text = new Render.Text(
			                                          0, 0, "", 11, new ColorBGRA(255, 0, 0, 255), "monospace");
		public static float timeWCasted = 0;
		public static float timeRCasted = 0;
		
		public static Obj_AI_Minion wShadow = null;
		public static bool wShadowCreated = false;
		public static bool wShadowFound = false;
		public static int wShadowTick = 0;
		public static int WCastTick = 0;
		
		public static Obj_AI_Minion rShadow;
		public static bool rShadowCreated = false;
		public static bool rShadowFound = false;
		public static int rShadowTick = 0;
		public static int RCastTick = 0;
		public static Vector3 rShadowNearPosition;
				
	/*	public static void Main(string[] args) {
				CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
		}*/
	
		public static void Game_OnGameLoad(EventArgs args) {
			if (player.ChampionName != champName)
				return;
			q = new Spell(SpellSlot.Q, 900);
			q.SetSkillshot(0.25f, 50f, 1700f, false, SkillshotType.SkillshotLine);
			w = new Spell(SpellSlot.W, 550);
			e = new Spell(SpellSlot.E, 290);
			r = new Spell(SpellSlot.R, 625);
			ignite = player.GetSpellSlot("summonerdot");
			
			InitializeMenu();
			
			menu.AddToMainMenu();
			Game.OnGameUpdate += Game_OnGameUpdate;
			Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
			//GameObject.OnCreate += OnSpellCast;
			Drawing.OnDraw += Draw_OnDraw;
			Game.PrintChat("<font color ='#33FFFF'>Pentakill Zed</font> by <font color = '#FFFF00'>GoldenGates</font> loaded, enjoy!");
		}
		
		public static void Game_OnGameUpdate(EventArgs args) {
			if (player.IsDead)
				return;
			Checks();
			if (menu.Item("comboLine").GetValue<KeyBind>().Active) {
				LineComboHandler();
			}

			switch (orbwalker.ActiveMode) {
				case Orbwalking.OrbwalkingMode.Combo:
					ComboHandler();
					break;
				case Orbwalking.OrbwalkingMode.Mixed:
					HarassHandler();
					break;
				case Orbwalking.OrbwalkingMode.LaneClear:
					LaneClearHandler();
					break;
				case Orbwalking.OrbwalkingMode.LastHit:
					LastHitHandler();
					break;
			}
		}
		
		public static void OnProcessSpellCast(Obj_AI_Base obj, GameObjectProcessSpellCastEventArgs castedSpell) {
			if (obj.Name == player.Name && castedSpell.SData.Name.ToLower().Contains("zedshadow")) {
				wShadowCreated = true;
			}
			if (obj.Name == player.Name && castedSpell.SData.Name.ToLower().Contains("zedult")) {
				Game.PrintChat("Ult casted");
				rShadowCreated = true;
				rShadowNearPosition = player.ServerPosition;
			}
		}
		
		public static void Checks() {
			if (wShadowCreated && !wShadowFound)
				SearchForClone("W");
			if (rShadowCreated && !rShadowFound)
				SearchForClone("R");

			if (wShadow != null && (wShadowTick < Environment.TickCount - 4000)) {
				wShadow = null;
				wShadowCreated = false;
				wShadowFound = false;
			}

			if (rShadow != null && (rShadowTick < Environment.TickCount - 6000)) {
				rShadow = null;
				rShadowCreated = false;
				rShadowFound = false;
			}
			
		}
		
		public static void SearchForClone(String p) {
			Obj_AI_Minion shadow;
			if (p != null && p == "W") {
				shadow = ObjectManager.Get<Obj_AI_Minion>().FirstOrDefault(hero => (hero.Name == "Shadow" && hero.IsAlly && (hero != rShadow)));
				if (shadow != null) {
					wShadow = shadow;
					wShadowFound = true;
					wShadowTick = Environment.TickCount;
				}
			}
			if (p == null || p != "R")
				return;
			shadow = ObjectManager.Get<Obj_AI_Minion>().FirstOrDefault(hero => ((hero.ServerPosition.Distance(rShadowNearPosition)) < 50) && hero.Name == "Shadow" && hero.IsAlly && hero != wShadow);
			if (shadow == null)
				return;
			rShadow = shadow;
			rShadowFound = true;
			rShadowTick = Environment.TickCount;
		}
		
		public static void LineComboHandler() {
			Obj_AI_Hero target = TargetSelector.GetTarget(q.Range - 100, TargetSelector.DamageType.Physical);
			if (target == null) {
				player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
			} else {
				player.IssueOrder(GameObjectOrder.AttackUnit, target);
			}
			if (GetComboDamage(target) > (target.Health * 0.7)) {
				if (player.Mana >= (GetEnergyCost(SpellSlot.Q) + GetEnergyCost(SpellSlot.W) + GetEnergyCost(SpellSlot.E))) {
					if (r.IsReady() && target != null && rShadow == null && !rShadowFound) {
						if (750 < (Environment.TickCount - timeRCasted))
							r.Cast(target, true);
						timeRCasted = Environment.TickCount;				
					}
					if (menu.Item("comboUseW").GetValue<bool>() && w.IsReady() && target != null && wShadow == null && !wShadowFound && rShadowCreated) {
						double theta = Math.Atan(Math.Abs(player.Position.Y - target.Position.Y) / Math.Abs(player.Position.X - target.Position.X));
						var wCastPos = new Vector3(0, 0, 0);
						if (player.Position.X >= target.Position.X && player.Position.Y >= target.Position.Y) {
							wCastPos = new Vector3((float)(target.Position.X - (Math.Cos(theta) * 300)), (float)(target.Position.Y - (Math.Sin(theta) * 300)), target.Position.Z);
						} else if (player.Position.X < target.Position.X && player.Position.Y >= target.Position.Y) {
							wCastPos = new Vector3((float)(target.Position.X + (Math.Cos(theta) * 300)), (float)(target.Position.Y - (Math.Sin(theta) * 300)), target.Position.Z);
						} else if (player.Position.X >= target.Position.X && player.Position.Y < target.Position.Y) {
							wCastPos = new Vector3((float)(target.Position.X - (Math.Cos(theta) * 300)), (float)(target.Position.Y + (Math.Sin(theta) * 300)), target.Position.Z);
						} else if (player.Position.X < target.Position.X && player.Position.Y < target.Position.Y) {
							wCastPos = new Vector3((float)(target.Position.X + (Math.Cos(theta) * 300)), (float)(target.Position.Y + (Math.Sin(theta) * 300)), target.Position.Z);
						}
						if (500 < (Environment.TickCount - timeWCasted))
							w.Cast(wCastPos, true);
						timeWCasted = Environment.TickCount;
				
					}
				}
				if (menu.Item("comboUseE").GetValue<bool>() && e.IsReady() && target != null) {
					if ((wShadow != null && target.Distance(wShadow) < e.Range) || (rShadow != null && target.Distance(rShadow) < e.Range) || target.Distance(player) < e.Range) {
						e.Cast();
					}
				}
				if (menu.Item("comboUseQ").GetValue<bool>() && q.IsReady() && target != null) {
					q.CastIfHitchanceEquals(target, HitChance.High, true);
				}
				if (GetComboDamage(target) > target.Health && target.HealthPercentage() < 30) {
					player.Spellbook.CastSpell(ignite, true);
				}
			}
		}
		
		public static void ComboHandler() {
			Obj_AI_Hero target = TargetSelector.GetTarget(q.Range - 50, TargetSelector.DamageType.Physical);
			if (player.Mana >= (GetEnergyCost(SpellSlot.Q) + GetEnergyCost(SpellSlot.W) + GetEnergyCost(SpellSlot.E))) {
				if (menu.Item("comboUseR").GetValue<bool>() && r.IsReady() && target != null && rShadow == null && !rShadowFound && GetComboDamage(target) > (target.Health * 0.8)) {
					if (1000 < (Environment.TickCount - timeRCasted))
						r.Cast(target, true);
					timeRCasted = Environment.TickCount;
				
				}
				if (menu.Item("comboUseW").GetValue<bool>() && w.IsReady() && target != null && wShadow == null && !wShadowFound) {
					if (500 < (Environment.TickCount - timeWCasted))
						w.Cast(new Vector3(target.Position.X, target.Position.Y, target.Position.Z), true);
					timeWCasted = Environment.TickCount;
				
				}
			}
			if (menu.Item("comboUseE").GetValue<bool>() && e.IsReady() && target != null) {
				if ((wShadow != null && target.Distance(wShadow) < e.Range) || (rShadow != null && target.Distance(rShadow) < e.Range) || target.Distance(player) < e.Range) {
					e.Cast();
				}
			}
			if (menu.Item("comboUseQ").GetValue<bool>() && q.IsReady() && target != null) {
				q.CastIfHitchanceEquals(target, HitChance.High, true);
			}
			if (GetComboDamage(target) > target.Health && target.HealthPercentage() < 30) {
				player.Spellbook.CastSpell(ignite, true);
			}
		}
		
		public static void HarassHandler() {
			Obj_AI_Hero target = TargetSelector.GetTarget(q.Range - 50, TargetSelector.DamageType.Physical);
			if (player.Mana >= (GetEnergyCost(SpellSlot.Q) + GetEnergyCost(SpellSlot.W))) {
				if (menu.Item("harassUseW").GetValue<bool>() && w.IsReady() && target != null && wShadow == null && !wShadowFound) {
					if (500 < (Environment.TickCount - timeWCasted))
						w.Cast(target.Position, true);
					timeWCasted = Environment.TickCount;
				
				}
				if (menu.Item("harassUseE").GetValue<bool>() && e.IsReady() && target != null) {
					if ((wShadow != null && target.Distance(wShadow) < e.Range) || target.Distance(player) < e.Range) {
						e.Cast();
					}
				}
				if (menu.Item("harassUseQ").GetValue<bool>() && q.IsReady() && target != null) {
					q.CastIfHitchanceEquals(target, HitChance.High, true);
				}
			}
		}
		
		public static void LaneClearHandler() {
			List<Obj_AI_Base> minionsQ = MinionManager.GetMinions(player.Position, q.Range);
			List<Obj_AI_Base> minionsE = MinionManager.GetMinions(player.Position, e.Range);
			if (player.ManaPercentage() > menu.Item("lcEnergyManager").GetValue<Slider>().Value) {
				if (menu.Item("lcUseE").GetValue<bool>() && e.IsReady()) {
					if (minionsE.Count > 2) {
						e.Cast();
					}
				}
				if (menu.Item("lcUseQ").GetValue<bool>() && q.IsReady()) {
					var line = q.GetLineFarmLocation(minionsQ, q.Width);
					if (line.MinionsHit > 2) {
						q.Cast(line.Position);
					}
				}
			}
		}
		
		public static void LastHitHandler() {
			Obj_AI_Base minion = MinionManager.GetMinions(player.Position, q.Range).FirstOrDefault(w => (w.Health < q.GetDamage(w) * .80));
			if (menu.Item("lastHitUseQ").GetValue<bool>() && minion != null) {
				if (q.IsReady() && player.Distance(minion) > player.AttackRange + 150 && player.ManaPercentage() > 50) {
					q.CastIfHitchanceEquals(minion, HitChance.High, true);
				}
			}			
		}
		
		public static float GetEnergyCost(SpellSlot spell) {
			if (SpellSlot.Q == spell)
				return 50 + (5 * q.Level);
			if (SpellSlot.W == spell)
				return 15 + (5 * w.Level);
			return 50;
		
		}
		
		public static void Draw_OnDraw(EventArgs args) {
			//Spell drawing
			if (menu.Item("drawQ").GetValue<bool>()) {
				if (q.IsReady())
					Render.Circle.DrawCircle(player.Position, q.Range, Color.LightGreen);
				else
					Render.Circle.DrawCircle(player.Position, q.Range, Color.Red);
			}
			if (menu.Item("drawW").GetValue<bool>()) {
				if (w.IsReady())
					Render.Circle.DrawCircle(player.Position, w.Range, Color.LightGreen);
				else
					Render.Circle.DrawCircle(player.Position, w.Range, Color.Red);
			}
			if (menu.Item("drawE").GetValue<bool>()) {
				if (e.IsReady())
					Render.Circle.DrawCircle(player.Position, e.Range, Color.LightGreen);
				else
					Render.Circle.DrawCircle(player.Position, e.Range, Color.Red);
			}
			if (menu.Item("drawR").GetValue<bool>()) {
				if (r.IsReady())
					Render.Circle.DrawCircle(player.Position, r.Range, Color.LightGreen);
				else
					Render.Circle.DrawCircle(player.Position, r.Range, Color.Red);
			}
			if (menu.Item("drawDmg").GetValue<bool>()) {
				DrawHPBarDamage();
			}
			
			if (rShadowFound && rShadow.Position.IsOnScreen()) {
				Render.Circle.DrawCircle(rShadow.Position, 100, Color.Yellow);
			}
			if (wShadowFound && wShadow.Position.IsOnScreen()) {
				Render.Circle.DrawCircle(wShadow.Position, 100, Color.Aquamarine);
			}
			Obj_AI_Hero target = TargetSelector.GetTarget(q.Range - 100, TargetSelector.DamageType.Physical);
			double theta = Math.Atan(Math.Abs(player.Position.Y - target.Position.Y) / Math.Abs(player.Position.X - target.Position.X));
			var wCastPos = new Vector3(0, 0, 0);
			if (player.Position.X >= target.Position.X && player.Position.Y >= target.Position.Y) {
				wCastPos = new Vector3((float)(target.Position.X - (Math.Cos(theta) * 500)), (float)(target.Position.Y - (Math.Sin(theta) * 500)), target.Position.Z);
			} else if (player.Position.X < target.Position.X && player.Position.Y >= target.Position.Y) {
				wCastPos = new Vector3((float)(target.Position.X + (Math.Cos(theta) * 500)), (float)(target.Position.Y - (Math.Sin(theta) * 500)), target.Position.Z);
			} else if (player.Position.X >= target.Position.X && player.Position.Y < target.Position.Y) {
				wCastPos = new Vector3((float)(target.Position.X - (Math.Cos(theta) * 500)), (float)(target.Position.Y + (Math.Sin(theta) * 500)), target.Position.Z);
			} else if (player.Position.X < target.Position.X && player.Position.Y < target.Position.Y) {
				wCastPos = new Vector3((float)(target.Position.X + (Math.Cos(theta) * 500)), (float)(target.Position.Y + (Math.Sin(theta) * 500)), target.Position.Z);
			}
			Render.Circle.DrawCircle(wCastPos, 100, Color.Red);
			
		}
		
		public static double GetComboDamage(Obj_AI_Base target) {
			double damage = player.GetAutoAttackDamage(target, true);
			if (r.IsReady()) {
				if (q.IsReady() && menu.Item("comboUseQ").GetValue<bool>()) {
					damage += player.GetSpellDamage(target, SpellSlot.Q);
					damage += player.GetSpellDamage(target, SpellSlot.Q) * 0.5;
				}
				if (e.IsReady() && menu.Item("comboUseE").GetValue<bool>()) {
					damage += player.GetSpellDamage(target, SpellSlot.E);
				}
				if (w.IsReady()) {
					damage += player.GetSpellDamage(target, SpellSlot.Q) * 0.25;						
				}
				damage += player.GetAutoAttackDamage(target, true) * 2;
				damage += player.BaseAttackDamage + (((20 + (r.Level - 1) * 15) / 100) * damage);
			} else if (w.IsReady()) {
				if (q.IsReady() && menu.Item("comboUseQ").GetValue<bool>()) {
					damage += player.GetSpellDamage(target, SpellSlot.Q);
					damage += player.GetSpellDamage(target, SpellSlot.Q) * 0.5;
				}
				if (e.IsReady() && menu.Item("comboUseE").GetValue<bool>()) {
					damage += player.GetSpellDamage(target, SpellSlot.E);
				}
			} else {
				if (q.IsReady() && menu.Item("comboUseQ").GetValue<bool>()) {
					damage += player.GetSpellDamage(target, SpellSlot.Q);
				}
				if (e.IsReady() && menu.Item("comboUseE").GetValue<bool>()) {
					damage += player.GetSpellDamage(target, SpellSlot.E);
				}
			}
			if (ignite.IsReady())
				damage += player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
			return damage;
		}
		
		public static void InitializeMenu() {
			menu = new Menu("Pentakill Zed", "PentakillZed", true);
			
			Menu orbwalkerMenu = menu.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
			orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
			
			Menu tsMenu = menu.AddSubMenu(new Menu("Target Selector", "TS"));
			TargetSelector.AddToMenu(tsMenu);
			
			//Combo
			Menu comboMenu = menu.AddSubMenu(new Menu("Combo", "Combo"));
			comboMenu.AddItem(new MenuItem("comboUseQ", "Use Q").SetValue(true));
			comboMenu.AddItem(new MenuItem("comboUseW", "Use W").SetValue(true));
			comboMenu.AddItem(new MenuItem("comboUseE", "Use E").SetValue(true));
			comboMenu.AddItem(new MenuItem("comboUseR", "Use R").SetValue(true));
			comboMenu.AddItem(new MenuItem("comboLine", "Line Combo").SetValue(new KeyBind("X".ToCharArray()[0], KeyBindType.Press)));
			
			//Harass
			Menu harassMenu = menu.AddSubMenu(new Menu("Harass", "harass"));
			harassMenu.AddItem(new MenuItem("harassUseQ", "Use Q").SetValue(true));
			harassMenu.AddItem(new MenuItem("harassUseW", "Use W").SetValue(true));
			harassMenu.AddItem(new MenuItem("harassUseE", "Use E").SetValue(true));
			
			//Lane Clear
			Menu lcMenu = menu.AddSubMenu(new Menu("Lane Clear", "laneClear"));
			lcMenu.AddItem(new MenuItem("lcUseQ", "Use Q").SetValue(true));
			lcMenu.AddItem(new MenuItem("lcUseE", "Use E").SetValue(true));
			lcMenu.AddItem(new MenuItem("lcEnergyManager", "Energy Manager (%)").SetValue(new Slider(30, 1, 100)));
			
			//Last Hit			
			Menu lastHitMenu = menu.AddSubMenu(new Menu("Last Hit", "lastHit"));
			lastHitMenu.AddItem(new MenuItem("lastHitUseQ", "Use Q").SetValue(true));
			
			Menu drawingMenu = menu.AddSubMenu(new Menu("Drawing", "drawing"));
			drawingMenu.AddItem(new MenuItem("drawQ", "Draw Q Range").SetValue(true));
			drawingMenu.AddItem(new MenuItem("drawW", "Draw W Range").SetValue(true));
			drawingMenu.AddItem(new MenuItem("drawE", "Draw E Range").SetValue(true));
			drawingMenu.AddItem(new MenuItem("drawR", "Draw R Range").SetValue(true));
			drawingMenu.AddItem(new MenuItem("drawDmg", "Draw Combo Damage").SetValue(true));
		}
		
		static void DrawHPBarDamage() {
			const int XOffset = 10;
			const int YOffset = 20;
			const int Width = 103;
			const int Height = 8;
			foreach (var unit in ObjectManager.Get<Obj_AI_Hero>().Where(h =>h.IsValid && h.IsHPBarRendered && h.IsEnemy)) {
				var barPos = unit.HPBarPosition;
				var damage = GetComboDamage(unit);
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