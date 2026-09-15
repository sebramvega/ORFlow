namespace ORFlow.Api.Contracts.Authentication;

public sealed record CurrentUserResponse(
    string Email,
    IReadOnlyList<string> Roles);
    