namespace ProfitFood.DAL.Repository.Interfaces
{
    public interface IProfitDbRepository
    {
        IUnitRepository UnitRepository { get; }
        IProductCategoryRepository ProductGroupRepository { get; }
        IProductRepository ProductRepository { get; }
    }
}