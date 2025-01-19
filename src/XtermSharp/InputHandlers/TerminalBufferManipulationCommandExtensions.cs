using System;
using System.Collections.Generic;

namespace XtermSharp.CommandExtensions {
	/// <summary>
	/// Commands that query the terminal for information about the current buffer
	/// </summary>
	internal static class TerminalBufferManipulationCommandExtensions {

		/// <summary>
		/// DECERA - Erase Rectangular Area
		/// CSI Pt ; Pl ; Pb ; Pr ; $ z
		/// </summary>
		public static void csiDECERA (this Terminal terminal, int [] pars)
		{
			var buffer = terminal.Buffer;
			var rect = GetRectangleFromRequest (buffer, terminal.OriginMode, 0, pars);

			if (rect.valid) {
				for (int row = rect.top; row <= rect.bottom; row++) {
					var line = buffer.Lines [row + buffer.YBase];
					for (int col = rect.left; col <= rect.right; col++) {
						line [col] = new CharData (terminal.CurAttr, ' ', 1, 32);
					}
				}
			}
		}

		/// <summary>
		/// DECSERA - Selective Erase Rectangular Area
		/// CSI Pt ; Pl ; Pb ; Pr ; $ {
		/// </summary>
		public static void csiDECSERA (this Terminal terminal, params int [] pars)
		{
			var buffer = terminal.Buffer;
			var rect = GetRectangleFromRequest (buffer, terminal.OriginMode, 0, pars);

			if (rect.valid) {
				for (int row = rect.top; row <= rect.bottom; row++) {
					var line = buffer.Lines [row + buffer.YBase];
					for (int col = rect.left; col <= rect.right; col++) {
						line [col] = new CharData (terminal.CurAttr, ' ', 1, 32);
					}
				}
			}
		}

		/// <summary>
		/// CSI Pc ; Pt ; Pl ; Pb ; Pr $ x Fill Rectangular Area (DECFRA), VT420 and up.
		/// </summary>
		public static void csiDECFRA (this Terminal terminal, params int [] pars)
		{
			var buffer = terminal.Buffer;
			var rect = GetRectangleFromRequest (buffer, terminal.OriginMode, 1, pars);

			if (rect.valid) {
				char fillChar = ' ';
				if (pars.Length > 0) {
					fillChar = (char)pars [0];
				}

				for (int row = rect.top; row <= rect.bottom; row++) {
					var line = buffer.Lines [row + buffer.YBase];
					for (int col = rect.left; col <= rect.right; col++) {
						line [col] = new CharData (terminal.CurAttr, fillChar, 1, (int)fillChar);
					}
				}
			}
		}

		/// <summary>
		/// Copy Rectangular Area (DECCRA), VT400 and up.
		/// CSI Pts ; Pls ; Pbs ; Prs ; Pps ; Ptd ; Pld ; Ppd $ v
		///  Pts ; Pls ; Pbs ; Prs denotes the source rectangle.
		///  Pps denotes the source page.
		///  Ptd ; Pld denotes the target location.
		///  Ppd denotes the target page.
		/// </summary>
		public static void csiDECCRA (this Terminal terminal, int [] pars, string collect)
		{
			var buffer = terminal.Buffer;
			if (collect == "$") {
				var parArray = new int [8];
				parArray [0] = (pars.Length > 1 && pars [0] != 0 ? pars [0] : 1); // Pts default 1
				parArray [1] = (pars.Length > 2 && pars [1] != 0 ? pars [1] : 1); // Pls default 1
				parArray [2] = (pars.Length > 3 && pars [2] != 0 ? pars [2] : buffer.Rows - 1); // Pbs default to last line of page
				parArray [3] = (pars.Length > 4 && pars [3] != 0 ? pars [3] : buffer.Cols - 1); // Prs defaults to last column
				parArray [4] = (pars.Length > 5 && pars [4] != 0 ? pars [4] : 1); // Pps page source = 1
				parArray [5] = (pars.Length > 6 && pars [5] != 0 ? pars [5] : 1); // Ptd default is 1
				parArray [6] = (pars.Length > 7 && pars [6] != 0 ? pars [6] : 1); // Pld default is 1
				parArray [7] = (pars.Length > 8 && pars [7] != 0 ? pars [7] : 1); // Ppd default is 1

				// We only support copying on the same page, and the page being 1
				if (parArray [4] == parArray [7] && parArray [4] == 1) {
					var rect = GetRectangleFromRequest (buffer, terminal.OriginMode, 0, parArray);
					if (rect.valid) {
						var rowTarget = parArray [5] - 1;
						var colTarget = parArray [6] - 1;

						// Block size
						var columns = rect.right - rect.left + 1;

						var cright = Math.Min (buffer.Cols - 1, rect.left + Math.Min (columns, buffer.Cols - colTarget));

						var lines = new List<List<CharData>> ();
						for (int row = rect.top; row <= rect.bottom; row++) {
							var line = buffer.Lines [row + buffer.YBase];
							var lineCopy = new List<CharData> ();
							for (int col = rect.left; col <= cright; col++) {
								lineCopy.Add (line [col]);
							}
							lines.Add (lineCopy);
						}

						for (int row = 0; row <= rect.bottom - rect.top; row++) {
							if (row + rowTarget >= buffer.Rows) {
								break;
							}

							var line = buffer.Lines [row + rowTarget + buffer.YBase];
							var lr = lines [row];
							for (int col = 0; col <= cright - rect.left; col++) {
								if (col >= buffer.Cols) {
									break;
								}

								line [colTarget + col] = lr [col];
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Required by the test suite
		/// CSI Pi ; Pg ; Pt ; Pl ; Pb ; Pr * y
		/// Request Checksum of Rectangular Area (DECRQCRA), VT420 and up.
		/// Response is
		/// DCS Pi ! ~ x x x x ST
		///   Pi is the request id.
		///   Pg is the page number.
		///   Pt ; Pl ; Pb ; Pr denotes the rectangle.
		///   The x's are hexadecimal digits 0-9 and A-F.
		/// </summary>
		public static void csiDECRQCRA (this Terminal terminal, params int [] pars)
		{
			var buffer = terminal.Buffer;

			int checksum = 0;
			var rid = pars.Length > 0 ? pars [0] : 1;
			var _ = pars.Length > 1 ? pars [1] : 0;
			var result = "0000";

			// Still need to imeplemnt the checksum here
			// Which is just the sum of the rune values
			if (terminal.Delegate.IsProcessTrusted ()) {
				var rect = GetRectangleFromRequest (buffer, terminal.OriginMode, 2, pars);

				var top = rect.top;
				var left = rect.left;
				var bottom = rect.bottom;
				var right = rect.right;

				for (int row = top; row <= bottom; row++) {
					var line = buffer.Lines [row + buffer.YBase];
					for (int col = left; col <= right; col++) {
						var cd = line [col];

						//var ch = cd.getCharacter ();
						//for (scalar in ch.unicodeScalars) {
						//	checksum += scalar.value;
						//}
						checksum += cd.IsNullChar() ? 32 : cd.Code;
					}
				}

				result = String.Format ("{0,4:X}", checksum);
			}

			terminal.SendResponse ($"{terminal.ControlCodes.DCS}{rid}!~{result}{terminal.ControlCodes.ST}");
		}

		/// <summary>
		/// Validates optional arguments for top, left, bottom, right sent by various
		/// escape sequences and returns validated top, left, bottom, right in our 0-based
		/// internal coordinates
		/// </summary>
		static (bool valid, int top, int left, int bottom, int right) GetRectangleFromRequest (Buffer buffer, bool originMode, int start, int [] pars)
		{
			var top = Math.Max (1, pars.Length > start ? pars [start] : 1);
			var left = Math.Max (pars.Length > start + 1 ? pars [start + 1] : 1, 1);
			var bottom = pars.Length > start + 2 ? pars [start + 2] : -1;
			var right = pars.Length > start + 3 ? pars [start + 3] : -1;

			var rect = GetRectangleFromRequest (buffer, originMode, top, left, bottom, right);
			return rect;
		}

		/// <summary>
		/// Validates optional arguments for top, left, bottom, right sent by various
		/// escape sequences and returns validated top, left, bottom, right in our 0-based
		/// internal coordinates
		/// </summary>
		static (bool valid, int top, int left, int bottom, int right) GetRectangleFromRequest (Buffer buffer, bool originMode, int top, int left, int bottom, int right)
		{
			if (bottom < 0) {
				bottom = buffer.Rows;
			}
			if (right < 0) {
				right = buffer.Cols;
			}
			if (right > buffer.Cols) {
				right = buffer.Cols;
			}
			if (bottom > buffer.Rows) {
				bottom = buffer.Rows;
			}
			if (originMode) {
				top += buffer.ScrollTop;
				bottom += buffer.ScrollTop;
				left += buffer.MarginLeft;
				right += buffer.MarginLeft;
			}

			if (top > bottom || left > right) {
				return (false, 0, 0, 0, 0);
			}

			return (true, top - 1, left - 1, bottom - 1, right - 1);
		}
	}
}