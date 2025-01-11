using System;
using System.Collections;
using System.Diagnostics;
using NStack;

namespace XtermSharp {
	/// <summary>
	/// This class represents a terminal buffer (an internal state of the terminal), where text contents, cursor and scroll position are stored.
	/// </summary>
	[DebuggerDisplay ("({X},{Y}) YD={YDisp}:YB={YBase} Scroll={ScrollBottom,ScrollTop}")]
	public class Buffer {
		CircularList<BufferLine> lines;
		int scrollTop;
		int scrollBottom;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:XtermSharp.Buffer"/> class.
		/// </summary>
		/// <param name="terminal">The terminal the Buffer will belong to.</param>
		/// <param name="hasScrollback">Whether the buffer should respect the scrollback of the terminal.</param>
		public Buffer (Terminal terminal, bool hasScrollback = true)
		{
			rows = terminal.Rows;
			cols = terminal.Cols;
			MarginLeft = 0;
			MarginRight = cols - 1;
			Terminal = terminal;
			this.hasScrollback = hasScrollback;
			Clear ();
		}

		/// <summary>
		/// Gets the number of columns of the buffer
		/// </summary>
		public int Cols => cols;

		/// <summary>
		/// Gets the number of rows of the buffer
		/// </summary>
		public int Rows => rows;

		/// <summary>
		/// Gets or sets the top scrolling region in the buffer when Origin Mode is turned on
		/// </summary>
		public int ScrollTop {
			get {
				return scrollTop;
			}
			set {
				if(value >= 0) {
					scrollTop = value;
				}
			}
		}

		/// <summary>
		/// Gets or sets the top scrolling region in the buffer when Origin Mode is turned on
		/// </summary>
		public int ScrollBottom {
			get {
				return scrollBottom;
			}
			set {
				if (value < 0)
					throw new ArgumentException ("Scroll bottom cannot be less than zero", nameof(ScrollBottom));

				scrollBottom = value;
			}
		}

		/// <summary>
		/// Gets or sets the left margin, 0 based
		/// </summary>
		public int MarginLeft { get; private set; }

		/// <summary>
		/// Gets or sets the right margin, 0 based
		/// </summary>
		public int MarginRight { get; private set; }

		public int YDisp, YBase;
		public int X;
		int y;
		public int Y {
			get => y;
			set {
				if (value < 0 || value > Terminal.Rows - 1)
					throw new ArgumentException ("Y cannot be outside the bounds of the terminal rows", nameof(Y));
				else
					y = value;
			}
		}
		BitArray tabStops;
		public int SavedX, SavedY, SavedAttr = CharData.DefaultAttr;
		public Terminal Terminal { get; private set; }
		bool hasScrollback;
		int cols, rows;

		/// <summary>
		/// Gets a value indicating whether this <see cref="T:XtermSharp.Buffer"/> has scrollback.
		/// </summary>
		/// <value><c>true</c> if has scrollback; otherwise, <c>false</c>.</value>
		public bool HasScrollback => hasScrollback && lines.MaxLength > Terminal.Rows;

		public CircularList<BufferLine> Lines => lines;

		// Gets the correct buffer length based on the rows provided, the terminal's
		// scrollback and whether this buffer is flagged to have scrollback or not.
		int getCorrectBufferLength (int rows)
		{
			if (!hasScrollback)
				return rows;
			var correct = rows + Terminal.Options.Scrollback ?? 0;
			return correct > Int32.MaxValue ? Int32.MaxValue : correct;
		}

		/// <summary>
		/// Returns the CharData at the specified position in the buffer
		/// </summary>
		public CharData GetChar (int col, int row)
		{
			var bufferRow = lines [row];
			if (bufferRow == null) {
				return CharData.Null;
			}

			if (col >= bufferRow.Length || col < 0)
				return CharData.Null;

			return bufferRow [col];
		}

		public BufferLine GetBlankLine (int attribute, bool isWrapped = false)
		{
			var cd = new CharData (attribute);

			return new BufferLine (Terminal.Cols, cd, isWrapped);
		}

		/// <summary>
		/// Clears the buffer to it's initial state, discarding all previous data.
		/// </summary>
		public void Clear ()
		{
			YDisp = 0;
			YBase = 0;
			X = 0;
			Y = 0;
			lines = new CircularList<BufferLine> (getCorrectBufferLength (Terminal.Rows));
			ScrollTop = 0;
			ScrollBottom = Terminal.Rows - 1;
			SetupTabStops ();
		}

		public bool IsCursorInViewport {
			get {
				var absoluteY = YBase + Y;
				var relativeY = absoluteY - YDisp;
				return (relativeY >= 0 && relativeY < Terminal.Rows);
			}
		}

		/// <summary>
		/// Sets the left and right margins
		/// </summary>
		public void SetMargins (int left, int right)
		{
			left = Math.Min (left, right);
			MarginLeft = left;
			MarginRight = right;
		}

		/// <summary>
		/// Saves the cursor position
		/// </summary>
		public void SaveCursor (int curAttr)
		{
			SavedX = X;
			SavedY = Y;
			SavedAttr = curAttr;
		}

		/// <summary>
		/// Restores the cursor
		/// </summary>
		public int RestoreCursor ()
		{
			X = SavedX;
			Y = SavedY;
			return SavedAttr;
		}

		/// <summary>
		/// Fills the buffer's viewport with blank lines.
		/// </summary>
		public void FillViewportRows (int? attribute = null)
		{
			// TODO: limitation in original, this does not cope with partial fills, it is either zero or nothing
			if (lines.Length != 0)
				return;
			var attr = attribute.HasValue ? attribute.Value : CharData.DefaultAttr;
			for (int i = Terminal.Rows; i > 0; i--)
				lines.Push (GetBlankLine (attr));
		}

		bool IsReflowEnabled => hasScrollback;// && Terminal.Options.WindowsMode;

		/// <summary>
		/// Resize the buffer, adjusting its data accordingly
		/// </summary>
		/// <returns>The resize.</returns>
		/// <param name="newCols">New columns.</param>
		/// <param name="newRows">New rows.</param>
		public void Resize (int newCols, int newRows)
		{
			var newMaxLength = getCorrectBufferLength (newRows);
			if (newMaxLength > lines.MaxLength) {
				lines.MaxLength = newMaxLength;
			}

			if (this.lines.Length > 0) {
				// Deal with columns increasing (reducing needs to happen after reflow)
				if (cols < newCols) {
					for (int i = 0; i < lines.MaxLength; i++) {
						lines [i]?.Resize (newCols, CharData.Null);
					}
				}

				// Resize rows in both directions as needed
				int addToY = 0;
				if (rows < newRows) {
					for (int y = rows; y < newRows; y++) {
						if (lines.Length < newRows + YBase) {
							//if (Terminal.Options.windowsMode) {
							//	// Just add the new missing rows on Windows as conpty reprints the screen with it's
							//	// view of the world. Once a line enters scrollback for conpty it remains there
							//	lines.Push (new BufferLine (newCols, CharData.Null));
							//} else {
							{
								if (YBase > 0 && lines.Length <= YBase + Y + addToY + 1) {
									// There is room above the buffer and there are no empty elements below the line,
									// scroll up
									YBase--;
									addToY++;
									if (YDisp > 0) {
										// Viewport is at the top of the buffer, must increase downwards
										YDisp--;
									}
								} else {
									// Add a blank line if there is no buffer left at the top to scroll to, or if there
									// are blank lines after the cursor
									lines.Push (new BufferLine (newCols, CharData.Null));
								}
							}
						}
					}
				} else { // (this._rows >= newRows)
					for (int y = rows; y > newRows; y--) {
						if (lines.Length > newRows + YBase) {
							if (lines.Length > YBase + this.y + 1) {
								// The line is a blank line below the cursor, remove it
								lines.Pop ();
							} else {
								// The line is the cursor, scroll down
								YBase++;
								YDisp++;
							}
						}
					}
				}

				// Reduce max length if needed after adjustments, this is done after as it
				// would otherwise cut data from the bottom of the buffer.
				if (newMaxLength < lines.MaxLength) {
					// Trim from the top of the buffer and adjust ybase and ydisp.
					int amountToTrim = lines.Length - newMaxLength;
					if (amountToTrim > 0) {
						lines.TrimStart (amountToTrim);
						YBase = Math.Max (YBase - amountToTrim, 0);
						YDisp = Math.Max (YDisp - amountToTrim, 0);
						SavedY = Math.Max (SavedY - amountToTrim, 0);
					}

					lines.MaxLength = newMaxLength;
				}

				// Make sure that the cursor stays on screen
				X = Math.Min (X, newCols - 1);
				Y = Math.Min (Y, newRows - 1);
				if (addToY != 0) {
					Y += addToY;
				}

				SavedX = Math.Min (SavedX, newCols - 1);

				ScrollTop = 0;
			}

			ScrollBottom = newRows - 1;

			if (IsReflowEnabled) {
				this.Reflow (newCols, newRows);

				// Trim the end of the line off if cols shrunk
				if (cols > newCols) {
					for (int i = 0; i < lines.MaxLength; i++) {
						lines [i]?.Resize (newCols, CharData.Null);
					}
				}
			}

			rows = newRows;
			cols = newCols;
			if (MarginRight > newCols - 1)
				MarginRight = newCols - 1;
			if (MarginLeft > MarginRight)
				MarginLeft = MarginRight;
		}

		/// <summary>
		/// Translates a buffer line to a string, with optional start and end columns.   Wide characters will count as two columns in the resulting string. This 
		/// function is useful for getting the actual text underneath the raw selection position.
		/// </summary>
		/// <returns>The buffer line to string.</returns>
		/// <param name="lineIndex">The line being translated.</param>
		/// <param name="trimRight">If set to <c>true</c> trim whitespace to the right.</param>
		/// <param name="startCol">The column to start at.</param>
		/// <param name="endCol">The column to end at.</param>
		public ustring TranslateBufferLineToString (int lineIndex, bool trimRight, int startCol = 0, int endCol = -1)
		{
			try {
				var line = lines [lineIndex];

				return line.TranslateToString (trimRight, startCol, endCol);
			} catch ( Exception ex)
			{
				return ustring.Empty;
			}
		}

		/// <summary>
		/// Setups the tab stops.
		/// </summary>
		/// <param name="index">Index to start setting tabs stops from.</param>
		public void SetupTabStops (int index = -1)
		{
			if (index != -1 && tabStops != null) {
				tabStops.Length = cols;

				var from = Math.Min (index, cols - 1);
				if (!tabStops [from])
					index = PreviousTabStop (from);
			} else {
				tabStops = new BitArray (cols);
				index = 0;
			}

			int tabStopWidth = Terminal.Options.TabStopWidth ?? 8;
			for (int i = index; i < cols; i += tabStopWidth)
				tabStops [i] = true;
		}

		public void TabSet (int pos)
		{
			if (pos < tabStops.Length)
				tabStops [pos] = true;
		}

		public void ClearStop (int pos)
		{
			if (pos < tabStops.Length)
				tabStops [pos] = false;
		}

		public void ClearTabStops ()
		{
			tabStops = new BitArray (tabStops.Count);
		}

		/// <summary>
		/// Move the cursor to the previous tab stop from the given position (default is current).
		/// </summary>
		/// <returns>The tab stop.</returns>
		/// <param name="index">The position to move the cursor to the previous tab stop.</param>
		public int PreviousTabStop (int index = -1)
		{
			if (index == -1)
				index = X;
			while (index > 0 && !tabStops [--index])
				;

			return index >= Cols ? Cols - 1 : index;
		}

		/// <summary>
		/// Move the cursor one tab stop forward from the given position (default is current).
		/// </summary>
		/// <returns>The tab stop.</returns>
		/// <param name="index">The position to move the cursor one tab stop forward.</param>
		public int NextTabStop (int index = -1)
		{
			// Users marginMode because apparently for tabs, there is no need to have originMode set
			var limit = Terminal.MarginMode ? MarginRight : (Cols - 1);
			if (index == -1)
				index = X;

			do {
				index++;
				if (index > limit)
					break;

				if (tabStops [index])
					break;
			} while (index < limit);

			return index >= limit ? limit : index;
		}

		void Reflow (int newCols, int newRows)
		{
			if (cols == newCols) {
				return;
			}

			// Iterate through rows, ignore the last one as it cannot be wrapped
			ReflowStrategy strategy;
			if (newCols > cols) {
				strategy = new ReflowWider (this);
			} else {
				strategy = new ReflowNarrower (this);
			}

			strategy.Reflow (newCols, newRows, cols, rows);
		}
	}
}
