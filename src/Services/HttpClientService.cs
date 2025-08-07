using System.Collections.Concurrent;
using System.Net.Http.Headers;

namespace CCXT.Simple.Services
{
    /// <summary>
    /// HttpClient management service for efficient connection pooling
    /// </summary>
    public class HttpClientService : IDisposable
    {
        private readonly ConcurrentDictionary<string, HttpClient> _httpClients;
        private readonly ConcurrentDictionary<string, DateTimeOffset> _lastUsed;
        private readonly TimeSpan _clientLifetime;
        private readonly Timer _cleanupTimer;
        private bool _disposed;

        /// <summary>
        /// Default instance for singleton pattern
        /// </summary>
        private static readonly Lazy<HttpClientService> _instance = new Lazy<HttpClientService>(() => new HttpClientService());
        
        public static HttpClientService Instance => _instance.Value;

        public HttpClientService(TimeSpan? clientLifetime = null)
        {
            _httpClients = new ConcurrentDictionary<string, HttpClient>();
            _lastUsed = new ConcurrentDictionary<string, DateTimeOffset>();
            _clientLifetime = clientLifetime ?? TimeSpan.FromMinutes(5);
            
            // Cleanup timer to remove unused clients
            _cleanupTimer = new Timer(CleanupUnusedClients, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
        }

        /// <summary>
        /// Get or create an HttpClient for a specific exchange
        /// </summary>
        /// <param name="exchangeName">Name of the exchange</param>
        /// <param name="baseUrl">Base URL for the exchange API</param>
        /// <param name="configureClient">Optional action to configure the client</param>
        /// <returns>HttpClient instance</returns>
        public HttpClient GetClient(string exchangeName, string baseUrl = null, Action<HttpClient> configureClient = null)
        {
            var key = exchangeName;
            
            var client = _httpClients.GetOrAdd(key, k =>
            {
                var handler = new HttpClientHandler
                {
                    AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate,
                    AllowAutoRedirect = false,
                    UseProxy = false
                };

                var newClient = new HttpClient(handler)
                {
                    Timeout = TimeSpan.FromSeconds(30)
                };

                if (!string.IsNullOrEmpty(baseUrl))
                {
                    newClient.BaseAddress = new Uri(baseUrl);
                }

                // Set default headers
                newClient.DefaultRequestHeaders.Accept.Clear();
                newClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                
                // Apply custom configuration if provided
                configureClient?.Invoke(newClient);

                return newClient;
            });

            // Update last used time
            _lastUsed.AddOrUpdate(key, DateTimeOffset.UtcNow, (k, v) => DateTimeOffset.UtcNow);

            return client;
        }

        /// <summary>
        /// Create a new HttpRequestMessage with proper headers
        /// </summary>
        public HttpRequestMessage CreateRequest(HttpMethod method, string requestUri)
        {
            return new HttpRequestMessage(method, requestUri);
        }

        /// <summary>
        /// Send an HTTP request with automatic retry logic
        /// </summary>
        public async Task<HttpResponseMessage> SendAsync(HttpClient client, HttpRequestMessage request, int maxRetries = 3)
        {
            HttpResponseMessage response = null;
            
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    // Clone the request for retry
                    var clonedRequest = await CloneHttpRequestMessageAsync(request);
                    response = await client.SendAsync(clonedRequest);
                    
                    if (response.IsSuccessStatusCode)
                        return response;
                    
                    // Don't retry on client errors (4xx)
                    if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                        return response;
                    
                    // Retry on server errors (5xx) or timeout
                    if (i < maxRetries - 1)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, i))); // Exponential backoff
                    }
                }
                catch (HttpRequestException) when (i < maxRetries - 1)
                {
                    await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, i)));
                }
                catch (TaskCanceledException) when (i < maxRetries - 1)
                {
                    await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, i)));
                }
            }
            
            return response;
        }

        /// <summary>
        /// Clone HttpRequestMessage for retry
        /// </summary>
        private async Task<HttpRequestMessage> CloneHttpRequestMessageAsync(HttpRequestMessage req)
        {
            var clone = new HttpRequestMessage(req.Method, req.RequestUri);

            // Copy headers
            foreach (var header in req.Headers)
            {
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            // Copy content
            if (req.Content != null)
            {
                var ms = new MemoryStream();
                await req.Content.CopyToAsync(ms);
                ms.Position = 0;
                clone.Content = new StreamContent(ms);

                if (req.Content.Headers != null)
                {
                    foreach (var header in req.Content.Headers)
                    {
                        clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }
            }

            return clone;
        }

        /// <summary>
        /// Remove an HttpClient from the pool
        /// </summary>
        public void RemoveClient(string exchangeName)
        {
            if (_httpClients.TryRemove(exchangeName, out var client))
            {
                client?.Dispose();
            }
            _lastUsed.TryRemove(exchangeName, out _);
        }

        /// <summary>
        /// Clean up unused clients periodically
        /// </summary>
        private void CleanupUnusedClients(object state)
        {
            var cutoffTime = DateTimeOffset.UtcNow.Subtract(_clientLifetime);
            
            foreach (var kvp in _lastUsed)
            {
                if (kvp.Value < cutoffTime)
                {
                    RemoveClient(kvp.Key);
                }
            }
        }

        /// <summary>
        /// Clear all cached HttpClients
        /// </summary>
        public void ClearAll()
        {
            foreach (var client in _httpClients.Values)
            {
                client?.Dispose();
            }
            
            _httpClients.Clear();
            _lastUsed.Clear();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _cleanupTimer?.Dispose();
                ClearAll();
                _disposed = true;
            }
        }
    }

    /// <summary>
    /// Extension methods for HttpClient
    /// </summary>
    public static class HttpClientExtensions
    {
        /// <summary>
        /// Send GET request with retry logic
        /// </summary>
        public static async Task<HttpResponseMessage> GetWithRetryAsync(this HttpClient client, string requestUri, HttpClientService service = null)
        {
            service = service ?? HttpClientService.Instance;
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            return await service.SendAsync(client, request);
        }

        /// <summary>
        /// Send POST request with retry logic
        /// </summary>
        public static async Task<HttpResponseMessage> PostWithRetryAsync(this HttpClient client, string requestUri, HttpContent content, HttpClientService service = null)
        {
            service = service ?? HttpClientService.Instance;
            var request = new HttpRequestMessage(HttpMethod.Post, requestUri) { Content = content };
            return await service.SendAsync(client, request);
        }

        /// <summary>
        /// Send DELETE request with retry logic
        /// </summary>
        public static async Task<HttpResponseMessage> DeleteWithRetryAsync(this HttpClient client, string requestUri, HttpClientService service = null)
        {
            service = service ?? HttpClientService.Instance;
            var request = new HttpRequestMessage(HttpMethod.Delete, requestUri);
            return await service.SendAsync(client, request);
        }
    }
}