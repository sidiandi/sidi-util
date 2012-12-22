// Copyright (c) 2012, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
// 
// This file is part of sidi-util.
// 
// sidi-util is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// sidi-util is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with sidi-util. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;

namespace Sidi.Util
{
    class Zip : IArchive
    {
        FastZip zip = new FastZip();
        string zipFileName;

        public Zip(string a_path)
        {
            zipFileName = a_path;
        }

        #region IArchive Members

        public void Extract(string destinationDirectory, string memberName)
        {
            zip.ExtractZip(
                zipFileName, 
                destinationDirectory, 
                FastZip.Overwrite.Always, 
                null, 
                String.Empty, 
                String.Empty, true);
        }

        public void ExtractAll(string destinationDirectory)
        {
            zip.ExtractZip(zipFileName, destinationDirectory, string.Empty);
        }

        #endregion
    }
}
