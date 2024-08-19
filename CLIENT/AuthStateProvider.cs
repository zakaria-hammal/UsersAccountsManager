using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace CLIENT
{
    public class AuthStateProvider : AuthenticationStateProvider
    {
        private readonly HttpClient _http;
        private readonly ILocalStorageService _localStorageService;

        public AuthStateProvider (HttpClient http, ILocalStorageService localStorageService)
        {
            _http = http;
            _localStorageService = localStorageService;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.

            string refreshToken = await _localStorageService.GetItemAsync<string>("Token");
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

            string token = "";
            var identity = new ClaimsIdentity();

            if (!string.IsNullOrEmpty(refreshToken))
            {
                var response = await _http.PostAsJsonAsync("api/Account/GetJwtToken", refreshToken);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    json = json.ToString();
            
                    for (int i = 1; i < json.Length - 1; i++)
                    {
                        token += json[i].ToString();
                    }

                    identity = new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt");
                    _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
                else
                {
                    await _localStorageService.RemoveItemAsync("Token");
                    var response1 = await _http.PostAsJsonAsync("/api/Account/logout", refreshToken);
                }
                
            }

            var user = new ClaimsPrincipal(identity);
            var state = new AuthenticationState(user);

            if (!string.IsNullOrEmpty(token))
            {
                var jwt = new JwtSecurityToken(token);

                if(jwt.ValidTo < DateTime.Now)
                {
                    var response = await _http.PostAsJsonAsync("/api/Account/logout", refreshToken);
                }
            }

            NotifyAuthenticationStateChanged(Task.FromResult(state));

            return state;
        }

        public static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var payload = jwt.Split('.')[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);
#pragma warning disable CS8604 // Possible null reference argument.
            return keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString()));
#pragma warning restore CS8604 // Possible null reference argument.
        }

        private static byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }
    }
}