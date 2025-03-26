using System.Diagnostics;

namespace ollama
{
    public static partial class Ollama
    {
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
    }
}
