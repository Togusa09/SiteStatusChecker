using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Security.Cryptography.X509Certificates;
using FluentAssertions;

namespace SiteStatusChecker
{
    public class DomainTest
    {
        private readonly string _domain;
        readonly List<string> _protocols = new List<string>();
        private string _redirectDomain;
        private string _redirectionProtocol;
        private Dictionary<string, HttpWebResponse> _protocolResponses = new Dictionary<string, HttpWebResponse>();
        private Dictionary<string, HttpWebRequest> _protocolRequests = new Dictionary<string, HttpWebRequest>();

        public DomainTest(string domain)
        {
            _domain = domain;
            ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(AcceptAllCertifications);
        }

        public bool AcceptAllCertifications(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certification, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        public DomainTest WithProtocol(string protocol)
        {
            _protocols.Add(protocol);
            return this;
        }

        public DomainTest AssertIsOnline()
        {
            var pingSender = new Ping();
            var reply = pingSender.Send(_domain);

            reply.Should().NotBeNull();
            reply.Status.Should().Be(IPStatus.Success, $"Because server for domain {_domain} is not online");
            return this;
        }

        public DomainTest AssertThatResolvesDns()
        {
            var entries = Dns.GetHostAddresses(_domain);
            entries.Should().NotBeNullOrEmpty($"Because unable to resolve dns entry for {_domain}");
            return this;
        }

        public DomainTest AssertThatCantResolvesDns()
        {
            var entries = Dns.GetHostAddresses(_domain);
            entries.Should().BeNullOrEmpty($"Because to resolved dns entry for {_domain}");
            return this;
        }

        public DomainTest AssertRedirectsTo(string redirectDomain, string redirectProtocol)
        {
            foreach (var protocol in _protocols)
            {
                var response = GetOrCreateResponse(protocol);
                var responseHost = response.ResponseUri.Host;
                if (!string.IsNullOrWhiteSpace(redirectDomain))
                {
                    responseHost.Should().Be(redirectDomain, "because domain was not redirected");
                }

                if (!string.IsNullOrWhiteSpace(redirectProtocol))
                {
                    response.ResponseUri.Scheme.Should().Be(redirectProtocol, "Because protocol was not redirected");
                }
            }

            return this;
        }
        public DomainTest AssertIsAccessible()
        {
            foreach (var protocol in _protocols)
            {
                var response = GetOrCreateResponse(protocol);
               
                response.StatusCode.Should().Be(HttpStatusCode.OK, "because was unable to successfully retrieve content");
            }

            return this;
        }


        private HttpWebRequest GetOrCreateRequest(string protocol)
        {
            if (_protocolRequests.ContainsKey(protocol))
            {
                return _protocolRequests[protocol];
            }

            var myUri = new UriBuilder(protocol, _domain);

            var request = (HttpWebRequest)WebRequest.Create(myUri.Uri);
            request.UserAgent = "Test User Agent";
            request.AllowAutoRedirect = true;

            _protocolRequests[protocol] = request;
            return request;
        }

        private HttpWebResponse GetOrCreateResponse(string protocol)
        {
            var request = GetOrCreateRequest(protocol);

            var response = (HttpWebResponse)request.GetResponse();
            _protocolResponses[protocol] = response;
            return response;
        }

        public DomainTest AssertCertIsValidFor(TimeSpan fromDays)
        {
            foreach (var protocol in _protocols)
            {
                var request = GetOrCreateRequest(protocol);
                var cert = request.ServicePoint.Certificate;
                cert.Should().NotBeNull("because site does not have a certificate");

                var cert2 = new X509Certificate2(cert);

                var cn = cert2.GetIssuerName();
                var cedate = cert2.GetExpirationDateString();
                var cpub = cert2.GetPublicKeyString();

                var certExpiryDate = DateTime.Parse(cedate);
                certExpiryDate.Should().BeAfter(DateTime.Now.Add(fromDays), "because certificate expires within specified timeframe");

            }

            return this;
        }
    }
}