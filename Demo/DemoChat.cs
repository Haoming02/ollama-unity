using ollama;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class DemoChat : MonoBehaviour
{
    [Header("Models")]
    [SerializeField]
    private string demoModel = "gemma3:4b";

    [Header("UI")]
    [SerializeField]
    private TMP_Text llmOutput;
    [SerializeField]
    private TMP_InputField userInput;

    private Queue<string> buffer;
    private bool isStreaming;

    private void StreamFinished() { isStreaming = false; }
    void OnEnable() { Ollama.OnStreamFinished += StreamFinished; }
    void OnDisable() { Ollama.OnStreamFinished -= StreamFinished; }

    private bool italic;
    private bool bold;

    void LateUpdate()
    {
        if (!isStreaming)
            return;

        // The following formatting is based on gemma3:4b
        while (buffer.TryDequeue(out string text))
        {
            text = text.Replace("\n\n", "\n");

            if (text.Contains("**"))
            {
                bold = !bold;
                if (bold)
                    text = text.Replace("**", "<b>");
                else
                    text = text.Replace("**", "</b>");
            }

            if (text.Contains('*'))
            {
                italic = !italic;
                if (italic)
                    text = text.Replace("*", "<i>");
                else
                    text = text.Replace("*", "</i>");
            }

            llmOutput.text += text;
        }
    }

    void Awake() { Ollama.Launch(); }

    void Start()
    {
        buffer = new Queue<string>();
        Ollama.InitChat();
    }

    /// <summary>Called by <b>TMP_InputField</b></summary>
    public async void OnSubmit(string input)
    {
        if (isStreaming)
            return;

        llmOutput.text += $"<align=right>\"{input}\"</align>\n<align=left><line-height=60%>";
        isStreaming = true;
        userInput.interactable = false;

        bold = false;
        italic = false;

        await Task.Run(async () =>
            await Ollama.ChatStream((string text) => buffer.Enqueue(text), demoModel, input)
        );

        llmOutput.text += "</line-height></align>\n";
        isStreaming = false;
        userInput.interactable = true;
    }
}
