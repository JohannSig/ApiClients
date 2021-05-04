using FrozenForge.Apis.Tests.Controllers;
using FrozenForge.Apis.Tests.Integrations.Models;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Xunit;

namespace FrozenForge.Apis.Tests.Integrations
{
    public class ApiClientTests : IClassFixture<TestFixture>
    {
        public ApiClientTests(TestFixture fixture)
        {
            Fixture = fixture;
        }

        public TestFixture Fixture { get; }

        [Fact]
        public async Task ApiClient__CanPostToEndpointWithNoParameters()
        {      
            var apiClient = new ApiClient(this.Fixture.HttpClient);

            var uri = CreateEndpointUri(
                    nameof(TestController),
                    nameof(TestController.OkWithNoParameters));

            var response = await apiClient.PostAsync<TestResult>(uri);

            Assert.True(response.IsValid);
        }

        [Fact]
        public async Task ApiClient__CanPostToEndpointWithParameters()
        {
            var apiClient = new ApiClient(this.Fixture.HttpClient);

            var uri = CreateEndpointUri(
                    nameof(TestController),
                    nameof(TestController.OkWithParameters));

            var parameters = new TestParameters
            {
                Name = "Jóhann Sigurðsson"
            };

            var response = await apiClient.PostAsync<TestResult>(uri, parameters);

            Assert.True(response.IsValid);
        }

        [Fact]
        public async Task ApiClient__CanPostToEndpointWithParametersAndNoResults()
        {
            var apiClient = new ApiClient(this.Fixture.HttpClient);

            var uri = CreateEndpointUri(
                    nameof(TestController),
                    nameof(TestController.OkWithParameters));

            var parameters = new TestParameters
            {
                Name = "Jóhann Sigurðsson"
            };

            var response = await apiClient.PostAsync(uri, parameters);

            Assert.True(response.IsValid);
        }

        [Fact]
        public async Task ApiClient__CanPostToEndpointWithNoParametersAndNoResults()
        {
            var apiClient = new ApiClient(this.Fixture.HttpClient);

            var uri = CreateEndpointUri(
                    nameof(TestController),
                    nameof(TestController.OkWithNoParameters));

            var response = await apiClient.PostAsync(uri);

            Assert.True(response.IsValid);
        }

        public Uri CreateEndpointUri(
            string controllerName, 
            string endpointName)
            => CreateEndpointUri<object>(controllerName, endpointName, null);

        public Uri CreateEndpointUri<TParameters>(
            string controllerName, 
            string endpointName, 
            TParameters parameters)
            => new Uri(this.Fixture.TestServer.Services
                .GetService<LinkGenerator>()
                .GetPathByAction(
                    endpointName,
                    controllerName.Substring(0, controllerName.LastIndexOf("Controller")),
                    parameters),
                UriKind.Relative);
    }


}
