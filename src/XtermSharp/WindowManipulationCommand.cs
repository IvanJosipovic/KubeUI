using System;
namespace XtermSharp {
        /// <summary>
        /// Commands send to the `windowCommand` delegate for the front-end to implement capabilities
        /// on behalf of the client.The expected return strings in some of these enumeration values is documented
        /// below.   Returns are only expected for the enum values that start with the prefix `report`
        /// </summary>
        public enum WindowManipulationCommand {
                /// <summary>
                /// Raised when the backend should deiconify a window, no return expected
		/// </summary>
                DeiconifyWindow,

                /// <summary>
                /// Raised when the backend should iconify  a window, no return expected
		/// </summary>
                IconifyWindow,

                /// <summary>
                /// Raised when the client would like the window to be moved to the x,y position int he screen, no return expected
		/// </summary>
		/// <remarks>(x: Int, y: Int)</remarks>
                MoveWindowTo,

                /// <summary>
                /// Raised when the client would like the window to be resized to the specified widht and heigh in pixels, no return expected
		/// </summary>
		/// <remarks>(width: Int, height: Int)</remarks>
                ResizeWindowTo,

                /// <summary>
                /// Raised to bring the terminal to the front
		/// </summary>
                BringToFront,

                /// <summary>
                /// Send the terminal to the back if possible
		/// </summary>
                SendToBack,

                /// <summary>
                /// Trigger a terminal refresh
		/// </summary>
                RefreshWindow,

                /// <summary>
                /// Request that the size of the terminal be changed to the specified cols and rows
		/// </summary>
		/// <remarks>(cols: Int, rows: Int)</remarks>
                ResizeTo,

                RestoreMaximizedWindow,

                /// <summary>
                /// Attempt to maximize the window
		/// </summary>
                MaximizeWindow,

                /// <summary>
                /// Attempt to maximize the window vertically
		/// </summary>
                MaximizeWindowVertically,

                /// <summary>
                /// Attempt to maximize the window horizontally
		/// </summary>
                MaximizeWindowHorizontally,

                UndoFullScreen,
                SwitchToFullScreen,
                ToggleFullScreen,
                ReportTerminalState,
                ReportTerminalPosition,
                ReportTextAreaPosition,
                ReporttextAreaPixelDimension,
                ReportSizeOfScreenInPixels,
                ReportCellSizeInPixels,
                ReportTextAreaCharacters,
                ReportScreenSizeCharacters,
                ReportIconLabel,
                ReportWindowTitle,

                /// <summary>
                /// Request that the size of the terminal be changed to the specified lines
		/// </summary>
		/// <remarks>(lines: Int)</remarks>
                ResizeToLines,
        }
}
