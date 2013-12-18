using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace PdfScribe
{
    internal class GhostScript64
    {
        /*
        This code was adapted from Matthew Ephraim's Ghostscript.Net project
        https://github.com/mephraim/ghostscriptsharp
        */

        /// <summary>
        /// Calls the Ghostscript API with a collection of arguments to be passed to it
        /// </summary>
        public static void CallAPI(string[] args)
        {
            // Get a pointer to an instance of the Ghostscript API and run the API with the current arguments
            IntPtr gsInstancePtr;
            lock (resourceLock)
            {
                NativeMethods.CreateAPIInstance(out gsInstancePtr, IntPtr.Zero);
                try
                {
                    int result = NativeMethods.InitAPI(gsInstancePtr, args.Length, args);

                    if (result < 0)
                    {
                        throw new ExternalException("Ghostscript conversion error", result);
                    }
                }
                finally
                {
                    Cleanup(gsInstancePtr);
                }
            }
        }

        /// <summary>
        /// Frees up the memory used for the API arguments and clears the Ghostscript API instance
        /// </summary>
        private static void Cleanup(IntPtr gsInstancePtr)
        {
            NativeMethods.ExitAPI(gsInstancePtr);
            NativeMethods.DeleteAPIInstance(gsInstancePtr);
        }


        /// <summary>
        /// GS can only support a single instance, so we need to bottleneck any multi-threaded systems.
        /// </summary>
        private static object resourceLock = new object();
    }
}
