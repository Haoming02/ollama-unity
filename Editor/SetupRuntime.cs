using System.Diagnostics;
using UnityEditor;

namespace ollama
{
    public static class SetupRuntime
    {
        [MenuItem("Ollama/Launch Ollama")]
        public static void StartOllama()
        {
            var ollama = new Process();
            ollama.StartInfo.FileName = "ollama";
            ollama.StartInfo.Arguments = "list";
            ollama.StartInfo.CreateNoWindow = false;
            ollama.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            ollama.Start();
            ollama.WaitForExit();
            ollama.Close();

            UnityEngine.Debug.Log("ollama launched~");
        }
    }
}
