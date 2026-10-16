using Microsoft.EntityFrameworkCore;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using RaceDay.API.Services;
using RaceDay.Tests.Helpers;
using Xunit;

namespace RaceDay.Tests
{
    public class AuthServiceTests
    {
        private static JwtSettings GetJwtSettings() => new JwtSettings
        {
            Key = "ThisIsMySuperSecretKeyForRaceDayApplication2026!MustBeAtLeast32CharactersLong",
            Issuer = "RaceDayAPI",
            Audience = "RaceDayClient",
            ExpiryInMinutes = 60
        };

        // Test 1: Register as Organiser succeeds
        [Fact]
        public async Task Register_Organiser_CreatesUserAndProfile()
        {
            using var context = TestDbContextFactory.Create();
            var jwt = new JwtService(GetJwtSettings());
            var service = new AuthService(context, jwt);

            var dto = new RegisterDto
            {
                Email = "organiser@test.com",
                Password = "Password123",
                FullName = "Test Organiser",
                Role = "Organiser",
                OrganisationName = "Test Org"
            };

            var result = await service.RegisterAsync(dto);

            Assert.NotNull(result);
            Assert.Equal("organiser@test.com", result.Email);
            Assert.Equal("Organiser", result.Role);
            Assert.NotEmpty(result.Token);

            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == "organiser@test.com");
            Assert.NotNull(user);
            Assert.NotEqual("Password123", user!.PasswordHash);

            var profile = await context.OrganiserProfiles
                .FirstOrDefaultAsync(p => p.UserId == user.UserId);
            Assert.NotNull(profile);
        }

        // Test 2: Login with correct credentials succeeds
        [Fact]
        public async Task Login_WithValidCredentials_ReturnsToken()
        {
            using var context = TestDbContextFactory.Create();
            var jwt = new JwtService(GetJwtSettings());
            var service = new AuthService(context, jwt);

            await service.RegisterAsync(new RegisterDto
            {
                Email = "login@test.com",
                Password = "Password123",
                FullName = "Login User",
                Role = "Participant"
            });

            var result = await service.LoginAsync(new LoginDto
            {
                Email = "login@test.com",
                Password = "Password123"
            });

            Assert.NotNull(result);
            Assert.NotEmpty(result.Token);
            Assert.Equal("Participant", result.Role);
        }

        // Test 3: Login with wrong password fails
        [Fact]
        public async Task Login_WithWrongPassword_ThrowsUnauthorized()
        {
            using var context = TestDbContextFactory.Create();
            var jwt = new JwtService(GetJwtSettings());
            var service = new AuthService(context, jwt);

            await service.RegisterAsync(new RegisterDto
            {
                Email = "wrong@test.com",
                Password = "Password123",
                FullName = "Wrong User",
                Role = "Participant"
            });

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                service.LoginAsync(new LoginDto
                {
                    Email = "wrong@test.com",
                    Password = "WrongPassword"
                }));
        }

        // Test 4: Duplicate email registration fails
        [Fact]
        public async Task Register_DuplicateEmail_ThrowsException()
        {
            using var context = TestDbContextFactory.Create();
            var jwt = new JwtService(GetJwtSettings());
            var service = new AuthService(context, jwt);

            var dto = new RegisterDto
            {
                Email = "dup@test.com",
                Password = "Password123",
                FullName = "First User",
                Role = "Participant"
            };
            await service.RegisterAsync(dto);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.RegisterAsync(dto));
        }

        // Test 5: Password is hashed
        [Fact]
        public async Task Register_PasswordIsHashed()
        {
            using var context = TestDbContextFactory.Create();
            var jwt = new JwtService(GetJwtSettings());
            var service = new AuthService(context, jwt);

            var plainPassword = "MySecret123";

            await service.RegisterAsync(new RegisterDto
            {
                Email = "hash@test.com",
                Password = plainPassword,
                FullName = "Hash Test",
                Role = "Participant"
            });

            var user = await context.Users.FirstAsync(u => u.Email == "hash@test.com");

            Assert.NotEqual(plainPassword, user.PasswordHash);
            Assert.True(user.PasswordHash.Length > 20);
        }
    }
}