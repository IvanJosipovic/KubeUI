namespace XtermSharp {
	public static class MouseModeExensions {
		/// <summary>
		/// Returns true if you should send a button press event (separate from release)
		/// </summary>
		public static bool SendButtonPress (this MouseMode mode)
		{
			return mode == MouseMode.VT200 || mode == MouseMode.ButtonEventTracking || mode == MouseMode.AnyEvent;
		}

		/// <summary>
		/// Returns true if you should send the button release event
		/// </summary>
		public static bool SendButtonRelease (this MouseMode mode)
		{
			return mode != MouseMode.Off;
		}

		/// <summary>
		/// Returns true if you should send a motion event when a button is pressed
		/// </summary>
		public static bool SendButtonTracking (this MouseMode mode)
		{
			return mode == MouseMode.ButtonEventTracking || mode == MouseMode.AnyEvent;
		}

		/// <summary>
		/// Returns true if you should send a motion event, regardless of button state
		/// </summary>
		public static bool SendMotionEvent (this MouseMode mode)
		{
			return mode == MouseMode.AnyEvent;
		}

		/// <summary>
		/// Returns true if the modifiers should be encoded
		/// </summary>
		public static bool SendsModifiers (this MouseMode mode)
		{
			return mode == MouseMode.VT200 || mode == MouseMode.ButtonEventTracking || mode == MouseMode.AnyEvent;
		}
	}
}
