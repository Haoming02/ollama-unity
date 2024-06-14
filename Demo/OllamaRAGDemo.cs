using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class OllamaRAGDemo : MonoBehaviour
{
    [SerializeField]
    private Text display;
    [SerializeField]
    private TextAsset[] data;

    [SerializeField]
    private string pythonPath;
    [SerializeField]
    private string authToken;

    private Queue<string> buffer;
    private bool isStream = false;
    private bool isSafe = true;

    void OnEnable()
    {
        Ollama.OnStreamFinished += () => isSafe = true;
        buffer = new Queue<string>();
    }

    void FixedUpdate()
    {
        if (isSafe)
            return;

        if (buffer.TryDequeue(out string text))
            display.text += text;
    }

    async void Start()
    {
        await Ollama.InitRAG(pythonPath, (authToken.Trim().Length == 0) ? null : authToken);

        display.text = "initializing...";
        foreach (var text in data)
            await Ollama.AppendData(text);
        display.text = "ready...";
    }

    /// <summary> Called by UnityEngine.UI.InputField </summary>
    public async void OnSend(string input)
    {
        if (isStream)
        {
            if (!isSafe)
                return;

            buffer.Clear();
            display.text = string.Empty;
            isSafe = false;

            await Task.Run(async () =>
                await Ollama.AskStream(input, (string text) => buffer.Enqueue(text))
            );
        }
        else
        {
            display.text = "processing...";
            var response = await Task.Run(async () => await Ollama.Ask(input));
            display.text = response;
        }
    }

    public void ToggleStream(bool val)
    {
        isStream = val;
    }
}
