using System;

public static partial class Ollama
{
    private class Request
    {
        public string model;
        public string prompt;
        public bool stream;

        public Request(string model, string prompt, bool stream)
        {
            this.model = model;
            this.prompt = prompt;
            this.stream = stream;
        }
    }

    public class Response
    {
        public string model;
        public DateTime created_at;
        public string response;
        public bool done;
        public int[] context;
        public long total_duration;
        public long load_duration;
        public int prompt_eval_count;
        public long prompt_eval_duration;
        public int eval_count;
        public long eval_duration;
    }
}
