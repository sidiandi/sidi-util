// Copyright (c) 2016, Andreas Grimme

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;

namespace Sidi.CredentialManagement
{
    /// <summary>
    /// Supplies a credential for a certain purpose
    /// </summary>
    public interface ICredentialProvider
    {
        /// <summary>
        /// Show a dialog to enter the credential
        /// </summary>
        void SetInteractive();

        NetworkCredential GetCredential();

        /// <summary>
        /// Clears cached credential information
        /// </summary>
        void Reset();
    }

    public class Factory
    {
        /// <summary>
        /// Create a credential provider that asks users for credentials and stores them user-specific for later reuse 
        /// </summary>
        /// <param name="purpose"></param>
        /// <returns></returns>
        public static ICredentialProvider GetCredentialProvider(string purpose)
        {
            return new CredentialProvider(purpose);
        }
    }
}
