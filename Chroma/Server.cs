using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;

public static partial class ChromaDB
{
    /// <summary> Try starting the ChromaDB server with the given python environment and auth token </summary>
    public static async Task StartServer(string pythonPath, string token = null, bool allowReset = false)
    {
        AUTH_TOKEN = token;

        try { await Heartbeat(); }
        catch (HttpListenerException) { LaunchServer(pythonPath, allowReset); }
    }

    private static void LaunchServer(string pythonPath, bool allowReset)
    {
        ProcessStartInfo startInfo = new()
        {
            FileName = Path.Combine(pythonPath, "Scripts", "chroma.exe"),
            Arguments = $"run --path \"{Path.Combine(Application.persistentDataPath, "db")}\"",
            UseShellExecute = false,
            CreateNoWindow = false
        };

        if (AUTH_TOKEN != null)
        {
            startInfo.EnvironmentVariables["CHROMA_SERVER_AUTHN_CREDENTIALS"] = AUTH_TOKEN;
            startInfo.EnvironmentVariables["CHROMA_SERVER_AUTHN_PROVIDER"] = "chromadb.auth.token_authn.TokenAuthenticationServerProvider";
        }

        if (allowReset)
            startInfo.EnvironmentVariables["ALLOW_RESET"] = "True";

        Process.Start(startInfo);
    }
}
