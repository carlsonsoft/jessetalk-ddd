using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Polly;
using Resilience;

namespace User.Identity.Infrastructure
{
    public class ResilienceClientFactory
    {
        private ILogger<ResilienceHttpClient> _logger;
        private IHttpContextAccessor _httpContextAccessor;
        private int _retryCount;
        private int _exceptionCountAllowedBeforeBreaking;
        public ResilienceClientFactory(ILogger<ResilienceHttpClient> logger, IHttpContextAccessor httpContextAccessor, int retryCount, int exceptionCountAllowedBeforeBreaking)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _retryCount = retryCount;
            _exceptionCountAllowedBeforeBreaking = exceptionCountAllowedBeforeBreaking;
        }
        public ResilienceHttpClient GetResilienceHttpClient()
        {
            return new ResilienceHttpClient(origin => CreatePolicy(origin), _logger, _httpContextAccessor);
        }

        private Policy[] CreatePolicy(string origin)
        {
            return new Policy[]
            {
                Policy.Handle<HttpRequestException>()
                    .WaitAndRetryAsync(_retryCount,retryAttempt => TimeSpan.FromSeconds(Math.Pow(2,retryAttempt))
                    , (exception, timeSpan, retryCount, context) =>
                    {
                        var msg = $"第{retryCount}次重试" +
                                  $"of {context.PolicyKey}" +
                                  $"at {context.ExecutionKey}" +
                                  $"due to: {exception}.";
                        _logger.LogWarning(msg);
                        _logger.LogDebug(msg);
                    }),
                Policy.Handle<HttpRequestException>()
                    .CircuitBreakerAsync(_exceptionCountAllowedBeforeBreaking,
                        TimeSpan.FromMinutes(1),
                        (exception, duration) =>
                        {
                            _logger.LogTrace("Circuit breaker opened");
                        },
                        () =>
                        {
                            _logger.LogTrace("Circuit breaker reset");
                        })
            };
        }
    }
}