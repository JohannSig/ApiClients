using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace FrozenForge.Apis
{
    public class ApiClient
    {
        public ApiClient(HttpClient httpClient)
            : this(new NullLogger<ApiClient>(), httpClient)
        {
        }

        public ApiClient(
            ILogger logger,
            HttpClient httpClient)
        {
            this.Logger = logger;
            this.HttpClient = httpClient;
        }

        public ILogger Logger { get; }
        public HttpClient HttpClient { get; }

        public Task<ApiResponse> PostAsync(Uri uri)
            => PostAsync<object>(uri)
            .ContinueWith(x => x.Result as ApiResponse);


        public Task<ApiResponse<TData>> PostAsync<TData>(Uri uri)
            => SendAsync<TData>(
                uri,
                HttpMethod.Post,
                CancellationToken.None);

        public Task<ApiResponse<TData>> PostAsync<TData>(
            Uri uri,
            CancellationToken cancellationToken)
            => SendAsync<TData>(
                uri, 
                HttpMethod.Post, 
                cancellationToken);

        public Task<ApiResponse<TData>> SendAsync<TData>(
            Uri uri,
            HttpMethod method,
            CancellationToken cancellationToken)
            => SendAsync<TData>(
                uri,
                content: default,
                method,
                cancellationToken);

        public Task<ApiResponse> PostAsync(
            Uri uri,
            object parameters)
            => PostAsync(
                uri,
                parameters,
                CancellationToken.None);

        public Task<ApiResponse> PostAsync(
            Uri uri,
            object parameters,
            CancellationToken cancellationToken)
            => PostAsync<object>(
                uri,
                parameters,
                cancellationToken)
            .ContinueWith(x => x.Result as ApiResponse, cancellationToken);

        public Task<ApiResponse<TData>> PostAsync<TData>(
            Uri uri,
            object parameters)
            => PostAsync<TData>(uri, parameters, CancellationToken.None);

        public Task<ApiResponse<TData>> PostAsync<TData>(
            Uri uri,
            object parameters,
            CancellationToken cancellationToken)
            => SendAsync<TData>(
                uri,
                parameters,
                new MediaTypeHeaderValue(MediaTypeNames.Application.Json),
                HttpMethod.Post,
                cancellationToken);

        public Task<ApiResponse<TData>> SendAsync<TData>(
            Uri uri,
            object parameters,
            HttpMethod method,
            CancellationToken cancellationToken)
            => SendAsync<TData>(
                uri,
                parameters,
                new MediaTypeHeaderValue(MediaTypeNames.Application.Json),
                method,
                cancellationToken);

        public Task<ApiResponse<TData>> SendAsync<TData>(
            Uri uri,
            object parameters,
            MediaTypeHeaderValue mediaTypeHeaderValue,
            HttpMethod method,
            CancellationToken cancellationToken)
            => SendAsync<TData>(
                uri,
                ToContent(parameters, mediaTypeHeaderValue),
                method,
                cancellationToken);

        /// <summary>
        /// An extension method for wrapping a message received by <see cref="HttpClient"/>.
        /// </summary>
        /// <typeparam name="TData">The type of data that will be deserialized into <see cref="RestResponse{TResponse}.Data"/>.</typeparam>
        /// <param name="httpClient">The <see cref="HttpClient"/> to use.</param>
        /// <param name="uri">The <see cref="Uri"/> to connect to. Can be relative, if <see cref="HttpClient.BaseAddress"/> has been set.</param>
        /// <param name="content">The <see cref="HttpContent"/> to send.</param>
        /// <param name="method">The <see cref="HttpMethod"/> (POST or GET) to use.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        public async Task<ApiResponse<TData>> SendAsync<TData>(
            Uri uri,
            HttpContent content,
            HttpMethod method,
            CancellationToken cancellationToken)
        {
            using var request = new HttpRequestMessage(method, uri)
            {
                Content = content,
            };

            try
            {
                using var httpResponse = await HttpClient.SendAsync(request, cancellationToken);

                // Success? Yay!
                if (httpResponse.IsSuccessStatusCode)
                {
                    return new ApiResponse<TData>
                    {
                        StatusCode = httpResponse.StatusCode,
                        ReasonPhrase = httpResponse.ReasonPhrase,
                        Data = await ToData<TData>(httpResponse.Content, cancellationToken),
                    };
                }

                // Errors? Boo!
                else
                {
                    return new ApiResponse<TData>
                    {
                        StatusCode = httpResponse.StatusCode,
                        ReasonPhrase = httpResponse.ReasonPhrase,
                        Body = await GetBodyAsync(httpResponse.Content, cancellationToken)
                    };
                }
            }
            catch (Exception exception)
            {
                this.Logger.LogError(exception, $"An exception occurred while making a {method} request to {uri} with content {content}");

                return new ApiResponse<TData>
                {
                    ReasonPhrase = $"An exception occurred while making a {method} request to {uri}.",
                    Body = exception.ToString()
                };
            }
        }

        private static Task<string> GetBodyAsync(HttpContent content, CancellationToken cancellationToken)
        {
            if (content.Headers.ContentLength is null)
            {
                return Task.FromResult(default(string));
            }

            if (content.Headers.ContentLength == 0)
            {
                return Task.FromResult(string.Empty);
            }

            return content.ReadAsStringAsync(cancellationToken);
        }

        private static HttpContent ToContent(object model, MediaTypeHeaderValue mediaTypeHeaderValue)
            => mediaTypeHeaderValue.MediaType switch
            {
                MediaTypeNames.Application.Json => JsonContent.Create(model, model.GetType(), mediaTypeHeaderValue),

                _ => throw new SerializationException($"Unsupported media type header value: {mediaTypeHeaderValue}."),
            };

        private static Task<TData> ToData<TData>(HttpContent content, CancellationToken cancellationToken)
        {
            if (content.Headers.ContentType is null || content.Headers.ContentLength is null or <= 0)
            {
                return default;
            }

            return content.Headers.ContentType.MediaType switch
            {
                MediaTypeNames.Application.Json => content.ReadFromJsonAsync<TData>(options: null, cancellationToken),

                _ => throw new SerializationException($"Unknown content type: {content.Headers.ContentType}")
            };
        }
    }
}
