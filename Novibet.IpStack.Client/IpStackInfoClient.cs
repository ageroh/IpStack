using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Novibet.IpStack.Abstractions;

namespace Novibet.IpStack.Client
{
    public class IpStackInfoClient : IIPInfoProvider
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _apiKey;
        private const string NotSuccess = "false";

        /// <summary>
        /// Instantiate an IpStackInfor Provider
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="configuration">Needs to provide BaseUrl and ApiKey strings</param>
        public IpStackInfoClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            
            var baseUrl = _configuration.GetSection("BaseUrl").Value;
            _httpClient.BaseAddress = new Uri(baseUrl);
            _apiKey = _configuration.GetSection("ApiKey").Value;
        }

        public IPDetails GetDetails(string ip)
        {
            var ipDetails = GetDetailsAsync(ip).GetAwaiter().GetResult();

            return ipDetails;
        }

        public async Task<IPDetails> GetDetailsAsync(string ip)
        {
            var response = await _httpClient.GetAsync($"/{ip}?access_key={_apiKey}");

            if (!response.IsSuccessStatusCode)
            {
                throw new IPServiceNotAvailableException($"IpStackInfoProvider exception occurred, status code: {response.StatusCode}");
            }

            var jsonResult = await response.Content.ReadAsStringAsync();

            var ipDetails = ConvertToIpDetails(jsonResult);

            return ipDetails;
        }

        private IPDetails ConvertToIpDetails(string response)
        {
            var settings = new JsonSerializerSettings()
            {
                DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
                NullValueHandling = NullValueHandling.Ignore,
            };

            var ipStackResponse = JsonConvert.DeserializeObject<IpStackResponse>(response, settings);

            if(ipStackResponse.Success == NotSuccess)
            {
                throw new IPServiceNotAvailableException($"Some invalid properties where passed to provider.");
            }

            return ipStackResponse;
        }
    }
}
