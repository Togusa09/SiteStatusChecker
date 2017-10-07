# Server Status Check

This library is designed to provide a simple  way to query the status of a webserver from integration tests.

Currently the library users the Fluent Assertions library for its assertions, so it supports all the testing frameworks that it supports

# Examples

## Setup 


```csharp
// Setup for fluent assertions
SiteChecker.SetFailureAssertion(message => Execute.Assertion.FailWith(message));

// Setup for NUnit
SiteChecker.SetFailureAssertion(Assert.Fail);
```

Sets up test, asserts that the site is accessible
```csharp
SiteChecker.ForDomain("google.com") // Set domain
                .WithProtocol("http") // Set protocol
                .AssertIsAccessible(); // Assert is accessible
```

# Assert methods

## AssertThatResolvesDNS
Asserts that it is possible to resolve the DNS entry for the domain.

## AssertThatIsonline
Asserts that the server is responding to ping. 

## AssertThatIsAccessible
Asserts that the server is reachable and returns a HTTP 200 response when queried with the setup protocol.

## AssertRedirectsTo
Takes a domain and protocol as parameters. Asserts that requests to the domain are redirected to the supplied domain and protocol.

## AssertCertIsValidFor
Takes a timespan as parameter. Asserts that the certificate will not expire within the specified timespan.

