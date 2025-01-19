//
// Note: does not handle combined, as this code uses Runes, rather than Utf16 encoded chars
//
using System;
using System.Collections.Generic;
using System.Diagnostics;
using NStack;

namespace XtermSharp {
	[DebuggerDisplay ("Line: {DebuggerDisplay}")]
	public class BufferLine {
		CharData [] data = Array.Empty<CharData> ();
		public int Length => data.Length;
		public bool IsWrapped;

		public BufferLine (int cols, CharData? fillCharData, bool isWrapped = false)
		{
			var fill = fillCharData ?? CharData.Null;

			data = new CharData [cols];
			for (int i = 0; i < cols; i++)
				data [i] = fill;
			this.IsWrapped = isWrapped;
		}

		public BufferLine (BufferLine other)
		{
			data = new CharData [other.data.Length];
			other.data.CopyTo (data, 0);
			IsWrapped = other.IsWrapped;
		}

		public CharData this [int idx] {
			get => data [idx];
			set {
				data [idx] = value;
			}
		}

		public int GetWidth (int index) => data [index].Width;

		/**
		   * Test whether contains any chars.
		   * Basically an empty has no content, but other cells might differ in FG/BG
		   * from real empty cells.
		   * */
		   // TODO: not sue this is completely right
		public bool HasContent (int index) => !data [index].IsNullChar() || data [index].Attribute != CharData.DefaultAttr;

		public bool HasAnyContent()
		{
			for (int i = 0; i < data.Length; i++) {
				if (HasContent (i)) return true;
			}

			return false;
		}

		string DebuggerDisplay {
			get {
				return TranslateToString (true, 0, -1).ToString ();
			}
		}

		public void InsertCells (int pos, int n, int rightMargin, CharData fillCharData)
		{
			var len = Math.Min (rightMargin + 1, Length);
			pos = pos % len;
			if (n < len - pos) {
				for (var i = len - pos - n - 1; i >= 0; --i)
					data [pos + n + i] = data [pos + i];
				for (var i = 0; i < n; i++)
					data [pos + i] = fillCharData;
			} else {
				for (var i = pos; i < len; ++i)
					data [i] = fillCharData;
			}
		}

		public void DeleteCells (int pos, int n, int rightMargin, CharData fillCharData)
		{
			var len = Math.Min(rightMargin + 1, Length);
			pos %= len;
			if (n < len - pos) {
				for (var i = 0; i < len - pos - n; ++i)
					data [pos + i] = this [pos + n + i];
				for (var i = len - n; i < len; ++i)
					data [i] = fillCharData;
			} else {
				for (var i = pos; i < len; ++i)
					data [i] = fillCharData;
			}
		}

		public void ReplaceCells (int start, int end, CharData fillCharData)
		{
			var len = Length;

			while (start < end && start < len)
				data [start++] = fillCharData;
		}
	
		public void Resize (int cols, CharData fillCharData)
		{
			var len = Length;
			if (cols == len)
				return;

			if (cols > len) {
				var newData = new CharData [cols];
				if (len > 0)
					data.CopyTo (newData, 0);
				data = newData;
				for (int i = len; i < cols; i++)
					data [i] = fillCharData;
			} else {
				if (cols > 0) {
					var newData = new CharData [cols];
					Array.Copy (data, newData, cols);
					data = newData;
				} else {
					data = Array.Empty<CharData> ();
				}
			}
		}

		/// <summary>
		/// Fills the line with fillCharData values
		/// </summary>
		public void Fill (CharData fillCharData)
		{
			var len = Length;
			for (int i = 0; i < len; i++)
				data [i] = fillCharData;
		}

		/// <summary>
		/// Fills the line with len fillCharData values from the given atCol
		/// </summary>
		public void Fill (CharData fillCharData, int atCol, int len)
		{
			for (int i = 0; i < len; i++)
				data [atCol + i] = fillCharData;
		}

		public void CopyFrom (BufferLine line)
		{
			if (data.Length != line.Length) 
				data = new CharData [line.Length];
			
			line.data.CopyTo (data, 0);

			IsWrapped = line.IsWrapped;
		}

		/// <summary>
		/// Copies a subrange the given source line into the current line
		/// </summary>
		public void CopyFrom (BufferLine source, int sourceCol, int destCol, int len)
		{
			Array.Copy (source.data, sourceCol, data, destCol, len);
		}

		public int GetTrimmedLength ()
		{
			for (int i = data.Length - 1; i >= 0; --i)
				if (!data [i].IsNullChar()) {
					int width = 0;
					for (int j = 0; j <= i; j++)
						width += data [i].Width;
					return width;
				}
			return 0;
		}

		public void CopyCellsFrom (BufferLine src, int srcCol, int dstCol, int len)
		{
			Array.Copy (src.data, srcCol, data, dstCol, len); 
		}

		public ustring TranslateToString (bool trimRight = false, int startCol = 0, int endCol = -1)
		{
			if (endCol == -1)
				endCol = data.Length;
			if (trimRight) {
				// make sure endCol is not before startCol if we set it to the trimmed length
				endCol = Math.Max (Math.Min (endCol, GetTrimmedLength ()), startCol);
			}

			Rune [] runes = new Rune [endCol - startCol];
			for (int i = startCol; i < endCol; i++)
				runes [i - startCol] = data [i].Rune;

			return ustring.Make (runes);
		}
	}
}
