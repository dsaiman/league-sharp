using System;
using LeagueSharp;
using LeagueSharp.Common;
using System.Linq;
using Color = System.Drawing.Color;

namespace PentakillZed {
	public class Zed {
		
		/**Common Variables**/
		public static Orbwalking.Orbwalker orbwalker;
		public static Spell q, w, e, r;
		public static SpellSlot ignite;
		public static Obj_AI_Hero player;
		public static MenuHelper menuHelper;
		public static ZedWData wData;
		public static ZedRData rData;
		
		public static void onGameLoad(EventArgs args) {
			/**Quick MenuHelper setup**/
			menuHelper = new MenuHelper();
			orbwalker = new Orbwalking.Orbwalker(menuHelper.getOrbwalkerMenu());
			wData = new ZedWData();
			rData = new ZedRData();
			
			player = ObjectManager.Player;
			if (player.ChampionName != "Zed")
				return;
			
			/**Spell Initiation**/
			q = new Spell(SpellSlot.Q, 900);
			q.SetSkillshot(0.25f, 50f, 1700f, false, SkillshotType.SkillshotLine);
			w = new Spell(SpellSlot.W, 550);
			e = new Spell(SpellSlot.E, 290);
			r = new Spell(SpellSlot.R, 625);
			ignite = player.GetSpellSlot("summonerdot");
			
			menuHelper.menu.AddToMainMenu();
			Game.OnGameUpdate += OnGameUpdate;
			Drawing.OnDraw += OnDraw;
			Console.Clear();
			Console.WriteLine("Pentakill Zed started...");
			Game.PrintChat("<font color ='#33FFFF'>Pentakill Zed</font> by <font color = '#FFFF00'>GoldenGates</font> loaded, enjoy!");
		}
		
		public static void OnGameUpdate(EventArgs argss) {
			if (player.IsDead)
				return;
			performChecks();
			if (menuHelper.menu.Item("comboLine").GetValue<KeyBind>().Active) {
				ModeHandler.LineCombo();
			}			
			switch (orbwalker.ActiveMode) {
				case Orbwalking.OrbwalkingMode.Combo:
					ModeHandler.Combo();
					break;
				case Orbwalking.OrbwalkingMode.Mixed:
					ModeHandler.Harass();
					break;
				case Orbwalking.OrbwalkingMode.LaneClear:
					ModeHandler.LaneClear();
					break;
				case Orbwalking.OrbwalkingMode.LastHit:
					ModeHandler.LastHit();
					break;
			}
						
		}
		
		public static void performChecks() {	
			if (LastCastedSpell.LastCastPacketSent.Slot == SpellSlot.W) {
				Obj_AI_Minion shadow = ObjectManager.Get<Obj_AI_Minion>().FirstOrDefault(x => x.Name == "Shadow");
				if (shadow != null) {
					wData.setCreated(true);
					wData.setShadow(shadow);	
					wData.setTimeCasted(Environment.TickCount);
				}
			}
			if (LastCastedSpell.LastCastPacketSent.Slot == SpellSlot.R) {
				Obj_AI_Minion shadow = ObjectManager.Get<Obj_AI_Minion>().FirstOrDefault(x => x.Name == "Shadow");
				if (shadow != null) {
					rData.setCreated(true);
					rData.setShadow(shadow);	
					rData.setTimeCasted(Environment.TickCount);
				}
			}		
			if (wData.isCreated() && (wData.getTimeCasted() < Environment.TickCount - 4000)) {
				wData.setShadow(null);
				wData.setCreated(false);
			}
			if (rData.isCreated() && (rData.getTimeCasted() < Environment.TickCount - 6000)) {
				rData.setShadow(null);
				rData.setCreated(false);
			}
		}
							
		public static void OnDraw(EventArgs args) {
			if (player.IsDead)
				return;
			if (menuHelper.menu.Item("drawQ").GetValue<bool>()) {
				if (q.IsReady())
					Render.Circle.DrawCircle(player.Position, q.Range, Color.LightGreen);
				else
					Render.Circle.DrawCircle(player.Position, q.Range, Color.Red);
			}
			if (menuHelper.menu.Item("drawW").GetValue<bool>()) {
				if (w.IsReady())
					Render.Circle.DrawCircle(player.Position, w.Range, Color.LightGreen);
				else
					Render.Circle.DrawCircle(player.Position, w.Range, Color.Red);
			}
			if (menuHelper.menu.Item("drawE").GetValue<bool>()) {
				if (e.IsReady())
					Render.Circle.DrawCircle(player.Position, e.Range, Color.LightGreen);
				else
					Render.Circle.DrawCircle(player.Position, e.Range, Color.Red);
			}
			if (menuHelper.menu.Item("drawR").GetValue<bool>()) {
				if (r.IsReady())
					Render.Circle.DrawCircle(player.Position, r.Range, Color.LightGreen);
				else
					Render.Circle.DrawCircle(player.Position, r.Range, Color.Red);
			}
			if (menuHelper.menu.Item("drawShadow").GetValue<bool>() && rData.getShadow() != null && rData.isCreated() && rData.getShadow().Position.IsOnScreen()) {
				Render.Circle.DrawCircle(rData.getShadow().Position, 100, Color.Yellow);
			}
			if (menuHelper.menu.Item("drawShadow").GetValue<bool>() && wData.getShadow() != null && wData.isCreated() && wData.getShadow().Position.IsOnScreen()) {
				Render.Circle.DrawCircle(wData.getShadow().Position, 100, Color.Cyan);
			}
			if (menuHelper.menu.Item("drawDmg").GetValue<bool>()) {
				MiscHelper.DrawHPBarDamage();
			}
		}
			
		public static void Main(string[] args) {
			CustomEvents.Game.OnGameLoad += onGameLoad;
		}
	}
}
