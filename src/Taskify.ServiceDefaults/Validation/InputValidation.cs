using Taskify.ServiceDefaults.ReferenceData;

namespace Taskify.ServiceDefaults.Validation;

public static class InputValidation
{
    public static bool HasValueAfterTrim(string? value) => !string.IsNullOrWhiteSpace(value);

    public static string? NormalizeNullableText(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    public static string NormalizeRequiredText(string? value, string fieldName)
    {
        if (!HasValueAfterTrim(value))
        {
            throw new ArgumentException($"The {fieldName} field is required.", fieldName);
        }

        return value!.Trim();
    }

    public static Guid RequirePredefinedUserId(Guid userId, string fieldName = "userId")
    {
        if (!PredefinedUsers.IsPredefinedUser(userId))
        {
            throw new ArgumentException("The specified user is not one of the predefined users.", fieldName);
        }

        return userId;
    }
}
