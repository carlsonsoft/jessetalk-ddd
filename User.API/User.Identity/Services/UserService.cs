using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DnsClient;
using Microsoft.Extensions.Options;
using Resilience;
using User.Identity.Entities;

namespace User.Identity.Services
{
    public class UserService : IUserService
    {
        private readonly IHttpClient _httpClient;
        private readonly string _userServiceUrl;

        public UserService(IHttpClient httpClient,IDnsQuery dns, IOptions<ServiceDiscoveryOptions> options)
        {
            _httpClient = httpClient;
            var result = dns.ResolveService("service.consul", options.Value.ServiceName);

            var addressList = result.First().AddressList;
            var address = addressList.Any() ? addressList.First().ToString() : result.First().HostName;
            var port = result.First().Port;
            _userServiceUrl = $"http://{address.TrimEnd('.')}:{port}";
        }
           
        public async Task<int> CheckOrCreate(string phone)
        {
            var formValuePair = new KeyValuePair<string, string>[]
            {
                new KeyValuePair<string, string>("phone", phone)
            };
            var response =await _httpClient.PostAsync(_userServiceUrl + "/api/users/check-or-create",
                new FormUrlEncodedContent(formValuePair));
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var userId = await response.Content.ReadAsStringAsync();
                int.TryParse(userId, out int intUserId);
                return intUserId;
            }

            return 0;
        }
    }
}