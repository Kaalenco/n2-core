namespace N2.Core;

public class SelectItem : ISelectItem
{
    public Guid PublicId { get; }
    public string Name { get; }
    public bool Selected { get; }
    public string Description { get; } = string.Empty;

    public SelectItem(Guid publicId, string name)
    {
        PublicId = publicId;
        Name = name;
    }

    public SelectItem(Guid publicId, string name, string description)
    {
        PublicId = publicId;
        Name = name;
        Description = description;
    }
}
