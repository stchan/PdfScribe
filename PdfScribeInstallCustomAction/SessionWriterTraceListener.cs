using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

using Microsoft.Deployment.WindowsInstaller;

namespace PdfScribeInstallCustomAction
{
    public class SessionLogWriterTraceListener : TextWriterTraceListener , IDisposable
    {

        protected MemoryStream listenerStream;
        protected Session installSession;
        private bool isDisposed;

        public SessionLogWriterTraceListener(Session session)
            : base()
        {
            this.listenerStream = new MemoryStream();
            this.Writer = new StreamWriter(this.listenerStream);
            this.installSession = session;
        }

        #region IDisposable impelementation

        /// <summary>
        /// Releases resources held by the listener -
        /// will not automatically flush and write
        /// trace data to the install session log -
        /// call CloseAndWriteLog() before disposing
        /// to ensure data is written
        /// </summary>
        public new void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dipose(bool disposing)
        {
            if (!this.isDisposed)
            {
                if (disposing)
                {
                    if (this.Writer != null)
                    {
                        this.Writer.Close();
                        this.Writer.Dispose();
                        this.Writer = null;
                    }
                    if (this.listenerStream != null)
                    {
                        this.listenerStream.Close();
                        this.listenerStream.Dispose();
                        this.listenerStream = null;
                    }
                    if (this.installSession != null)
                        this.installSession = null;
                }
                this.isDisposed = true;
            }

            base.Dispose(disposing);
        }

        #endregion

        /// <summary>
        /// Closes the listener and writes accumulated
        /// trace data to the install session's log (Session.Log)
        /// The listener should not be used after calling
        /// this method, and should be disposed of.
        /// </summary>
        public void CloseAndWriteLog()
        {
            if (this.listenerStream != null &&
                this.installSession != null)
            {
                this.Flush();
                if (this.listenerStream.Length > 0)
                {
                    listenerStream.Position = 0;
                    using (StreamReader listenerStreamReader = new StreamReader(this.listenerStream))
                    {
                        this.installSession.Log(listenerStreamReader.ReadToEnd());
                    }
                }
                this.Close();
                this.Dispose();
                this.installSession = null;
            }
        }
    }
}
