using AuthApi.Models;
using System.Net.Http.Json;
using Newtonsoft.Json;
using FluentAssertions;
using System.Net.Http.Headers;
using System.Net;
using NumismaticClub.Models;
using System.Text;

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

        [Theory]
        [InlineData("User", "12345")] // Correct
        [InlineData("User", "123456")] // Same name
        [InlineData("", "12345")] // Empty name
        public async Task CreateUser_ShouldCreateUserInAuthAndUser(
            string login,
            string password)
        {
            var tokens = await SignupAndSignin(login, password);

            await GetProfilePage(tokens.AccessToken);

        }

        [Fact]
        public async Task RefreshJwt_GetNewAccessByRefresh()
        {
            // Arrange
            var tokens = await SignupAndSignin("Ivan", "12345");

            // Act
            var refreshResponse = await _httpClient.PostAsJsonAsync("/auth/refresh", tokens);

            // Assert
            refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var responseContent = await refreshResponse.Content.ReadAsStringAsync();
            var newTokens = JsonConvert.DeserializeObject<TokenModel>(responseContent);

            newTokens.AccessToken.Should().NotBeNullOrWhiteSpace();
            newTokens.RefreshToken.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task DeleteUser_DeleteFromAuthAndUser()
        {
            string login = "ZOZO";
            string password = "12345";

            // Arrange
            var tokens = await SignupAndSignin(login, password);
            var user = await GetProfilePage(tokens.AccessToken);

            var deleteRequest = new HttpRequestMessage(
                HttpMethod.Delete, $"/auth/{user.Id}/delete");

            deleteRequest.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer", tokens.AccessToken);

            // Act 1
            var deleteResponse = await _httpClient.SendAsync(deleteRequest);

            // Assert 1
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Act 2
            var signinResponse = await _httpClient.PostAsJsonAsync("/auth/signin", new AuthInfo
            {
                Login = login,
                Password = password
            });

            // Assert 2
            signinResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        public static IEnumerable<object[]> GetCoinData()
        {
            // Correct
            yield return new object[]
            {
                new Coin { Id = "", Year = 1999, Value = 50, Country = "Turkey" }
            };

            // Empty country
            yield return new object[]
            {
                new Coin { Id = "", Year = 2020, Value = 100, Country = "" }
            };

            // Without year
            yield return new object[]
            {
                new Coin { Id = "", Value = 100, Country = "USA" }
            };
        }

        [Theory]
        [MemberData(nameof(GetCoinData))]
        public async Task CreateCoin_AddToUserAndSetConfirmed(Coin coin)
        {
            // Arrange
            var tokens = await SignupAndSignin("Vovka", "1234");

            var createCoin = new HttpRequestMessage(HttpMethod.Post, "/coins") 
            { 
                Content = new StringContent(
                    JsonConvert.SerializeObject(coin), Encoding.UTF8, "application/json")
            };

            createCoin.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer", tokens.AccessToken);

            // Act 1
            var createCoinResponse = await _httpClient.SendAsync(createCoin);

            // Assert 1
            createCoinResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            
            var createCoinContent = await createCoin.Content.ReadAsStringAsync();
            var coinId = JsonConvert.DeserializeObject<Coin>(createCoinContent).Id;

            // Act 2
            bool addedToUser = false;
            for (int i = 0; i < 5; i++)
            {                
                var user = await GetProfilePage(tokens.AccessToken);

                // TODO: reg obj changed in db, but no here
                if (user.RegisteredObjects == 1) 
                { 
                    addedToUser = true;
                    break;
                }

                await Task.Delay(10000);
            }

            // Assert 2
            addedToUser.Should().BeTrue();

            // Act 3
            bool addedConfirmed = false;
            for (int i = 0; i < 5; i++)
            {
                var getCoin = await _httpClient.GetAsync($"coins/{coinId}");

                if (getCoin.IsSuccessStatusCode)
                {
                    var updatedCoin = JsonConvert.DeserializeObject<Coin>(
                        await getCoin.Content.ReadAsStringAsync());
                    updatedCoin.Should().NotBeNull();
                    
                    if (updatedCoin.Confirmed != "Not confirmed.")
                    {
                        addedConfirmed = true;
                        break;
                    }
                }

                await Task.Delay(2000);
            }

            addedConfirmed.Should().BeTrue();
        }

        private async Task<TokenModel> SignupAndSignin(string login, string password)
        {
            // Arrange
            var authInfo = new AuthInfo { Login = login, Password = password };

            // Act 1
            var signupResponse = await _httpClient.PostAsJsonAsync("/auth/signup", authInfo);

            // Assert 1
            signupResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            // Act 2
            var signinResponse = await _httpClient.PostAsJsonAsync("/auth/signin", authInfo);

            // Assert 2
            signinResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // Get tokens
            var responseContent = await signinResponse.Content.ReadAsStringAsync();
            var tokens = JsonConvert.DeserializeObject<TokenModel>(responseContent);

            // Check tokens
            tokens.AccessToken.Should().NotBeNullOrWhiteSpace();
            tokens.RefreshToken.Should().NotBeNullOrWhiteSpace();

            return tokens;
        }

        private async Task<UserApi.Models.User> GetProfilePage(string accessToken)
        {
            var user = new UserApi.Models.User();

            bool profileCreated = false;
            for (int i = 0; i < 5; i++)
            {
                // Arrange
                var profileRequest = new HttpRequestMessage(
                    HttpMethod.Get, "/users/my-profile");

                profileRequest.Headers.Authorization = new AuthenticationHeaderValue(
                    "Bearer", accessToken);

                // Act
                var profileResponse = await _httpClient.SendAsync(profileRequest);

                // Assert
                if (profileResponse.IsSuccessStatusCode)
                {
                    user = JsonConvert.DeserializeObject<UserApi.Models.User>(
                        await profileResponse.Content.ReadAsStringAsync());
                    user.Should().NotBeNull();
                    user.Id.Should().NotBeNullOrWhiteSpace();
                    profileCreated = true;
                    break;
                }

                // Wait if page not found
                await Task.Delay(1000);
            }

            profileCreated.Should().BeTrue();

            return user;
        }

        
    }
}