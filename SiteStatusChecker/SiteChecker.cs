namespace SiteStatusChecker
{
    public static class SiteChecker
    {
        public static DomainTest ForDomain(string domainName)
        {
            return new DomainTest(domainName);
        }
    }
}
