using System;
using System.Diagnostics;

namespace ollama
{
    public static partial class Ollama
    {
        /// <summary>Subscribe to this Event to know when a Streaming response is finished</summary>
        public static Action OnStreamFinished;

        public const int UnloadImmediately = 0;
        public const int LoadForever = -1;

        /// <summary>Call this function to ensure Ollama is running</summary>
        public static void Launch()
        {
            var ollama = new Process();
            ollama.StartInfo.FileName = "ollama";
            ollama.StartInfo.Arguments = "list";
            ollama.StartInfo.CreateNoWindow = false;
            ollama.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            ollama.Start();
            ollama.WaitForExit();
            ollama.Close();
        }

        /// <summary>The max streaming iterations to handle server issues</summary>
        private const int MaxIterations = 65536;
    }
}
