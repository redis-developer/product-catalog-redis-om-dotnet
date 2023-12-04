using Microsoft.AspNetCore.Mvc;

namespace ProductCatalog.Controllers;

[ApiController]
[Route("[controller]")]
public class ImagesController : ControllerBase
{
    [HttpGet]
    public IActionResult Get([FromQuery]int id)
    {
        var datasetRoot = Environment.GetEnvironmentVariable("DATASET_ROOT") ?? string.Empty;
        var filename = Path.Combine(datasetRoot, "images", $"{id}.jpg");
        if (!System.IO.File.Exists(filename))
        {
            return NotFound();
        }

        var fileStream = System.IO.File.OpenRead(filename);
        return File(fileStream, "image/jpeg");
    }
}