using Microsoft.ML.Data;
using Redis.OM;
using Redis.OM.Modeling;
using Redis.OM.Modeling.Vectors;
using Redis.OM.Vectorizers.AllMiniLML6V2;
using Redis.OM.Vectorizers.Resnet18;

namespace ProductCatalog.Model;

[Document(StorageType = StorageType.Json)]
public class Product
{
    [RedisIdField] [Indexed] public int Id { get; set; }

    [Indexed(DistanceMetric = DistanceMetric.COSINE, Algorithm = VectorAlgorithm.HNSW)] [ImageVectorizer]public Vector<string> ImageUrl { get; set; }    

    [Indexed(Algorithm = VectorAlgorithm.FLAT, DistanceMetric = DistanceMetric.COSINE)] [SentenceVectorizer] public Vector<string> ProductDisplayName { get; set; }
    
    public VectorScores? Scores { get; set; }

    [Indexed] public string Gender { get; set; }

    [Indexed] public string Category { get; set; }

    [Indexed] public string SubCategory { get; set; }
    
    [Indexed] public string ArticleType { get; set; }

    [Indexed] public string BaseColor { get; set; }

    [Indexed] public string Season { get; set; }

    [Indexed] public int? Year { get; set; }

    [Indexed] public string Usage { get; set; }    
}