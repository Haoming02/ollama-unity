using System;

public static partial class Ollama
{
    private static class Request
    {
        public class Generate
        {
            public string model;
            public string prompt;
            public bool stream;
            public string[] images;

            public Generate(string model, string prompt, bool stream, string[] images = null)
            {
                this.model = model;
                this.prompt = prompt;
                this.stream = stream;
                this.images = images;
            }
        }
    }

    private static class Response
    {
        public class Generate : BaseResponse
        {
            public string response;
        }

        public abstract class BaseResponse
        {
            public string model;
            public DateTime created_at;
            public bool done;
            public int[] context;
            public long total_duration;
            public long load_duration;
            public int prompt_eval_count;
            public long prompt_eval_duration;
            public int eval_count;
            public long eval_duration;
        }

        public class List
        {
            public Model[] models;

            public class Model
            {
                public string name;
                public DateTime modified_at;
                public long size;
                public string digest;
                public ModelDetail details;

                public class ModelDetail
                {
                    public string format;
                    public string family;
                    public string[] families;
                    public string parameter_size;
                    public string quantization_level;
                }
            }
        }
    }
}
