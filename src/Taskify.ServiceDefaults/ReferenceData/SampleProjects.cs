namespace Taskify.ServiceDefaults.ReferenceData;

public static class SampleProjects
{
    public static readonly Guid Q4RoadmapId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    public static readonly Guid BugTriageId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    public static readonly Guid ResearchSprintId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

    public static IReadOnlyList<SampleProjectDefinition> All { get; } =
    [
        new(Q4RoadmapId, "Q4 Roadmap", PredefinedUsers.MayaChenId),
        new(BugTriageId, "Bug Triage", PredefinedUsers.JordanLeeId),
        new(ResearchSprintId, "Research Sprint", PredefinedUsers.PriyaShahId),
    ];
}
