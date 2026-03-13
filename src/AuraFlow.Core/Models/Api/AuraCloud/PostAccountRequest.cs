namespace AuraFlow.Core.Models.Api.AuraCloud;

public record PostAccountRequest(
    string Email,
    string Password,
    string ConfirmPassword,
    string AccountName
);
