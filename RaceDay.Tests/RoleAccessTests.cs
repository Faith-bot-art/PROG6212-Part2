using Microsoft.EntityFrameworkCore;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using RaceDay.API.Services;
using RaceDay.Tests.Helpers;
using Xunit;

namespace RaceDay.Tests
{
    public class RoleAccessTests
    {
        // Test 6: Participant cannot create an event
        [Fact]
        public async Task Participant_CannotCreateEvent()
        {
            using var context = TestDbContextFactory.Create();

            var user = new User
            {
                Email = "p@test.com",
                PasswordHash = "h",
                FullName = "Participant",
                Role = "Participant"
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var service = new EventService(context);

            var dto = new CreateEventDto
            {
                Name = "Test Event",
                EventType = "Running",
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(2),
                Location = "X",
                DistanceKm = 5
            };

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                service.CreateEventAsync(user.UserId, dto));
        }

        // Test 7: Organiser can create an event
        [Fact]
        public async Task Organiser_CanCreateEvent()
        {
            using var context = TestDbContextFactory.Create();

            var user = new User
            {
                Email = "o@test.com",
                PasswordHash = "h",
                FullName = "Organiser",
                Role = "Organiser"
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var profile = new OrganiserProfile
            {
                UserId = user.UserId,
                OrganisationName = "Test Org"
            };
            context.OrganiserProfiles.Add(profile);
            await context.SaveChangesAsync();

            var service = new EventService(context);

            var dto = new CreateEventDto
            {
                Name = "Cape Town Marathon",
                EventType = "Running",
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(2),
                Location = "Cape Town",
                DistanceKm = 42.2m
            };

            var result = await service.CreateEventAsync(user.UserId, dto);

            Assert.NotNull(result);
            Assert.Equal("Cape Town Marathon", result.Name);
            Assert.True(result.EventId > 0);
        }

        // Test 8: Organiser cannot enrol in an event
        [Fact]
        public async Task Organiser_CannotEnrolInEvent()
        {
            using var context = TestDbContextFactory.Create();

            var orgUser = new User
            {
                Email = "o@test.com",
                PasswordHash = "h",
                FullName = "Org",
                Role = "Organiser"
            };
            context.Users.Add(orgUser);
            await context.SaveChangesAsync();

            var orgProfile = new OrganiserProfile
            {
                UserId = orgUser.UserId,
                OrganisationName = "Org"
            };
            context.OrganiserProfiles.Add(orgProfile);
            await context.SaveChangesAsync();

            var ev = new Event
            {
                OrganiserId = orgProfile.ProfileId,
                Name = "Test",
                EventType = "Running",
                StartDate = DateTime.UtcNow.AddDays(10),
                EndDate = DateTime.UtcNow.AddDays(10).AddHours(3),
                Location = "X",
                DistanceKm = 10,
                IsActive = true
            };
            context.Events.Add(ev);
            await context.SaveChangesAsync();

            var cat = new Category { EventId = ev.EventId, Name = "Open", EntryFee = 100 };
            context.Categories.Add(cat);
            await context.SaveChangesAsync();

            var service = new EnrolmentService(context);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                service.CreateEnrolmentAsync(orgUser.UserId, new CreateEnrolmentDto
                {
                    EventId = ev.EventId,
                    CategoryId = cat.CategoryId
                }));
        }

        // Test 9: Participant can enrol in an event
        [Fact]
        public async Task Participant_CanEnrolInEvent()
        {
            using var context = TestDbContextFactory.Create();

            var orgUser = new User
            {
                Email = "o@test.com",
                PasswordHash = "h",
                FullName = "Org",
                Role = "Organiser"
            };
            context.Users.Add(orgUser);
            await context.SaveChangesAsync();

            var orgProfile = new OrganiserProfile
            {
                UserId = orgUser.UserId,
                OrganisationName = "Org"
            };
            context.OrganiserProfiles.Add(orgProfile);
            await context.SaveChangesAsync();

            var ev = new Event
            {
                OrganiserId = orgProfile.ProfileId,
                Name = "Test Event",
                EventType = "Running",
                StartDate = DateTime.UtcNow.AddDays(10),
                EndDate = DateTime.UtcNow.AddDays(10).AddHours(3),
                Location = "X",
                DistanceKm = 10,
                IsActive = true
            };
            context.Events.Add(ev);
            await context.SaveChangesAsync();

            var cat = new Category { EventId = ev.EventId, Name = "Open", EntryFee = 100 };
            context.Categories.Add(cat);
            await context.SaveChangesAsync();

            var pUser = new User
            {
                Email = "p@test.com",
                PasswordHash = "h",
                FullName = "Participant",
                Role = "Participant"
            };
            context.Users.Add(pUser);
            await context.SaveChangesAsync();

            var pProfile = new ParticipantProfile { UserId = pUser.UserId };
            context.ParticipantProfiles.Add(pProfile);
            await context.SaveChangesAsync();

            var service = new EnrolmentService(context);

            var result = await service.CreateEnrolmentAsync(pUser.UserId, new CreateEnrolmentDto
            {
                EventId = ev.EventId,
                CategoryId = cat.CategoryId
            });

            Assert.NotNull(result);
            Assert.Equal("Registered", result.Status);
            Assert.NotNull(result.BibNumber);

            var enrolment = await context.Enrolments
                .FirstOrDefaultAsync(e => e.EnrolmentId == result.EnrolmentId);
            Assert.NotNull(enrolment);
            Assert.Equal(ev.EventId, enrolment!.EventId);
            Assert.Equal(cat.CategoryId, enrolment.CategoryId);
            Assert.Equal(pProfile.ProfileId, enrolment.ParticipantId);
        }
    }
}