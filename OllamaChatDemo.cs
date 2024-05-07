using UnityEngine;
using UnityEngine.UI;

public class OllamaChatDemo : MonoBehaviour
{
    [SerializeField]
    private Text display;

    private bool isStream = false;

    void Start()
    {
        Ollama.InitChat();
        display.text = string.Empty;
    }

    /// <summary> Called by UnityEngine.UI.InputField </summary>
    public async void OnSend(string input)
    {
        if (isStream)
        {
            display.text = string.Empty;
            await Ollama.ChatStream(input, (string text) => { display.text += text; });
        }
        else
        {
            var response = await Ollama.Chat(input);
            display.text = response;
        }
    }

    public void ToggleStream(bool val)
    {
        isStream = val;
    }
}
