namespace XtermSharp {
	/// <summary>
	/// Represents the mouse operation mode that the terminal is currently using and higher level
	/// implementations should use the functions in this enumeration to determine what events to
	/// send
	/// </summary>
	public enum MouseMode {
		/// <summary>
		/// </summary>
		/// No mouse events are reported
		Off,

		/// <summary>
		/// X10 Compatibility mode - only sends events in button press
		/// </summary>
		X10,

		/// <summary>
		/// VT200, also known as Normal Tracking Mode - sends both press and release events
		/// </summary>
		VT200,

		/// <summary>
		/// ButtonEventTracking - In addition to sending button press and release events, it sends motion events when the button is pressed
		/// </summary>
		ButtonEventTracking,

		/// <summary>
		/// Sends button presses, button releases, and motion events regardless of the button state
		/// </summary>
		AnyEvent

		// Unsupported modes:
		// - vt200Highlight, this can deadlock the terminal
		// - declocator, rarely used
	}
}
