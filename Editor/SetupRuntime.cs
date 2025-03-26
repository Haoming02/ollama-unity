using UnityEditor;

namespace ollama
{
    public static class SetupRuntime
    {
        [MenuItem("Ollama/Launch Ollama")]
        public static void LaunchOllama()
        {
            Ollama.Launch();
            UnityEngine.Debug.Log("ollama launched~");
        }
    }
}
