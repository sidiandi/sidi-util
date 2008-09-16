// Copyright (c) 2008, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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
            zip.ExtractZip(zipFileName, destinationDirectory, FastZip.Overwrite.Always, null, String.Empty, String.Empty, true);
        }

        public void ExtractAll(string destinationDirectory)
        {
            zip.ExtractZip(zipFileName, destinationDirectory, string.Empty);
        }

        #endregion
    }
}
