using System;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using System.Linq;
using System.Collections.Generic;
using Color = System.Drawing.Color;

namespace PentakillZed {
	
	public static class ModeHandler {
		
		public static void LineCombo() {
			Obj_AI_Hero target = TargetSelector.GetTarget(Zed.q.Range - 100, TargetSelector.DamageType.Physical);
			if (target == null) {
				Zed.player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
			} else {
				Zed.player.IssueOrder(GameObjectOrder.AttackUnit, target);
			}
			if (MiscHelper.GetComboDamage(target) > (target.Health * 0.7)) {
				if (Zed.player.Mana >= (MiscHelper.GetEnergyCost(SpellSlot.Q) + MiscHelper.GetEnergyCost(SpellSlot.W) + MiscHelper.GetEnergyCost(SpellSlot.E))) {
					if (Zed.r.IsReady() && target != null && !Zed.rData.isCreated()) {
						if (750 < (Environment.TickCount - Zed.rData.getTimeCasted()))
							Zed.r.Cast(target, true);
						Zed.rData.setTimeCasted(Environment.TickCount);
					}
					if (Zed.menuHelper.menu.Item("comboUseW").GetValue<bool>() && Zed.w.IsReady() && target != null && !Zed.wData.isCreated() && Zed.rData.isCreated()) {
						if (500 < (Environment.TickCount - Zed.wData.getTimeCasted()))
							Zed.w.Cast(MiscHelper.GetWCastPosition(target), true);
						Zed.wData.setTimeCasted(Environment.TickCount);
					}
				}
				if (Zed.menuHelper.menu.Item("comboUseE").GetValue<bool>() && Zed.e.IsReady() && target != null) {
					if ((Zed.wData.isCreated() && target.Distance(Zed.wData.getShadow()) < Zed.e.Range) || (Zed.rData.isCreated() && target.Distance(Zed.rData.getShadow()) < Zed.e.Range) || target.Distance(Zed.player) < Zed.e.Range) {
						Zed.e.Cast();
					}
				}
				if (Zed.menuHelper.menu.Item("comboUseQ").GetValue<bool>() && Zed.q.IsReady() && target != null) {
					Zed.q.CastIfHitchanceEquals(target, HitChance.High, true);
				}
				if (MiscHelper.GetComboDamage(target) > target.Health && target.HealthPercentage() < 30) {
					Zed.player.Spellbook.CastSpell(Zed.ignite, true);
				}
			}
		}
		
		public static void Combo() {
			Obj_AI_Hero target = TargetSelector.GetTarget(Zed.q.Range - 50, TargetSelector.DamageType.Physical);
			if (Zed.player.Mana >= (MiscHelper.GetEnergyCost(SpellSlot.Q) + MiscHelper.GetEnergyCost(SpellSlot.W) + MiscHelper.GetEnergyCost(SpellSlot.E))) {
				if (Zed.menuHelper.menu.Item("comboUseR").GetValue<bool>() && Zed.r.IsReady() && target != null && !Zed.rData.isCreated() && MiscHelper.GetComboDamage(target) > (target.Health * 0.8)) {
					if (750 < (Environment.TickCount - Zed.rData.getTimeCasted()))
						Zed.r.Cast(target, true);
					Zed.rData.setTimeCasted(Environment.TickCount);
					
				}
				if (Zed.menuHelper.menu.Item("comboUseW").GetValue<bool>() && Zed.w.IsReady() && target != null && !Zed.wData.isCreated()) {
					if (500 < (Environment.TickCount - Zed.wData.getTimeCasted()))
						Zed.w.Cast(new Vector3(target.Position.X, target.Position.Y, target.Position.Z), true);
					Zed.wData.setTimeCasted(Environment.TickCount);
					
				}
			}
			if (Zed.menuHelper.menu.Item("comboUseE").GetValue<bool>() && Zed.e.IsReady() && target != null) {
				if ((Zed.wData.isCreated() && target.Distance(Zed.wData.getShadow()) < Zed.e.Range) || (Zed.rData.isCreated() && target.Distance(Zed.rData.getShadow()) < Zed.e.Range) || target.Distance(Zed.player) < Zed.e.Range) {
					Zed.e.Cast();
				}
			}
			if (Zed.menuHelper.menu.Item("comboUseQ").GetValue<bool>() && Zed.q.IsReady() && target != null) {
				Zed.q.CastIfHitchanceEquals(target, HitChance.High, true);
			}
			if (Zed.menuHelper.menu.Item("comboIgn").GetValue<bool>() && MiscHelper.GetComboDamage(target) > target.Health && target.HealthPercentage() < 30 && target != null) {
				Zed.player.Spellbook.CastSpell(Zed.ignite, true);
			}
		}
		
		public static void Harass() {
			Obj_AI_Hero target = TargetSelector.GetTarget(Zed.q.Range - 50, TargetSelector.DamageType.Physical);
			if (Zed.player.Mana >= (MiscHelper.GetEnergyCost(SpellSlot.Q) + MiscHelper.GetEnergyCost(SpellSlot.W) + MiscHelper.GetEnergyCost(SpellSlot.E))) {
				if (Zed.menuHelper.menu.Item("harassUseW").GetValue<bool>() && Zed.w.IsReady() && target != null && !Zed.wData.isCreated()) {
					if (500 < (Environment.TickCount - Zed.wData.getTimeCasted()))
						Zed.w.Cast(target.Position, true);
					Zed.wData.setTimeCasted(Environment.TickCount);
					
				}
				if (Zed.menuHelper.menu.Item("harassUseE").GetValue<bool>() && Zed.e.IsReady() && target != null) {
					if ((Zed.wData.isCreated() && target.Distance(Zed.wData.getShadow()) < Zed.e.Range) || target.Distance(Zed.player) < Zed.e.Range) {
						Zed.e.Cast();
					}
				}
				if (Zed.menuHelper.menu.Item("harassUseQ").GetValue<bool>() && Zed.q.IsReady() && target != null) {
					Zed.q.CastIfHitchanceEquals(target, HitChance.High, true);
				}
			}
		}
		
		public static void LaneClear() {
			List<Obj_AI_Base> minionsQ = MinionManager.GetMinions(Zed.player.Position, Zed.q.Range);
			List<Obj_AI_Base> minionsE = MinionManager.GetMinions(Zed.player.Position, Zed.e.Range);
			if (Zed.player.ManaPercentage() > Zed.menuHelper.menu.Item("lcEnergyManager").GetValue<Slider>().Value) {
				if (Zed.menuHelper.menu.Item("lcUseE").GetValue<bool>() && Zed.e.IsReady()) {
					if (minionsE.Count > 2) {
						Zed.e.Cast();
					}
				}
				if (Zed.menuHelper.menu.Item("lcUseQ").GetValue<bool>() && Zed.q.IsReady()) {
					var line = Zed.q.GetLineFarmLocation(minionsQ, Zed.q.Width);
					if (line.MinionsHit > 2) {
						Zed.q.Cast(line.Position);
					}
				}
			}
		}
		
		public static void LastHit() {
			Obj_AI_Base minion = MinionManager.GetMinions(Zed.player.Position, Zed.q.Range).FirstOrDefault(w => (w.Health < Zed.q.GetDamage(w) * .80));
			if (Zed.menuHelper.menu.Item("lastHitUseQ").GetValue<bool>() && minion != null) {
				if (Zed.q.IsReady() && Zed.player.Distance(minion) > Zed.player.AttackRange + 150 && Zed.player.ManaPercentage() > 50) {
					Zed.q.CastIfHitchanceEquals(minion, HitChance.High, true);
				}
			}
		}
	}
}
