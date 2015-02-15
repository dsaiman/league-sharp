using System;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using System.Linq;
using Color = System.Drawing.Color;

namespace PentakillZed {
	public static class MiscHelper {
		
		public static readonly Render.Text Text = new Render.Text(
			                                          0, 0, "", 11, new ColorBGRA(255, 0, 0, 255), "monospace");
				
		public static void DrawHPBarDamage() {
			const int XOffset = 10;
			const int YOffset = 20;
			const int Width = 103;
			const int Height = 8;
			foreach (var unit in ObjectManager.Get<Obj_AI_Hero>().Where(h =>h.IsValid && h.IsHPBarRendered && h.IsEnemy)) {
				var damage = GetComboDamage(unit);
				var barPos = unit.HPBarPosition;
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
		
		public static float GetEnergyCost(SpellSlot spell) {
			if (SpellSlot.Q == spell)
				return 50 + (5 * Zed.q.Level);
			if (SpellSlot.W == spell)
				return 15 + (5 * Zed.w.Level);
			return 50;
		
		}
		
		public static double GetComboDamage(Obj_AI_Base target) {
			double damage = Zed.player.GetAutoAttackDamage(target, true);
			bool useQ = Zed.menuHelper.menu.Item("comboUseQ").GetValue<bool>();
			bool useW = Zed.menuHelper.menu.Item("comboUseW").GetValue<bool>();
			bool useE = Zed.menuHelper.menu.Item("comboUseE").GetValue<bool>();
			if (Zed.r.IsReady()) {
				if (Zed.q.IsReady() && useQ)
					damage += Zed.player.GetSpellDamage(target, SpellSlot.Q) * 1.5;
				if (Zed.e.IsReady() && useE)
					damage += Zed.player.GetSpellDamage(target, SpellSlot.E);
				if (Zed.w.IsReady() && useW)
					damage += Zed.player.GetSpellDamage(target, SpellSlot.Q) * 0.25;						
				damage += Zed.player.GetAutoAttackDamage(target, true) * 2;
				damage += Zed.player.BaseAttackDamage + (((20 + (Zed.r.Level - 1) * 15) / 100) * damage);
			} else if (Zed.w.IsReady()) {
				if (Zed.q.IsReady() && useQ)
					damage += Zed.player.GetSpellDamage(target, SpellSlot.Q) * 1.5;
				if (Zed.e.IsReady() && useE)
					damage += Zed.player.GetSpellDamage(target, SpellSlot.E);
			} else {
				if (Zed.q.IsReady() && useQ)
					damage += Zed.player.GetSpellDamage(target, SpellSlot.Q);
				if (Zed.e.IsReady() && useE)
					damage += Zed.player.GetSpellDamage(target, SpellSlot.E);
			}
			if (Zed.ignite.IsReady())
				damage += Zed.player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
			return damage;
		}
		
		public static Vector3 GetWCastPosition(Obj_AI_Base target) {
			double theta = Math.Atan(Math.Abs(Zed.player.Position.Y - target.Position.Y) / Math.Abs(Zed.player.Position.X - target.Position.X));
			if (Zed.player.Position.X >= target.Position.X && Zed.player.Position.Y >= target.Position.Y) {
				return new Vector3((float)(target.Position.X - (Math.Cos(theta) * 300)), (float)(target.Position.Y - (Math.Sin(theta) * 300)), target.Position.Z);
			} else if (Zed.player.Position.X < target.Position.X && Zed.player.Position.Y >= target.Position.Y) {
				return new Vector3((float)(target.Position.X + (Math.Cos(theta) * 300)), (float)(target.Position.Y - (Math.Sin(theta) * 300)), target.Position.Z);
			} else if (Zed.player.Position.X >= target.Position.X && Zed.player.Position.Y < target.Position.Y) {
				return new Vector3((float)(target.Position.X - (Math.Cos(theta) * 300)), (float)(target.Position.Y + (Math.Sin(theta) * 300)), target.Position.Z);
			} else if (Zed.player.Position.X < target.Position.X && Zed.player.Position.Y < target.Position.Y) {
				return new Vector3((float)(target.Position.X + (Math.Cos(theta) * 300)), (float)(target.Position.Y + (Math.Sin(theta) * 300)), target.Position.Z);
			}
			return Vector3.Zero;
		}
		
	}
}
