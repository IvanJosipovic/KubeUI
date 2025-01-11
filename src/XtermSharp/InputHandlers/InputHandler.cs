using System;
using System.Collections.Generic;
using System.Text;
using NStack;
using XtermSharp.CommandExtensions;

// 
// Not implemented (either in xterm.js):
// DECUDK (https://vt100.net/docs/vt510-rm/DECUDK.html)
// DCS + q Pt ST (xterm) * Request Terminfo String
//  DCS + p Pt ST (xterm) * set terminfo data

namespace XtermSharp {
	// 
	// The terminal's standard implementation of IInputHandler, this handles all
	// input from the Parser.
	// 
	// Refer to http://invisible-island.net/xterm/ctlseqs/ctlseqs.html to understand
	// each function's header comment.
	// 
	class InputHandler {
		readonly ReadingBuffer readingBuffer;
		readonly Terminal terminal;
		readonly EscapeSequenceParser parser;

		public InputHandler (Terminal terminal)
		{
			this.terminal = terminal;
			readingBuffer = new ReadingBuffer ();

			parser = new EscapeSequenceParser ();
			parser.SetCsiHandlerFallback ((string collect, int [] pars, int flag) => {
				terminal.Error ("Unknown CSI code", collect, pars, flag);
			});
			parser.SetEscHandlerFallback ((string collect, int flag) => {
				terminal.Error ("Unknown ESC code", collect, flag);
			});
			parser.SetExecuteHandlerFallback ((code) => {
				terminal.Error ("Unknown EXECUTE code", code);
			});
			parser.SetOscHandlerFallback ((int identifier, string data) => {
				terminal.Error ("Unknown OSC code", identifier, data);
			});

			// Print handler
			unsafe { parser.SetPrintHandler (Print); }
			parser.PrintStateReset = PrintStateReset;

			// CSI handler
			parser.SetCsiHandler ('@', (pars, collect) => InsertChars (pars));
			parser.SetCsiHandler ('A', (pars, collect) => terminal.csiCUU (pars));
			parser.SetCsiHandler ('B', (pars, collect) => terminal.csiCUD (pars));
			parser.SetCsiHandler ('C', (pars, collect) => terminal.csiCUF (pars));
			parser.SetCsiHandler ('D', (pars, collect) => terminal.csiCUB (pars));
			parser.SetCsiHandler ('E', (pars, collect) => CursorNextLine (pars));
			parser.SetCsiHandler ('F', (pars, collect) => CursorPrecedingLine (pars));
			parser.SetCsiHandler ('G', (pars, collect) => terminal.csiCHA (pars));
			parser.SetCsiHandler ('H', (pars, collect) => terminal.csiCUP (pars));
			parser.SetCsiHandler ('I', (pars, collect) => CursorForwardTab (pars));
			parser.SetCsiHandler ('J', (pars, collect) => EraseInDisplay (pars));
			parser.SetCsiHandler ('K', (pars, collect) => EraseInLine (pars));
			parser.SetCsiHandler ('L', (pars, collect) => InsertLines (pars));
			parser.SetCsiHandler ('M', (pars, collect) => terminal.csiDL (pars));
			parser.SetCsiHandler ('P', (pars, collect) => terminal.csiDCH (pars));
			parser.SetCsiHandler ('S', (pars, collect) => ScrollUp (pars));
			parser.SetCsiHandler ('T', (pars, collect) => ScrollDown (pars));
			parser.SetCsiHandler ('X', (pars, collect) => EraseChars (pars));
			parser.SetCsiHandler ('Z', (pars, collect) => terminal.csiCBT (pars));
			parser.SetCsiHandler ('`', (pars, collect) => CharPosAbsolute (pars));
			parser.SetCsiHandler ('a', (pars, collect) => HPositionRelative (pars));
			parser.SetCsiHandler ('b', (pars, collect) => RepeatPrecedingCharacter (pars));
			parser.SetCsiHandler ('c', (pars, collect) => terminal.csiDA1 (pars, collect));
			parser.SetCsiHandler ('d', (pars, collect) => LinePosAbsolute (pars));
			parser.SetCsiHandler ('e', (pars, collect) => VPositionRelative (pars));
			parser.SetCsiHandler ('f', (pars, collect) => HVPosition (pars));
			parser.SetCsiHandler ('g', (pars, collect) => TabClear (pars));
			parser.SetCsiHandler ('h', (pars, collect) => SetMode (pars, collect));
			parser.SetCsiHandler ('l', (pars, collect) => ResetMode (pars, collect));
			parser.SetCsiHandler ('m', (pars, collect) => CharAttributes (pars));
			parser.SetCsiHandler ('n', (pars, collect) => terminal.csiDSR (pars, collect));
			parser.SetCsiHandler ('p', (pars, collect) => {
				switch (collect) {
				case "!":
					terminal.SoftReset ();
					break;
				case "\"":
					//TODO: SetConformanceLevel (pars, collect);
					break;
				default:
					terminal.Error ("Unknown CSI code", collect, pars, "p");
					break;
				}
			});
			parser.SetCsiHandler ('q', (pars, collect) => SetCursorStyle (pars, collect));
			parser.SetCsiHandler ('r', (pars, collect) => {
				if (collect == "") {
					terminal.csiDECSTBM (pars);
				}
			});
			parser.SetCsiHandler ('s', (pars, collect) => {
				// "CSI s" is overloaded, can mean save cursor, but also set the margins with DECSLRM
				if (terminal.MarginMode) {
					this.terminal.csiDECSLRM (pars);
				} else {
					terminal.SaveCursor ();
				}
			});
			parser.SetCsiHandler ('t', (pars, collect) => terminal.csiDISPATCH (pars));
			parser.SetCsiHandler ('u', (pars, collect) => terminal.RestoreCursor ());
			parser.SetCsiHandler ('v', (pars, collect) => terminal.csiDECCRA (pars, collect));
			parser.SetCsiHandler ('y', (pars, collect) => terminal.csiDECRQCRA (pars));
			parser.SetCsiHandler ('x', (pars, collect) => {
				switch (collect) {
				case "$":
					terminal.csiDECFRA (pars);
					break;
				default:
					terminal.Error ("Unknown CSI code", collect, pars, "x");
					break;
				}
			});
			parser.SetCsiHandler ('z', (pars, collect) => {
				switch (collect) {
				case "$":
					terminal.csiDECERA (pars);
					break;
				case "'":
					// TODO: Enable Locator Reporting (DECELR)
					// Enable Locator Reporting (DECELR).
					// Valid values for the first parameter:
					//   Ps = 0  ⇒  Locator disabled (default).
					//   Ps = 1  ⇒  Locator enabled.
					//   Ps = 2  ⇒  Locator enabled for one report, then disabled.
					// The second parameter specifies the coordinate unit for locator
					// reports.
					// Valid values for the second parameter:
					//   Pu = 0  or omitted ⇒  default to character cells.
					//   Pu = 1  ⇐  device physical pixels.
					//   Pu = 2  ⇐  character cells.
					break;
				default:
					terminal.Error ("Unknown CSI code", collect, pars, "z");
					break;
				}
			});
			parser.SetCsiHandler ('{', (pars, collect) => {
				switch (collect) {
				case "$":
					terminal.csiDECSERA (pars);
					break;
				default:
					terminal.Error ("Unknown CSI code", collect, pars, "{");
					break;
				}
			});
			parser.SetCsiHandler ('}', (pars, collect) => {
				switch (collect) {
				case "'":
					terminal.csiDECIC (pars);
					break;
				default:
					terminal.Error ("Unknown CSI code", collect, pars, "}");
					break;
				}
			});
			parser.SetCsiHandler ('~', (pars, collect) => terminal.csiDECDC (pars));




			// Execute Handler
			parser.SetExecuteHandler (7, terminal.Bell);
			parser.SetExecuteHandler (10, terminal.LineFeed);
			parser.SetExecuteHandler (11, terminal.LineFeedBasic);   // VT Vertical Tab - ignores auto-new-line behavior in ConvertEOL
			parser.SetExecuteHandler (12, terminal.LineFeedBasic);
			parser.SetExecuteHandler (13, terminal.CarriageReturn);
			parser.SetExecuteHandler (8, terminal.Backspace);
			parser.SetExecuteHandler (9, Tab);
			parser.SetExecuteHandler (14, ShiftOut);
			parser.SetExecuteHandler (15, ShiftIn);
			// Comment in original FIXME:   What do to with missing? Old code just added those to print.

			// some C1 control codes - FIXME: should those be enabled by default?
			parser.SetExecuteHandler (0x84 /* Index */, () => terminal.Index ());
			parser.SetExecuteHandler (0x85 /* Next Line */, terminal.NextLine);
			parser.SetExecuteHandler (0x88 /* Horizontal Tabulation Set */, TabSet);

			// 
			// OSC handler
			// 
			//   0 - icon name + title
			parser.SetOscHandler (0, SetTitleAndIcon);
			//   1 - icon name
			parser.SetOscHandler (1, SetIconTitle);
			//   2 - title
			parser.SetOscHandler (2, SetTitle);
			//   3 - set property X in the form "prop=value"
			//   4 - Change Color Number()
			//   5 - Change Special Color Number
			//   6 - Enable/disable Special Color Number c
			//   7 - current directory? (not in xterm spec, see https://gitlab.com/gnachman/iterm2/issues/3939)
			//  10 - Change VT100 text foreground color to Pt.
			//  11 - Change VT100 text background color to Pt.
			//  12 - Change text cursor color to Pt.
			//  13 - Change mouse foreground color to Pt.
			//  14 - Change mouse background color to Pt.
			//  15 - Change Tektronix foreground color to Pt.
			//  16 - Change Tektronix background color to Pt.
			//  17 - Change highlight background color to Pt.
			//  18 - Change Tektronix cursor color to Pt.
			//  19 - Change highlight foreground color to Pt.
			//  46 - Change Log File to Pt.
			//  50 - Set Font to Pt.
			//  51 - reserved for Emacs shell.
			//  52 - Manipulate Selection Data.
			// 104 ; c - Reset Color Number c.
			// 105 ; c - Reset Special Color Number c.
			// 106 ; c; f - Enable/disable Special Color Number c.
			// 110 - Reset VT100 text foreground color.
			// 111 - Reset VT100 text background color.
			// 112 - Reset text cursor color.
			// 113 - Reset mouse foreground color.
			// 114 - Reset mouse background color.
			// 115 - Reset Tektronix foreground color.
			// 116 - Reset Tektronix background color.

			// 
			// ESC handlers
			// 
			parser.SetEscHandler ("7", (c, f) => terminal.SaveCursor());
			parser.SetEscHandler ("8", (c, f) => terminal.RestoreCursor());
			parser.SetEscHandler ("D", (c, f) => terminal.Index ());
			parser.SetEscHandler ("E", (c, b) => terminal.NextLine ());
			parser.SetEscHandler ("H", (c, f) => TabSet ());
			parser.SetEscHandler ("M", (c, f) => ReverseIndex ());
			parser.SetEscHandler ("=", (c, f) => KeypadApplicationMode ());
			parser.SetEscHandler (">", (c, f) => KeypadNumericMode ());
			parser.SetEscHandler ("c", (c, f) => Reset ());
			parser.SetEscHandler ("n", (c, f) => SetgLevel (2));
			parser.SetEscHandler ("o", (c, f) => SetgLevel (3));
			parser.SetEscHandler ("|", (c, f) => SetgLevel (3));
			parser.SetEscHandler ("}", (c, f) => SetgLevel (2));
			parser.SetEscHandler ("~", (c, f) => SetgLevel (1));
			parser.SetEscHandler ("%@", (c, f) => SelectDefaultCharset ());
			parser.SetEscHandler ("%G", (c, f) => SelectDefaultCharset ());
			parser.SetEscHandler ("#3", (c, f) => SetDoubleHeightTop ());		    // dhtop
			parser.SetEscHandler ("#4", (c, f) => SetDoubleHeightBottom ());            // dhbot
			parser.SetEscHandler ("#5", (c, f) => SingleWidthSingleHeight ());          // swsh
			parser.SetEscHandler ("#6", (c, f) => DoubleWidthSingleHeight ());          // dwsh
			foreach (var bflag in CharSets.All.Keys) {
				char flag = (char)bflag;
				parser.SetEscHandler ("(" + flag, (code, f) => SelectCharset ("(" + flag));
				parser.SetEscHandler (")" + flag, (code, f) => SelectCharset (")" + flag));
				parser.SetEscHandler ("*" + flag, (code, f) => SelectCharset ("*" + flag));
				parser.SetEscHandler ("+" + flag, (code, f) => SelectCharset ("+" + flag));
				parser.SetEscHandler ("-" + flag, (code, f) => SelectCharset ("-" + flag));
				parser.SetEscHandler ("." + flag, (code, f) => SelectCharset ("." + flag));
				parser.SetEscHandler ("/" + flag, (code, f) => SelectCharset ("/" + flag)); // TODO: supported?
			}

			// Error handler
			parser.SetErrorHandler ((state) => {
				terminal.Error ("Parsing error, state: ", state);
				return state;
			});

			// DCS Handler
			parser.SetDcsHandler ("$q", new DECRQSS (terminal));
		}

		public void Parse (byte [] data, int length = -1)
		{
			if (length == -1)
				length = data.Length;

			var buffer = terminal.Buffer;
			var cursorStartX = buffer.X;
			var cursorStartY = buffer.Y;

			unsafe {
				fixed (byte* p = &data [0]) {
					parser.Parse (p, length);
				}
			}

			buffer = terminal.Buffer;
		}

		public void Parse (IntPtr data, int length)
		{
			var buffer = terminal.Buffer;
			var cursorStartX = buffer.X;
			var cursorStartY = buffer.Y;

			unsafe { parser.Parse ((byte*)data, length); }

			buffer = terminal.Buffer;
		}

		// 
		// CSI Ps L
		// Insert Ps Line(s) (default = 1) (IL).
		// 
		private void InsertLines (int [] pars)
		{
			var p = Math.Max (pars.Length == 0 ? 1 : pars [0], 1);
			var buffer = terminal.Buffer;
			var row = buffer.Y + buffer.YBase;

			var scrollBottomRowsOffset = terminal.Rows - 1 - buffer.ScrollBottom;
			var scrollBottomAbsolute = terminal.Rows - 1 + buffer.YBase - scrollBottomRowsOffset + 1;

			var eraseAttr = terminal.EraseAttr ();
			while (p-- != 0) {
				// test: echo -e '\e[44m\e[1L\e[0m'
				// blankLine(true) - xterm/linux behavior
				buffer.Lines.Splice (scrollBottomAbsolute - 1, 1);
				var newLine = buffer.GetBlankLine (eraseAttr);
				buffer.Lines.Splice (row, 0, newLine);
			}

			// this.maxRange();
			terminal.UpdateRange (buffer.Y);
			terminal.UpdateRange (buffer.ScrollBottom);
		}

		// 
		// ESC ( C
		//   Designate G0 Character Set, VT100, ISO 2022.
		// ESC ) C
		//   Designate G1 Character Set (ISO 2022, VT100).
		// ESC * C
		//   Designate G2 Character Set (ISO 2022, VT220).
		// ESC + C
		//   Designate G3 Character Set (ISO 2022, VT220).
		// ESC - C
		//   Designate G1 Character Set (VT300).
		// ESC . C
		//   Designate G2 Character Set (VT300).
		// ESC / C
		//   Designate G3 Character Set (VT300). C = A  -> ISO Latin-1 Supplemental. - Supported?
		// 
		void SelectCharset (string p)
		{
			if (p.Length != 2)
				SelectDefaultCharset ();
			byte ch;

			Dictionary<byte, string> charset;
			if (!CharSets.All.TryGetValue ((byte)p [1], out charset))
				charset = null;

			switch (p [0]) {
			case '(':
				ch = 0;
				break;
			case ')':
			case '-':
				ch = 1;
				break;
			case '*':
			case '.':
				ch = 2;
				break;
			case '+':
				ch = 3;
				break;
			default:
				// includes '/' -> unsupported? (MIGUEL TODO)
				return;
			}
			terminal.SetgCharset (ch, charset);
		}

		//
		// ESC # NUMBER
		//
		void DoubleWidthSingleHeight ()
		{
		}

		//
		// dhtop
		//
		void SetDoubleHeightTop () {}

		// dhbot
		void SetDoubleHeightBottom () { }          // dhbot

		//
		// swsh
		//
		void SingleWidthSingleHeight () { }

		// 
		// ESC % @
		// ESC % G
		//   Select default character set. UTF-8 is not supported (string are unicode anyways)
		//   therefore ESC % G does the same.
		// 
		void SelectDefaultCharset ()
		{
			terminal.SetgLevel (0);
			terminal.SetgCharset (0, CharSets.Default);
		}

		// 
		// ESC n
		// ESC o
		// ESC |
		// ESC }
		// ESC ~
		//   DEC mnemonic: LS (https://vt100.net/docs/vt510-rm/LS.html)
		//   When you use a locking shift, the character set remains in GL or GR until
		//   you use another locking shift. (partly supported)
		// 
		void SetgLevel (int n)
		{
			terminal.SetgLevel (n);
		}

		// 
		// ESC c
		//   DEC mnemonic: RIS (https://vt100.net/docs/vt510-rm/RIS.html)
		//   Reset to initial state.
		// 
		void Reset ()
		{
			parser.Reset ();
			terminal.Reset ();
		}

		// 
		// ESC >
		//   DEC mnemonic: DECKPNM (https://vt100.net/docs/vt510-rm/DECKPNM.html)
		//   Enables the keypad to send numeric characters to the host.
		// 
		void KeypadNumericMode ()
		{
			terminal.ApplicationKeypad = false;
			terminal.SyncScrollArea ();
		}

		// 
		// ESC =
		//   DEC mnemonic: DECKPAM (https://vt100.net/docs/vt510-rm/DECKPAM.html)
		//   Enables the numeric keypad to send application sequences to the host.
		// 
		void KeypadApplicationMode ()
		{
			terminal.ApplicationKeypad = true;
			terminal.SyncScrollArea ();
		}

		// 
		// ESC M
		// C1.RI
		//   DEC mnemonic: HTS
		//   Moves the cursor up one line in the same column. If the cursor is at the top margin,
		//   the page scrolls down.
		// 
		void ReverseIndex ()
		{
			terminal.ReverseIndex ();
		}

		/// <summary>
		/// OSC 0; <data> ST (set window and icon title)
		///   Proxy to set window title.
		/// </summary>
		/// <param name="data"></param>
		void SetTitleAndIcon (string data)
		{
			terminal.SetTitle (data ?? string.Empty);
			terminal.SetIconTitle (data ?? string.Empty);
		}

		/// <summary>
		/// OSC 2; <data> ST (set window title)
		///   Proxy to set window title.
		/// </summary>
		/// <param name="data"></param>
		void SetTitle (string data)
		{
			terminal.SetTitle (data ?? string.Empty);
		}

		/// <summary>
		/// OSC 1; <data> ST (set window title)
		///   Proxy to set icon title.
		/// </summary>
		void SetIconTitle (string data)
		{
			terminal.SetIconTitle (data ?? string.Empty);
		}

		// 
		// ESC H
		// C1.HTS
		//   DEC mnemonic: HTS (https://vt100.net/docs/vt510-rm/HTS.html)
		//   Sets a horizontal tab stop at the column position indicated by
		//   the value of the active column when the terminal receives an HTS.
		// 
		void TabSet ()
		{
			terminal.Buffer.TabSet (terminal.Buffer.X);
		}

		// SI
		// ShiftIn (Control-O) Switch to standard character set.  This invokes the G0 character set
		void ShiftIn ()
		{
			terminal.SetgLevel (0);
		}

		// SO
		// ShiftOut (Control-N) Switch to alternate character set.  This invokes the G1 character set
		void ShiftOut ()
		{
			terminal.SetgLevel (1);
		}

		//
		// Horizontal tab (Control-I)
		//
		void Tab ()
		{
			var originalX = terminal.Buffer.X;
			terminal.Buffer.X = terminal.Buffer.NextTabStop ();
			if (terminal.Options.ScreenReaderMode)
				terminal.EmitA11yTab (terminal.Buffer.X - originalX);
		}

		// 
		// Helper method to erase cells in a terminal row.
		// The cell gets replaced with the eraseChar of the terminal.
		// @param y row index
		// @param start first cell index to be erased
		// @param end   end - 1 is last erased cell
		// 
		void EraseInBufferLine (int y, int start, int end, bool clearWrap = false)
		{
			var line = terminal.Buffer.Lines [terminal.Buffer.YBase + y];
			var cd = new CharData (terminal.EraseAttr ());
			line.ReplaceCells (start, end, cd);
			if (clearWrap)
				line.IsWrapped = false;
		}

		// 
		// Helper method to reset cells in a terminal row.
		// The cell gets replaced with the eraseChar of the terminal and the isWrapped property is set to false.
		// @param y row index
		// 
		void ResetBufferLine (int y)
		{
			EraseInBufferLine (y, 0, terminal.Cols, true);
		}

		// 
		// CSI Ps SP q  Set cursor style (DECSCUSR, VT520).
		//   Ps = 0  -> blinking block.
		//   Ps = 1  -> blinking block (default).
		//   Ps = 2  -> steady block.
		//   Ps = 3  -> blinking underline.
		//   Ps = 4  -> steady underline.
		//   Ps = 5  -> blinking bar (xterm).
		//   Ps = 6  -> steady bar (xterm).
		// 
		void SetCursorStyle (int [] pars, string collect)
		{
			if (collect != " ")
				return;

			var p = Math.Max (pars.Length == 0 ? 1 : pars [0], 1);
			switch (p) {
			case 1:
				terminal.SetCursorStyle (CursorStyle.BlinkBlock);
				break;
			case 2:
				terminal.SetCursorStyle (CursorStyle.SteadyBlock);
				break;
			case 3:
				terminal.SetCursorStyle (CursorStyle.BlinkUnderline);
				break;
			case 4:
				terminal.SetCursorStyle (CursorStyle.SteadyUnderline);
				break;
			case 5:
				terminal.SetCursorStyle (CursorStyle.BlinkingBar);
				break;
			case 6:
				terminal.SetCursorStyle (CursorStyle.SteadyBar);
				break;
			}
		}


		// 
		// CSI Pm m  Character Attributes (SGR).
		//     Ps = 0  -> Normal (default).
		//     Ps = 1  -> Bold.
		//     Ps = 2  -> Faint, decreased intensity (ISO 6429).
		//     Ps = 4  -> Underlined.
		//     Ps = 5  -> Blink (appears as Bold).
		//     Ps = 7  -> Inverse.
		//     Ps = 8  -> Invisible, i.e., hidden (VT300).
		//     Ps = 2 2  -> Normal (neither bold nor faint).
		//     Ps = 2 4  -> Not underlined.
		//     Ps = 2 5  -> Steady (not blinking).
		//     Ps = 2 7  -> Positive (not inverse).
		//     Ps = 2 8  -> Visible, i.e., not hidden (VT300).
		//     Ps = 3 0  -> Set foreground color to Black.
		//     Ps = 3 1  -> Set foreground color to Red.
		//     Ps = 3 2  -> Set foreground color to Green.
		//     Ps = 3 3  -> Set foreground color to Yellow.
		//     Ps = 3 4  -> Set foreground color to Blue.
		//     Ps = 3 5  -> Set foreground color to Magenta.
		//     Ps = 3 6  -> Set foreground color to Cyan.
		//     Ps = 3 7  -> Set foreground color to White.
		//     Ps = 3 9  -> Set foreground color to default (original).
		//     Ps = 4 0  -> Set background color to Black.
		//     Ps = 4 1  -> Set background color to Red.
		//     Ps = 4 2  -> Set background color to Green.
		//     Ps = 4 3  -> Set background color to Yellow.
		//     Ps = 4 4  -> Set background color to Blue.
		//     Ps = 4 5  -> Set background color to Magenta.
		//     Ps = 4 6  -> Set background color to Cyan.
		//     Ps = 4 7  -> Set background color to White.
		//     Ps = 4 9  -> Set background color to default (original).
		// 
		//   If 16-color support is compiled, the following apply.  Assume
		//   that xterm's resources are set so that the ISO color codes are
		//   the first 8 of a set of 16.  Then the aixterm colors are the
		//   bright versions of the ISO colors:
		//     Ps = 9 0  -> Set foreground color to Black.
		//     Ps = 9 1  -> Set foreground color to Red.
		//     Ps = 9 2  -> Set foreground color to Green.
		//     Ps = 9 3  -> Set foreground color to Yellow.
		//     Ps = 9 4  -> Set foreground color to Blue.
		//     Ps = 9 5  -> Set foreground color to Magenta.
		//     Ps = 9 6  -> Set foreground color to Cyan.
		//     Ps = 9 7  -> Set foreground color to White.
		//     Ps = 1 0 0  -> Set background color to Black.
		//     Ps = 1 0 1  -> Set background color to Red.
		//     Ps = 1 0 2  -> Set background color to Green.
		//     Ps = 1 0 3  -> Set background color to Yellow.
		//     Ps = 1 0 4  -> Set background color to Blue.
		//     Ps = 1 0 5  -> Set background color to Magenta.
		//     Ps = 1 0 6  -> Set background color to Cyan.
		//     Ps = 1 0 7  -> Set background color to White.
		// 
		//   If xterm is compiled with the 16-color support disabled, it
		//   supports the following, from rxvt:
		//     Ps = 1 0 0  -> Set foreground and background color to
		//     default.
		// 
		//   If 88- or 256-color support is compiled, the following apply.
		//     Ps = 3 8  ; 5  ; Ps -> Set foreground color to the second
		//     Ps.
		//     Ps = 4 8  ; 5  ; Ps -> Set background color to the second
		//     Ps.
		// 
		void CharAttributes (int [] pars)
		{
			// Optimize a single SGR0.
			if (pars.Length == 1 && pars [0] == 0) {
				terminal.CurAttr = CharData.DefaultAttr;
				return;
			}

			var parCount = pars.Length;
			var flags = (FLAGS)(terminal.CurAttr >> 18);
			var fg = (terminal.CurAttr >> 9) & 0x1ff;
			var bg = terminal.CurAttr & 0x1ff;
			var def = CharData.DefaultAttr;

			for (var i = 0; i < parCount; i++) {
				int p = pars [i];
				if (p >= 30 && p <= 37) {
					// fg color 8
					fg = p - 30;
				} else if (p >= 40 && p <= 47) {
					// bg color 8
					bg = p - 40;
				} else if (p >= 90 && p <= 97) {
					// fg color 16
					p += 8;
					fg = p - 90;
				} else if (p >= 100 && p <= 107) {
					// bg color 16
					p += 8;
					bg = p - 100;
				} else if (p == 0) {
					// default

					flags = (FLAGS)(def >> 18);
					fg = (def >> 9) & 0x1ff;
					bg = def & 0x1ff;
					// flags = 0;
					// fg = 0x1ff;
					// bg = 0x1ff;
				} else if (p == 1) {
					// bold text
					flags |= FLAGS.BOLD;
				} else if (p == 3) {
					// italic text
					flags |= FLAGS.ITALIC;
				} else if (p == 4) {
					// underlined text
					flags |= FLAGS.UNDERLINE;
				} else if (p == 5) {
					// blink
					flags |= FLAGS.BLINK;
				} else if (p == 7) {
					// inverse and positive
					// test with: echo -e '\e[31m\e[42mhello\e[7mworld\e[27mhi\e[m'
					flags |= FLAGS.INVERSE;
				} else if (p == 8) {
					// invisible
					flags |= FLAGS.INVISIBLE;
				} else if (p == 2) {
					// dimmed text
					flags |= FLAGS.DIM;
				} else if (p == 22) {
					// not bold nor faint
					flags &= ~FLAGS.BOLD;
					flags &= ~FLAGS.DIM;
				} else if (p == 23) {
					// not italic
					flags &= ~FLAGS.ITALIC;
				} else if (p == 24) {
					// not underlined
					flags &= ~FLAGS.UNDERLINE;
				} else if (p == 25) {
					// not blink
					flags &= ~FLAGS.BLINK;
				} else if (p == 27) {
					// not inverse
					flags &= ~FLAGS.INVERSE;
				} else if (p == 28) {
					// not invisible
					flags &= ~FLAGS.INVISIBLE;
				} else if (p == 39) {
					// reset fg
					fg = (CharData.DefaultAttr >> 9) & 0x1ff;
				} else if (p == 49) {
					// reset bg
					bg = CharData.DefaultAttr & 0x1ff;
				} else if (p == 38) {
					if (i + 1 < parCount) {
						// fg color 256
						if (pars [i + 1] == 2) {
							// Well this is a problem, if there are 3 arguments, expect R/G/B, if there are
							// more than 3, skip the first that would be the colorspace
							if (i + 5 < parCount) {
								i += 1;
							}
							if (i + 4 < parCount) {
								fg = terminal.MatchColor (
									pars [i + 2] & 0xff,
									pars [i + 3] & 0xff,
									pars [i + 4] & 0xff);
								if (fg == -1)
									fg = 0x1ff;
							}
							// Given the historical disagreement that was caused by an ambiguous spec,
							// we eat all the remaining parameters.
							i = parCount;
						} else if (pars [i + 1] == 5) {
							if (i + 2 < parCount) {
								p = pars [i + 2] & 0xff;
								fg = p;
								i += 1;
							}
							i += 1;
						}
					}
				} else if (p == 48) {
					if (i + 1 < parCount) {
						// bg color 256
						if (pars [i + 1] == 2) {
							// Well this is a problem, if there are 3 arguments, expect R/G/B, if there are
							// more than 3, skip the first that would be the colorspace
							if (i + 5 < parCount) {
								i += 1;
							}
							if (i + 4 < parCount) {
								bg = terminal.MatchColor (
									pars [i + 2] & 0xff,
									pars [i + 3] & 0xff,
									pars [i + 4] & 0xff);
								if (bg == -1)
									bg = 0x1ff;
							}
							// Given the historical disagreement that was caused by an ambiguous spec,
							// we eat all the remaining parameters.
							i = parCount;
						} else if (pars [i + 1] == 5) {
							if (i + 2 < parCount) {
								p = pars [i + 2] & 0xff;
								bg = p;
								i += 1;
							}
							i += 1;
						}
					}
				} else if (p == 100) {
					// reset fg/bg
					fg = (def >> 9) & 0x1ff;
					bg = def & 0x1ff;
				} else {
					terminal.Error ("Unknown SGR attribute: %d.", p);
				}
			}
			terminal.CurAttr = ((int)flags << 18) | (fg << 9) | bg;
		}

		void ResetMode (int [] pars, string collect)
		{
			if (pars.Length == 0)
				return;

			if (pars.Length > 1) {
				for (var i = 0; i < pars.Length; i++)
					terminal.csiDECRESET (pars [i], "");


				return;
			}
			terminal.csiDECRESET (pars [0], collect);
		}

		void SetMode (int [] pars, string collect)
		{
			if (pars.Length == 0)
				return;

			if (pars.Length > 1) {
				for (var i = 0; i < pars.Length; i++)
					terminal.csiDECSET (pars [i], "");


				return;
			}
			terminal.csiDECSET (pars [0], collect);
		}


		// 
		// CSI Ps g  Tab Clear (TBC).
		//     Ps = 0  -> Clear Current Column (default).
		//     Ps = 3  -> Clear All.
		// Potentially:
		//   Ps = 2  -> Clear Stops on Line.
		//   http://vt100.net/annarbor/aaa-ug/section6.html
		// 
		void TabClear (int [] pars)
		{
			var p = pars.Length == 0 ? 0 : pars [0];
			var buffer = terminal.Buffer;
			if (p == 0)
				buffer.ClearStop (buffer.X);
			else if (p == 3)
				buffer.ClearTabStops ();
		}

		//
		// CSI Ps ; Ps f
		//   Horizontal and Vertical Position [row;column] (default =
		//   [1,1]) (HVP).
		//
		void HVPosition (int [] pars)
		{
			int p = 1;
			int q = 1;
			if (pars.Length > 0) {
				p = Math.Max (pars [0], 1);
				if (pars.Length > 1)
					q = Math.Max (pars [1], 1);
			}
			var buffer = terminal.Buffer;
			buffer.Y = p - 1;
			if (buffer.Y >= terminal.Rows)
				buffer.Y = terminal.Rows - 1;

			buffer.X = q - 1;
			if (buffer.X >= terminal.Cols)
				buffer.X = terminal.Cols - 1;
		}

		// 
		// CSI Pm e  Vertical Position Relative (VPR)
		//   [rows] (default = [row+1,column])
		// reuse CSI Ps B ?
		// 
		void VPositionRelative (int [] pars)
		{
			var p = Math.Max (pars.Length == 0 ? 1 : pars [0], 1);
			var buffer = terminal.Buffer;

			var newY = buffer.Y + p;

			if (newY >= terminal.Rows) {
				buffer.Y = terminal.Rows - 1;
			} else
				buffer.Y = newY;

			// If the end of the line is hit, prevent this action from wrapping around to the next line.
			if (buffer.X >= terminal.Cols)
				buffer.X--;
		}

		// 
		// CSI Pm d  Vertical Position Absolute (VPA)
		//   [row] (default = [1,column])
		// 
		void LinePosAbsolute (int [] pars)
		{
			var p = Math.Max (pars.Length == 0 ? 1 : pars [0], 1);
			var buffer = terminal.Buffer;

			if (p - 1 >= terminal.Rows)
				buffer.Y = terminal.Rows - 1;
			else
				buffer.Y = p - 1;
		}

		// 
		// CSI Ps b  Repeat the preceding graphic character Ps times (REP).
		//
		void RepeatPrecedingCharacter (int [] pars)
		{
			var p = Math.Max (pars.Length == 0 ? 1 : pars [0], 1);

			var buffer = terminal.Buffer;
			var line = buffer.Lines [buffer.YBase + buffer.Y];
			CharData cd = buffer.X - 1 < 0 ? new CharData (CharData.DefaultAttr) : line [buffer.X - 1];
			line.ReplaceCells (buffer.X,
				  buffer.X + p,
				      cd);
			// FIXME: no UpdateRange here?
		}

		//
		//CSI Pm a  Character Position Relative
		//  [columns] (default = [row,col+1]) (HPR)
		//reuse CSI Ps C ?
		//
		void HPositionRelative (int [] pars)
		{
			var p = Math.Max (pars.Length == 0 ? 1 : pars [0], 1);
			var buffer = terminal.Buffer;

			buffer.X += p;
			if (buffer.X >= terminal.Cols)
				buffer.X = terminal.Cols - 1;
		}

		// 
		// CSI Pm `  Character Position Absolute
		//   [column] (default = [row,1]) (HPA).
		// 
		void CharPosAbsolute (int [] pars)
		{
			var p = Math.Max (pars.Length == 0 ? 1 : pars [0], 1);
			var buffer = terminal.Buffer;

			buffer.X = p - 1;
			if (buffer.X >= terminal.Cols)
				buffer.X = terminal.Cols - 1;
		}

		// 
		// CSI Ps X
		// Erase Ps Character(s) (default = 1) (ECH).
		// 
		void EraseChars (int [] pars)
		{
			var p = Math.Max (pars.Length == 0 ? 1 : pars [0], 1);

			var buffer = terminal.Buffer;
			buffer.Lines [buffer.Y + buffer.YBase].ReplaceCells (
				  buffer.X,
				  buffer.X + p,
				new CharData (terminal.EraseAttr ()));
		}

		// 
		// CSI Ps T  Scroll down Ps lines (default = 1) (SD).
		// 
		void ScrollDown (int [] pars)
		{
			var p = Math.Max (pars.Length == 0 ? 1 : pars [0], 1);
			var buffer = terminal.Buffer;

			while (p-- != 0) {
				buffer.Lines.Splice (buffer.YBase + buffer.ScrollBottom, 1);
				buffer.Lines.Splice (buffer.YBase + buffer.ScrollBottom, 0, buffer.GetBlankLine (CharData.DefaultAttr));
			}
			// this.maxRange();
			terminal.UpdateRange (buffer.ScrollTop);
			terminal.UpdateRange (buffer.ScrollBottom);

		}

		// 
		// CSI Ps S  Scroll up Ps lines (default = 1) (SU).
		// 
		void ScrollUp (int [] pars)
		{
			var p = Math.Max (pars.Length == 0 ? 1 : pars [0], 1);
			var buffer = terminal.Buffer;

			while (p-- != 0) {
				buffer.Lines.Splice (buffer.YBase + buffer.ScrollTop, 1);
				buffer.Lines.Splice (buffer.YBase + buffer.ScrollBottom, 0, buffer.GetBlankLine (CharData.DefaultAttr));
			}
			// this.maxRange();
			terminal.UpdateRange (buffer.ScrollTop);
			terminal.UpdateRange (buffer.ScrollBottom);
		}

		// 
		// CSI Ps K  Erase in Line (EL).
		//     Ps = 0  -> Erase to Right (default).
		//     Ps = 1  -> Erase to Left.
		//     Ps = 2  -> Erase All.
		// CSI ? Ps K
		//   Erase in Line (DECSEL).
		//     Ps = 0  -> Selective Erase to Right (default).
		//     Ps = 1  -> Selective Erase to Left.
		//     Ps = 2  -> Selective Erase All.
		// 
		void EraseInLine (int [] pars)
		{
			var p = pars.Length == 0 ? 0 : pars [0];
			var buffer = terminal.Buffer;
			switch (p) {
			case 0:
				EraseInBufferLine (buffer.Y, buffer.X, terminal.Cols);
				break;
			case 1:
				EraseInBufferLine (buffer.Y, 0, buffer.X + 1);
				break;
			case 2:
				EraseInBufferLine (buffer.Y, 0, terminal.Cols);
				break;
			}
			terminal.UpdateRange (buffer.Y);
		}

		// 
		// CSI Ps J  Erase in Display (ED).
		//     Ps = 0  -> Erase Below (default).
		//     Ps = 1  -> Erase Above.
		//     Ps = 2  -> Erase All.
		//     Ps = 3  -> Erase Saved Lines (xterm).
		// CSI ? Ps J
		//   Erase in Display (DECSED).
		//     Ps = 0  -> Selective Erase Below (default).
		//     Ps = 1  -> Selective Erase Above.
		//     Ps = 2  -> Selective Erase All.
		// 
		void EraseInDisplay (int [] pars)
		{
			var p = pars.Length == 0 ? 0 : pars [0];
			var buffer = terminal.Buffer;
			int j;
			switch (p) {
			case 0:
				j = buffer.Y;
				terminal.UpdateRange (j);
				EraseInBufferLine (j++, buffer.X, terminal.Cols, buffer.X == 0);
				for (; j < terminal.Rows; j++) {
					ResetBufferLine (j);
				}
				terminal.UpdateRange (j - 1);
				break;
			case 1:
				j = buffer.Y;
				terminal.UpdateRange (j);
				// Deleted front part of line and everything before. This line will no longer be wrapped.
				EraseInBufferLine (j, 0, buffer.X + 1, true);
				if (buffer.X + 1 >= terminal.Cols) {
					// Deleted entire previous line. This next line can no longer be wrapped.
					buffer.Lines [j + 1].IsWrapped = false;
				}
				while (j-- != 0) {
					ResetBufferLine (j);
				}
				terminal.UpdateRange (0);
				break;
			case 2:
				j = terminal.Rows;
				terminal.UpdateRange (j - 1);
				while (j-- != 0) {
					ResetBufferLine (j);
				}
				terminal.UpdateRange (0);
				break;
			case 3:
				// Clear scrollback (everything not in viewport)
				var scrollBackSize = buffer.Lines.Length - terminal.Rows;
				if (scrollBackSize > 0) {
					buffer.Lines.TrimStart (scrollBackSize);
					buffer.YBase = Math.Max (buffer.YBase - scrollBackSize, 0);
					buffer.YDisp = Math.Max (buffer.YDisp - scrollBackSize, 0);
				}
				break;

			}
		}

		// 
		// CSI Ps I
		//   Cursor Forward Tabulation Ps tab stops (default = 1) (CHT).
		// 
		void CursorForwardTab (int [] pars)
		{
			int param = Math.Max (pars.Length > 0 ? pars [0] : 1, 1);
			var buffer = terminal.Buffer;
			while (param-- != 0)
				buffer.X = buffer.NextTabStop ();
		}

		// 
		// CSI Ps F
		// Cursor Preceding Line Ps Times (default = 1) (CNL).
		// reuse CSI Ps A ?
		// 
		void CursorPrecedingLine (int [] pars)
		{
			int param = Math.Max (pars.Length > 0 ? pars [0] : 1, 1);
			var buffer = terminal.Buffer;

			buffer.Y -= param;
			var newY = buffer.Y - param;
			if (newY < 0)
				buffer.Y = 0;
			else
				buffer.Y = newY;
			buffer.X = 0;
		}

		// 
		// CSI Ps E
		// Cursor Next Line Ps Times (default = 1) (CNL).
		// same as CSI Ps B?
		// 
		void CursorNextLine (int [] pars)
		{
			int param = Math.Max (pars.Length > 0 ? pars [0] : 1, 1);
			var buffer = terminal.Buffer;

			var newY = buffer.Y + param;

			if (newY >= terminal.Rows)
				buffer.Y = terminal.Rows - 1;
			else
				buffer.Y = newY;

			buffer.X = 0;
		}

		//
		// CSI Ps @
		// Insert Ps (Blank) Character(s) (default = 1) (ICH).
		//
		void InsertChars (int [] pars)
		{
			terminal.RestrictCursor ();
			var buffer = terminal.Buffer;
			var cd = new CharData (terminal.EraseAttr ());

			buffer.Lines [buffer.Y + buffer.YBase].InsertCells (
				buffer.X,
				pars.Length > 0 ? Math.Max(pars [0], 1) : 1,
				rightMargin: terminal.MarginMode ? buffer.MarginRight : buffer.Cols - 1,
				cd);

			terminal.UpdateRange (buffer.Y);
		}

		void PrintStateReset()
		{
			readingBuffer.Reset ();
		}

		unsafe void Print (byte* data, int start, int end)
		{
			readingBuffer.Prepare (data, start, end - start);

			var buffer = terminal.Buffer;
			var charset = terminal.Charset;
			var screenReaderMode = terminal.Options.ScreenReaderMode;
			var cols = terminal.Cols;
			var wrapAroundMode = terminal.Wraparound;
			var insertMode = terminal.InsertMode;
			var curAttr = terminal.CurAttr;
			var bufferRow = buffer.Lines [buffer.Y + buffer.YBase];

			terminal.UpdateRange (buffer.Y);

			while (readingBuffer.HasNext()) {
				var ch = ' ';
				var chWidth = 0;
				int code = 32;
				byte bufferValue = readingBuffer.GetNext();
				var n = RuneExt.ExpectedSizeFromFirstByte(bufferValue);
				if (n == -1 || n == 1) {
					var chSet = false;
					if (bufferValue < 127 && charset != null) {
						var str = charset[bufferValue];
						if (!string.IsNullOrEmpty(str)) {
							ch = str[0];
							// Every single mapping in the charset only takes one slot
							chWidth = 1;
							chSet = true;
						}
					}

					if (!chSet) {
						(var rune, var size) = Rune.DecodeRune(new byte[] { bufferValue });
						chWidth = size;
						ch = rune.ToString()[0];
					}

				}
				// Invalid UTF-8 sequence, client sent us some junk, happens if we run with the wrong locale set
				// for example if LANG=en
				else if (readingBuffer.BytesLeft() >= (n - 1)) {
					var bytes = new byte[n];
					bytes[0] = bufferValue;
					for (int j = 1; j < n; j++)
						bytes[j] = readingBuffer.GetNext();

					string s = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
					if (s.Length > 0) {
						ch = s[0];
					}
					else {
						ch = ' ';
					}

					// Now the challenge is that we have a character, not a rune, and we want to compute
					// the width of it.
					if (s.Length == 1) {
						chWidth = RuneHelper.ConsoleWidth(s[0]);
					}
					else {
						chWidth = 0;

						foreach (var scalar in s) {
							chWidth = Math.Max(chWidth, RuneHelper.ConsoleWidth(scalar));
						}
					}

				}
				else {
					readingBuffer.Putback(bufferValue);
					return;
				}

				// get charset replacement character
				// charset are only defined for ASCII, therefore we only
				// search for an replacement char if code < 127
				if (bufferValue < 127 && charset != null) {

					// MIGUEL-FIXME - this is broken for dutch cahrset that returns two letters "ij", need to figure out what to do
					if (charset.TryGetValue (bufferValue, out var str)) {
						ch = str [0];
						code = ch;
					}
				}
				if (screenReaderMode)
					terminal.EmitChar (ch);

				// insert combining char at last cursor position
				// FIXME: needs handling after cursor jumps
				// buffer.x should never be 0 for a combining char
				// since they always follow a cell consuming char
				// therefore we can test for buffer.x to avoid overflow left
				if (chWidth == 0 && buffer.X > 0) {
					// MIGUEL TODO: in the original code the getter might return a null value
					// does this mean that JS returns null for out of bounsd?
					if (buffer.X >= 1 && buffer.X < bufferRow.Length) {
						var chMinusOne = bufferRow [buffer.X - 1];
						if (chMinusOne.Width == 0) {
							// found empty cell after fullwidth, need to go 2 cells back
							// it is save to step 2 cells back here
							// since an empty cell is only set by fullwidth chars
							if (buffer.X >= 2) {
								var chMinusTwo = bufferRow [buffer.X - 2];

								chMinusTwo.Code += ch;
								chMinusTwo.Rune = (uint)code;
								bufferRow [buffer.X - 2] = chMinusTwo; // must be set explicitly now
							}
						} else {
							chMinusOne.Code += ch;
							chMinusOne.Rune = (uint)code;
							bufferRow [buffer.X - 1] = chMinusOne; // must be set explicitly now
						}
					}
					continue;
				}

				// goto next line if ch would overflow
				// TODO: needs a global min terminal width of 2
				// FIXME: additionally ensure chWidth fits into a line
				//   -->  maybe forbid cols<xy at higher level as it would
				//        introduce a bad runtime penalty here
				var right = terminal.MarginMode ? buffer.MarginRight : cols - 1;
				if (buffer.X + chWidth - 1 > right) {
					// autowrap - DECAWM
					// automatically wraps to the beginning of the next line
					if (wrapAroundMode) {
						buffer.X = terminal.MarginMode ? buffer.MarginLeft : 0;

						if (buffer.Y >= buffer.ScrollBottom) {
							terminal.Scroll (isWrapped: true);
						} else {
							// The line already exists (eg. the initial viewport), mark it as a
							// wrapped line
							buffer.Lines [++buffer.Y].IsWrapped = true;
						}

						// row changed, get it again
						bufferRow = buffer.Lines [buffer.Y + buffer.YBase];
					} else {
						if (chWidth == 2) {
							// FIXME: check for xterm behavior
							// What to do here? We got a wide char that does not fit into last cell
							continue;
						}

						buffer.X = right;
					}
				}

				var empty = CharData.Null;
				empty.Attribute = curAttr;
				// insert mode: move characters to right
				if (insertMode) {
					// right shift cells according to the width
					bufferRow.InsertCells (buffer.X, chWidth, terminal.MarginMode ? buffer.MarginRight : cols - 1, empty);
					// test last cell - since the last cell has only room for
					// a halfwidth char any fullwidth shifted there is lost
					// and will be set to eraseChar
					var lastCell = bufferRow [cols - 1];
					if (lastCell.Width == 2)
						bufferRow [cols - 1] = empty;

				}

				// write current char to buffer and advance cursor
				var charData = new CharData (curAttr, ch, chWidth);
				bufferRow [buffer.X++] = charData;

				// fullwidth char - also set next cell to placeholder stub and advance cursor
				// for graphemes bigger than fullwidth we can simply loop to zero
				// we already made sure above, that buffer.x + chWidth will not overflow right
				if (chWidth > 0) {
					while (--chWidth != 0) {
						bufferRow [buffer.X++] = empty;
					}
				}
			}
			terminal.UpdateRange (buffer.Y);
			readingBuffer.Done ();
		}
	}
}
