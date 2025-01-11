using System;

namespace XtermSharp {
	/// <summary>
	/// The mouse coordinates can be encoded in a number of ways, and obey to historical
	/// upgrades to the protocol, but also attempts at fixing limitations of the different
	/// encodings.
	/// </summary>
	public enum MouseProtocolEncoding {
                /// <summary>
                /// The default x10 mode is limited to coordinates up to 223.
                /// (255-32).   The other modes solve this limitaion
                /// </summary>
                X10,

                /// <summary>
                /// Extends the range of a coordinate to 2015 by using UTF-8 encoding of the
                /// coordinate value.   This encoding is troublesome for applications that
                /// do not support utf8 input.
                /// </summary>
                UTF8,

                /// <summary>
                /// The response uses CSI < ButtonValue ; Px ; Py [Mm]
                /// </summary>
                SGR,

                /// <summary>
                // Different response style, with possible ambiguities, not recommended
                /// </summary>
                URXVT
        }
}
