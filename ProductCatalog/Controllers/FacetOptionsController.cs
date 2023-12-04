using Microsoft.AspNetCore.Mvc;
using ProductCatalog.Model;
using Redis.OM;
using Redis.OM.Contracts;

namespace ProductCatalog.Controllers;

[ApiController]
[Route("[controller]")]
public class FacetOptionsController : ControllerBase
{
    private readonly IRedisConnectionProvider _provider;

    public FacetOptionsController(IRedisConnectionProvider provider)
    {
        _provider = provider;
    }

    [HttpGet]
    public async Task<FacetOptions> Get()
    {
        var facetOptions = _provider.Connection.Get<FacetOptions>("facetOptions");
        if (facetOptions is not null)
        {
            return facetOptions;
        }

        var aggregationSet = _provider.AggregationSet<Product>();
        var categories = aggregationSet.Distinct(x => x.RecordShell!.Category).FirstAsync();
        var subCategories = aggregationSet.Distinct(x => x.RecordShell.SubCategory).FirstAsync();
        var genders = aggregationSet.Distinct(x => x.RecordShell.Gender).FirstAsync();
        var years = aggregationSet.Distinct(x => x.RecordShell.Year).FirstAsync();
        var seasons = aggregationSet.Distinct(x => x.RecordShell.Season).FirstAsync();
        var usage = aggregationSet.Distinct(x => x.RecordShell.Usage).FirstAsync();
        var colors = aggregationSet.Distinct(x => x.RecordShell.BaseColor).FirstAsync();

        await Task.WhenAll(categories.AsTask(), subCategories.AsTask(), genders.AsTask(), years.AsTask(), seasons.AsTask(), usage.AsTask(), colors.AsTask());

        facetOptions = new FacetOptions();
        facetOptions.Categories = categories.Result["Category_TOLIST"].ToArray().Select(x => (string)x).ToArray();
        facetOptions.SubCategories = subCategories.Result["SubCategory_TOLIST"].ToArray().Select(x => (string)x).ToArray();
        facetOptions.Genders = genders.Result["Gender_TOLIST"].ToArray().Select(x => (string)x).ToArray();
        facetOptions.Years = years.Result["Year_TOLIST"].ToArray().Select(x => (int?)int.Parse(x)).ToArray();
        facetOptions.Seasons = seasons.Result["Season_TOLIST"].ToArray().Select(x => (string)x).ToArray();
        facetOptions.Usages = usage.Result["Usage_TOLIST"].ToArray().Select(x => (string)x).ToArray();
        facetOptions.Colors = colors.Result["BaseColor_TOLIST"].ToArray().Select(x => (string)x).ToArray();
        await _provider.Connection.JsonSetAsync("facetOptions", "$", facetOptions);
        return facetOptions;
    }
}