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
using System.Xml;

namespace fixcsproj
{
    class PropertyGroup
    {
        static string csprojNamespace = "http://schemas.microsoft.com/developer/msbuild/2003";
        XmlNode node;
        XmlNamespaceManager mgr;

        public PropertyGroup(XmlNode a_node)
        {
            node = a_node;
            mgr = new XmlNamespaceManager(node.OwnerDocument.NameTable);
            mgr.AddNamespace("ms", csprojNamespace);
        }

        public string this[string key]
        {
            get
            {
                return "";
            }
            set
            {
                foreach (XmlNode old in node.SelectNodes("ms:" + key, mgr))
                {
                    node.RemoveChild(old);
                }

                XmlElement e = node.OwnerDocument.CreateElement(key, csprojNamespace);
                e.InnerText = value;
                node.AppendChild(e);
            }
        }
    }
}
