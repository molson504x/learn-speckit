namespace Taskify.ServiceDefaults.ReferenceData;

public enum PredefinedUserRole
{
    ProductManager,
    Engineer,
}

public sealed record PredefinedUser(Guid Id, string Name, PredefinedUserRole Role);
