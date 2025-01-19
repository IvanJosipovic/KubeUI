using System;
using XtermSharp.CommandExtensions;

namespace XtermSharp.CsiCommandExtensions {
	/// <summary>
	/// Simple extensions to map CSI commands to terminal commands. Useful in porting esc tests
	/// </summary>
	internal static class CsiCommands {
		public static void csiDECSET (this Terminal terminal, int mode)
		{
			terminal.csiDECSET (mode, "?");
		}

		public static void csiCUP (this Terminal terminal, (int col, int row) point)
		{
			// switch the params around
			terminal.csiCUP (point.row, point.col);
		}

		public static void csiDECRESET (this Terminal terminal, int mode)
		{
			terminal.csiDECRESET (mode, "?");
		}
	}
}