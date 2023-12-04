using Redis.OM.Modeling;

namespace ProductCatalog.Model;

[Document(StorageType = StorageType.Json)]
public class FacetOptions
{
    public string[]? Categories { get; set; }

    public string[]? SubCategories { get; set; }

    public string[]? Genders { get; set; }

    public int?[]? Years { get; set; }

    public string[]? Seasons { get; set; }

    public string[]? Usages { get; set; }

    public string[]? Colors { get; set; }

    public string? ImageUrl { get; set; }

    public string? Description { get; set; }

    public int Offset { get; set; } = 0;
}