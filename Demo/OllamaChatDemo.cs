using UnityEngine;
using UnityEngine.UI;

public class OllamaChatDemo : MonoBehaviour
{
    [SerializeField]
    private Text display;
    [SerializeField]
    [Tooltip("Remember to:\n- Enable Read/Write\n- Disable Compression")]
    private Texture2D image;

    private bool isStream = false;

    void Start()
    {
        Ollama.LoadChatHistory();
        display.text = string.Empty;
    }

    /// <summary> Called by UnityEngine.UI.InputField </summary>
    public async void OnSend(string input)
    {
        if (isStream)
        {
            display.text = string.Empty;
            await Ollama.ChatStream(input, (string text) =>
            {
                if (display != null)
                    display.text += text;
            });
        }
        else
        {
            display.text = "processing...";
            var response = await Ollama.Chat(input);
            display.text = response;
        }
    }

    /// <summary> Called by UnityEngine.UI.Button </summary>
    public void SaveChat()
    {
        Ollama.SaveChatHistory();
    }

    /// <summary> Called by UnityEngine.UI.Button </summary>
    public void ResetChat()
    {
        Ollama.InitChat();
    }

    public void ToggleStream(bool val)
    {
        isStream = val;
    }
}