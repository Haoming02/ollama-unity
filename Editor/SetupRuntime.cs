using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class SetupRuntime : EditorWindow
{
    private const string PythonURL = "https://www.python.org/ftp/python/3.10.10/python-3.10.10-embed-amd64.zip";
    private const string PipURL = "https://bootstrap.pypa.io/get-pip.py";
    private static readonly string[] RequirePackages = new string[] { "chromadb" };

    [MenuItem("Ollama/Launch Ollama")]
    public static void StartOllama()
    {
        System.Diagnostics.Process ollama = new();
        ollama.StartInfo.FileName = "ollama";
        ollama.StartInfo.Arguments = "list";
        ollama.StartInfo.CreateNoWindow = true;
        ollama.Start();
    }

    [MenuItem("Ollama/Obtain Python")]
    public static void ShowWindow()
    {
        GetWindow<SetupRuntime>("Setup Runtime");
    }

    private static string path;

    void OnGUI()
    {
        this.maxSize = new Vector2(320.0f, 160.0f);
        this.minSize = this.maxSize;

        GUILayout.Label("Obtain Python 3.10.10 + ChromaDB", EditorStyles.boldLabel);
        path = EditorGUILayout.TextField("Path to Install:", path);

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        if (GUILayout.Button("Install Python"))
        {
            if (!Directory.Exists(path))
                Debug.LogError("Invalid Path");
            else
                Main();
        }
        GUILayout.Label("Takes ~1 min", EditorStyles.miniLabel);

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        if (GUILayout.Button("Install Packages"))
        {
            if (!Directory.Exists(path))
                Debug.LogError("Invalid Path");
            else if (!Directory.Exists(ToPath("python")))
                Debug.LogError("Install Python First!");
            else
                InstallPackages();
        }
        GUILayout.Label("Takes ~5 min", EditorStyles.miniLabel);
    }

    private static async void Main()
    {
        // Clear Existing Junks
        if (Directory.Exists(ToPath("temp")))
            Directory.Delete(ToPath("temp"), true);
        if (Directory.Exists(ToPath("python")))
        {
            Debug.LogWarning("Reinstalling Python!");
            Directory.Delete(ToPath("python"), true);
        }

        // Step 0. Create temporary folder
        Directory.CreateDirectory(ToPath("temp"));

        // Step 1. Download Windows Embeddable Package
        if (!(await DownloadPython()))
            return;

        // Step 2. Setup Python
        if (!InstallPython())
            return;

        // Step 3. Setup Pip
        if (!(await InstallPip()))
            return;

        if (Directory.Exists(ToPath("temp")))
            Directory.Delete(ToPath("temp"), true);
        if (File.Exists(ToPath("temp.meta")))
            File.Delete(ToPath("temp.meta"));

        Debug.Log("Success!");
    }

    private static async Task<bool> DownloadPython()
    {
        Debug.Log("Downloading Python...");
        return await DownloadFileAsync(PythonURL, ToPath("temp", "py.zip"));
    }

    private static bool InstallPython()
    {
        Debug.Log("Installing Python...");
        Directory.CreateDirectory(ToPath("python"));

        try
        {
            using (ZipArchive archive = ZipFile.OpenRead(ToPath("temp", "py.zip")))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    string filePath = ToPath("python", entry.FullName);
                    entry.ExtractToFile(filePath, true);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error Extracting Python:\n{e}");
            return false;
        }

        File.Delete(ToPath("temp", "py.zip"));
        return true;
    }

    private static async Task<bool> InstallPip()
    {
        Debug.Log("Enabling Pip...");

        if (!(await DownloadFileAsync(PipURL, ToPath("python", "get-pip.py"))))
            return false;

        System.Diagnostics.ProcessStartInfo startInfo = new()
        {
            FileName = ToPath("python", "python.exe"),
            Arguments = $"\"{ToPath("python", "get-pip.py")}\"",
            RedirectStandardOutput = false,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        startInfo.EnvironmentVariables["TEMP"] = ToPath("temp");

        using (var process = System.Diagnostics.Process.Start(startInfo))
        {
            string error = process.StandardError.ReadToEnd();

            process.WaitForExit();

            if (!string.IsNullOrEmpty(error) && !error.Contains("WARNING"))
            {
                Debug.LogError($"Python Error: {error}");
                return false;
            }
        }

        string pth = await File.ReadAllTextAsync(ToPath("python", "python310._pth"));
        pth = pth.Replace("#import site", "import site");
        await File.WriteAllTextAsync(ToPath("python", "python310._pth"), pth);

        return true;
    }

    private static void InstallPackages()
    {
        Debug.Log("Installing Packages...");

        foreach (var pkg in RequirePackages)
        {
            System.Diagnostics.ProcessStartInfo startInfo = new()
            {
                FileName = ToPath("python", "python.exe"),
                Arguments = $"-m pip install {pkg} --no-cache-dir --prefer-binary --no-warn-script-location",
                RedirectStandardOutput = false,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = System.Diagnostics.Process.Start(startInfo))
            {
                string error = process.StandardError.ReadToEnd();

                process.WaitForExit();

                if (!string.IsNullOrEmpty(error))
                {
                    Debug.LogError($"Error Installing {pkg}: {error}");
                    return;
                }
            }
        }

        Debug.Log("Success!");
    }


    private static string ToPath(string folder, string file = null)
    {
        if (file == null)
            return Path.Combine(path, folder);
        else
            return Path.Combine(path, folder, file);
    }

    private static async Task<bool> DownloadFileAsync(string url, string path)
    {
        using UnityWebRequest webRequest = UnityWebRequest.Get(url);
        var operation = webRequest.SendWebRequest();

        while (!operation.isDone)
            await Task.Yield();

        if (webRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Error Downloading: {url}\n\t{webRequest.error}");
            return false;
        }

        await File.WriteAllBytesAsync(path, webRequest.downloadHandler.data);
        return true;
    }
}
