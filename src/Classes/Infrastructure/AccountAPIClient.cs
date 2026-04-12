using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;


using Application.Interfaces;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure
{
    public record UserResponse
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    public class AccountAPIClient : IAccountAPIClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AccountAPIClient> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AccountAPIClient(HttpClient httpClient, ILogger<AccountAPIClient> logger, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        private void SetAuthTokenToHttp()
        {
            // Forward the incoming Bearer token
            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(token))
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token.Replace("Bearer ", ""));

        }

        public async Task<bool> InstructorExistsAsync(Guid instructorId)
        {
            try
            {
                SetAuthTokenToHttp();

                var response = await _httpClient.GetAsync($"/api/users/exists/{instructorId}");
                if (response.IsSuccessStatusCode)
                {
                    var exists = await response.Content.ReadFromJsonAsync<bool>();
                    return exists;
                }
                _logger.LogWarning("AccountAPI returned {StatusCode} for instructor existence check", response.StatusCode);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to call AccountAPI for instructor existence");
                return false;
            }
        }



        public async Task<string?> GetInstructorNameAsync(Guid instructorId)
        {
            try
            {
                SetAuthTokenToHttp();

                var response = await _httpClient.GetAsync($"/api/users/{instructorId}");
                if (response.IsSuccessStatusCode)
                {
                    var user = await response.Content.ReadFromJsonAsync<UserResponse>();
                    return user?.Username;
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch instructor name from AccountAPI");
                return null;
            }
        }
    }
}
