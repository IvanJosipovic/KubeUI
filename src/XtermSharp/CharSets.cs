using System;
using System.Collections.Generic;

namespace XtermSharp {
	public class CharSets {
		public static Dictionary<byte, Dictionary<byte, string>> All;

		// This is the "B" charset, null	
		public static Dictionary<byte, string> Default = null;

		static CharSets ()
		{
			All = new Dictionary<byte, Dictionary<byte, string>> ();
			// 
			// DEC Special Character and Line Drawing Set.
			// Reference: http://vt100.net/docs/vt102-ug/table5-13.html
			// A lot of curses apps use this if they see TERM=xterm.
			// testing: echo -e '\e(0a\e(B'
			// The xterm output sometimes seems to conflict with the
			// reference above. xterm seems in line with the reference
			// when running vttest however.
			// The table below now uses xterm's output from vttest.
			//
			All [(byte)'0'] = new Dictionary<byte, string> ()  {
				{ (byte) '`', "\u25c6"}, // '◆'
				{ (byte) 'a', "\u2592"}, // '▒'
				{ (byte) 'b', "\u2409"}, // [ht]
				{ (byte) 'c', "\u240c"}, // [ff]
				{ (byte) 'd', "\u240d"}, // [cr]
				{ (byte) 'e', "\u240a"}, // [lf]
				{ (byte) 'f', "\u00b0"}, // '°'
				{ (byte) 'g', "\u00b1"}, // '±'
				{ (byte) 'h', "\u2424"}, // [nl]
				{ (byte) 'i', "\u240b"}, // [vt]
				{ (byte) 'j', "\u2518"}, // '┘'
				{ (byte) 'k', "\u2510"}, // '┐'
				{ (byte) 'l', "\u250c"}, // '┌'
				{ (byte) 'm', "\u2514"}, // '└'
				{ (byte) 'n', "\u253c"}, // '┼'
				{ (byte) 'o', "\u23ba"}, // '⎺'
				{ (byte) 'p', "\u23bb"}, // '⎻'
				{ (byte) 'q', "\u2500"}, // '─'
				{ (byte) 'r', "\u23bc"}, // '⎼'
				{ (byte) 's', "\u23bd"}, // '⎽'
				{ (byte) 't', "\u251c"}, // '├'
				{ (byte) 'u', "\u2524"}, // '┤'
				{ (byte) 'v', "\u2534"}, // '┴'
				{ (byte) 'w', "\u252c"}, // '┬'
				{ (byte) 'x', "\u2502"}, // '│'
				{ (byte) 'y', "\u2264"}, // '≤'
				{ (byte) 'z', "\u2265"}, // '≥'
				{ (byte) '{', "\u03c0"}, // 'π'
				{ (byte) '|', "\u2260"}, // '≠'
				{ (byte) '}', "\u00a3"}, // '£'
				{ (byte) '~', "\u00b7"}  // '·'
			};

			// (DEC Alternate character ROM special graphics)
			All [(byte)'2'] = All [(byte)'0'];

			/**
			 * British character set
			 * ESC (A
			 * Reference: http://vt100.net/docs/vt220-rm/table2-5.html
			 */
			All [(byte)'A'] = new Dictionary<byte, string> {
				{(byte) '#', "£"}
			};

			/**
			 * United States character set
			 * ESC (B
			 */
			All [(byte)'B'] = null;

			/**
			 * Dutch character set
			 * ESC (4
			 * Reference: http://vt100.net/docs/vt220-rm/table2-6.html
			 */
			All [(byte)'4'] = new Dictionary<byte, string> {
				{ (byte) '#', "£"},
				{ (byte) '@', "¾"},
				{ (byte) '[', "ij"},
				{ (byte) '\\', "½"},
				{ (byte) ']', "|"},
				{ (byte) '{', "¨"},
				{ (byte) '|', "f"},
				{ (byte) '}', "¼"},
				{ (byte) '~', "´"}
			};

			/**
			 * Finnish character set
			 * ESC (C or ESC (5
			 * Reference: http://vt100.net/docs/vt220-rm/table2-7.html
			 */
			All [(byte)'C'] =
			All [(byte)'5'] = new Dictionary<byte, string> {
				{ (byte) '[', "Ä"},
				{ (byte) '\\', "Ö"},
				{ (byte) ']', "Å"},
				{ (byte) '^', "Ü"},
				{ (byte) '`', "é"},
				{ (byte) '{', "ä"},
				{ (byte) '|', "ö"},
				{ (byte) '}', "å"},
				{ (byte) '~', "ü"}
			};

			/**
			 * French character set
			 * ESC (R
			 * Reference: http://vt100.net/docs/vt220-rm/table2-8.html
			 */
			All [(byte)'R'] = new Dictionary<byte, string> {
				{ (byte) '#', "£"},
				{ (byte) '@', "à"},
				{ (byte) '[', "°"},
				{ (byte) '\\', "ç"},
				{ (byte) ']', "§"},
				{ (byte) '{', "é"},
				{ (byte) '|', "ù"},
				{ (byte) '}', "è"},
				{ (byte) '~', "¨"}
			};

			/**
			 * French Canadian character set
			 * ESC (Q
			 * Reference: http://vt100.net/docs/vt220-rm/table2-9.html
			 */
			All [(byte)'Q'] = new Dictionary<byte, string> {
				{ (byte) '@', "à"},
				{ (byte) '[', "â"},
				{ (byte) '\\', "ç"},
				{ (byte) ']', "ê"},
				{ (byte) '^', "î"},
				{ (byte) '`', "ô"},
				{ (byte) '{', "é"},
				{ (byte) '|', "ù"},
				{ (byte) '}', "è"},
				{ (byte) '~', "û"}
			};

			/**
			 * German character set
			 * ESC (K
			 * Reference: http://vt100.net/docs/vt220-rm/table2-10.html
			 */
			All [(byte)'K'] = new Dictionary<byte, string> {
				{ (byte) '@', "§"},
				{ (byte) '[', "Ä"},
				{ (byte) '\\', "Ö"},
				{ (byte) ']', "Ü"},
				{ (byte) '{', "ä"},
				{ (byte) '|', "ö"},
				{ (byte) '}', "ü"},
				{ (byte) '~', "ß"}
			};

			/**
			 * Italian character set
			 * ESC (Y
			 * Reference: http://vt100.net/docs/vt220-rm/table2-11.html
			 */
			All [(byte)'Y'] = new Dictionary<byte, string> {
				{ (byte) '#', "£"},
				{ (byte) '@', "§"},
				{ (byte) '[', "°"},
				{ (byte) '\\', "ç"},
				{ (byte) ']', "é"},
				{ (byte) '`', "ù"},
				{ (byte) '{', "à"},
				{ (byte) '|', "ò"},
				{ (byte) '}', "è"},
				{ (byte) '~', "ì"}
			};

			/**
			 * Norwegian/Danish character set
			 * ESC (E or ESC (6
			 * Reference: http://vt100.net/docs/vt220-rm/table2-12.html
			 */
			All [(byte)'E'] =
			All [(byte)'6'] = new Dictionary<byte, string> {
				{ (byte) '@', "Ä"},
				{ (byte) '[', "Æ"},
				{ (byte) '\\', "Ø"},
				{ (byte) ']', "Å"},
				{ (byte) '^', "Ü"},
				{ (byte) '`', "ä"},
				{ (byte) '{', "æ"},
				{ (byte) '|', "ø"},
				{ (byte) '}', "å"},
				{ (byte) '~', "ü"}
			};

			/**
			 * Spanish character set
			 * ESC (Z
			 * Reference: http://vt100.net/docs/vt220-rm/table2-13.html
			 */

			All [(byte)'Z'] = new Dictionary<byte, string> {
				{ (byte) '#', "£"},
				{ (byte) '@', "§"},
				{ (byte) '[', "¡"},
				{ (byte) '\\', "Ñ"},
				{ (byte) ']', "¿"},
				{ (byte) '{', "°"},
				{ (byte) '|', "ñ"},
				{ (byte) '}', "ç"}
			};

			/**
			 * Swedish character set
			 * ESC (H or ESC (7
			 * Reference: http://vt100.net/docs/vt220-rm/table2-14.html
			 */
			All [(byte)'H'] =
			All [(byte)'7'] = new Dictionary<byte, string> {
				{ (byte) '@', "É"},
				{ (byte) '[', "Ä"},
				{ (byte) '\\', "Ö"},
				{ (byte) ']', "Å"},
				{ (byte) '^', "Ü"},
				{ (byte) '`', "é"},
				{ (byte) '{', "ä"},
				{ (byte) '|', "ö"},
				{ (byte) '}', "å"},
				{ (byte) '~', "ü"}
			};

			/**
			 * Swiss character set
			 * ESC (=
			 * Reference: http://vt100.net/docs/vt220-rm/table2-15.html
			 */
			All [(byte)'='] = new Dictionary<byte, string> {
				{ (byte) '#', "ù"},
				{ (byte) '@', "à"},
				{ (byte) '[', "é"},
				{ (byte) '\\', "ç"},
				{ (byte) ']', "ê"},
				{ (byte) '^', "î"},
				{ (byte) '_', "è"},
				{ (byte) '`', "ô"},
				{ (byte) '{', "ä"},
				{ (byte) '|', "ö"},
				{ (byte) '}', "ü"},
				{ (byte) '~', "û" }
			};
		}
	}

	public class CharSet {

	}
}
