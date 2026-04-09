namespace ProfitFood.Applications.Dto.References
{
    public sealed class UnitItemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ShortName { get; set; } = string.Empty;
        public string UnitTypeName { get; set; } = string.Empty;
        public bool IsBase { get; set; }
    }
}