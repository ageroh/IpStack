using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Novibet.IpStack.Client;
using Xunit;

namespace Novibet.Integration.Tests
{
    public class IpStackClientTests
    {
        private readonly IConfiguration _configuration;

        public IpStackClientTests()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
        }
        
        [Theory]
        [InlineData("79.129.229.233")]
        [InlineData("8.8.8.8")]
        public async Task IpStackInfoProvider_GetDetailsAsync_Real_Ip_Returns_Success(string ip)
        {
            var ipStackInfoProvider = new IpStackInfoClient(new System.Net.Http.HttpClient(), _configuration);

            var actual = await ipStackInfoProvider.GetDetailsAsync(ip);

            actual.Should().NotBeNull();
            actual.Country.Should().NotBeNullOrWhiteSpace();
            actual.City.Should().NotBeNullOrWhiteSpace();
            actual.Continent.Should().NotBeNullOrWhiteSpace();
        }
        
        [Fact]
        public async Task IpStackInfoProvider_GetDetailsAsync_With_InvalidIp_Returns_Empty()
        {
            var ipStackInfoProvider = new IpStackInfoClient(new System.Net.Http.HttpClient(), _configuration);
            var ip = "notvalidip";

            var actual = await ipStackInfoProvider.GetDetailsAsync(ip);

            actual.Should().NotBeNull();
            actual.Country.Should().BeNullOrWhiteSpace();
            actual.City.Should().BeNullOrWhiteSpace();
            actual.Continent.Should().BeNullOrWhiteSpace();
        }
        
        [Fact]
        public void IpStackInfoProvider_GetDetailsAsync_NotValidKey_Throws_Exception()
        {
            var configuration = new Mock<IConfiguration>();
            configuration.Setup(z => z.GetSection("BaseUrl").Value).Returns("http://api.ipstack.com/");
            
            // setup an invalid key.
            configuration.Setup(z => z.GetSection("ApiKey").Value).Returns("11111111");

            var ipStackInfoProvider = new IpStackInfoClient(new System.Net.Http.HttpClient(), configuration.Object);
            var ip = "8.8.8.8";

            // Act
            Func<Task> action = async () => { await ipStackInfoProvider.GetDetailsAsync(ip); };

            // Assert
            action.Should().Throw<IPServiceNotAvailableException>();
        }
    }
}
