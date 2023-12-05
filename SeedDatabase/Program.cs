// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.Text.Json;
using ProductCatalog.Model;
using Redis.OM;
using Redis.OM.Vectorizers.AllMiniLML6V2;
using Redis.OM.Vectorizers.Resnet18;

var provider = new RedisConnectionProvider("redis://localhost:6379");
var outputDir = Environment.GetEnvironmentVariable("OUTPUT_DIR");
var fileName = Path.Join(outputDir, "catalog_entries.json");
var dataSetRoot = Environment.GetEnvironmentVariable("DATASET_ROOT");
var isLarge = File.Exists(Path.Join(dataSetRoot, "images.csv"));

if(! await provider.Connection.CreateIndexAsync(typeof(Product)))
{
    return;
}
var watch = Stopwatch.StartNew();
var collection = provider.RedisCollection<Product>();
if (File.Exists(fileName))
{
    var insertionTasks = new List<Task>();
    await using var readStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true);
    using var reader = new StreamReader(readStream);

    string? line;
    var i = 0;
    
    while ((line = await reader.ReadLineAsync()) != null)
    {
        var entry = JsonSerializer.Deserialize<Product>(line);
        if (entry is not null)
        {
            insertionTasks.Add(collection.InsertAsync(entry));
        }

        if (i % 1000 == 0)
        {
            await Task.WhenAll(insertionTasks);
            insertionTasks.Clear();
            DrawProgressBar(i, 44000, 50, watch.ElapsedMilliseconds, "Inserting from File.");
        }
        i++;
        
    }

    await Task.WhenAll(insertionTasks);
    return;
}

var records = ReadCsvIntoListOfDictionaries($"{dataSetRoot}/styles.csv").Where(x=>File.Exists($"{dataSetRoot}/images/{x["id"]}.jpg")).ToArray();

var chunkSize = 500;
await using var stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true);
await using var writer = new StreamWriter(stream);

for (var offset = 0; offset < records.Length; offset += chunkSize)
{
    var insertionTasks = new List<Task>();
    DrawProgressBar(offset, records.Length, 50, watch.ElapsedMilliseconds, "generating");    
    var imageVectors = ImageVectorizer.VectorizeFiles(records.Skip(offset).Take(chunkSize).Select(x=> $"{dataSetRoot}/images/{x["id"]}.jpg"));
    var sentenceVectors = SentenceVectorizer.Vectorize(records.Select(x => x["productDisplayName"]).Skip(offset).Take(chunkSize).ToArray());
    for (var i = 0; i < imageVectors.Length; i++)
    {
        var dict = records[offset + i];
        var imageVector = Vector.Of($"images?id={dict["id"]}&large={isLarge}");
        imageVector.Embedding = imageVectors[i].SelectMany(BitConverter.GetBytes).ToArray();
        var sentenceVector = Vector.Of(dict["productDisplayName"]);
        sentenceVector.Embedding = sentenceVectors[i].SelectMany(BitConverter.GetBytes).ToArray();
        dict.TryGetValue("year", out var yearStr);
        int.TryParse(yearStr, out var year);
        var entry = new Product { 
            Id = int.Parse(dict["id"]), 
            ImageUrl = imageVector, 
            Gender = dict["gender"], 
            Category = dict["masterCategory"], 
            SubCategory = dict["subCategory"], 
            ArticleType = dict["articleType"], 
            BaseColor = dict["baseColour"], 
            Season = dict["season"], 
            Year = year, 
            Usage = dict["usage"], 
            ProductDisplayName = sentenceVector 
        };
        insertionTasks.Add(collection.InsertAsync(entry));
        var jsonStr = JsonSerializer.Serialize(entry);
        await writer.WriteLineAsync(jsonStr);
    }

    await Task.WhenAll(insertionTasks);
}

Console.WriteLine($" Finished creating image vectors in {watch.ElapsedMilliseconds}ms");


static void DrawProgressBar(int complete, int maxVal, int barSize, long elapsedTime, string step)
{
    Console.CursorLeft = 0;
    Console.Write($"{step}: ");
    Console.Write("["); // Start
    float percent = (float)complete / maxVal;
    int chars = (int)Math.Floor(percent / (1.0f / barSize));
    for (int i = 0; i < chars; i++) Console.Write("=");
    for (int i = chars; i < barSize; i++) Console.Write(" ");
    Console.Write("]"); // End
    Console.Write(" {0}% complete", (int)(percent * 100));
    Console.Write($" {elapsedTime/1000}s elapsed");
}

static List<Dictionary<string, string>> ReadCsvIntoListOfDictionaries(string filePath)
{
    var records = new List<Dictionary<string, string>>();
    using (var reader = new StreamReader(filePath))
    {
        string[] headers = reader.ReadLine().Split(',');
        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
            var values = line.Split(',');

            var record = new Dictionary<string, string>();
            for (int i = 0; i < headers.Length; i++)
            {
                record[headers[i]] = values[i];
            }

            records.Add(record);
        }
    }
    return records;
}