using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidi.IO.Windows
{
    internal class FindHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public FindHandle()
            : base(true)
        {
        }

        public override bool IsInvalid
        {
            get { return this.handle == IntPtr.Zero || this.handle == (IntPtr)(-1); }
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.FindClose(this.handle);
            return true;
        }
    }
}
