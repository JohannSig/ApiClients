using FrozenForge.Apis;
using System;
using System.Net.Http;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IHttpClientBuilder AddApiClient<TClient>(this IServiceCollection services)
            where TClient : ApiClient
            => services.AddApiClient<TClient, TClient>();

        public static IHttpClientBuilder AddApiClient<IClient, TClient>(this IServiceCollection services)
            where TClient : ApiClient, IClient
            where IClient : class
        {
            return services.AddHttpClient<IClient, TClient>();
        }

        public static IHttpClientBuilder AddApiClient<TClient>(this IServiceCollection services, Action<IServiceProvider, HttpClient> configureClient)
            where TClient : ApiClient
            => services.AddApiClient<TClient, TClient>(configureClient);

        public static IHttpClientBuilder AddApiClient<IClient, TClient>(this IServiceCollection services, Action<IServiceProvider, HttpClient> configureClient)
            where TClient : ApiClient, IClient
            where IClient : class
        {
            return services.AddHttpClient<IClient, TClient>(configureClient);
        }
    }
}
