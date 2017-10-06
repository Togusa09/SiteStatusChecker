using System;

namespace SiteStatusChecker
{
    public delegate void AssertCallback(object expected, string message);

    public static class SiteChecker
    {
        private static Action<string> _failureAssertion;

        public static DomainTest ForDomain(string domainName)
        {
            return new DomainTest(domainName, _failureAssertion);
        }

        public static void SetFailureAssertion(Action<string> func)
        {
            _failureAssertion = func;
        }
    }
}
