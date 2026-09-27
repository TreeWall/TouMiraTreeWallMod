using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TreeWallMod
{
	public static class Colors
	{
		// Crewmate Role Colors
		public static Color Runner => new Color32(255, 0, 255, 255);
		public static Color Psychic => new Color32(130, 24, 186, 255);
		public static Color Syringe => new Color32(148, 232, 86, 255);

		// Neutral Role Colors
		public static Color Marksman => new Color32(157, 37, 37, 255);

		// Modifier Colors
		public static Color HeadlessModifier => new Color32(255, 0, 0, 255);
		public static Color WEEEModifer => new Color32(250, 94, 24, 255);
	}
}
