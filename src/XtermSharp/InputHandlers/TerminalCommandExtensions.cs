using System;

namespace XtermSharp.CommandExtensions {
	/// <summary>
	/// Commands that operate on a terminal from CSI params
	/// </summary>
	internal static class TerminalCommandExtensions {
		/// <summary>
		// CSI Ps A
		// Cursor Up Ps Times (default = 1) (CUU).
		/// </summary>
		public static void csiCUU (this Terminal terminal, params int [] pars)
		{
			int param = Math.Max (pars.Length > 0 ? pars [0] : 1, 1);
			terminal.CursorUp (param);
		}

		/// <summary>
		// CSI Ps B
		// Cursor Down Ps Times (default = 1) (CUD).
		/// </summary>
		public static void csiCUD (this Terminal terminal, params int [] pars)
		{
			int param = Math.Max (pars.Length > 0 ? pars [0] : 1, 1);
			terminal.CursorDown (param);
		}

		/// <summary>
		// CSI Ps C
		// Cursor Forward Ps Times (default = 1) (CUF).
		/// </summary>
		public static void csiCUF (this Terminal terminal, params int [] pars)
		{
			int param = Math.Max (pars.Length > 0 ? pars [0] : 1, 1);
			terminal.CursorForward (param);
		}

		/// <summary>
		/// CSI Ps D
		/// Cursor Backward Ps Times (default = 1) (CUB).
		/// </summary>
		public static void csiCUB (this Terminal terminal, int [] pars)
		{
			int param = Math.Max (pars.Length > 0 ? pars [0] : 1, 1);
			terminal.CursorBackward (param);
		}

		/// <summary>
		/// CSI Ps G
		/// Cursor Character Absolute  [column] (default = [row,1]) (CHA).
		/// </summary>
		public static void csiCHA (this Terminal terminal, int [] pars)
		{
			int param = Math.Max (pars.Length > 0 ? pars [0] : 1, 1);
			terminal.CursorCharAbsolute (param);
		}

		/// <summary>
		/// Sets the cursor position from csi CUP
		/// CSI Ps ; Ps H
		/// Cursor Position [row;column] (default = [1,1]) (CUP).
		/// </summary>
		public static void csiCUP (this Terminal terminal, params int [] pars)
		{
			int col, row;
			switch (pars.Length) {
			case 1:
				row = pars [0] - 1;
				col = 0;
				break;
			case 2:
				row = pars [0] - 1;
				col = pars [1] - 1;
				break;
			default:
				col = 0;
				row = 0;
				break;
			}

			terminal.SetCursor (col, row);
		}

		/// <summary>
		/// Deletes lines
		/// </summary>
		/// <remarks>
		// CSI Ps M
		// Delete Ps Line(s) (default = 1) (DL).
		/// </remarks>
		public static void csiDL (this Terminal terminal, params int [] pars)
		{
			var p = Math.Max (pars.Length == 0 ? 1 : pars [0], 1);
			terminal.DeleteLines (p);
		}

		/// <summary>
		/// CSI Ps P
		/// Delete Ps Character(s) (default = 1) (DCH).
		/// </summary>
		public static void csiDCH (this Terminal terminal, params int [] pars)
		{
			var p = Math.Max (pars.Length == 0 ? 1 : pars [0], 1);
			terminal.DeleteChars (p);
		}

		/// <summary>
		/// CSI Ps Z  Cursor Backward Tabulation Ps tab stops (default = 1) (CBT).
		/// </summary>
		public static void csiCBT (this Terminal terminal, params int [] pars)
		{
			var p = Math.Max (pars.Length == 0 ? 1 : pars [0], 1);
			terminal.CursorBackwardTab (p);
		}

		/// <summary>
		/// Sets the margins from csi DECSLRM
		/// </summary>
		public static void csiDECSLRM (this Terminal terminal, params int [] pars)
		{
			var buffer = terminal.Buffer;
			var left = (pars.Length > 0 ? pars [0] : 1) - 1;
			var right = (pars.Length > 1 ? pars [1] : buffer.Cols) - 1;

			buffer.SetMargins (left, right);
		}

		/// <summary>
		/// CSI Ps ; Ps r
		///   Set Scrolling Region [top;bottom] (default = full size of win-
		///   dow) (DECSTBM).
		// CSI ? Pm r
		/// </summary>
		public static void csiDECSTBM (this Terminal terminal, params int [] pars)
		{
			var top = pars.Length > 0 ? Math.Max (pars [0] - 1, 0) : 0;
			var bottom = pars.Length > 1 ? pars [1] : 0;

			terminal.SetScrollRegion (top, bottom);
		}

		/// <summary>
		/// CSI # }   Pop video attributes from stack (XTPOPSGR), xterm.  Popping
		///           restores the video-attributes which were saved using XTPUSHSGR
		///           to their previous state.
		///
		/// CSI Pm ' }
		///           Insert Ps Column(s) (default = 1) (DECIC), VT420 and up.
		/// </summary>
		public static void csiDECIC (this Terminal terminal, int[] pars)
		{
			var n = pars.Length > 0 ? Math.Max (pars [0], 1) : 1;
			terminal.InsertColumn (n);
		}

		/// <summary>
		/// CSI Ps ' ~
		/// Delete Ps Column(s) (default = 1) (DECDC), VT420 and up.
		///
		/// @vt: #Y CSI DECDC "Delete Columns"  "CSI Ps ' ~"  "Delete `Ps` columns at cursor position."
		/// DECDC deletes `Ps` times columns at the cursor position for all lines with the scroll margins,
		/// moving content to the left. Blank columns are added at the right margin.
		/// DECDC has no effect outside the scrolling margins.
		/// </summary>
		public static void csiDECDC (this Terminal terminal, params int [] pars)
		{
			var n = pars.Length > 0 ? Math.Max (pars [0], 1) : 1;
			terminal.DeleteColumn (n);
		}

		/// <summary>
		/// CSI Ps ; Ps ; Ps t - Various window manipulations and reports (xterm)
		/// See https://invisible-island.net/xterm/ctlseqs/ctlseqs.html for a full
		/// list of commans for this escape sequence
		/// </summary>
		public static void csiDISPATCH (this Terminal terminal, int [] pars)
		{
			if (pars == null || pars.Length == 0)
				return;

			if (pars.Length == 3 && pars [0] == 3) {
				terminal.Delegate.WindowCommand (terminal, WindowManipulationCommand.MoveWindowTo, pars [1], pars [2]);
				return;
			}
			if (pars.Length == 3 && pars [0] == 4) {
				terminal.Delegate.WindowCommand (terminal, WindowManipulationCommand.MoveWindowTo, pars [1], pars [2]);
				return;
			}

			if (pars.Length == 3 && pars [0] == 8) {
				terminal.Delegate.WindowCommand (terminal, WindowManipulationCommand.ResizeTo, pars [1], pars [2]);
				return;
			}

			if (pars.Length == 2 && pars [0] == 9) {
				switch (pars [1]) {
				case 0:
					terminal.Delegate.WindowCommand (terminal, WindowManipulationCommand.RestoreMaximizedWindow);
					return;
				case 1:
					terminal.Delegate.WindowCommand (terminal, WindowManipulationCommand.MaximizeWindow);
					return;
				case 2:
					terminal.Delegate.WindowCommand (terminal, WindowManipulationCommand.MaximizeWindowVertically);
					return;
				case 3:
					terminal.Delegate.WindowCommand (terminal, WindowManipulationCommand.MaximizeWindowHorizontally);
					return;
				default:
					return;
				}
			}

			if (pars.Length == 2 && pars [0] == 10) {
				switch (pars [1]) {
				case 0:
					terminal.Delegate.WindowCommand (terminal, WindowManipulationCommand.UndoFullScreen);
					return;
				case 1:
					terminal.Delegate.WindowCommand (terminal, WindowManipulationCommand.SwitchToFullScreen);
					return;
				case 2:
					terminal.Delegate.WindowCommand (terminal, WindowManipulationCommand.ToggleFullScreen);
					return;
				default:
					return;
				}
			}

			if (pars.Length == 2 && pars [0] == 22) {
				switch (pars [1]) {
				case 0:
					terminal.PushTitle ();
					terminal.PushIconTitle ();
					return;
				case 1:
					terminal.PushIconTitle ();
					return;
				case 2:
					terminal.PushTitle ();
					return;
				default:
					return;
				}
			}

			if (pars.Length == 2 && pars [0] == 23) {
				switch (pars [1]) {
				case 0:
					terminal.PopTitle ();
					terminal.PopIconTitle ();
					return;
				case 1:
					terminal.PopTitle ();
					return;
				case 2:
					terminal.PopIconTitle ();
					return;
				default:
					return;
				}
			}

			if (pars.Length == 1) {
				string response = null;
				switch (pars [0]) {
				case 0:
					terminal.Delegate.WindowCommand (terminal, WindowManipulationCommand.DeiconifyWindow);
					return;
				case 1:
					terminal.Delegate.WindowCommand (terminal, WindowManipulationCommand.IconifyWindow);
					return;
				case 2:
					return;
				case 3:
					return;
				case 4:
					return;
				case 5:
					terminal.Delegate.WindowCommand (terminal, WindowManipulationCommand.BringToFront);
					return;
				case 6:
					terminal.Delegate.WindowCommand (terminal, WindowManipulationCommand.SendToBack);
					return;
				case 7:
					terminal.Delegate.WindowCommand (terminal, WindowManipulationCommand.RefreshWindow);
					return;
				case 15:
					response = terminal.Delegate.WindowCommand (terminal, WindowManipulationCommand.ReportSizeOfScreenInPixels);
					if (response == null) {
						response = $"{terminal.ControlCodes.CSI}5;768;1024t";
					}

					terminal.SendResponse (response);
					return;
				case 16:
					response = terminal.Delegate.WindowCommand (terminal, WindowManipulationCommand.ReportCellSizeInPixels);
					if (response == null) {
						response = $"{terminal.ControlCodes.CSI}6;16;10t";
					}

					terminal.SendResponse (response);
					return;
				case 17:
					return;
				case 18:
					response = terminal.Delegate.WindowCommand (terminal, WindowManipulationCommand.ReportScreenSizeCharacters);
					if (response == null) {
						response = $"{terminal.ControlCodes.CSI}8;{terminal.Rows};{terminal.Cols}t";
					}

					terminal.SendResponse (response);
					return;
				case 19:
					response = terminal.Delegate.WindowCommand (terminal, WindowManipulationCommand.ReportScreenSizeCharacters);
					if (response == null) {
						response = $"{terminal.ControlCodes.CSI}9;{terminal.Rows};{terminal.Cols}t";
					}

					terminal.SendResponse (response);
					return;
				case 20:
					response = terminal.IconTitle.Replace ("\\", "");
					terminal.SendResponse ($"{terminal.ControlCodes.OSC}l{response}{terminal.ControlCodes.ST}");
					return;
				case 21:
					response = terminal.Title.Replace ("\\", "");
					terminal.SendResponse ($"{terminal.ControlCodes.OSC}l{response}{terminal.ControlCodes.ST}");
					return;
				default:
					return;
				}
			}
		}

	}
}