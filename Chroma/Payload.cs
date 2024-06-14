using Newtonsoft.Json;

public static partial class ChromaDB
{
    private static class Request
    {
        public class Create_Collection
        {
            public string name;
            public string embeddingFunction;
            public string embedding_function;

            public Create_Collection(string name)
            {
                this.name = name;
                this.embeddingFunction = null;
                this.embedding_function = null;
            }
        }

        public class Add
        {
            public string[] ids;
            public float[][] embeddings;

            public Add(string id, float[] embedding)
            {
                this.ids = new string[] { id };
                this.embeddings = new float[][] { embedding };
            }
        }

        public class Get
        {
            public string[] ids;

            public Get(string id)
            {
                this.ids = new string[] { id };
            }
        }

        public class Query
        {
            public float[][] query_embeddings;
            public int n_results;

            public Query(float[] embedding, int numOfResults)
            {
                this.query_embeddings = new float[][] { embedding };
                this.n_results = numOfResults;
            }
        }
    }


    private static class Response
    {
        public class Heartbeat
        {
            [JsonProperty("nanosecond heartbeat")]
            public long ns;
        }

        public class Collection
        {
            public string name;
            public string id;
            public string metadata;
            public string tenant;
            public string database;
        }

        public class DataPoint
        {
            public string[] ids;
            public float[][] embeddings;
            public string[] metadatas;
            public string[] documents;
            public string[] uris;
            public string[] data;
        }

        public class Query
        {
            public string[][] ids;
            public float[][] distances;
            public float[][] embeddings;
            public string[][] metadatas;
            public string[][] documents;
            public string[][] uris;
            public string[][] data;
        }
    }
}
