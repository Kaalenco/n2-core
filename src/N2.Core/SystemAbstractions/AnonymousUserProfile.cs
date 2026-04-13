
using N2.Core;
using N2.Core.Identity;

namespace Kaalenco.Common.SystemAbstractions;

public class AnonymousUserProfile : IUserContext
{
    public Guid PublicId => Guid.Parse(DefaultValues.AnonymousUserId);
    public string Name => "Anonymous";
    public string Email => string.Empty;
    public string? PhoneNumber => null;
    public string? ProfileImagePath => null;
    public string? ProfileThumbnailPath => null;
    public string? ProfileBackgroundImagePath => null;
    public int PrimaryPartitionKey => 0;

    public string Description => string.Empty;
    public IEnumerable<UserAlert> Alerts { get; } = [];
    public bool IsAuthenticated { get; }
    public Guid CurrentTenantId { get; }
    public string CurrentTenantName { get => string.Empty; }

    public bool IsInRole(string role) => false;
    public bool HasPolicy(string policy) => false;
    public T? PolicyValue<T>(string policy) => default;
    public void Alert(string message, Priority priority) { }
    public IEnumerable<string> CurrentRoles() => [];
    public bool CanPublish() => false;
    public bool CanModifyRights() => false;
    public bool CanDesign() => false;
    public bool IsAdmin() => false;
    public bool SetTenantContext(Guid tenantId) => false;
    public bool SetTenantContext(string tenantName) => false;
    public bool IsInTenant(Guid tenantId) => false;
    public bool IsInTenant(string tenantName) => false;
}