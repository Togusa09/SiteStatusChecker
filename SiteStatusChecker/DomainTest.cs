using System.Net;

namespace SiteStatusChecker
{
    public class DomainTest : DomainAssert
    {
       

        public DomainTest(string domain)
        {
            _domain = domain;
            ServicePointManager.ServerCertificateValidationCallback =
                new System.Net.Security.RemoteCertificateValidationCallback(AcceptAllCertifications);
        }

        public bool AcceptAllCertifications(object sender,
            System.Security.Cryptography.X509Certificates.X509Certificate certification,
            System.Security.Cryptography.X509Certificates.X509Chain chain,
            System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        public DomainTest WithProtocol(string protocol)
        {
            _protocols.Add(protocol);
            return this;
        }
    }
}