using System;
using System.Security;

namespace Sidi.CredentialManagement
{
    internal interface ICredentialPrompt: IDisposable
    {
        DialogResult ShowDialog();
        DialogResult ShowDialog(IntPtr owner);

        string Username { get; set; }
        SecureString SecurePassword { get; set; }

        string Title { get; set; }
        string Message { get; set; }
        
        bool SaveChecked { get; set; }

        bool GenericCredential { get; set; }
        bool ShowSaveCheckBox { get; set; }
        int ErrorCode { get; set; }

    }

    internal static class ICredentialsPromptExtensions
    {
        public static Credential GetCredential(this ICredentialPrompt prompt)
        {
            return new Credential(prompt.Username, prompt.SecurePassword, prompt.Title);
        }
    }
}
