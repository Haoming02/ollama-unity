using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class OllamaChatDemo : MonoBehaviour
{
    [SerializeField]
    private string demoModel = "gemma2:2b";

    [SerializeField]
    private Text display;

    private Queue<string> buffer = new Queue<string>();
    private bool isSafe = true;

    private void StreamFinished() { isSafe = true; }
    void OnEnable() { Ollama.OnStreamFinished += StreamFinished; }
    void OnDisable() { Ollama.OnStreamFinished -= StreamFinished; }

    void FixedUpdate()
    {
        if (isSafe)
            return;

        if (buffer.TryDequeue(out string text))
            display.text += text;
    }

    void Start()
    {
        Ollama.InitChat();
        display.text = string.Empty;
    }

    /// <summary>Called by <b>UnityEngine.UI.InputField</b></summary>
    public async void OnSubmit(string input)
    {
        if (!isSafe)
            return;

        buffer.Clear();
        display.text += $"[User]\n{input}\n\n[LLM]\n";
        isSafe = false;

        await Task.Run(async () =>
            await Ollama.ChatStream((string text) => buffer.Enqueue(text), demoModel, input)
        );

        display.text += "\n\n";
    }
}
