using UnityEngine;
using UnityEngine.UI;

public class OllamaRAGDemo : MonoBehaviour
{
    [SerializeField]
    private Text display;
    [SerializeField]
    private TextAsset text;

    private bool isStream = false;

    void Start()
    {
        Ollama.InitRAG(text);
    }

    /// <summary> Called by UnityEngine.UI.InputField </summary>
    public async void OnSend(string input)
    {
        if (isStream)
        {
            display.text = string.Empty;
            await Ollama.AskStream(input, (string text) =>
            {
                if (display != null)
                    display.text += text;
            });
        }
        else
        {
            display.text = "processing...";
            var response = await Ollama.Ask(input);
            display.text = response;
        }
    }

    public void ToggleStream(bool val)
    {
        isStream = val;
    }
}
