using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;

namespace RaceDay.API.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UserProfileDto?> GetProfileAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.OrganiserProfile)
                .Include(u => u.ParticipantProfile)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null) return null;

            var profile = new UserProfileDto
            {
                UserId = user.UserId,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role,
                DateOfBirth = user.DateOfBirth,
                Phone = user.Phone,
                CreatedAt = user.CreatedAt
            };

            if (user.Role == "Organiser" && user.OrganiserProfile != null)
            {
                profile.OrganisationName = user.OrganiserProfile.OrganisationName;
                profile.Website = user.OrganiserProfile.Website;
            }
            else if (user.Role == "Participant" && user.ParticipantProfile != null)
            {
                profile.EmergencyContactName = user.ParticipantProfile.EmergencyContactName;
                profile.EmergencyContactPhone = user.ParticipantProfile.EmergencyContactPhone;
                profile.MedicalConditions = user.ParticipantProfile.MedicalConditions;
            }

            return profile;
        }

        public async Task<UserProfileDto> UpdateProfileAsync(int userId, UpdateProfileDto dto)
        {
            var user = await _context.Users
                .Include(u => u.OrganiserProfile)
                .Include(u => u.ParticipantProfile)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                throw new InvalidOperationException("User not found");

            // Update base user fields
            user.FullName = dto.FullName;
            user.DateOfBirth = dto.DateOfBirth;
            user.Phone = dto.Phone;

            // Update role-specific fields
            if (user.Role == "Organiser" && user.OrganiserProfile != null)
            {
                user.OrganiserProfile.OrganisationName = dto.OrganisationName ?? user.OrganiserProfile.OrganisationName;
                user.OrganiserProfile.Website = dto.Website;
            }
            else if (user.Role == "Participant" && user.ParticipantProfile != null)
            {
                user.ParticipantProfile.EmergencyContactName = dto.EmergencyContactName;
                user.ParticipantProfile.EmergencyContactPhone = dto.EmergencyContactPhone;
                user.ParticipantProfile.MedicalConditions = dto.MedicalConditions;
            }

            await _context.SaveChangesAsync();

            return await GetProfileAsync(userId) ?? throw new InvalidOperationException("Failed to update profile");
        }
    }
}