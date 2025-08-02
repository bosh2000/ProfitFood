namespace ProfitFood.DAL.Repository.Interfaces
{
    public interface IProfitDbRepository
    {
        IBaseUnitRepository BaseUnitRepository { get; }
        IBaseUnitStorageRepository BaseUnitStorageRepository { get; }
        IProductGroupRepository ProductGroupRepository { get; }
        IProductRepository ProductRepository { get; }
    }
}