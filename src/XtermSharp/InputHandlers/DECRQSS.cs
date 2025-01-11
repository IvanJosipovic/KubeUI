using System;
using System.Collections.Generic;

// 
// Not implemented (either in xterm.js):
// DECUDK (https://vt100.net/docs/vt510-rm/DECUDK.html)
// DCS + q Pt ST (xterm) * Request Terminfo String
//  DCS + p Pt ST (xterm) * set terminfo data

namespace XtermSharp {
	/// <summary>
	/// DCS Subparser implementations
	/// 
	/// DCS $ q Pt ST
	/// DECRQSS (https://vt100.net/docs/vt510-rm/DECRQSS.html)
	/// Request Status String (DECRQSS), VT420 and up.
	/// Response: DECRPSS (https://vt100.net/docs/vt510-rm/DECRPSS.html)
	/// </summary>
	class DECRQSS : IDcsHandler {
		readonly Terminal terminal;
		List<byte> data;

		public DECRQSS (Terminal terminal)
		{
			this.terminal = terminal;
		}

		public void Hook (string collect, int [] parameters, int flag)
		{
			data = new List<byte> ();
		}

		unsafe public void Put (byte* data, int start, int end)
		{
			for (int i = start; i < end; i++)
				this.data.Add (data [i]);
		}

		public void Unhook ()
		{
			var newData = System.Text.Encoding.Default.GetString (data.ToArray ());
			int ok = 1; // 0 means the request is valid according to docs, but tests expect 0?
			string result = null;

			switch (newData) {
			case "\"q": // DECCSA - Set Character Attribute
				result = "\"q";
				return;
			case "\"p": // DECSCL - conformance level
				result = "65;1\"p";
				break;
			case "r": // DECSTBM - the top and bottom margins
				result = $"{terminal.Buffer.ScrollTop + 1};{terminal.Buffer.ScrollBottom + 1}r";
				break;
			case "m": // SGR- the set graphic rendition
				  // TODO: report real settings instead of 0m
				result = CharacterAttribute.ToSGR (terminal.CurAttr);
				break;
			case "s": // DECSLRM - the current left and right margins
				result = $"{terminal.Buffer.MarginLeft + 1};{terminal.Buffer.MarginRight + 1}s";
				break;
			case " q": // DECSCUSR - the set cursor style
				// TODO this should send a number for the current cursor style 2 for block, 4 for underline and 6 for bar
				var style = "2"; // block
				result = $"{style} q";
				break;
			default:
				ok = 0; // this means the request is not valid, report that to the host.
				result = string.Empty;
				// invalid: DCS 0 $ r Pt ST (xterm)
				terminal.Error ($"Unknown DCS + {newData}");
				break;
			}

			terminal.SendResponse ($"{terminal.ControlCodes.DCS}{ok}$r{result}{terminal.ControlCodes.ST}");
		}
	}
}
