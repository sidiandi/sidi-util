namespace Sidi.CredentialManagement
{
    internal enum CredentialType: uint 
    {
        None = 0,
        Generic = 1,
        DomainPassword = 2,
        DomainCertificate = 3,
        DomainVisiblePassword = 4
    }
}
