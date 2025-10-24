using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace EncrypCoin.API.Services.Base
{
    public abstract class BaseHttpService
    {
        private readonly HttpClient _client;
        private readonly ILogger _logger;

        protected BaseHttpService(HttpClient client, ILogger logger)
        {
            _client = client;
            _logger = logger;
        }

        protected async Task<T> GetJsonAsync<T>(string endpoint)
        {
            try
            {
                var response = await _client.GetAsync(endpoint);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();

                return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                })!;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Erro HTTP ao chamar endpoint: {Endpoint}", endpoint);
                throw new HttpRequestException(ex.Message);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Erro ao desserializar JSON do endpoint: {Endpoint}", endpoint);
                throw new JsonException(ex.Message);
            }
        }
    }
}

