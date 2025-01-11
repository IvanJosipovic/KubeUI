using System;
using System.Collections.Generic;

namespace XtermSharp {

	public class Color {
		public byte Red, Green, Blue;
		public static List<Color> DefaultAnsiColors;
		public static Color DefaultForeground = new Color (0xff, 0xff, 0xff);
		public static Color DefaultBackground = new Color (0, 0, 0);

		static Color ()
		{
			DefaultAnsiColors = new List<Color> () {
				// dark colors
				new Color (0x2e, 0x34, 0x36),
				new Color (0xcc, 0x00, 0x00),
				new Color (0x4e, 0x9a, 0x06),
				new Color (0xc4, 0xa0, 0x00),
				new Color (0x34, 0x65, 0xa4),
				new Color (0x75, 0x50, 0x7b),
				new Color (0x06, 0x98, 0x9a),
				new Color (0xd3, 0xd7, 0xcf),
				// bright colors
				new Color (0x55, 0x57, 0x53),
				new Color (0xef, 0x29, 0x29),
				new Color (0x8a, 0xe2, 0x34),
				new Color (0xfc, 0xe9, 0x4f),
				new Color (0x72, 0x9f, 0xcf),
				new Color (0xad, 0x7f, 0xa8),
				new Color (0x34, 0xe2, 0xe2),
				new Color (0xee, 0xee, 0xec),
			};

			// Fill in the remaining 240 ANSI colors.
			var v = new int [] { 0x00, 0x5f, 0x87, 0xaf, 0xd7, 0xff };

			// Generate colors (16-231)
			for (var i = 0; i < 216; i++) {
				var r = v [(i / 36) % 6];
				var g = v [(i / 6) % 6];
				var b = v [i % 6];

				DefaultAnsiColors.Add (new Color ((byte)r, (byte)g, (byte)b));
			}

			// Generate greys (232-255)
			for (int i = 0; i < 24; i++) {
				var c = (byte)(8 + i * 10);
				DefaultAnsiColors.Add (new Color (c, c, c));
			}
		}

		public Color (byte red, byte green, byte blue)
		{
			Red = red;
			Green = green;
			Blue = blue;
		}
	}
}
