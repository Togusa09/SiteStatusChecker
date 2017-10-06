using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Security.Cryptography.X509Certificates;

namespace SiteStatusChecker
{
    

    public class DomainAssert
    {
        protected string _domain;
        protected readonly List<string> _protocols = new List<string>();
        private string _redirectDomain;
        private string _redirectionProtocol;
        private Dictionary<string, HttpWebResponse> _protocolResponses = new Dictionary<string, HttpWebResponse>();
        private Dictionary<string, HttpWebRequest> _protocolRequests = new Dictionary<string, HttpWebRequest>();

        private Action<string> _failureAssertion;

        public DomainAssert(Action<string> failureAssertion)
        {
            _failureAssertion = failureAssertion;
        }

        public DomainAssert AssertIsOnline()
        {
            var pingSender = new Ping();
            var reply = pingSender.Send(_domain);

            if (reply == null)
            {
                _failureAssertion("Server reply should not be null");
            }

            if (reply.Status != IPStatus.Success)
            {
                _failureAssertion($"Because server for domain {_domain} is not online");
            }

            return this;
        }

        public DomainAssert AssertThatResolvesDns()
        {
            var entries = Dns.GetHostAddresses(_domain);

            if (!entries.Any())
                _failureAssertion($"Because unable to resolve dns entry for {_domain}");

            return this;
        }

        public DomainAssert AssertThatCantResolvesDns()
        {
            var entries = Dns.GetHostAddresses(_domain);
            //entries.Should().BeNullOrEmpty($"Because to resolved dns entry for {_domain}");
            if (entries.Any())
                _failureAssertion($"Because resolved dns entry for {_domain}");

            return this;
        }

        public DomainAssert AssertRedirectsTo(string redirectDomain, string redirectProtocol)
        {
            foreach (var protocol in _protocols)
            {
                var response = GetOrCreateResponse(protocol);
                var responseHost = response.ResponseUri.Host;
                if (!string.IsNullOrWhiteSpace(redirectDomain))
                {
                    if (responseHost != redirectDomain)
                        _failureAssertion("Because domain was not redirected");
                }

                if (!string.IsNullOrWhiteSpace(redirectProtocol))
                {
                    if (response.ResponseUri.Scheme != redirectProtocol)
                    {
                        _failureAssertion("Because protocol was not redirected");
                    }
                }
            }

            return this;
        }
        public DomainAssert AssertIsAccessible()
        {
            foreach (var protocol in _protocols)
            {
                var response = GetOrCreateResponse(protocol);
               
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    _failureAssertion("Because was unable to successfully retrieve content");
                }
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

        public DomainAssert AssertCertIsValidFor(TimeSpan fromDays)
        {
            foreach (var protocol in _protocols)
            {
                var request = GetOrCreateRequest(protocol);
                var cert = request.ServicePoint.Certificate;
                if (cert == null)
                {
                    _failureAssertion("Because site does not have a certificate");
                }

                var cert2 = new X509Certificate2(cert);

                var cn = cert2.GetIssuerName();
                var cedate = cert2.GetExpirationDateString();
                var cpub = cert2.GetPublicKeyString();

                var certExpiryDate = DateTime.Parse(cedate);
                //certExpiryDate.Should().BeAfter(DateTime.Now.Add(fromDays), "because certificate expires within specified timeframe");
                if (certExpiryDate < DateTime.Today.Add(fromDays))
                {
                    _failureAssertion("Because certificate expires within specified timeframe");
                }

            }

            return this;
        }
    }
}