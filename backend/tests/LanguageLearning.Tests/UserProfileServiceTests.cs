using LanguageLearning.Application.DTOs.Auth;
using LanguageLearning.Application.Interfaces;
using LanguageLearning.Application.Services;
using LanguageLearning.Domain.Entities;
using Moq;

namespace LanguageLearning.Tests;

public class UserProfileServiceTests
{
    private readonly Guid _userId = Guid.NewGuid();
    private readonly User _user;
    private readonly Mock<IUserRepository> _users = new();
    private readonly UserProfileService _service;

    public UserProfileServiceTests()
    {
        _user = new User
        {
            Id = _userId,
            Email = "learner@example.com",
            DisplayName = "Learner",
            Timezone = "Asia/Ho_Chi_Minh",
            IsActive = true
        };
        _users.Setup(repository => repository.GetByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_user);
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(service => service.GetRequiredUserId()).Returns(_userId);
        _service = new UserProfileService(_users.Object, currentUser.Object);
    }

    [Fact]
    public async Task GetReturnsCurrentUserProfile()
    {
        var profile = await _service.GetAsync();

        Assert.Equal(_userId, profile.Id);
        Assert.Equal("learner@example.com", profile.Email);
        Assert.Equal("Asia/Ho_Chi_Minh", profile.Timezone);
    }

    [Fact]
    public async Task UpdateSavesDisplayNameAndTimezone()
    {
        var profile = await _service.UpdateAsync(new UpdateUserProfileRequest
        {
            DisplayName = "  New name  ",
            Timezone = "Asia/Tokyo"
        });

        Assert.Equal("New name", profile.DisplayName);
        Assert.Equal("Asia/Tokyo", profile.Timezone);
        _users.Verify(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task UpdateRejectsBlankDisplayName(string name)
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.UpdateAsync(new UpdateUserProfileRequest { DisplayName = name }));

        _users.Verify(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateRejectsInvalidTimezone()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.UpdateAsync(new UpdateUserProfileRequest { Timezone = "Not/A_Timezone" }));

        _users.Verify(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
