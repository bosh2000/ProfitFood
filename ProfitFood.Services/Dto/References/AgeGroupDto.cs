namespace ProfitFood.Applications.Dto.References
{
    /// <summary>Строка справочника возрастных групп.</summary>
    public sealed class AgeGroupDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int AgeFromMonths { get; set; }
        public int AgeToMonths { get; set; }
    }
}
