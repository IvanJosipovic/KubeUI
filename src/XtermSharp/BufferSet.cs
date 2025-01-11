//
// At: 857ae4b702b17381f6b862909a3570a6c3ab30b4
//
using System;
namespace XtermSharp {

	/// <summary>
	/// The BufferSet represents the set of two buffers used by xterm terminals (normal and alt) and
 	/// provides also utilities for working with them.
	/// </summary>
	public class BufferSet {
		public Buffer Normal { get; private set; }
		public Buffer Alt { get; private set; }
		public Buffer Active { get; private set; }

		public BufferSet (Terminal terminal)
		{
			Normal = new Buffer (terminal, hasScrollback: true);
			Normal.FillViewportRows ();

			// The alt buffer should never have scrollback.
			// See http://invisible-island.net/xterm/ctlseqs/ctlseqs.html#h2-The-Alternate-Screen-Buffer
			Alt = new Buffer (terminal, hasScrollback: false);

			Active = Normal;
			SetupTabStops ();
		}

		/// <summary>
		/// Gets a value indicating whether the active buffer is the alternate buffer
		/// </summary>
		public bool IsAlternateBuffer => Active == Alt;

		/// <summary>
		/// Raised when a buffer is activated, the parameters is the buffer that was activated and the second parameter is the Inactive buffer.
		/// </summary>
		public event Action<Buffer, Buffer> Activated;

		/// <summary>
		/// Sets the normal Buffer of the BufferSet as its currently active Buffer
		/// </summary>
		public void ActivateNormalBuffer (bool clearAlt)
		{
			if (Active == Normal)
				return;

			Normal.X = Alt.X;
			Normal.Y = Alt.Y;

			// The alt buffer should always be cleared when we switch to the normal
			// buffer. This frees up memory since the alt buffer should always be new
			// when activated.

			if (clearAlt) {
				Alt.Clear ();
			}

			Active = Normal;
			Activated?.Invoke (Normal, Alt);
		}

		/// <summary>
		/// Sets the alt Buffer of the BufferSet as its currently active Buffer
		/// </summary>
		/// <param name="fillAttr">Attribute to fill the screen with</param>
		public void ActivateAltBuffer (int? fillAttr)
		{
			if (Active == Alt)
				return;
			Alt.X = Normal.X;
			Alt.Y = Normal.Y;
			// Since the alt buffer is always cleared when the normal buffer is
			// activated, we want to fill it when switching to it.

			Alt.FillViewportRows (fillAttr);
			Active = Alt;
			if (Activated != null)
				Activated (Alt, Normal);
		}

		/// <summary>
		/// Resizes both normal and alt buffers, adjusting their data accordingly.
		/// </summary>
		/// <returns>The resize.</returns>
		/// <param name="newColumns">The new number of columns.</param>
		/// <param name="newRows">The new number of rows.</param>
		public void Resize (int newColumns, int newRows)
		{
			Normal.Resize (newColumns, newRows);
			Alt.Resize (newColumns, newRows);
		}

		/// <summary>
		/// Setups the tab stops.
		/// </summary>
		/// <param name="index">The index to start setting up tab stops from, or -1 to do it from the start.</param>
		public void SetupTabStops (int index = -1)
		{
			Normal.SetupTabStops (index);
			Alt.SetupTabStops (index);
		}

	}
}
