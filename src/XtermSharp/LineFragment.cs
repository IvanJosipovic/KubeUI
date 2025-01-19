using System.Diagnostics;

namespace XtermSharp {
	/// <summary>
	/// Represents a fragment of text from a line. A fragment is wholly enclosed
	/// in a buffer line.
	/// </summary>
	[DebuggerDisplay ("{DebuggerDisplay}")]
	public class LineFragment {
		public LineFragment (string text, int line, int location)
		{
			Text = text ?? string.Empty;
			Line = line;
			Location = location;
			Length = Text.Length;
		}

		/// <summary>
		/// Gets the line in the buffer in which this fragment exists
		/// </summary>
		public int Line { get; }

		/// <summary>
		/// Gets the position in the buffer line where this fragment starts
		/// </summary>
		public int Location { get; }

		/// <summary>
		/// Gets the text representation of this fragment
		/// </summary>
		public string Text { get; }

		/// <summary>
		/// Gets the length of the text fragment
		/// </summary>
		public int Length { get; }

		string DebuggerDisplay {
			get {
				if (Text == "\n") {
					return $"{Line}:{Location}:\\n";
				}

				return $"{Line}:{Location}:{Text}";
			}
		}

		public static LineFragment NewLine (int line)
		{
			// TOOD: is this location correct or useful?
			return new LineFragment ("\n", line, -1);
		}
	}
}
