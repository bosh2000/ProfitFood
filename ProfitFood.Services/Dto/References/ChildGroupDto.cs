namespace ProfitFood.Applications.Dto.References
{
    /// <summary>Строка справочника групп детей.</summary>
    public sealed class ChildGroupDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid AgeGroupId { get; set; }
        public string AgeGroupName { get; set; } = string.Empty;
        public int PlannedChildrenCount { get; set; }
    }
}
