using System;
using LeagueSharp;
using SharpDX;
using System.Linq;
using Color = System.Drawing.Color;

namespace PentakillZed {
	
	public class ZedRData {
		
		float timeCasted;
		Obj_AI_Minion shadow;
		bool created;
			
		public ZedRData() {
			timeCasted = 0;
			shadow = null;
			created = false;
		}
		
		public float getTimeCasted() {
			return timeCasted;
		}
		
		public void setTimeCasted(float timeCasted) {
			this.timeCasted = timeCasted;
		}
		
		public Obj_AI_Minion getShadow() {
			return shadow;
		}
		
		public void setShadow(Obj_AI_Minion shadow) {
			this.shadow = shadow;
		}
		
		public bool isCreated() {
			return created;
		}
		
		public void setCreated(bool created) {
			this.created = created;
		}
	}
}
