using LanguageLearning.Application.Common.Exceptions;
using LanguageLearning.Application.DTOs.Auth;
using LanguageLearning.Application.Interfaces;
using LanguageLearning.Domain.Entities;

namespace LanguageLearning.Application.Services;

public class UserProfileService : IUserProfileService
{
    private readonly IUserRepository _users;
    private readonly ICurrentUserService _currentUser;

    public UserProfileService(IUserRepository users, ICurrentUserService currentUser)
    {
        _users = users;
        _currentUser = currentUser;
    }

    public async Task<UserProfileResponse> GetAsync(CancellationToken cancellationToken = default)
    {
        var user = await GetCurrentUserAsync(cancellationToken);
        return ToResponse(user);
    }

    public async Task<UserProfileResponse> UpdateAsync(
        UpdateUserProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.DisplayName is null && request.Timezone is null)
            throw new ArgumentException("Display name or timezone is required.");

        var user = await GetCurrentUserAsync(cancellationToken);
        if (request.DisplayName is not null)
        {
            var displayName = request.DisplayName.Trim();
            if (displayName.Length is < 1 or > 80)
                throw new ArgumentException("Display name must contain 1 to 80 characters.");
            user.DisplayName = displayName;
        }

        if (request.Timezone is not null)
        {
            var timezone = request.Timezone.Trim();
            if (timezone.Length > 100 || !TimeZoneInfo.TryFindSystemTimeZoneById(timezone, out _))
                throw new ArgumentException("A valid timezone is required.");
            user.Timezone = timezone;
        }

        await _users.SaveChangesAsync(cancellationToken);
        return ToResponse(user);
    }

    private async Task<User> GetCurrentUserAsync(CancellationToken cancellationToken)
    {
        var id = _currentUser.GetRequiredUserId();
        var user = await _users.GetByIdAsync(id, cancellationToken);
        if (user is null || !user.IsActive)
            throw new NotFoundException("User not found.");
        return user;
    }

    private static UserProfileResponse ToResponse(User user) => new()
    {
        Id = user.Id,
        Email = user.Email,
        DisplayName = user.DisplayName,
        Timezone = user.Timezone,
        Role = user.Role.ToString()
    };
}
