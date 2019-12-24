using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using Polly.Wrap;

namespace Resilience
{
    public class ResilienceHttpClient:IHttpClient
    {
        private readonly HttpClient _httpClient;
        private readonly Func<string, IEnumerable<Policy>> _policyCreator;
        private readonly ConcurrentDictionary<string, PolicyWrap> _policyWrappers;
        private ILogger<ResilienceHttpClient> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public ResilienceHttpClient(Func<string, IEnumerable<Policy>> policyCreator
            , ILogger<ResilienceHttpClient> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _policyCreator = policyCreator;
            _policyWrappers = new ConcurrentDictionary<string, PolicyWrap> ();
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _httpClient = new HttpClient();
        }

        public async Task<HttpResponseMessage> PostAsync<T>(string url, T item, string authorizationToken=null,
            string requestId = null,
            string authorizationMethod = "Bearer")
        {
            return await DoPostAsync(HttpMethod.Post, url, CreateHttpContent(item), authorizationToken, requestId, authorizationMethod);
        }

        public async Task<HttpResponseMessage> PostAsync(string url, Dictionary<string, string> form,
            string authorizationToken = null,
            string requestId = null,
            string authorizationMethod = "Bearer")
        {
            return await DoPostAsync(HttpMethod.Post, url, CreateHttpContent(form), authorizationToken, requestId, authorizationMethod);
        }

        public Task<HttpResponseMessage> DoPostAsync( HttpMethod method,string url, HttpContent httpContent, string authorizationToken, string requestId = null,
            string authorizationMethod = "Bearer")
        {
            if (method != HttpMethod.Post && method != HttpMethod.Put)
            {
                throw new ArgumentException("Value must be either post or put",nameof(method));
            }

            var origin = GetOriginFromUri(url);
            return HttpInvoker(origin, async () =>
            {
                var requestMessage = new HttpRequestMessage(method, url);
                SetAuthorizationHeader(requestMessage);
                requestMessage.Content = httpContent;
                if (authorizationToken != null)
                {
                    requestMessage.Headers.Authorization = new AuthenticationHeaderValue(authorizationMethod,authorizationToken);
                }

                if (requestId != null)
                { 
                    requestMessage.Headers.Add("x-requestid",requestId);
                }

                var response = await _httpClient.SendAsync(requestMessage);
                if (response.StatusCode == HttpStatusCode.InternalServerError)
                {
                    throw new HttpRequestException();
                }

                return response; 
            });
        }

        private HttpContent CreateHttpContent<T>( T item)
        {
            return new StringContent(JsonConvert.SerializeObject(item),Encoding.UTF8,"application/json");
        }

        private HttpContent CreateHttpContent(Dictionary<string,string> form)
        {
            return new FormUrlEncodedContent(form);
        }
        private async Task<T> HttpInvoker<T>(string origin, Func<Task<T>> action)
        {
            var normalizedOrigin = NormalizeOrigin(origin);
            if (!_policyWrappers.TryGetValue(normalizedOrigin,out PolicyWrap policyWrap))
            {
                policyWrap = Policy.Wrap(_policyCreator(normalizedOrigin).ToArray());
                _policyWrappers.TryAdd(normalizedOrigin, policyWrap);
            }

            return await policyWrap.ExecuteAsync(action, new Context(normalizedOrigin));
        }

        private static string NormalizeOrigin(string origin)
        {
            return origin?.Trim()?.ToLower();
        }

        private static string GetOriginFromUri(string uri)
        {
            var url = new Uri(uri);
            var origin = $"{url.Scheme}://{url.DnsSafeHost}:{url.Port}";
            return origin;
        }

        private void SetAuthorizationHeader(HttpRequestMessage requestMessage)
        {
            var authorizationHeader = _httpContextAccessor.HttpContext.Request.Headers["Authorization"];
            if (!string.IsNullOrEmpty(authorizationHeader))
            {
                requestMessage.Headers.Add("Authorization",new List<string>(){authorizationHeader});
            }
        }
    }
}