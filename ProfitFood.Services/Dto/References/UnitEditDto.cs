namespace ProfitFood.Applications.Dto.References
{
    public sealed class UnitEditDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ShortName { get; set; } = string.Empty;
        public string UnitType { get; set; } = string.Empty;
        public decimal BaseFactor { get; set; } = 1m;
        public bool IsBase { get; set; }
    }
}