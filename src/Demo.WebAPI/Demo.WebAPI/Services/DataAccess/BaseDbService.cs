using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Demo.Models.Dto;
using Demo.Models.Filters;
using Demo.Models.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Demo.WebAPI.Services.DataAccess {
    public abstract class BaseDbService<TEntity, TDto, TFilter>
        where TEntity : class, IIdEntity
        where TDto : class, IIdEntity
        where TFilter : BaseFilter {
        protected readonly DemoContext DbContext;
        protected readonly DbSet<TEntity> DbSet;
        protected readonly IMapper Mapper;

        protected BaseDbService(DemoContext dbContext, IMapper mapper) {
            DbContext = dbContext;
            Mapper = mapper;
            DbSet = dbContext.Set<TEntity>();
        }

        #region Filer and Sort

        /// <summary>
        /// Сортировка запрашиваемой страницы
        /// </summary>
        /// <param name="query"></param>
        /// <param name="filter"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        internal virtual async Task<IQueryable<TEntity>> ApplySort(IQueryable<TEntity> query, TFilter filter,
            CancellationToken cancellationToken) {
            return await Task.Run(() => query, cancellationToken);
        }

        /// <summary>
        /// Фильтрация запрашиваемой страницы
        /// </summary>
        /// <param name="query"></param>
        /// <param name="filter"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        internal virtual async Task<IQueryable<TEntity>> ApplyFilter(IQueryable<TEntity> query, TFilter filter,
            CancellationToken cancellationToken) {
            return await Task.Run(() => query, cancellationToken);
        }

        #endregion

        #region CRUD

        /// <summary>
        /// Возвращает одну запись из базы данных
        /// </summary>
        /// <param name="id">Идентификатор записи</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns></returns>
        public virtual async Task<TDto> Get(long id, CancellationToken cancellationToken) {
            return await DbSet
                .Where(p => p.Id == id)
                .ProjectTo<TDto>(Mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// Возвращает страницу записей из базы данных
        /// </summary>
        /// <param name="filter">Фильтр</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns></returns>
        public virtual async Task<PageResult<TDto>> GetPage(TFilter filter, CancellationToken cancellationToken) {
            var filteredQuery = await ApplyFilter(DbSet, filter, cancellationToken);
            var sortedQuery = await ApplySort(filteredQuery, filter, cancellationToken);
            var items = await sortedQuery
                .ProjectTo<TDto>(Mapper.ConfigurationProvider)
                .OrderBy(x => x.Id)
                .Skip(filter.Skip)
                .Take(filter.Take)
                .ToListAsync(cancellationToken);

            var result = new PageResult<TDto> {
                Items = items,
                Total = await filteredQuery.CountAsync(cancellationToken)
            };

            return result;
        }
        
#pragma warning disable 1998
        public virtual async Task<TDto> Save(TDto dto, CancellationToken cancellationToken = default)
#pragma warning restore 1998
        {
            throw new Exception("Механизм сохранения объекта не реализован");
        }

        /// <summary>
        /// Создает новую запись в базе данных или сохраняет изменения в существующей записи
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="propsNames">Список имён свойств для пометки в трекере как модифицированных</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <param name="removeTrackingEntries">Следует ли удалить сущность из трекера после завершения операции</param>
        /// <returns></returns>
        protected async Task<TDto> Save(TDto dto, List<string> propsNames,
            CancellationToken cancellationToken = default, bool removeTrackingEntries = true) {
            var entity = Mapper.Map<TEntity>(dto);

            var entityDb = entity.Id == default
                ? null
                : await DbSet.Where(p => p.Id == entity.Id).FirstOrDefaultAsync(cancellationToken);

            propsNames ??= new List<string>();

            if (typeof(INamedEntity).IsAssignableFrom(typeof(TEntity))) {
                ((INamedEntity) entity).NormalizedName =
                    ((INamedEntity) entity).Name.ToUpper(CultureInfo.InvariantCulture);
                propsNames.AddRange(new List<string> {"Name", "NormalizedName"});
            }

            if (entityDb is null) {
                await DbSet.AddAsync(entity, cancellationToken);
                await DbContext.SaveChangesAsync(cancellationToken);
            } else {
                var entry = DbContext.Entry(entity);
                var trackedEntityEntry = DbContext.ChangeTracker.Entries()
                    .FirstOrDefault(x => x.Entity.GetType() == typeof(TEntity));

                if (trackedEntityEntry != null) {
                    propsNames.ForEach(propName => {
                        trackedEntityEntry.Property(propName).CurrentValue = entry.Property(propName).CurrentValue;
                        trackedEntityEntry.Property(propName).IsModified = true;
                    });
                    await DbContext.SaveChangesAsync(cancellationToken);
                    return Mapper.Map<TDto>(entity);
                }

                DbSet.Update(entity);
                propsNames.ForEach(propName => entry.Property(propName).IsModified = true);
                await DbContext.SaveChangesAsync(cancellationToken);

                if (removeTrackingEntries) entry.State = EntityState.Detached;
            }

            return Mapper.Map<TDto>(entity);
        }

        #region Remove

        /// <summary>
        /// Удаление списка сущностей из базы
        /// </summary>
        /// <param name="dtos"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual async Task Remove(TEntity[] dtos, CancellationToken cancellationToken) {
            if (dtos == null) throw new ArgumentNullException(nameof(dtos));
            if (!dtos.Any()) return;
            DbSet.RemoveRange(dtos);
            await DbContext.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Удаление сущности из базы
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        public virtual async Task Remove(TDto dto, CancellationToken cancellationToken) {
            if (dto?.Id == null) throw new ArgumentNullException(nameof(dto.Id));
            var entityDb = await DbSet.FirstOrDefaultAsync(p => p.Id == dto.Id, cancellationToken);
            if (entityDb == null) throw new Exception($"Удаляемая запись с идентификатором {dto.Id} не найдена");
            DbSet.Remove(entityDb);
            await DbContext.SaveChangesAsync(cancellationToken);
        }

        #endregion

        #endregion
    }
}