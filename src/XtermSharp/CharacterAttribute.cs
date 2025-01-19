using System;

namespace XtermSharp {
	// TODO: rename to CharacterAttributes or similar
	[Flags]
	public enum FLAGS {
		BOLD = 1,
		UNDERLINE = 2,
		BLINK = 4,
		INVERSE = 8,
		INVISIBLE = 16,
		DIM = 32,
		ITALIC = 64,
		CrossedOut = 128
	}

	public static class CharacterAttribute {
		// Temporary, longer term in Attribute we will add a proper encoding
		public static string ToSGR (int attribute)
		{
			var result = "0";

			var ca = (FLAGS)(attribute >> 18);
			if (ca.HasFlag (FLAGS.BOLD)) {
				result += ";1";
			}
			if (ca.HasFlag (FLAGS.UNDERLINE)) {
				result += ";4";
			}
			if (ca.HasFlag (FLAGS.BLINK)) {
				result += ";5";
			}
			if (ca.HasFlag (FLAGS.INVERSE)) {
				result += ";7";
			}
			if (ca.HasFlag (FLAGS.INVISIBLE)) {
				result += ";8";
			}

			int fg = (attribute >> 9) & 0x1ff;

			if (fg != Renderer.DefaultColor) {
				if (fg > 16) {
					result += $";38;5;{fg}";
				} else {
					if (fg >= 8) {
						result += $";{9}{fg - 8};";
					} else {
						result += $";{3}{fg};";
					}
				}
			}

			int bg = attribute & 0x1ff;
			if (bg != Renderer.DefaultColor) {
				if (bg > 16) {
					result += $";48;5;{bg}";
				} else {
					if (bg >= 8) {
						result += $";{10}{bg - 8};";
					} else {
						result += $";{4}{bg};";
					}
				}
			}

			result += "m";
			return result;
		}

	}
}
