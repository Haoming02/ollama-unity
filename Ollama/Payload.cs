using System;
using System.Collections.Generic;
using UnityEngine;

namespace ollama
{
    public static partial class Ollama
    {
        private static class Request
        {
            public abstract class Base
            {
                public string model;
                public int keep_alive;
            }

            public class Generate : Base
            {
                public string prompt;
                public bool stream;
                public string format;
                public string[] images;

                public Generate(string model, string prompt, bool stream, int keep_alive, string format = null, string[] images = null)
                {
                    this.model = model;
                    this.prompt = prompt;
                    this.stream = stream;
                    this.keep_alive = keep_alive;
                    this.format = format;
                    this.images = images;
                }
            }

            public class Chat : Base
            {
                public List<Message> messages;
                public bool stream;

                public Chat(string model, List<Message> messages, bool stream, int keep_alive)
                {
                    this.model = model;
                    this.messages = messages;
                    this.stream = stream;
                    this.keep_alive = keep_alive;
                }
            }

            public class Embeddings : Base
            {
                public string[] input;

                public Embeddings(string model, string input, int keep_alive)
                {
                    this.model = model;
                    this.input = new string[] { input };
                    this.keep_alive = keep_alive;
                }

                public Embeddings(string model, string[] input, int keep_alive)
                {
                    this.model = model;
                    this.input = input;
                    this.keep_alive = keep_alive;
                }
            }
        }

        private static class Response
        {
            public abstract class Base { };

            public class Generate : BaseResponse
            {
                public string response;
            }

            public class Chat : BaseResponse
            {
                public Message message;
            }

            public abstract class BaseResponse : Base
            {
                public string model;
                public DateTime created_at;
                public bool done;
#if OLLAMA_ADVANCED
                public int[] context;
                public long total_duration;
                public long load_duration;
                public int prompt_eval_count;
                public long prompt_eval_duration;
                public int eval_count;
                public long eval_duration;
#endif
            }

            public class List : Base
            {
                public Model[] models;
            }

            public class Embeddings : Base
            {
                public float[][] embeddings;
#if OLLAMA_ADVANCED
                public string model;
                public long total_duration;
                public long load_duration;
                public int prompt_eval_count;
#endif
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
                this.images = (image == null) ? null : new string[] { image };
            }
        }

        private static string[] EncodeTextures(Texture2D[] textures, bool fullQuality = true)
        {
            if (textures == null)
                return null;

            int l = textures.Length;
            string[] imagesBase64 = new string[l];

            for (int i = 0; i < l; i++)
                imagesBase64[i] = Texture2Base64(textures[i], fullQuality);

            return imagesBase64;
        }

        private static string Texture2Base64(Texture2D texture, bool fullQuality = true)
        {
            if (texture == null) return null;
            return Convert.ToBase64String(fullQuality ? texture.EncodeToPNG() : texture.EncodeToJPG());
        }
    }
}
