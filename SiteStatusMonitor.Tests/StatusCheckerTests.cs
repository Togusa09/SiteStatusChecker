using System;
using SiteStatusChecker;
using Xunit;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit.Sdk;

namespace SiteStatusMonitor
{
    public class StatusCheckerTests
    {
        public StatusCheckerTests()
        {
            SiteChecker.SetFailureAssertion(message => Execute.Assertion.FailWith(message));
        }

        [Theory]
        [InlineData("google.com.au", "http")]
        [InlineData("google.com.au", "https")]
        [InlineData("anarks2.com", "https")]
        [InlineData("blog.anarks2.com", "https")]
        public void TestThatSitesAreUp(string domain, string protocol)
        {
            SiteChecker.ForDomain(domain)
                .WithProtocol(protocol)
                .AssertThatResolvesDns()
                .AssertIsOnline()
                .AssertIsAccessible();
        }

        [Fact]
        public void TestThatGoogleRedirectsToLocalSite()
        {
            SiteChecker.ForDomain("google.com")
                .WithProtocol("http")
                // I'm in australia, so google redirects to the au site
                .AssertRedirectsTo("www.google.com.au", "http");
        }

        [Fact]
        public void TestThatGoogleHasValidCert()
        {
            SiteChecker.ForDomain("google.com")
                .WithProtocol("https")
                // I'm in australia, so google redirects to the au site
                .AssertRedirectsTo("www.google.com.au", "https")
                .AssertCertIsValidFor(TimeSpan.FromDays(30));
        }

        [Fact]
        public void TestThatBlogRedirectsAndHasValidCert()
        {
            SiteChecker.ForDomain("anarks2.com")
                .WithProtocol("http")
                .AssertRedirectsTo("blog.anarks2.com", "https")
                .AssertCertIsValidFor(TimeSpan.FromDays(30));
        }
    }

}
