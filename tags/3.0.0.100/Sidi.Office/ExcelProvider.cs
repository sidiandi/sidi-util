using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sidi.Office
{
    public class ExcelProvider : IDisposable
    {
        public static ExcelProvider GetActiveOrNew()
        {
            var p = new ExcelProvider();
            var active = GetActiveApplication();
            if (active != null)
            {
                p.m_application = active;
                p.m_quitOnDispose = false;
            }
            else
            {
                p.m_application = new Application();
                p.m_quitOnDispose = true;
            }
            return p;
        }

        Application m_application;
        bool m_quitOnDispose = false;

        public void KeepAlive()
        {
            m_quitOnDispose = false;
        }

        ExcelProvider()
        {
        }

        public static ExcelProvider GetActive()
        {
            var active = GetActiveApplication();
            if (active == null)
            {
                throw new InvalidOperationException("No active Excel instance found");
            }
            return new ExcelProvider { m_application = active, m_quitOnDispose = false };
        }

        public static ExcelProvider GetNew()
        {
            return new ExcelProvider
            {
                m_application = new Application(),
                m_quitOnDispose = true
            };
        }

        /// <summary>
        /// Returns the currently running Excel application
        /// </summary>
        static Application GetActiveApplication()
        {
            try
            {
                return (Application)System.Runtime.InteropServices.Marshal.GetActiveObject("Excel.Application");
            }
            catch
            {
                return null;
            }
        }

        public Application Application
        {
            get
            {
                return m_application;
            }
        }

        public Worksheet ActiveWorksheet
        {
            get
            {
                return Application.GetActiveWorksheet();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (m_application != null)
                {
                    if (m_quitOnDispose)
                    {
                        m_application.DisplayAlerts = false;
                        m_application.Quit();
                    }
                    m_application = null;
                }
            }
        }
    }
}
