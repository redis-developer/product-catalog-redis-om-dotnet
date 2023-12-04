# Redis OM .NET Vector Search Product Catalog Demo

This demo is a practical example of how to use Redis Stack and Redis Enterprise's Vector Search with Redis OM .NET. This allows you to use Redis easily as a highly performant Vector Database in your applications.

## Capabilities demonstrated

The following Redis Vector Search capabilities are demonstrated by this app:

* Vector Search:
    * Using Images
    * Using Text
* Vector Indexing Types
    * HNSW
    * Flat(brute-force)
* Easily extensible hybrid queries to pre-filter your indexes

## Application

This repo contains two applications

1. SeedDatabase - a simple console app to seed the database from the dataset.
2. ProductCatalog - a Single Page Application (SPA) to display and filter products from the dataset.

## Technologies Used

These applications make use of the following:

* [Redis Stack](https://redis.io/docs/about/about-stack/): For Vector Search + JSON document storage
* ASP.NET Core: To build the API backend.
* ReactL for the frontend
* Redis OM .NET: For index modeling, vector generation, all Document Creation, Reading, Updating, and Deleting (CRUD) in Redis.
    * Redis OM uses Resnet 18 for Image Vectorization
    * Redis OM uses [All-MiniLM-L6-V2](https://huggingface.co/sentence-transformers/all-MiniLM-L6-v2) for sentence vectorization

## How to Run this Demo

### Get the Dataset

Before running the app, you must acquire the "fashion dataset". You can use either of the following:

* [Large](https://www.kaggle.com/datasets/paramaggarwal/fashion-product-images-dataset) ~ 25 GB
* [Small](https://www.kaggle.com/datasets/paramaggarwal/fashion-product-images-small) ~ 500MB

Either is permissible, however if you use the small data set, your images will look a bit grainy in the app.

Download either data set and unzip it, the root of the unzipped folder will be our `DATASET_ROOT` environment variable.

### Configure Redis

#### Redis Cloud (recommended)

1. [Configure your Redis Cloud Instance](https://app.redislabs.com/#/) if needed
2. Set the following environment variables to point at your Redis Cloud instance:
    1. `REDIS_HOST`
    2. `REDIS_PORT`
    3. `REDIS_PASSWORD`

#### Redis in Docker:

If you want to run Redis locally in docker run the following:

`docker run -d -p 6379:6379 redis/redis-stack-server`

### Clone this Repository

Clone this repository using the following:


```sh
git clone https://github.com/redis-developer/ProductCatalog
```

Change directory into the newly created `ProductCatalog` directory `cd ProductCatalog`

### Seed your Database

To seed your database, take the base path of the dataset (the `DATASET_ROOT` variable from before), and decide where you want to put the JSON backup of your vectors (at the root of the `ProductCatalog` repo is fine) and run the following:

```sh
OUTPUT_DIR=<output_dir> DATASET_ROOT=<dataset_root> dotnet run --project SeedDatabase
```

Replacing `<output_dir>` and `DATASET_ROOT` the the appropriate directories.

Seeding the database takes some time (95% of which is simply generating the vectors).

### Configure the app

Open `ProductCatalog/Properties/launchSettings.json`, and update the `DATASET_ROOT` environment variable to point at your dataset root (absolute path).

### Run the App

To Run the app just run:

```sh
dotnet run --project ProductCatalog
```

The app will come up, and you can start the SPA app by visiting `https://localhost:7161` in your browser.

## How it Works

The application leverage Redis OM for 

1. Index Creation
2. Document/Vector creation & insertion
3. Document/Vector querying.

Let's explore each of these

### Index Creation

All Index creation is handled by one line inside of `SeedDatabase/Program.cs`:

```cs
await provider.Connection.CreateIndexAsync(typeof(Product))
```

Redis OM looks at the class `Product`, and basis it's decision about how to generate the index based off of that, let's take a look at this class in `ProductCatalog.Shared/Model/Product.cs`:

``` cs
[Document(StorageType = StorageType.Json)]
public class Product
{
    [Indexed(DistanceMetric = DistanceMetric.COSINE, Algorithm = VectorAlgorithm.HNSW)] 
    [ImageVectorizer]
    public Vector<string> ImageUrl { get; set; }    

    [Indexed(Algorithm = VectorAlgorithm.HNSW, DistanceMetric = DistanceMetric.COSINE)] 
    [SentenceVectorizer] 
    public Vector<string> ProductDisplayName { get; set; }
    
    public VectorScores? Scores { get; set; }

    // ... other indexed facets
}
```

The two key fields are `ImageUrl` and `ProductDisplayName`, they are both of type `Vector<string>`. The `IndexedAttribute` decorating each of them tells Redis OM that the item is meant to be indexed, and the Particular Vectorizers above each of them `ImageVectorizer` and `SentenceVectorizer` above each of them contain the behavior for how the vectorization should be performed for each. The `ImageVectorizer` downloads the image and runs it through ResNet18, and the `SentenceVectorizer` simply runs the ProductDisplayName through [All-MiniLM-L6-V2](https://huggingface.co/sentence-transformers/all-MiniLM-L6-v2). Both use ML.NET for vectorization.

### Insertion

The Insertion tasks are all performed in `SeedDatabase/Program.cs` For the sake of speeding up the initial load, we leverage the batch-vectorizers from both `ImageVectorizer` and `SentenceVectorizer`, so there's a tiny bit more to insertions (as you create the embeddings and the set the Vectors embedding to the embeddings), but generally, insertion is as simple as:

```cs
var entry = new Product { 
            Id = id, 
            ImageUrl = Vector.Of(imageUrl), 
            ProductDisplayName = Vector.Of(productDisplayName)
            // other fields
        };
await collection.InsertAsync(entry)
```

### Querying Nearest Neighbors

To query Nearest Neighbors using Redis OM, all you need to do is invoke the `NearestNeighbors` extension of the RedisCollection. See the `Filter` method in `ProductCatalog/Controllers/ProductController.cs`,

```cs
collection = collection.NearestNeighbors(x => x.ImageUrl, numNeighbors,  $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/{options.ImageUrl}").OrderBy(x=>x.Scores!.NearestNeighborsScore);
```

That's it, simply provide the Field, the number of neighbors you want to query, and the item you want to compare.

### Hybrid Queries

If you want to perform hybrid queries, Nearest Neighbor queries first filtered by other facets, simply query those indexed fields from `Product` that interest you. E.g.

```cs
collection = collection.Where(x => options.Genders.Contains(x.Gender));
```

That's it!

## What if I want to use My own Vectorization logic

Implementing your own Vectorizers is simple, all you need to do is create a class that Extends `VectorizerAttribute<T>` and another `IVectorizer<T>` that actually performs the vectorization logic. You will just need to know the shape of your vectors ahead of time.