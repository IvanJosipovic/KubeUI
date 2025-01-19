using System;
using System.Collections.Generic;

namespace XtermSharp {
	/// <summary>
	/// Buffer for processing input
	/// </summary>
	/// <remarks>
	/// Because data might not be complete, we need to put back data that we read to process on
	/// a future read.  To prepare for reading, on every call to parse, the prepare method is
	/// given the new buffer to read from.
	///
	/// the `hasNext` describes whether there is more data left on the buffer, and `bytesLeft`
	/// returnes the number of bytes left.   The `getNext` method fetches either the next
	/// value from the putback buffer, or when it is empty, it returns it from the buffer that
	/// was passed during prepare.
	///
	/// Additionally, the terminal parser needs to reset the parser state on demand, and
	/// that is surfaced via reset
	/// </remarks>
	class ReadingBuffer {
		byte[] putbackBuffer = new byte [0];
		unsafe byte* buffer;
		int bufferStart;
		int totalCount;
		int index;

		unsafe public void Prepare (byte* data, int start, int length)
		{
			buffer = data;
			bufferStart = start;

			index = 0;
			totalCount = putbackBuffer.Length + length;
		}

		public int BytesLeft ()
		{
			return totalCount - index;
		}

		public bool HasNext ()
		{
			return index < totalCount;
		}

		unsafe public byte GetNext ()
		{
			byte val;
			if (index < putbackBuffer.Length) {
				// grab from putback buffer
				val = putbackBuffer [index];
			} else {
				// grab from the prepared buffer
				val = buffer [bufferStart + (index - putbackBuffer.Length)];
			}

			index++;
			return val;
		}

		/// <summary>
		/// Puts back code and the remainder of the buffer
		/// </summary>
		public void Putback (byte code)
		{
			var left = BytesLeft ();
			byte [] newPutback = new byte[left + 1];
			newPutback [0] = code;

			for (int i = 0; i < left; i++) {
				newPutback [i + 1] = GetNext ();
			}

			putbackBuffer = newPutback;
		}

		unsafe public void Done ()
		{
			if (index < putbackBuffer.Length) {
				byte [] newPutback = new byte [putbackBuffer.Length - index];
				Array.Copy (putbackBuffer, index, newPutback, 0, newPutback.Length);
				putbackBuffer = newPutback;
			} else {
				putbackBuffer = new byte [0];
			}

			buffer = null;
		}

		public void Reset ()
		{
			putbackBuffer = new byte [0];
			index = 0;
		}
	}
}
