using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;

namespace RaceDay.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IJwtService _jwtService;

        public AuthService(ApplicationDbContext context, IJwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            // Check if email already exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (existingUser != null)
            {
                throw new InvalidOperationException("Email already exists");
            }

            // Validate role
            if (dto.Role != "Organiser" && dto.Role != "Participant")
            {
                throw new InvalidOperationException("Role must be 'Organiser' or 'Participant'");
            }

            // Create user with hashed password
            var user = new User
            {
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                FullName = dto.FullName,
                Role = dto.Role,
                DateOfBirth = dto.DateOfBirth,
                Phone = dto.Phone,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Create the appropriate profile based on role
            if (dto.Role == "Organiser")
            {
                var organiserProfile = new OrganiserProfile
                {
                    UserId = user.UserId,
                    OrganisationName = dto.OrganisationName ?? dto.FullName,
                    Website = dto.Website,
                    CreatedAt = DateTime.UtcNow
                };
                _context.OrganiserProfiles.Add(organiserProfile);
            }
            else // Participant
            {
                var participantProfile = new ParticipantProfile
                {
                    UserId = user.UserId,
                    EmergencyContactName = dto.EmergencyContactName,
                    EmergencyContactPhone = dto.EmergencyContactPhone,
                    MedicalConditions = dto.MedicalConditions,
                    CreatedAt = DateTime.UtcNow
                };
                _context.ParticipantProfiles.Add(participantProfile);
            }

            await _context.SaveChangesAsync();

            // Generate JWT token
            var token = _jwtService.GenerateToken(user);

            return new AuthResponseDto
            {
                Token = token,
                UserId = user.UserId,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role,
                ExpiresAt = _jwtService.GetTokenExpiry()
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            // Find user by email
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
            {
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            // Generate JWT token
            var token = _jwtService.GenerateToken(user);

            return new AuthResponseDto
            {
                Token = token,
                UserId = user.UserId,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role,
                ExpiresAt = _jwtService.GetTokenExpiry()
            };
        }
    }
}