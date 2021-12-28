using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace PdfScribe
{
    internal static class NativeMethods
    {
        /*
        This code was adapted from Matthew Ephraim's Ghostscript.Net project -
        external dll definitions moved into NativeMethods to
        satisfy FxCop requirements
        https://github.com/mephraim/ghostscriptsharp
        */

        #region Hooks into Ghostscript DLL
        internal const int GS_ARG_ENCODING_LOCAL = 0;
        internal const int GS_ARG_ENCODING_UTF8 = 1;

        [DllImport("gsdll64.dll", EntryPoint = "gsapi_new_instance")]
        internal static extern int CreateAPIInstance(out IntPtr pinstance, IntPtr caller_handle);

        [DllImport("gsdll64.dll", EntryPoint = "gsapi_init_with_args")]
        internal static extern int InitAPI(IntPtr instance, int argc, IntPtr[] argv);

        [DllImport("gsdll64.dll", EntryPoint = "gsapi_set_arg_encoding")]
        internal static extern int SetEncoding(IntPtr inst, int encoding);

        [DllImport("gsdll64.dll", EntryPoint = "gsapi_exit")]
        internal static extern int ExitAPI(IntPtr instance);

        [DllImport("gsdll64.dll", EntryPoint = "gsapi_delete_instance")]
        internal static extern void DeleteAPIInstance(IntPtr instance);
        #endregion

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern bool SetDllDirectory(string lpPathName);

        internal static IntPtr NativeUtf8FromString(string managedString)
        {
            int len = Encoding.UTF8.GetByteCount(managedString);
            byte[] buffer = new byte[len + 1]; // null-terminator allocated
            Encoding.UTF8.GetBytes(managedString, 0, managedString.Length, buffer, 0);
            IntPtr nativeUtf8 = Marshal.AllocHGlobal(buffer.Length);
            Marshal.Copy(buffer, 0, nativeUtf8, buffer.Length);
            return nativeUtf8;
        }
    }
}
