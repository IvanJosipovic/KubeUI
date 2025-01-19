using System;
using System.Collections.Generic;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;

namespace XtermSharp {

	public partial class Terminal: ObservableObject
    {
		const int MINIMUM_COLS = 2;
		const int MINIMUM_ROWS = 1;

		//static Dictionary<int, int> matchColorCache = new Dictionary<int, int> ();
		readonly ITerminalDelegate terminalDelegate;
		readonly ControlCodes controlCodes;
		readonly List<string> titleStack;
		readonly List<string> iconTitleStack;
		readonly BufferSet buffers;
		readonly InputHandler input;

		BufferLine blankLine;

		// modes
		bool insertMode;
		bool bracketedPasteMode;

		// saved modes
		bool savedMarginMode;
		bool savedOriginMode;
		bool savedWraparound;
		bool savedReverseWraparound;

		// unsorted
		bool applicationKeypad, applicationCursor;
		bool cursorHidden;
		Dictionary<byte, string> charset;
		int gcharset;
		int gLevel;
		int refreshStart = Int32.MaxValue;
		int refreshEnd = -1;
		bool userScrolling;

		public Terminal (ITerminalDelegate terminalDelegate = null, TerminalOptions options = null)
		{
			this.terminalDelegate = terminalDelegate ?? new SimpleTerminalDelegate ();
			controlCodes = new ControlCodes () { Send8bit = false };
			titleStack = new List<string> ();
			iconTitleStack = new List<string> ();
			input = new InputHandler (this);

			Options = options ?? new TerminalOptions ();
			Cols = Math.Max (Options.Cols, MINIMUM_COLS);
			Rows = Math.Max (Options.Rows, MINIMUM_ROWS);

			buffers = new BufferSet (this);
			Setup ();
		}

		/// <summary>
		/// Gets the delegate for the terminal
		/// </summary>
		public ITerminalDelegate Delegate => terminalDelegate;

		/// <summary>
		/// Gets the control codes for the terminal
		/// </summary>
		public ControlCodes ControlCodes => controlCodes;

		/// <summary>
		/// Gets the current title of the terminal
		/// </summary>
		public string Title { get; private set; }

		/// <summary>
		/// Gets the current icon title of the terminal
		/// </summary>
		public string IconTitle { get; private set; }

		/// <summary>
		/// Gets the currently active buffer
		/// </summary>
		public Buffer Buffer => buffers.Active;

		/// <summary>
		/// Gets the BufferSet for the terminal
		/// </summary>
		public BufferSet Buffers => buffers;

		/// <summary>
		/// Gets the margin mode of the terminal
		/// </summary>
		public bool MarginMode { get; internal set; }

		/// <summary>
		/// Gets the origin mode of the terminal
		/// </summary>
		public bool OriginMode { get; internal set; }

		/// <summary>
		/// Gets the Wraparound mode of the terminal
		/// </summary>
		public bool Wraparound { get; internal set; }

		/// <summary>
		/// Gets the ReverseWraparound mode of the terminal
		/// </summary>
		public bool ReverseWraparound { get; internal set; }

		/// <summary>
		/// Gets the current mouse mode
		/// </summary>
		public MouseMode MouseMode { get; internal set; }

		/// <summary>
		/// Gets the current mouse protocol
		/// </summary>
		public MouseProtocolEncoding MouseProtocol { get; internal set; }

		/// <summary>
		/// Gets a value indicating whether the terminal can be resized to 132
		/// </summary>
		public bool Allow80To132 { get; internal set; }

		public Dictionary<byte, string> Charset {
			get => charset;
			set {
				charset = value;
			}
		}

		public bool ApplicationCursor { get; internal set; }
		public int SavedCols { get; internal set; }
		public bool ApplicationKeypad { get; internal set; }

		public bool SendFocus { get; internal set; }

		public bool CursorHidden { get; internal set; }
		public bool BracketedPasteMode { get; internal set; }

		public TerminalOptions Options { get; private set; }

        [ObservableProperty]
		public partial int Cols { get; private set; }

        [ObservableProperty]
        public partial int Rows { get; private set; }

		public bool InsertMode;
		public int CurAttr;

		/// <summary>
		/// Provides a baseline set of environment variables that would be useful to run the terminal,
		/// you can customzie these accordingly.
		/// </summary>
		public static string [] GetEnvironmentVariables (string termName = null)
		{
			var l = new List<string> ();
			if (termName == null)
				termName = "xterm-256color";

			l.Add ("TERM=" + termName);

			// Without this, tools like "vi" produce sequences that are not UTF-8 friendly
			l.Add ("LANG=en_US.UTF-8");
			var env = Environment.GetEnvironmentVariables ();
			foreach (var x in new [] { "LOGNAME", "DISPLAY", "LC_TYPE", "USER", "HOME", "PATH" })
				if (env.Contains (x))
					l.Add ($"{x}={env [x]}");
			return l.ToArray ();
		}

		/// <summary>
		/// Called by input handlers to set the title
		/// </summary>
		internal void SetTitle (string text)
		{
			Title = text;
			terminalDelegate.SetTerminalTitle (this, text);
		}

		/// <summary>
		/// Called by input handlers to push the current title onto the stack
		/// </summary>
		internal void PushTitle ()
		{
			titleStack.Insert (0, Title);
		}

		/// <summary>
		/// Called by input handlers to pop and set the title to the last one on the stack
		/// </summary>
		internal void PopTitle ()
		{
			if (titleStack.Count > 0) {
				Title = titleStack[0];
				titleStack.RemoveAt (0);
			}

			terminalDelegate.SetTerminalTitle (this, Title);
		}

		/// <summary>
		/// Called by input handlers to set the icon title
		/// </summary>
		internal void SetIconTitle (string text)
		{
			IconTitle = text;
			terminalDelegate.SetTerminalIconTitle (this, text);
		}

		/// <summary>
		/// Called by input handlers to push the current icon title onto the stack
		/// </summary>
		internal void PushIconTitle ()
		{
			iconTitleStack.Insert (0, IconTitle);
		}

		/// <summary>
		/// Called by input handlers to pop and set the icon title to the last one on the stack
		/// </summary>
		internal void PopIconTitle ()
		{
			if (iconTitleStack.Count > 0) {
				IconTitle = iconTitleStack [0];
				iconTitleStack.RemoveAt (0);
			}

			terminalDelegate.SetTerminalIconTitle (this, IconTitle);
		}

		/// <summary>
		/// Sends a response to a command
		/// </summary>
		public void SendResponse (string txt)
		{
			terminalDelegate.Send (Encoding.UTF8.GetBytes (txt));
		}

		/// <summary>
		/// Sends a response to a command
		/// </summary>
		public void SendResponse (params object[] args)
		{
			if (args == null) {
				return;
			}

			int len = args.Length;
			for (int i = 0; i < args.Length; i++) {
				if (args [i] is string s) {
					len += s != null ? s.Length - 1: 0;
				} else if (args [i] is byte [] ba) {
					len += ba.Length - 1;
				}
			}

			var buffer = new byte [len];

			int bufferIndex = 0;
			for (int i = 0; i < args.Length; i++) {
				if (args[i] == null) {
					buffer [bufferIndex] = 0;
				} else if (args[i] is byte b) {
					buffer [bufferIndex] = b;
				} else if (args[i] is string s) {
					if (s == null) {
						buffer [bufferIndex] = 0;
					} else {
						foreach (var sb in Encoding.UTF8.GetBytes (s)) {
							buffer [bufferIndex++] = sb;
						}
					}
					bufferIndex--;
				} else if (args[i] is byte[] ba) {
					foreach (var bab in ba) {
						buffer [bufferIndex++] = bab;
					}
					bufferIndex--;
				} else {
					Error ("Unsupported type in SendResponse", args[i].GetType());
				}

				bufferIndex++;
			}

			terminalDelegate.Send (buffer);
		}

		/// <summary>
		/// Reports an error to the system log
		/// </summary>
		public void Error (string txt, params object [] args)
		{
			Report ("ERROR", txt, args);
		}

		/// <summary>
		/// Logs a message to the system log
		/// </summary>
		public void Log (string text, params object [] args)
		{
			Report ("LOG", text, args);
		}

		public void Feed (byte [] data, int len = -1)
		{
			input.Parse (data, len);
		}

		public void Feed (IntPtr data, int len = -1)
		{
			input.Parse (data, len);
		}

		public void Feed (string text)
		{
			var bytes = Encoding.UTF8.GetBytes (text);
			Feed (bytes, bytes.Length);
		}

		internal void UpdateRange (int y)
		{
			if (y < 0)
				throw new ArgumentException ();

			if (y < refreshStart)
				refreshStart = y;
			if (y > refreshEnd)
				refreshEnd = y;
		}

		public void GetUpdateRange (out int startY, out int endY)
		{
			startY = refreshStart;
			endY = refreshEnd;
		}

		public void ClearUpdateRange ()
		{
			refreshStart = Int32.MaxValue;
			refreshEnd = -1;
		}

		internal void EmitChar (int ch)
		{
			// For accessibility purposes 'a11y.char' in the original source.
		}

		//
		// ESC c Full Reset (RIS)
		//
		internal void Reset ()
		{
			Options.Rows = Rows;
			Options.Cols = Cols;

			var savedCursorHidden = cursorHidden;
			Setup ();
			cursorHidden = savedCursorHidden;
			Refresh (0, Rows - 1);
			SyncScrollArea ();
		}

		//
		// ESC D Index (Index is 0x84)
		//
		internal void Index ()
		{
			var buffer = Buffer;
			var newY = buffer.Y + 1;
			if (newY > buffer.ScrollBottom) {
				Scroll ();
			} else {
				buffer.Y = newY;
			}
			// If the end of the line is hit, prevent this action from wrapping around to the next line.
			if (buffer.X > Cols)
				buffer.X--;
		}

		internal void Scroll (bool isWrapped = false)
		{
			var buffer = Buffer;
			BufferLine newLine = blankLine;
			if (newLine == null || newLine.Length != Cols || newLine [0].Attribute != EraseAttr ()) {
				newLine = buffer.GetBlankLine (EraseAttr (), isWrapped);
				blankLine = newLine;
			}
			newLine.IsWrapped = isWrapped;

			var topRow = buffer.YBase + buffer.ScrollTop;
			var bottomRow = buffer.YBase + buffer.ScrollBottom;

			if (buffer.ScrollTop == 0) {
				// Determine whether the buffer is going to be trimmed after insertion.
				var willBufferBeTrimmed = buffer.Lines.IsFull;

				// Insert the line using the fastest method
				if (bottomRow == buffer.Lines.Length - 1) {
					if (willBufferBeTrimmed) {
						buffer.Lines.Recycle ().CopyFrom (newLine);
					} else {
						buffer.Lines.Push (new BufferLine (newLine));
					}
				} else {
					buffer.Lines.Splice (bottomRow + 1, 0, new BufferLine (newLine));
				}

				// Only adjust ybase and ydisp when the buffer is not trimmed
				if (!willBufferBeTrimmed) {
					buffer.YBase++;
					// Only scroll the ydisp with ybase if the user has not scrolled up
					if (!userScrolling) {
						buffer.YDisp++;
					}
				} else {
					// When the buffer is full and the user has scrolled up, keep the text
					// stable unless ydisp is right at the top
					if (userScrolling) {
						buffer.YDisp = Math.Max (buffer.YDisp - 1, 0);
					}
				}
			} else {
				// scrollTop is non-zero which means no line will be going to the
				// scrollback, instead we can just shift them in-place.
				var scrollRegionHeight = bottomRow - topRow + 1/*as it's zero-based*/;

				if (scrollRegionHeight > 1) {
					buffer.Lines.ShiftElements (topRow + 1, scrollRegionHeight - 1, -1);
				}

				buffer.Lines [bottomRow] = new BufferLine (newLine);
			}

			// Move the viewport to the bottom of the buffer unless the user is
			// scrolling.
			if (!userScrolling) {
				buffer.YDisp = buffer.YBase;
			}

			// Flag rows that need updating
			UpdateRange (buffer.ScrollTop);
			UpdateRange (buffer.ScrollBottom);

			/**
			 * This event is emitted whenever the terminal is scrolled.
			 * The one parameter passed is the new y display position.
			 *
			 * @event scroll
			 */
			Scrolled?.Invoke (this, buffer.YDisp);
		}

		/// <summary>
		/// Scroll the display of the terminal
		/// </summary>
		/// <param name="disp">The number of lines to scroll down (negative scroll up)</param>
		/// <param name="suppressScrollEvent">Don't emit the scroll event as scrollLines. This is use to avoid unwanted
		/// events being handled by the viewport when the event was triggered from the viewport originally.</param>
		public void ScrollLines(int disp, bool suppressScrollEvent = false)
		{
			if (disp < 0) {
				if (Buffer.YDisp == 0) {
					return;
				}

				this.userScrolling = true;
			} else if (disp + Buffer.YDisp >= Buffer.YBase) {
				this.userScrolling = false;
			}

			int oldYdisp = Buffer.YDisp;
			Buffer.YDisp = Math.Max (Math.Min (Buffer.YDisp + disp, Buffer.YBase), 0);

			// No change occurred, don't trigger scroll/refresh
			if (oldYdisp == Buffer.YDisp) {
				return;
			}

			if (!suppressScrollEvent) {
				Scrolled?.Invoke (this, Buffer.YDisp);
			}

			Refresh (0, this.Rows - 1);
		}

		public event Action<Terminal, int> Scrolled;

		public event Action<Terminal, string> DataEmitted;

		internal void Bell ()
		{
			//Console.WriteLine ("beep");
		}

		public void EmitLineFeed ()
		{
			if (LineFeedEvent != null)
				LineFeedEvent (this);
		}

		public event Action<Terminal> LineFeedEvent;

		internal void EmitA11yTab (object p)
		{
			throw new NotImplementedException ();
		}

		internal void SetgLevel (int v)
		{
			gLevel = v;
			if (CharSets.All.TryGetValue ((byte)v, out var cs))
				Charset = cs;
			else
				Charset = null;
		}

		internal int EraseAttr ()
		{
			return (CharData.DefaultAttr & ~0x1ff) | CurAttr & 0x1ff;
		}

		internal void EmitScroll (int v)
		{
			return;
			throw new NotImplementedException ();
		}

		internal void SetgCharset (byte v, Dictionary<byte, string> charset)
		{
			CharSets.All [v] = charset;
			if (gLevel == v)
				this.charset = charset;
		}

		public void Resize (int cols, int rows)
		{
			if (cols < MINIMUM_COLS)
				cols = MINIMUM_COLS;
			if (rows < MINIMUM_ROWS)
				rows = MINIMUM_ROWS;
			if (cols == Cols && rows == Rows)
				return;

			var oldCols = Cols;
			Cols = cols;
			Rows = rows;
			Buffers.Resize (cols, rows);
			buffers.SetupTabStops (oldCols);
			Refresh (0, Rows - 1);
		}

		internal void SyncScrollArea ()
		{
			// This should call the viewport syncscrollarea
			//throw new NotImplementedException ();
		}

		/// <summary>
		/// Implemented by subclasses - must refresh the display from the starting to the end row.
		/// </summary>
		/// <param name="startRow">Initial row to update, offset starts at zero.</param>
		/// <param name="endRow">Last row to update.</param>
		public void Refresh (int startRow, int endRow)
		{
			// TO BE HONEST - This probably should not be called directly,
			// instead the view shoudl after feeding data, determine if there is a need
			// to refresh based on the parameters provided for refresh ranges, and then
			// update, to avoid the backend rtiggering this multiple times.

			UpdateRange (startRow);
			UpdateRange (endRow);
		}

		public void ShowCursor ()
		{
			if (cursorHidden == false)
				return;
			cursorHidden = false;
			Refresh (Buffer.Y, Buffer.Y);
			terminalDelegate.ShowCursor (this);
		}

		/// <summary>
		/// Encodes button and position to characters
		/// </summary>
		void EncodeMouseUtf (List<byte> data, int ch)
		{
			if (ch == 2047) {
				data.Add (0);
				return;
			}
			if (ch < 127) {
				data.Add ((byte)ch);
			} else {
				if (ch > 2047)
					ch = 2047;
				data.Add ((byte)(0xC0 | (ch >> 6)));
				data.Add ((byte)(0x80 | (ch & 0x3F)));
			}
		}

		/// <summary>
		/// Encodes the mouse button.
		/// </summary>
		/// <returns>The mouse button.</returns>
		/// <param name="button">Button (0, 1, 2 for left, middle, right) and 4 for wheel up, and 5 for wheel down.</param>
		/// <param name="release">If set to <c>true</c> release.</param>
		/// <param name="wheelUp">If set to <c>true</c> wheel up.</param>
		/// <param name="shift">If set to <c>true</c> shift.</param>
		/// <param name="meta">If set to <c>true</c> meta.</param>
		/// <param name="control">If set to <c>true</c> control.</param>
		public int EncodeMouseButton (int button, bool release, bool shift, bool meta, bool control)
		{
			int value;

			if (release)
				value = 3;
			else {
				switch (button) {
				case 0:
					value = 0;
					break;
				case 1:
					value = 1;
					break;
				case 2:
					value = 2;
					break;
				case 4:
					value = 64;
					break;
				case 5:
					value = 65;
					break;
				default:
					value = 0;
					break;
				}
			}

			if (MouseMode.SendsModifiers()) {
				if (shift)
					value |= 4;
				if (meta)
					value |= 8;
				if (control)
					value |= 16;
			}
			return value;
		}

		/// <summary>
		/// Sends a mouse event for a specific button at the specific location
		/// </summary>
		/// <param name="buttonFlags">Button flags encoded in Cb mode.</param>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		public void SendEvent (int buttonFlags, int x, int y)
		{
			switch (MouseProtocol) {
			case MouseProtocolEncoding.X10:
				SendResponse (ControlCodes.CSI, "M", (byte)(buttonFlags + 32), (byte)Math.Min (255, (32 + x + 1)), (byte)Math.Min (255, (32 + y + 1)));
				break;
			case MouseProtocolEncoding.SGR:
				var bflags = ((buttonFlags & 3) == 3) ? (buttonFlags & ~3) : buttonFlags;
				var m = ((buttonFlags & 3) == 3) ? "m" : "M";
				SendResponse (ControlCodes.CSI, $"<{bflags};{x+1};{y+1}{m}");
				break;
			case MouseProtocolEncoding.URXVT:
				SendResponse (ControlCodes.CSI, $"{buttonFlags+32};{x+1};{y+1}M");
				break;
			case MouseProtocolEncoding.UTF8:
				var utf8 = new List<byte> () { 0x4d /* M */ };
				EncodeMouseUtf (utf8, ch: buttonFlags+32);
				EncodeMouseUtf (utf8, ch: x+33);
				EncodeMouseUtf (utf8, ch: y+33);
				SendResponse (ControlCodes.CSI, utf8.ToArray());
				break;
			}
		}

		public void SendMouseMotion (int buttonFlags, int x, int y)
		{
			SendEvent (buttonFlags + 32, x, y);

		}

		public int MatchColor (int r1, int g1, int b1)
		{
			throw new NotImplementedException ();
		}

		internal void EmitData (string txt)
		{
			DataEmitted?.Invoke (this, txt);
		}

		/// <summary>
		/// Implement to change the cursor style, call the base implementation.
		/// </summary>
		/// <param name="style"></param>
		public void SetCursorStyle (CursorStyle style)
		{
		}

		internal void ReverseIndex ()
		{
			var buffer = Buffer;

			if (buffer.Y == buffer.ScrollTop) {
				// possibly move the code below to term.reverseScroll();
				// test: echo -ne '\e[1;1H\e[44m\eM\e[0m'
				// blankLine(true) is xterm/linux behavior
				var scrollRegionHeight = buffer.ScrollBottom - buffer.ScrollTop;
				buffer.Lines.ShiftElements (buffer.Y + buffer.YBase, scrollRegionHeight, 1);
				buffer.Lines [buffer.Y + buffer.YBase] = buffer.GetBlankLine (EraseAttr ());
				UpdateRange (buffer.ScrollTop);
				UpdateRange (buffer.ScrollBottom);
			} else {
				buffer.Y--;
			}
		}


		#region Cursor Commands
		/// <summary>
		/// Sets the location of the cursor (zero based)
		/// </summary>
		public void SetCursor (int col, int row)
		{
			var buffer = Buffer;

			// make sure we stay within the boundaries
			col = Math.Min (Math.Max (col, 0), buffer.Cols - 1);
			row = Math.Min (Math.Max (row, 0), buffer.Rows - 1);

			if (OriginMode) {
				buffer.X = col + (IsUsingMargins () ? buffer.MarginLeft : 0);
				buffer.Y = buffer.ScrollTop + row;
			} else {
				buffer.X = col;
				buffer.Y = row;
			}
		}
		/// <summary>
		// Moves the cursor up by rows
		/// </summary>
		public void CursorUp (int rows)
		{
			var buffer = Buffer;
			var top = buffer.ScrollTop;

			if (buffer.Y < top) {
				top = 0;
			}

			if (buffer.Y - rows < top)
				buffer.Y = top;
			else
				buffer.Y -= rows;
		}

		/// <summary>
		// Moves the cursor down by rows
		/// </summary>
		public void CursorDown (int rows)
		{
			var buffer = Buffer;
			var bottom = buffer.ScrollBottom;

			// When the cursor starts below the scroll region, CUD moves it down to the
			// bottom of the screen.
			if (buffer.Y > bottom) {
				bottom = buffer.Rows - 1;
			}

			var newY = buffer.Y + rows;

			if (newY >= bottom)
				buffer.Y = bottom;
			else
				buffer.Y = newY;

			// If the end of the line is hit, prevent this action from wrapping around to the next line.
			if (buffer.X >= Cols)
				buffer.X--;
		}

		/// <summary>
		// Moves the cursor forward by cols
		/// </summary>
		public void CursorForward (int cols)
		{
			var buffer = Buffer;
			var right = MarginMode ? buffer.MarginRight : buffer.Cols - 1;

			if (buffer.X > right) {
				right = buffer.Cols - 1;
			}

			buffer.X += cols;
			if (buffer.X > right) {
				buffer.X = right;
			}
		}

		/// <summary>
		// Moves the cursor forward by cols
		/// </summary>
		public void CursorBackward (int cols)
		{
			var buffer = Buffer;

			// What is our left margin - depending on the settings.
			var left = MarginMode ? buffer.MarginLeft : 0;

			// If the cursor is positioned before the margin, we can go backwards to the first column
			if (buffer.X < left) {
				left = 0;
			}
			buffer.X -= cols;

			if (buffer.X < left) {
				buffer.X = left;
			}
		}

		/// <summary>
		/// Performs a backwards tab
		/// </summary>
		public void CursorBackwardTab (int tabs)
		{
			var buffer = Buffer;
			while (tabs-- != 0) {
				buffer.X = buffer.PreviousTabStop ();
			}
		}

		/// <summary>
		/// Moves the cursor to the given column
		/// </summary>
		public void CursorCharAbsolute (int col)
		{
			var buffer = Buffer;
			buffer.X = (IsUsingMargins () ? buffer.MarginLeft : 0) + Math.Min (col - 1, buffer.Cols - 1);
		}

		/// <summary>
		/// Performs a linefeed
		/// </summary>
		public void LineFeed ()
		{
			var buffer = Buffer;
			if (Options.ConvertEol) {
				buffer.X = MarginMode ? buffer.MarginLeft : 0;
			}

			LineFeedBasic ();
		}

		/// <summary>
		/// Performs a basic linefeed
		/// </summary>
		public void LineFeedBasic ()
		{
			var buffer = Buffer;
			var by = buffer.Y;

			if (by == buffer.ScrollBottom) {
				Scroll (isWrapped: false);
			} else if (by == buffer.Rows - 1) {
			} else {
				buffer.Y = by + 1;
			}

			// If the end of the line is hit, prevent this action from wrapping around to the next line.
			if (buffer.X >= buffer.Cols) {
				buffer.X -= 1;
			}

			// This event is emitted whenever the terminal outputs a LF or NL.
			EmitLineFeed ();
		}

		/// <summary>
		/// Moves cursor to first position on next line.
		/// </summary>
		public void NextLine ()
		{
			var buffer = Buffer;
			buffer.X = IsUsingMargins () ? buffer.MarginLeft : 0;
			Index ();
		}

		/// <summary>
		/// Save cursor (ANSI.SYS).
		/// </summary>
		public void SaveCursor ()
		{
			var buffer = Buffer;
			buffer.SaveCursor (CurAttr);
		}

		/// <summary>
		/// Restores the cursor and modes
		/// </summary>
		public void RestoreCursor ()
		{
			var buffer = Buffer;
			CurAttr = buffer.RestoreCursor();
			MarginMode = savedMarginMode;
			OriginMode = savedOriginMode;
			Wraparound = savedWraparound;
			ReverseWraparound = savedReverseWraparound;
		}

		/// <summary>
		/// Restrict cursor to viewport size / scroll margin (origin mode)
		/// - Parameter limitCols: by default it is true, but the reverseWraparound mechanism in Backspace needs `x` to go beyond.
		/// </summary>
		public void RestrictCursor (bool limitCols = true)
		{
			var buffer = Buffer;
			buffer.X = Math.Min (buffer.Cols - (limitCols ? 1 : 0), Math.Max (0, buffer.X));
			buffer.Y = OriginMode
				? Math.Min (buffer.ScrollBottom, Math.Max (buffer.ScrollTop, buffer.Y))
				: Math.Min (buffer.Rows - 1, Math.Max (0, buffer.Y));

			UpdateRange (buffer.Y);
		}

		/// <summary>
		/// Returns true if the terminal is using margins in origin mode
		/// </summary>
		internal bool IsUsingMargins ()
		{
			return OriginMode && MarginMode;
		}

		#endregion

		/// <summary>
		/// Performs a carriage return
		/// </summary>
		public void CarriageReturn ()
		{
			var buffer = Buffer;
			if (MarginMode) {
				if (buffer.X < buffer.MarginLeft) {
					buffer.X = 0;
				} else {
					buffer.X = buffer.MarginLeft;
				}
			} else {
				buffer.X = 0;
			}
		}

		#region Text Manupulation
		/// <summary>
		/// Backspace handler (Control-h)
		/// </summary>
		public void Backspace ()
		{
			var buffer = Buffer;

			RestrictCursor (!ReverseWraparound);

			int left = MarginMode ? buffer.MarginLeft : 0;
			int right = MarginMode ? buffer.MarginRight : buffer.Cols - 1;

			if (buffer.X > left) {
				buffer.X--;
			} else if (ReverseWraparound) {
				if (buffer.X <= left) {
					if (buffer.Y > buffer.ScrollTop && buffer.Y <= buffer.ScrollBottom && (buffer.Lines [buffer.Y + buffer.YBase].IsWrapped || MarginMode)) {
						if (!MarginMode) {
							buffer.Lines [buffer.Y + buffer.YBase].IsWrapped = false;
						}

						buffer.Y--;
						buffer.X = right;
						// TODO: find actual last cell based on width used
					} else if (buffer.Y == buffer.ScrollTop) {
						buffer.X = right;
						buffer.Y = buffer.ScrollBottom;
					} else if (buffer.Y > 0) {
						buffer.X = right;
						buffer.Y--;
					}
				}
			} else {
				if (buffer.X < left && buffer.X > 0) {
					// This compensates for the scenario where backspace is supposed to move one step
					// backwards if the "x" position is behind the left margin.
					// Test BS_MovesLeftWhenLeftOfLeftMargin
					buffer.X--;
				} else if (buffer.X > left) {
					// If we have not reached the limit, we can go back, otherwise stop at the margin
					// Test BS_StopsAtLeftMargin
					buffer.X--;

				}
			}
		}

		/// <summary>
		/// Deletes charstoDelete chars from the cursor position to the right margin
		/// </summary>
		public void DeleteChars (int charsToDelete)
		{
			var buffer = Buffer;

			if (MarginMode) {
				if (buffer.X + charsToDelete > buffer.MarginRight) {
					charsToDelete = buffer.MarginRight - buffer.X;
				}
			}

			buffer.Lines [buffer.Y + buffer.YBase].DeleteCells (buffer.X, charsToDelete, MarginMode ? buffer.MarginRight : buffer.Cols - 1, new CharData (EraseAttr ()));

			UpdateRange (buffer.Y);
		}

		/// <summary>
		/// Deletes lines
		/// </summary>
		public void DeleteLines (int rowsToDelete)
		{
			RestrictCursor ();
			var buffer = Buffer;
			var row = buffer.Y + buffer.YBase;

			int j;
			j = buffer.Rows - 1 - buffer.ScrollBottom;
			j = buffer.Rows - 1 + buffer.YBase - j;

			var eraseAttr = EraseAttr ();

			if (MarginMode) {
				if (buffer.X >= buffer.MarginLeft && buffer.X <= buffer.MarginRight) {
					var columnCount = buffer.MarginRight - buffer.MarginLeft + 1;
					var rowCount = buffer.ScrollBottom - buffer.ScrollTop;
					while (rowsToDelete-- > 0) {
						for (int i = 0; i < rowCount; i++) {
							var src = buffer.Lines [row + i + 1];
							var dst = buffer.Lines [row + i];

							if (src != null) {
								dst.CopyFrom (src, buffer.MarginLeft, buffer.MarginLeft, columnCount);
							}
						}

						var last = buffer.Lines [row + rowCount];
						last?.Fill (new CharData (eraseAttr), atCol: buffer.MarginLeft, len: columnCount);
					}
				}
			} else {
				if (buffer.Y >= buffer.ScrollTop && buffer.Y <= buffer.ScrollBottom) {
					while (rowsToDelete-- > 0) {
						buffer.Lines.Splice (row, 1);
						buffer.Lines.Splice (j, 0, buffer.GetBlankLine (eraseAttr));
					}
				}
			}

			UpdateRange (buffer.Y);
			UpdateRange (buffer.ScrollBottom);
		}

		/// <summary>
		/// Inserts columns
		/// </summary>
		public void InsertColumn (int columns)
		{
			var buffer = Buffer;

			for (int row = buffer.ScrollTop; row < buffer.ScrollBottom; row++) {
				var line = buffer.Lines [row + buffer.YBase];
				// TODO:is this the right filldata?
				line.InsertCells (buffer.X, columns, MarginMode ? buffer.MarginRight : buffer.Cols - 1, CharData.WhiteSpace);
				line.IsWrapped = false;
			}

			UpdateRange (buffer.ScrollTop);
			UpdateRange (buffer.ScrollBottom);
		}

		/// <summary>
		/// Deletes columns
		/// </summary>
		public void DeleteColumn (int columns)
		{
			var buffer = Buffer;

			if (buffer.Y > buffer.ScrollBottom || buffer.Y < buffer.ScrollTop)
				return;

			for (int row = buffer.ScrollTop; row < buffer.ScrollBottom; row++) {
				var line = buffer.Lines [row + buffer.YBase];
				line.DeleteCells (buffer.X, columns, MarginMode ? buffer.MarginRight : buffer.Cols - 1, CharData.Null);
				line.IsWrapped = false;
			}

			UpdateRange (buffer.ScrollTop);
			UpdateRange (buffer.ScrollBottom);
		}



		#endregion

		/// <summary>
		/// Sets the scroll region
		/// </summary>
		public void SetScrollRegion (int top, int bottom)
		{
			var buffer = Buffer;

			if (bottom == 0)
				bottom = buffer.Rows;
			bottom = Math.Min (bottom, buffer.Rows);

			// normalize (make zero based)
			bottom--;

			// only set the scroll region if top < bottom
			if (top < bottom) {
				buffer.ScrollBottom = bottom;
				buffer.ScrollTop = top;
			}

			SetCursor (0, 0);
		}


		/// <summary>
		/// Performs a soft reset
		/// </summary>
		public void SoftReset ()
		{
			var buffer = Buffer;

			CursorHidden = false;
			InsertMode = false;
			OriginMode = false;

			Wraparound = true;  // defaults: xterm - true, vt100 - false
			ReverseWraparound = false;
			ApplicationKeypad = false;
			SyncScrollArea ();
			ApplicationCursor = false;
			CurAttr = CharData.DefaultAttr;

			Charset = null;
			SetgLevel (0);

			savedOriginMode = false;
			savedMarginMode = false;
			savedWraparound = false;
			savedReverseWraparound = false;

			buffer.ScrollTop = 0;
			buffer.ScrollBottom = buffer.Rows - 1;
			buffer.SavedAttr = CharData.DefaultAttr;
			buffer.SavedY = 0;
			buffer.SavedX = 0;
			buffer.SetMargins (0, buffer.Cols - 1);
			//conformance = .vt500
		}


		/// <summary>
		/// Reports a message to the system log
		/// </summary>
		void Report (string prefix, string text, object [] args)
		{
			Console.WriteLine ($"{prefix}: {text}");
			for (int i = 0; i < args.Length; i++)
				Console.WriteLine ("    {0}: {1}", i, args [i]);
		}

		/// <summary>
		/// Sets up the terminals initial state
		/// </summary>
		void Setup ()
		{
			cursorHidden = false;

			// modes
			applicationKeypad = false;
			applicationCursor = false;
			OriginMode = false;
			MarginMode = false;
			InsertMode = false;
			Wraparound = true;
			bracketedPasteMode = false;

			// charset
			charset = null;
			gcharset = 0;
			gLevel = 0;

			CurAttr = CharData.DefaultAttr;

			MouseMode = MouseMode.Off;
			MouseProtocol = MouseProtocolEncoding.X10;

			Allow80To132 = false;
			// TODO REST
		}
	}
}
