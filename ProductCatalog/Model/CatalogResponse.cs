namespace ProductCatalog.Model;

public class CatalogResponse
{
    public int Id { get; set; }

    public string Gender { get; set; }

    public string ImageUrl { get; set; }

    public string Category { get; set; }

    public string SubCategory { get; set; }
    
    public string ArticleType { get; set; }

    public string BaseColor { get; set; }

    public string Season { get; set; }

    public int? Year { get; set; }

    public string Usage { get; set; }

    public string ProductDisplayName { get; set; }
    
    public double? Score { get; set; }

    public static CatalogResponse Of(Product entry)
    {
        return new()
        {
            Id = entry.Id,
            Gender = entry.Gender,
            Category = entry.Category,
            ImageUrl = entry.ImageUrl.Value,
            SubCategory = entry.SubCategory,
            ArticleType = entry.ArticleType,
            BaseColor = entry.BaseColor,
            Season = entry.Season,
            Year = entry.Year,
            Usage = entry.Usage,
            ProductDisplayName = entry.ProductDisplayName.Value,
            Score = entry.Scores?.NearestNeighborsScore
        };
    }
    
}