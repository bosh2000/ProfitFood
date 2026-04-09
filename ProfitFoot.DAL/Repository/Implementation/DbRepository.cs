using AutoMapper;
using ProfitFood.DAL.Repository.Interfaces;
using ProfitFood.Infrastructure.Repository.Implementation.ItemRepository;
using ProfitFood.Infrastructure.Repository.Interfaces;
using ProfitFoot.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProfitFood.Infrastructure.Repository.Implementation
{
    public class DbRepository : IDbRepository
    {
        private ProfitFoodDbContext _context;
        private IMapper _mapper;

        public DbRepository(ProfitFoodDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public IUnitRepository unitRepository => new UnitRepository(_context, _mapper);
    }
}