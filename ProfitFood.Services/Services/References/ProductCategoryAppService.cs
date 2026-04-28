using AutoMapper;
using ProfitFood.Applications.Dto.References;
using ProfitFood.Applications.Dto.References.ProfitFood.Applications.Dto.References;
using ProfitFood.Applications.Services.Interfaces;
using ProfitFood.Domain.Entities.References;
using ProfitFood.Infrastructure.Repository.Interfaces;
using ProfitFood.UI.Models.References;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProfitFood.Infrastructure.Services.References
{
    public class ProductCategoryAppService : IProductCategoryAppService
    {
        private readonly IDbRepository _dbRepository;
        private readonly IMapper _mapper;

        public ProductCategoryAppService(IDbRepository dbRepository, IMapper mapper)
        {
            _dbRepository = dbRepository;
            _mapper = mapper;
        }

        public async Task<IReadOnlyCollection<ProductCategoryTreeDto>> GetTreeAsync(
            CancellationToken cancellationToken = default)
        {
            var categories = await _dbRepository.productCategoryRepository.ToListAsync();

            var dtoList = categories
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Name)
                .Select(x => new ProductCategoryTreeDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    ParentCategoryId = x.ParentCategoryId,
                    SortOrder = x.SortOrder
                })
                .ToList();

            return BuildTree(dtoList);
        }

        public async Task<IReadOnlyCollection<ProductCategoryLookupDto>> GetLookupAsync(
            CancellationToken cancellationToken = default)
        {
            var categories = await _dbRepository.productCategoryRepository.ToListAsync();

            return categories
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Name)
                .Select(x => new ProductCategoryLookupDto
                {
                    Id = x.Id,
                    Name = x.Name
                })
                .ToList();
        }

        public async Task SaveAsync(
            ProductCategorySaveDto model,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
                throw new InvalidOperationException("Наименование категории продуктов не может быть пустым.");

            if (model.Id != Guid.Empty)
            {
                var canMove = await CanMoveAsync(model.Id, model.ParentCategoryId, cancellationToken);

                if (!canMove)
                    throw new InvalidOperationException("Нельзя назначить категорию родителем самой себе или своей дочерней категории.");

                var entity = await _dbRepository.productCategoryRepository
                    .FirstOfDefaultAsync(x => x.Id == model.Id);

                if (entity == null)
                    throw new InvalidOperationException("Категория продуктов не найдена.");

                entity.Name = model.Name.Trim();
                entity.ParentCategoryId = model.ParentCategoryId;
                entity.SortOrder = model.SortOrder;

                await _dbRepository.productCategoryRepository.UpdateAsync(entity);
            }
            else
            {
                var entity = new ProductCategory
                {
                    // Id = Guid.NewGuid(),
                    Name = model.Name.Trim(),
                    ParentCategoryId = model.ParentCategoryId,
                    SortOrder = model.SortOrder
                };

                await _dbRepository.productCategoryRepository.CreateASync(entity);
            }
        }

        public async Task DeleteAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var canDelete = await CanDeleteAsync(id, cancellationToken);

            if (!canDelete)
                throw new InvalidOperationException("Категорию нельзя удалить: есть дочерние категории или продукты.");

            var entity = await _dbRepository.productCategoryRepository
                .FirstOfDefaultAsync(x => x.Id == id);

            if (entity == null)
                throw new InvalidOperationException("Категория продуктов не найдена.");

            await _dbRepository.productCategoryRepository.DeleteAsync(entity);
        }

        public async Task<bool> CanDeleteAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var children = await _dbRepository.productCategoryRepository
                .ConditionToListAsync(x => x.ParentCategoryId == id);

            if (children.Any())
                return false;

            var products = await _dbRepository.productCategoryRepository
                .ConditionToListAsync(x => x.ParentCategoryId == id);

            return !products.Any();
        }

        public async Task<bool> CanMoveAsync(
            Guid categoryId,
            Guid? newParentCategoryId,
            CancellationToken cancellationToken = default)
        {
            if (newParentCategoryId == null)
                return true;

            if (categoryId == newParentCategoryId.Value)
                return false;

            var categories = await _dbRepository.productCategoryRepository.ToListAsync();

            var currentParentId = newParentCategoryId;

            while (currentParentId != null)
            {
                if (currentParentId.Value == categoryId)
                    return false;

                var parent = categories.FirstOrDefault(x => x.Id == currentParentId.Value);

                if (parent == null)
                    return true;

                currentParentId = parent.ParentCategoryId;
            }

            return true;
        }

        private static IReadOnlyCollection<ProductCategoryTreeDto> BuildTree(
            List<ProductCategoryTreeDto> categories)
        {
            var lookup = categories.ToDictionary(x => x.Id);

            foreach (var category in categories)
            {
                if (category.ParentCategoryId == null)
                    continue;

                if (lookup.TryGetValue(category.ParentCategoryId.Value, out var parent))
                {
                    parent.Children.Add(category);
                }
            }

            return categories
                .Where(x => x.ParentCategoryId == null)
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Name)
                .ToList();
        }
    }
}