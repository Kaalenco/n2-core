using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

using Microsoft.EntityFrameworkCore;

using N2.Core.Commands;
namespace N2.Core.Entity;

public abstract class CoreDataContext(DbContextOptions options) : DbContext(options), ICoreDataContext
{
    public virtual DbSet<ChangeLog> ChangeLog { get; set; }
    public IQueryable<IChangeLog> ChangeLogs => ChangeLog.AsNoTracking();
    public string CurrentDatabaseName => Database.GetDbConnection().Database;

    public void AddChangeLog<T>(
        Guid publicId,
        string message,
        Guid userId,
        string userName)
    where T : class
    {
        ChangeLog logEntry = new()
        {
            TableName = typeof(T).Name,
            ReferenceId = publicId,
            Message = message,
            CreatedBy = userId,
            CreatedByName = userName,
            Created = DateTime.UtcNow
        };
        ChangeLog.Add(logEntry);
    }

    public void AddChangeLog(IChangeLog changeLog)
    {
        if (changeLog == null)
        {
            return;
        }

        ChangeLog logEntry = new()
        {
            TableName = changeLog.TableName,
            ReferenceId = changeLog.ReferenceId,
            Message = changeLog.Message,
            CreatedBy = changeLog.CreatedBy,
            CreatedByName = changeLog.CreatedByName,
            Created = changeLog.Created
        };
        ChangeLog.Add(logEntry);
    }

    public void AddRecord<T>(T model) where T : class
    {
        if (model == null)
        {
            return;
        }

        IDbBaseModel? dbModel = model as IDbBaseModel;
        if (dbModel != null)
        {
            if (dbModel.PublicId == Guid.Empty)
            {
                dbModel.PublicId = Guid.NewGuid();
            }
            dbModel.Created = DateTime.UtcNow;
            dbModel.Modified = DateTime.UtcNow;
        }

        Set<T>().Add(model);
    }

    public async Task<(int resultCode, string message)> DeleteAsync<T>(Guid publicId) where T : class
    {
        DbSet<T> dbSet = Set<T>();
        if (typeof(IDbBaseModel).IsAssignableFrom(typeof(T)))
        {
            T? baseItem = await dbSet.FirstOrDefaultAsync(x => ((IDbBaseModel)x).PublicId == publicId);
            IDbBaseModel? baseModel = baseItem as IDbBaseModel;
            if (baseModel == null)
            {
                return (404, "Not found");
            }
            baseModel.Removed = DateTime.UtcNow;
            baseModel.IsRemoved = true;
            return new(204, "Removed");
        }

        T? dbItem = await dbSet.FindAsync(publicId);

        if (dbItem == null)
        {
            return (404, "Not found");
        }
        dbSet.Remove(dbItem);
        return new(204, "Removed");
    }

    public Task<T?> FindRecordAsync<T>(Guid publicId) where T : class
    {
        if (typeof(IDbBaseModel).IsAssignableFrom(typeof(T)))
        {
            return Set<T>().FirstOrDefaultAsync(x => ((IDbBaseModel)x).PublicId == publicId);
        }
        return Set<T>().FindAsync(publicId).AsTask();
    }

    public abstract Task<List<KeyValuePair<string, string>>> GetSelectListAsync(string tableName);

    public async Task<(ResponseStatus status, string message)> SaveChangesAsync()
    {
        // Check if ChangeLog needs to be updated
        try
        {
            int modified = await base.SaveChangesAsync();
            return new(ResponseStatus.Success, $"{modified} records modified");
        }
        catch (DbException ex)
        {
            return new(ResponseStatus.ServerError, ex.Message);
        }
    }

    protected override void OnModelCreating([NotNull] ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Add the ChangeLog model(s) to the data context.
        ChangeLogBuilder.BuildModel(modelBuilder);
    }

}