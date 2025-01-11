using System;
namespace XtermSharp {
	/// <summary>
	/// Represents a circular list; a list with a maximum size that wraps around when push is called overriding values at the start of the list.
	/// </summary>
	public class CircularList<T> {
		int length;
		T [] array;
		int startIndex;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:XtermSharp.CircularList`1"/> class with the specified number of elements.
		/// </summary>
		/// <param name="maxLength">Max length.</param>
		public CircularList (int maxLength)
		{
			array = new T [maxLength];
			length = 0;
		}

		// Gets the cyclic index for the specified regular index. The cyclic index can then be used on the
		// backing array to get the element associated with the regular index.
		int GetCyclicIndex (int index)
		{
			return (startIndex + index) % array.Length;
		}

		/// <summary>
		/// Gets or sets the maximum length of the circular list
		/// </summary>
		/// <value>The length of the max.</value>
		public int MaxLength {
			get => array.Length;

			set {
				if (value <= 0)
					throw new ArgumentException (nameof (value));

				if (value == array.Length)
					return;

				// Reconstruct array, starting at index 0. Only transfer values from the
				// indexes 0 to length.
				var newArray = new T [value];
				var top = Math.Min (value, array.Length);
				for (int i = 0; i < top; i++)
					newArray [i] = array [GetCyclicIndex (i)];
				startIndex = 0;
				array = newArray;
			}
		}

		/// <summary>
		/// The current length of the circular buffer
		/// </summary>
		/// <value>The length.</value>
		public int Length {
			get => length;
			set {
				if (value > length) {
					for (int i = length; i < value; i++)
						array [i] = default (T);
				}
				length = value;
			}
		}

		/// <summary>
		/// Invokes the specificied callback for each items of the circular list, the first parameter is the value, the second is the ith-index.
		/// </summary>
		/// <param name="callback">Callback.</param>
		public void ForEach (Action<T, int> callback)
		{
			var top = length;
			for (int i = 0; i < top; i++)
				callback (this [i], i);
		}

		/// <summary>
		/// Gets or sets the <see cref="T:XtermSharp.CircularList`1"/> at the specified index.
		/// </summary>
		/// <param name="index">Index.</param>
		public T this [int index] {
			get => array [GetCyclicIndex (index)];
			set => array [GetCyclicIndex (index)] = value;
		}

		/// <summary>
		/// Event raised when an item is removed from the circular array, the parameter is the number of items removed.
		/// </summary>
		public Action<int> Trimmed;

		/// <summary>
		/// Pushes a new value onto the list, wrapping around to the start of the array, overriding index 0 if the maximum length is reached
		/// </summary>
		/// <returns>The push.</returns>
		/// <param name="value">Value to push.</param>
		public void Push (T value)
		{
			array [GetCyclicIndex (length)] = value;
			if (length == array.Length) {
				startIndex++;
				if (startIndex == array.Length)
					startIndex = 0;

				Trimmed?.Invoke (1);
			} else {
				length++;
			}
		}

		public T Recycle ()
		{
			if (Length != MaxLength) {
				throw new Exception ("Can only recycle when the buffer is full");
			}
			startIndex = ++startIndex % MaxLength;

			return array [GetCyclicIndex (Length - 1)];
		}

		/// <summary>
		///   Removes and returns the last value on the list. 
		/// </summary>
		/// <returns>The popped value.</returns>
		public T Pop ()
		{
			return array [GetCyclicIndex (length-- - 1)];
		}

		/// <summary>
		/// Deletes and/or inserts items at a particular index (in that order).
		/// </summary>
		/// <returns>The splice.</returns>
		/// <param name="start">The index to delete and/or insert.</param>
		/// <param name="deleteCount">The number of elements to delete.</param>
		/// <param name="items">The items to insert.</param>
		public void Splice (int start, int deleteCount, params T [] items)
		{
			// delete items
			if (deleteCount > 0) {
				for (int i = start; i < length - deleteCount; i++)
					array [GetCyclicIndex (i)] = array [GetCyclicIndex (i + deleteCount)];
				length -= deleteCount;
			}
			if (items.Length != 0) {
				// add items
				for (int i = length - 1; i >= start; i--)
					array [GetCyclicIndex (i + items.Length)] = array [GetCyclicIndex (i)];
				for (int i = 0; i < items.Length; i++)
					array [GetCyclicIndex (start + i)] = items [i];
			}

			// Adjust length as needed
			if (length + items.Length > array.Length) {
				int countToTrim = length + items.Length - array.Length;
				startIndex += countToTrim;
				length = array.Length;
				Trimmed?.Invoke (countToTrim);
			} else {
				length += items.Length;
			}
		}

		/// <summary>
		/// Trims a number of items from the start of the list.
		/// </summary>
		/// <param name="count">The number of items to remove..</param>
		public void TrimStart (int count)
		{
			if (count > length)
				count = length;

			// TODO: perhaps bug in original code, this does not clamp the value of startIndex
			startIndex += count;
			length -= count;
			Trimmed?.Invoke (count);
		}

		/// <summary>
		/// Shifts the elements.
		/// </summary>
		/// <param name="start">Start.</param>
		/// <param name="count">Count.</param>
		/// <param name="offset">Offset.</param>
		public void ShiftElements (int start, int count, int offset)
		{
			if (count < 0)
				return;
			if (start < 0 || start >= length)
				throw new ArgumentException ("Start argument is out of range");
			if (start + offset < 0)
				throw new ArgumentException ("Can not shift elements in list beyond index 0");
			if (offset > 0) {
				for (var i = count - 1; i >= 0; i--) {
					this [start + i + offset] = this [start + i];
				}
				var expandListBy = (start + count + offset) - length;
				if (expandListBy > 0) {
					length += expandListBy;
					while (length > array.Length) {
						length--;
						startIndex++;
						Trimmed.Invoke (1);
					}
				}
			} else {
				for (var i = 0; i < count; i++) {
					this [start + i + offset] = this [start + i];
				}
			}
		}

		public bool IsFull => Length == MaxLength;

		public T[] ToArray()
		{
			var result = new T [Length];
			for (int i = 0; i < Length; i++) {
				result [i] = array [GetCyclicIndex (i)];
			}

			return result;
		}
	}
}
