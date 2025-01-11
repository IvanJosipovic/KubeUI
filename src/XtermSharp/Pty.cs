using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Diagnostics;
using System.Text;

namespace XtermSharp {
	[StructLayout(LayoutKind.Sequential)]
	public struct UnixWindowSize {
		public short row, col, xpixel, ypixel;
	}

	public class Pty {
		[DllImport ("util")]
		extern static int forkpty (ref int master, IntPtr dataReturn, IntPtr termios, ref UnixWindowSize WinSz);

		[DllImport ("libc")]
		extern static int execv (string process, string [] args);

		[DllImport ("libc")]
		extern static int execve (string process, string [] args, string [] env);

		[DllImport ("libpty.dylib", SetLastError = true, EntryPoint="fork_and_exec")]
		extern static unsafe int HeavyFork (string process, byte** args, byte** env, ref int master, ref UnixWindowSize winSize);

		static bool HeavyDuty = true;
		/// <summary>
		/// Forks a process and returns a file handle that is connected to the standard output of the child process
		/// </summary>
		/// <param name="programName">Name of the program to run</param>
		/// <param name="args">Argument to pass to the program</param>
		/// <param name="env">Desired environment variables for the program</param>
		/// <param name="master">The file descriptor connected to the input and output of the child process</param>
		/// <param name="winSize">Desired window size</param>
		/// <returns></returns>
		public static int ForkAndExec (string programName, string [] args, string [] env, ref int master, UnixWindowSize winSize)
		{
			if (HeavyDuty) {
				return DoHeavyFork (programName, args, env, ref  master, ref winSize);
			} else {
				var pid = forkpty (ref master, IntPtr.Zero, IntPtr.Zero, ref winSize);
				if (pid < 0)
					throw new Exception ("Could not create Pty");

				if (pid == 0) {
					execve (programName, args, env);
				}
				return pid;
			}
		}


		static unsafe int DoHeavyFork (string programName, string [] args, string [] env, ref int master, ref UnixWindowSize winSize)
		{
			byte** argvPtr = null, envpPtr = null;
			int result;
			try {
				AllocNullTerminatedArray (args, ref argvPtr);
				AllocNullTerminatedArray (env, ref envpPtr);
				result = HeavyFork (programName, argvPtr, envpPtr, ref master, ref winSize);

				if (result < 0)
                {
					throw new ArgumentException($"Invalid PID. Last error { Marshal.GetLastWin32Error() }");
                }

				return result;
			} finally {
				FreeArray (argvPtr, args.Length);
				FreeArray (envpPtr, env.Length);
			}
		}

		private static unsafe void AllocNullTerminatedArray (string [] arr, ref byte** arrPtr)
		{
			int arrLength = arr.Length + 1; // +1 is for null termination

			// Allocate the unmanaged array to hold each string pointer.
			// It needs to have an extra element to null terminate the array.
			arrPtr = (byte**)Marshal.AllocHGlobal (sizeof (IntPtr) * arrLength);
			Debug.Assert (arrPtr != null);

			// Zero the memory so that if any of the individual string allocations fails,
			// we can loop through the array to free any that succeeded.
			// The last element will remain null.
			for (int i = 0; i < arrLength; i++) {
				arrPtr [i] = null;
			}

			// Now copy each string to unmanaged memory referenced from the array.
			// We need the data to be an unmanaged, null-terminated array of UTF8-encoded bytes.
			for (int i = 0; i < arr.Length; i++) {
				byte [] byteArr = Encoding.UTF8.GetBytes (arr [i]);

				arrPtr [i] = (byte*)Marshal.AllocHGlobal (byteArr.Length + 1); //+1 for null termination
				Debug.Assert (arrPtr [i] != null);

				Marshal.Copy (byteArr, 0, (IntPtr)arrPtr [i], byteArr.Length); // copy over the data from the managed byte array
				arrPtr [i] [byteArr.Length] = (byte)'\0'; // null terminate
			}
		}

		private static unsafe void FreeArray (byte** arr, int length)
		{
			if (arr != null) {
				// Free each element of the array
				for (int i = 0; i < length; i++) {
					if (arr [i] != null) {
						Marshal.FreeHGlobal ((IntPtr)arr [i]);
						arr [i] = null;
					}
				}

				// And then the array itself
				Marshal.FreeHGlobal ((IntPtr)arr);
			}
		}

		[DllImport ("libc", SetLastError = true)]
		extern static int ioctl (int fd, ulong cmd, ref UnixWindowSize WinSz);

		[DllImport ("libpty.dylib", EntryPoint = "set_window_size")]
		extern static unsafe int set_window_size (int master, ref UnixWindowSize winSize);

		/// <summary>
		/// Sends a request to the pseudo terminal to set the size to the specified one
		/// </summary>
		/// <param name="fd">File descriptor returned by ForkPty</param>
		/// <param name="winSize">The desired window size</param>
		/// <returns></returns>
		public static int SetWinSize (int fd, ref UnixWindowSize winSize)
		{
			var r = set_window_size (fd, ref winSize);
			if (r == -1) {
				var lastErr = Marshal.GetLastWin32Error ();
				Console.WriteLine (lastErr);
			}
			return r;
		}

		[DllImport ("libc", SetLastError = true)]
		extern static int ioctl (int fd, long cmd, ref long size);

		/// <summary>
		/// Returns the number of bytes available for reading on a file descriptor
		/// </summary>
		/// <param name="fd"></param>
		/// <param name="size"></param>
		/// <returns></returns>
		public static int AvailableBytes (int fd, ref long size)
		{
			const long MAC_FIONREAD = 0x4004667f;
			var r = ioctl (fd, MAC_FIONREAD, ref size);
			if (r == -1) {
				var lastErr = Marshal.GetLastWin32Error ();
				Console.WriteLine (lastErr);
			}
			return r;
		}
	}
}
