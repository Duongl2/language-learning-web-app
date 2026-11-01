using LanguageLearning.Application.DTOs.Auth;

namespace LanguageLearning.Application.Interfaces;

public interface IUserProfileService
{
    Task<UserProfileResponse> GetAsync(CancellationToken cancellationToken = default);
    Task<UserProfileResponse> UpdateAsync(UpdateUserProfileRequest request, CancellationToken cancellationToken = default);
}
