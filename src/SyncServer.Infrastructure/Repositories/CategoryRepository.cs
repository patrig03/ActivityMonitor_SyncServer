using SyncServer.Core.Domain.Entities;
using SyncServer.Core.Interfaces;
using SyncServer.Infrastructure.Data;

namespace SyncServer.Infrastructure.Repositories;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(AppDbContext context) : base(context) { }
}