namespace LanguageLearning.Application.DTOs.Auth;

public class UserProfileResponse
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string Timezone { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
}
