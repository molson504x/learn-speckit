namespace Taskify.ServiceDefaults.ReferenceData;

public static class PredefinedUsers
{
    public static readonly Guid MayaChenId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid JordanLeeId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid PriyaShahId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly Guid LuisGarciaId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    public static readonly Guid AvaWilliamsId = Guid.Parse("55555555-5555-5555-5555-555555555555");

    public static IReadOnlyList<PredefinedUser> All { get; } = new[]
    {
        new PredefinedUser(MayaChenId, "Maya Chen", PredefinedUserRole.ProductManager),
        new PredefinedUser(JordanLeeId, "Jordan Lee", PredefinedUserRole.Engineer),
        new PredefinedUser(PriyaShahId, "Priya Shah", PredefinedUserRole.Engineer),
        new PredefinedUser(LuisGarciaId, "Luis Garcia", PredefinedUserRole.Engineer),
        new PredefinedUser(AvaWilliamsId, "Ava Williams", PredefinedUserRole.Engineer),
    };

    public static bool IsPredefinedUser(Guid userId) => TryGet(userId, out _);

    public static bool TryGet(Guid userId, out PredefinedUser user)
    {
        user = All.FirstOrDefault(candidate => candidate.Id == userId)!;
        return user is not null;
    }
}
