using UnityEditor;

public static class SetupRuntime
{
    [MenuItem("Ollama/Launch Ollama")]
    public static void StartOllama()
    {
        System.Diagnostics.Process ollama = new();
        ollama.StartInfo.FileName = "ollama";
        ollama.StartInfo.Arguments = "list";
        ollama.StartInfo.CreateNoWindow = true;
        ollama.Start();
    }
}
