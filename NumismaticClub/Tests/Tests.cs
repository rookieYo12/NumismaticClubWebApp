using AuthApi.Models;
using System.Net.Http.Json;
using FluentAssertions;
using System.Text.Json;

namespace Tests
{
    public class Tests
    {
        private readonly HttpClient _httpClient;
        private readonly string _gatewayUrl = "http://localhost:5290";

        [Fact]
        public async Task RegisterUser_ShouldCreateUserInAuthAndUser()
        {
            // Arrange
            var authInfo = new AuthInfo
            {
                Login = "Test",
                Password = "testtest"
            };

            // Act 1: registration request
            var regResponse = await _httpClient.PostAsJsonAsync(
                $"{_gatewayUrl}/auth/signup", authInfo);

            // Assert 1: check registration response
            regResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);

            // Act 2: signin request
            var signinRequest = await _httpClient.PostAsJsonAsync(
                $"{_gatewayUrl}/auth/signin", authInfo);

            // Assert 2: check signin request
            signinRequest.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            // Get tokens
            var signinResponse = await signinRequest.Content.ReadAsStringAsync();
            var tokens = JsonSerializer.Deserialize<TokenModel>(signinResponse);
            
            // Check tokens
            tokens.AccessToken.Should().NotBeNullOrWhiteSpace();
            tokens.RefreshToken.Should().NotBeNullOrWhiteSpace();

            // TODO: add kafka, and decode tokens
            // https://chatgpt.com/share/676536da-675c-8004-a683-e7bf790ee05f

            // Act 2: check that the user card has appeared
            await Task.Delay(5000);
        }
    }
}