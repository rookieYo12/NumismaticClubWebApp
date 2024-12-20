using AuthApi.Models;
using UserApi.Models;
using System.Net.Http.Json;
using Newtonsoft.Json;
using FluentAssertions;
using System.Text.Json;
using System.Net.Http.Headers;

namespace Tests
{
    public class Tests
    {
        private readonly HttpClient _httpClient;

        public Tests()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:8080")
            };
        }

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
                "/auth/signup", authInfo);

            // Assert 1: check registration response
            regResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);

            // Act 2: signin request
            var signinRequest = await _httpClient.PostAsJsonAsync(
                "/auth/signin", authInfo);

            // Assert 2: check signin request
            signinRequest.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            // Get tokens
            var signinResponse = await signinRequest.Content.ReadAsStringAsync();
            var tokens = JsonConvert.DeserializeObject<TokenModel>(signinResponse);
            
            // Check tokens
            tokens.AccessToken.Should().NotBeNullOrWhiteSpace();
            tokens.RefreshToken.Should().NotBeNullOrWhiteSpace();

            // Assert 3: check profile page
            bool profileCreated = false;
            for (int i = 0; i < 5; i++)
            {
                var profileRequest = new HttpRequestMessage(
                    HttpMethod.Get, "/users/my-profile");

                profileRequest.Headers.Authorization = new AuthenticationHeaderValue(
                    "Bearer", tokens.AccessToken);

                var profileResponse = await _httpClient.SendAsync(profileRequest);
                
                if (profileResponse.IsSuccessStatusCode)
                {
                    var user = JsonConvert.DeserializeObject<UserApi.Models.User>(
                        await profileResponse.Content.ReadAsStringAsync());
                    user.Should().NotBeNull();
                    user.Id.Should().NotBeNullOrWhiteSpace();
                    profileCreated = true;
                    break;
                }

                await Task.Delay(1000);
            }

            profileCreated.Should().BeTrue();
        }
    }
}