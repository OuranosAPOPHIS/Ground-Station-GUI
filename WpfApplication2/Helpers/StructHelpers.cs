using System;
using System.Runtime.InteropServices;

namespace APOPHIS.GroundStation.Helpers {
  static class StructHelpers {
    static public byte[] GetBytes<T>(this T str) where T : struct {
      int size = Marshal.SizeOf(str);
      byte[] arr = new byte[size];
      IntPtr ptr = Marshal.AllocHGlobal(size);

      Marshal.StructureToPtr(str, ptr, true);
      Marshal.Copy(ptr, arr, 0, size);
      Marshal.FreeHGlobal(ptr);

      return arr;
    }

    static public T FromBytes<T>(this byte[] arr) where T : struct {
      T str = default(T);

      int size = Marshal.SizeOf(str);
      IntPtr ptr = Marshal.AllocHGlobal(size);

      Marshal.Copy(arr, 0, ptr, size);

      str = (T)Marshal.PtrToStructure(ptr, str.GetType());
      Marshal.FreeHGlobal(ptr);

      return str;
    }
  }
}
