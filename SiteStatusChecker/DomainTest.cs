using System;
using System.Net;

namespace SiteStatusChecker
{
    public class DomainTest : DomainAssert
    {
        public DomainTest(string domain, Action<string> failureAssertion) : base(failureAssertion)
        {
            _domain = domain;
            ServicePointManager.ServerCertificateValidationCallback = AcceptAllCertifications;
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