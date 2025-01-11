using System;
namespace XtermSharp {
	public static class EscapeSequences {
		public static byte[] CmdNewline = { 10 };
		public static byte[] CmdRet = { 13 };
		public static byte[] CmdEsc = { 0x1b };
		public static byte[] CmdDel = { 0x7f };
		public static byte[] CmdDelKey = { 0x1b, (byte) '[', (byte) '3', (byte) '~' };
		public static byte[] MoveUpApp = { 0x1b, (byte)'O', (byte)'A' };
		public static byte[] MoveUpNormal = { 0x1b, (byte)'[', (byte)'A' };
		public static byte[] MoveDownApp = { 0x1b, (byte)'O', (byte)'B' };
		public static byte[] MoveDownNormal = { 0x1b, (byte)'[', (byte)'B' };
		public static byte[] MoveLeftApp = { 0x1b, (byte)'O', (byte)'D' };
		public static byte[] MoveLeftNormal = { 0x1b, (byte)'[', (byte)'D' };
		public static byte[] MoveRightApp = { 0x1b, (byte)'O', (byte)'C' };
		public static byte[] MoveRightNormal = { 0x1b, (byte)'[', (byte)'C' };
		public static byte[] MoveHomeApp = { 0x1b, (byte)'O', (byte)'H' };
		public static byte[] MoveHomeNormal = { 0x1b, (byte)'[', (byte)'H' };
		public static byte[] MoveEndApp = { 0x1b, (byte)'O', (byte)'F' };
		public static byte[] MoveEndNormal = { 0x1b, (byte)'[', (byte)'F' };
		public static byte[] CmdTab = { 9 };
		public static byte[] CmdBackTab = { 0x1b, (byte)'[', (byte)'Z' };
		public static byte[] CmdPageUp = { 0x1b, (byte)'[', (byte)'5', (byte)'~' };
		public static byte[] CmdPageDown = { 0x1b, (byte)'[', (byte)'6', (byte)'~' };

		public static byte[][] CmdF = {
			new byte [] { 0x1b, (byte) 'O', (byte) 'P' }, /* F1 */
			new byte [] { 0x1b, (byte) 'O', (byte) 'Q' }, /* F2 */
			new byte [] { 0x1b, (byte) 'O', (byte) 'R' }, /* F3 */
			new byte [] { 0x1b, (byte) 'O', (byte) 'S' }, /* F4 */
			new byte [] { 0x1b, (byte) '[', (byte) '1', (byte) '5', (byte) '~' }, /* F5 */
			new byte [] { 0x1b, (byte) '[', (byte) '1', (byte) '7', (byte) '~' }, /* F6 */
			new byte [] { 0x1b, (byte) '[', (byte) '1', (byte) '8', (byte) '~' }, /* F7 */
			new byte [] { 0x1b, (byte) '[', (byte) '1', (byte) '9', (byte) '~' }, /* F8 */
			new byte [] { 0x1b, (byte) '[', (byte) '2', (byte) '0', (byte) '~' }, /* F9 */
			new byte [] { 0x1b, (byte) '[', (byte) '2', (byte) '1', (byte) '~' }, /* F10 */
			new byte [] { 0x1b, (byte) '[', (byte) '2', (byte) '3', (byte) '~' }, /* F11 */
			new byte [] { 0x1b, (byte) '[', (byte) '2', (byte) '4', (byte) '~' }, /* F12 */
		};
    }
}
