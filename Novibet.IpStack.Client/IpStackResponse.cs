using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Novibet.IpStack.Abstractions;

namespace Novibet.IpStack.Client
{
    // http://json2csharp.com/ 
    public class IpStackResponse : IPDetails
    {
        [JsonProperty("ip")]
        public string Ip { get; set; }

        [JsonProperty("continent_name")]
        public string Continent { get; set; }

        [JsonProperty("country_name")]
        public string Country { get; set; }

        [JsonProperty("region_name")]
        public string Region { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("latitude")]
        public double Latitude { get; set; }
        
        [JsonProperty("longitude")]
        public double Longitude { get; set; }

        [JsonProperty("success")]
        public string Success { get; set; }

    }
}
