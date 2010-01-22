// Copyright (c) 2009, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
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

#region Copyright © 2009 Andreas Grimme. All rights reserved.
/*
Copyright © 2009 Andreas Grimme. All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions
are met:

1. Redistributions of source code must retain the above copyright
   notice, this list of conditions and the following disclaimer.
2. Redistributions in binary form must reproduce the above copyright
   notice, this list of conditions and the following disclaimer in the
   documentation and/or other materials provided with the distribution.
3. The name of the author may not be used to endorse or promote products
   derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE AUTHOR "AS IS" AND ANY EXPRESS OR
IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE. 
*/
#endregion

using System;
using System.Diagnostics;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;
using Microsoft.Win32;
using System.Reflection;
using System.Net;
using System.Collections.Generic;

// $Id: SvnClient.cs 461 2009-02-19 03:30:28Z pwelter34 $

namespace Sidi.Build.GoogleCode
{
    /// <summary>
    /// Upload a file to a project hosted at http://code.google.com 
    /// </summary>
    public class Upload : ITask
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly byte[] NewLineAsciiBytes = Encoding.ASCII.GetBytes("\r\n");
        private static readonly string Boundary = Guid.NewGuid().ToString();

        /// <summary>
        /// User name for the Google Code profile
        /// </summary>
        /// The UserName can also be specified in the registry under HKEY_CURRENT_USER\Software\MSBuild.Community.Tasks.GoogleCode.Upload\$(ProjectName)\UserName
        public string UserName { set; get; }

        /// <summary>
        /// Password for the Google Code profile. Note that this is NOT your global Google Account password.
        /// </summary>
        /// The Pasword can also be specified in the registry under HKEY_CURRENT_USER\Software\MSBuild.Community.Tasks.GoogleCode.Upload\$(ProjectName)\Password
        public string Password { set; get; }

        /// <summary>
        /// Name of the Google Code project
        /// </summary>
        public string ProjectName { set; get; }

        /// <summary>
        /// Local path of the file to be uploaded
        /// </summary>
        public string LocalFile { set; get; }

        /// <summary>
        /// Name of the file on the server
        /// </summary>
        public string RemoteFile { set; get; }

        /// <summary>
        /// Summary for the upload
        /// </summary>
        public string Summary { set; get; }

        string GetRegString(string valueName)
        {
            string keyName = Registry.CurrentUser.Name + "\\Software\\" + GetType().FullName + "\\" + ProjectName;
            log.InfoFormat("Looking for {0} in {1}", valueName, keyName);
            string value = (string)Registry.GetValue(keyName, valueName, null);
            if (value == null)
            {
                string defaultKeyName = Registry.CurrentUser.Name + "\\Software\\" + GetType().FullName;
                log.InfoFormat("Looking for {0} in {1}", valueName, defaultKeyName);
                value = (string)Registry.GetValue(defaultKeyName, valueName, null);
            }
            return value;
        }
        
        /// <summary>
        /// Upload the file
        /// </summary>
        /// <returns></returns>
        public bool Execute()
        {
            if (UserName == null)
            {
                UserName = GetRegString("UserName");
            }

            if (Password == null)
            {
                Password = GetRegString("Password");
            }
            
            if (UserName == null)
                throw new InvalidOperationException("UserName cannot be null");

            if (Password == null)
                throw new InvalidOperationException("Password cannot be null");

            if (ProjectName == null)
                throw new InvalidOperationException("ProjectName cannot be null");

            if (LocalFile == null)
                throw new InvalidOperationException("LocalFile cannot be null");

            if (RemoteFile == null)
                throw new InvalidOperationException("RemoteFile cannot be null");

            if (Summary == null)
                throw new InvalidOperationException("Summary cannot be null");

            DoUpload();

            return true;
        }

        void DoUpload()
		{
			HttpWebRequest request = (HttpWebRequest) WebRequest.Create(String.Format("https://{0}.googlecode.com/files", ProjectName));
			request.Method = "POST";
			request.ContentType = String.Concat("multipart/form-data; boundary=" + Boundary);
            request.UserAgent = String.Concat(this.GetType().FullName + " " + Assembly.GetExecutingAssembly().GetName().Version.ToString());
			request.Headers.Add("Authorization", String.Concat("Basic ", CreateAuthorizationToken(UserName, Password)));

			log.Info(request.UserAgent);
            log.InfoFormat("Upload URL: {0}", request.Address.ToString());
            log.InfoFormat("Username: {0}", UserName);
            log.InfoFormat("Local file: {0}", LocalFile);
            log.InfoFormat("Remote file: {0}", RemoteFile);
            log.InfoFormat("Summary: {0}", Summary);

			using (Stream stream = request.GetRequestStream())
			{
			    log.Info("Sending summary...");
				WriteLine(stream, String.Concat("--", Boundary));
				WriteLine(stream, @"content-disposition: form-data; name=""summary""");
				WriteLine(stream, "");
				WriteLine(stream, Summary);

                log.Info("Sending file...");
				WriteLine(stream, String.Concat("--", Boundary));
				WriteLine(stream, String.Format(@"content-disposition: form-data; name=""filename""; filename=""{0}""", RemoteFile));
				WriteLine(stream, "Content-Type: application/octet-stream");
				WriteLine(stream, "");
				WriteFile(stream, LocalFile);
				WriteLine(stream, "");
				WriteLine(stream, String.Concat("--", Boundary, "--"));
			}

			request.GetResponse();
		}

        /// <summary>
        /// Creates the authorization token.
        /// </summary>
        internal static string CreateAuthorizationToken(string username, string password)
        {
            if (username == null)
                throw new ArgumentNullException("username");

            if (password == null)
                throw new ArgumentNullException("password");

            byte[] authBytes = Encoding.ASCII.GetBytes(String.Concat(username, ":", password));
            return Convert.ToBase64String(authBytes);
        }

        /// <summary>
        /// Writes the string to the specified stream and concatenates a newline.
        /// </summary>
        internal static void WriteLine(Stream outputStream, string valueToWrite)
        {
            if (valueToWrite == null)
                throw new ArgumentNullException("valueToWrite");

            List<byte> bytesToWrite = new List<byte>(Encoding.ASCII.GetBytes(valueToWrite));
            bytesToWrite.AddRange(NewLineAsciiBytes);
            outputStream.Write(bytesToWrite.ToArray(), 0, bytesToWrite.Count);
        }

        /// <summary>
        /// Writes the specified file to the specified stream.
        /// </summary>
        internal void WriteFile(Stream outputStream, string fileToWrite)
        {
            if (outputStream == null)
                throw new ArgumentNullException("outputStream");

            if (fileToWrite == null)
                throw new ArgumentNullException("fileToWrite");

            using (FileStream fileStream = new FileStream(fileToWrite, FileMode.Open))
            {
                byte[] buffer = new byte[1024];
                int count;
                while ((count = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    outputStream.Write(buffer, 0, count);
                }
            }
        }

        public IBuildEngine BuildEngine { get; set; }
        public ITaskHost HostObject { get; set; }
    }
}
