using System.Collections;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using TrackWay.Domain.Common;
using TrackWay.Domain.Interfaces;
using TrackWay.Infrastructure.Persistence;

namespace TrackWay.Infrastructure.Repositories;

/// <summary>
/// Implementación de Unit of Work
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly TrackWayDbContext _context;
    private readonly Hashtable _repositories = new();
    private IDbContextTransaction? _currentTransaction;
    private bool _disposed;

    public UnitOfWork(TrackWayDbContext context)
    {
        _context = context;
    }

    public IRepository<T> Repository<T>() where T : Entity
    {
        var type = typeof(T).Name;

        if (_repositories.ContainsKey(type))
        {
            return (IRepository<T>)_repositories[type]!;
        }

        var repositoryType = typeof(Repository<>);
        var repositoryInstance = Activator.CreateInstance(repositoryType.MakeGenericType(typeof(T)), _context);
        
        _repositories.Add(type, repositoryInstance);
        return (IRepository<T>)repositoryInstance!;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
        {
            return;
        }
        _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            _currentTransaction?.Commit();
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            if (_currentTransaction != null)
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _currentTransaction?.RollbackAsync(cancellationToken)!;
        }
        finally
        {
            if (_currentTransaction != null)
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _context.Dispose();
        }
        _disposed = true;
    }
}
