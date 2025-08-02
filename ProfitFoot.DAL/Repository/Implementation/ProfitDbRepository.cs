using ProfitFood.DAL.Repository.Implementation.ItemRepository;
using ProfitFood.DAL.Repository.Interfaces;
using ProfitFoot.DAL;

namespace ProfitFood.DAL.Repository.Implementation
{
    public class ProfitDbRepository : IProfitDbRepository
    {
        public readonly ProfitFoodDbContext _context;

        public ProfitDbRepository(ProfitFoodDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IBaseUnitRepository BaseUnitRepository => new BaseUnitRepository(_context);
        public IBaseUnitStorageRepository BaseUnitStorageRepository => new BaseUnitStorageRepository(_context);
        public IProductGroupRepository ProductGroupRepository => new ProductGroupRepositiry(_context);
        public IProductRepository ProductRepository => new ProductRepository(_context);
    }
}