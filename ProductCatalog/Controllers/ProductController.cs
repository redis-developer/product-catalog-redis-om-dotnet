using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using ProductCatalog.Model;
using Redis.OM;
using Redis.OM.Contracts;

namespace ProductCatalog.Controllers;

[ApiController]
[Route("[controller]")]
public class ProductController : ControllerBase
{
    private readonly IRedisConnectionProvider _provider;

    public ProductController(IRedisConnectionProvider provider)
    {
        _provider = provider;
    }

    [HttpGet]
    public IEnumerable<CatalogResponse> Get()
    {
        var collection = _provider.RedisCollection<Product>();
        var response = collection.Skip(0).Take(15).ToArray();
        return response.Select(CatalogResponse.Of);
    }

    [HttpGet("byImage")]
    public IEnumerable<CatalogResponse> ByImage([FromQuery]string url)
    {
        var collection = _provider.RedisCollection<Product>();
        var response = collection.NearestNeighbors(x => x.ImageUrl, 15, url);
        return response.Select(CatalogResponse.Of);
    }

    [HttpGet("byDescription")]
    public IEnumerable<CatalogResponse> ByDescription([FromQuery] string description)
    {
        var collection = _provider.RedisCollection<Product>();
        var response = collection.NearestNeighbors(x => x.ProductDisplayName, 15, description);
        return response.Select(CatalogResponse.Of);
    }

    [HttpPost("filter")]
    public IEnumerable<CatalogResponse> Filter([FromBody] FacetOptions options)
    {
        var collection = _provider.RedisCollection<Product>();

        var numNeighbors = 15 + options.Offset;
        if (options.ImageUrl is not null)
        {
            collection = collection.NearestNeighbors(x => x.ImageUrl, numNeighbors,  $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/{options.ImageUrl}").OrderBy(x=>x.Scores!.NearestNeighborsScore);
        }
        else if (options.Description is not null)
        {
            collection = collection.NearestNeighbors(x => x.ProductDisplayName, numNeighbors, options.Description).OrderBy(x=>x.Scores!.NearestNeighborsScore);
        }

        if (options.Categories != null && options.Categories.Any())
        {
            collection = collection.Where(x => options.Categories.Contains(x.Category));
        }
    
        if (options.SubCategories is not null && options.SubCategories.Any())
        {
            collection = collection.Where(x => options.SubCategories.Contains(x.SubCategory));
        }

        if (options.Genders is not null && options.Genders.Any())
        {
            collection = collection.Where(x => options.Genders.Contains(x.Gender));
        }

        if (options.Years is not null && options.Years.Any())
        {
            collection = collection.Where(x => options.Years.Contains(x.Year));
        }

        if (options.Seasons is not null && options.Seasons.Any())
        {
            collection = collection.Where(x => options.Seasons.Contains(x.Season));
        }

        if (options.Usages is not null && options.Usages.Any())
        {
            collection = collection.Where(x => options.Usages.Contains(x.Usage));
        }

        if (options.Colors is not null && options.Colors.Any())
        {
            collection = collection.Where(x => options.Colors.Contains(x.BaseColor));
        }

        return collection.Skip(options.Offset).Take(15).ToList().Select(CatalogResponse.Of);
    }

}