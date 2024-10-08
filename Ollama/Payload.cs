using System;
using UnityEngine;

public static partial class Ollama
{
    public enum KeepAlive { unload_immediately = 0, five_minute = 300, loaded_forever = -1 };

    private static class Request
    {
        public class Generate
        {
            public string model;
            public string prompt;
            public string[] images;
            public string format;
            public bool stream;
            public int keep_alive;

            public Generate(string model, string prompt, string[] images, string format, bool stream, KeepAlive keep_alive)
            {
                this.model = model;
                this.prompt = prompt;
                this.images = images;
                this.format = format;
                this.stream = stream;
                this.keep_alive = (int)keep_alive;
            }
        }

        public class Chat
        {
            public string model;
            public Message[] messages;
            public bool stream;
            public int keep_alive;

            public Chat(string model, Message[] messages, bool stream, KeepAlive keep_alive)
            {
                this.model = model;
                this.messages = messages;
                this.stream = stream;
                this.keep_alive = (int)keep_alive;
            }
        }

        public class Embeddings
        {
            public string model;
            public string[] input;
            public int keep_alive;

            public Embeddings(string model, string input, KeepAlive keep_alive)
            {
                this.model = model;
                this.input = new string[] { input };
                this.keep_alive = (int)keep_alive;
            }

            public Embeddings(string model, string[] input, KeepAlive keep_alive)
            {
                this.model = model;
                this.input = input;
                this.keep_alive = (int)keep_alive;
            }
        }
    }

    private static class Response
    {
        public class Generate : BaseResponse
        {
            public string response;
        }

        public class Chat : BaseResponse
        {
            public Message message;
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
        }

        public class Embeddings
        {
            public string model;
            public float[][] embeddings;
        }
    }

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

    private class Message
    {
        public string role;
        public string content;
        public string[] images;

        public Message(string role, string content, string image = null)
        {
            this.role = role;
            this.content = content;
            if (image == null)
                this.images = null;
            else
                this.images = new string[] { image };
        }
    }

    public static string[] EncodeTextures(Texture2D[] textures, bool fullQuality = false)
    {
        if (textures == null)
            return null;

        int l = textures.Length;
        string[] imagesBase64 = new string[l];
        for (int i = 0; i < l; i++)
            imagesBase64[i] = Texture2Base64(textures[i]);

        return imagesBase64;
    }

    private static string Texture2Base64(Texture2D texture, bool fullQuality = false)
    {
        if (texture == null)
            return null;

        if (fullQuality)
            return Convert.ToBase64String(texture.EncodeToPNG());
        else
            return Convert.ToBase64String(texture.EncodeToJPG());
    }
}
