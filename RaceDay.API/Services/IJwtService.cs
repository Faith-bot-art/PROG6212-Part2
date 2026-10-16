using RaceDay.API.Models;

namespace RaceDay.API.Services
{
    public interface IJwtService
    {
        string GenerateToken(User user);
        DateTime GetTokenExpiry();
    }
}