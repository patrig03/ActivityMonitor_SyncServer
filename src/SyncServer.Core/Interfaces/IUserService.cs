using SyncServer.Core.Domain.Entities;

namespace SyncServer.Core.Interfaces;

public interface IUserService
{
    Task<User?> GetByEmailAsync(string email);
    Task<User> CreateAsync(string email, string passwordHash);
    Task<bool> ValidatePasswordAsync(string email, string passwordHash);
}