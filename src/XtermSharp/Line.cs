using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace XtermSharp {
	/// <summary>
	/// Gets a line of text that consists of one or more fragments
	/// </summary>
	[DebuggerDisplay ("{DebuggerDisplay}")]
	public class Line {
		readonly List<LineFragment> fragments;

		public Line ()
		{
			fragments = new List<LineFragment> ();
		}

		/// <summary>
		/// Gets the line number of the first fragment. 
		/// </summary>
		public int StartLine {
			get {
				if (fragments.Count > 0) {
					return fragments [0].Line;
				}

				return 0;
			}
		}

		/// <summary>
		/// Gets the location of the first fragment. 
		/// </summary>
		public int StartLocation {
			get {
				if (fragments.Count > 0) {
					return fragments [0].Location;
				}

				return 0;
			}
		}

		/// <summary>
		/// Gets the length of the line
		/// </summary>
		public int Length { get; private set; }

		string DebuggerDisplay {
			get {
				if (fragments.Count < 1) {
					return "[]";
				}

				var sb = new StringBuilder ();
				sb.Append ($"{fragments.Count}/{Length} : [");
				for (int i = 0; i < fragments.Count; i++) {
					if (fragments [i].Text == "\n") {
						sb.Append ("\\n");
					} else {
						sb.Append (fragments [i].Text);
					}

					if (i < fragments.Count - 1)
						sb.Append ("][");
				}
				sb.Append ("]");

				return sb.ToString ();
			}
		}

		public void Add (LineFragment fragment)
		{
			fragments.Add (fragment);

			Length += fragment.Length;
		}

		public void GetFragmentStrings (StringBuilder builder)
		{
			foreach (var fragment in fragments) {
				builder.Append (fragment.Text);
			}
		}


		/// <summary>
		/// For a given line, find the fragment with the  position
		/// </summary>
		public int GetFragmentIndexForPosition (int pos)
		{
			int count = 0;
			for (int i = 0; i < fragments.Count; i++) {
				count += fragments [i].Length;
				if (count > pos) {
					return i;
				}
			}

			return fragments.Count - 1;
		}

		public LineFragment GetFragment (int index)
		{
			return fragments [index];
		}


		public override string ToString ()
		{
			var sb = new StringBuilder ();
			foreach (var fragment in fragments) {
				sb.Append (fragment.Text);
			}

			return sb.ToString ();
		}
	}
}
