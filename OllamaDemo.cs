using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class OllamaDemo : MonoBehaviour
{
    [SerializeField]
    private Text display;

    private bool isStream = false;

    async void Start()
    {
        var response = await Ollama.List();
        StringBuilder stringBuilder = new StringBuilder();

        for (int i = 0; i < response.Length; i++)
            stringBuilder.Append($"[{i + 1}]: {response[i]}\n");

        display.text = stringBuilder.ToString();
    }

    /// <summary> Called by UnityEngine.UI.InputField </summary>
    public async void OnSend(string input)
    {
        if (isStream)
        {
            display.text = string.Empty;
            await Ollama.GenerateStream(input, (string text) => { display.text += text; });
        }
        else
        {
            display.text = "processing...";
            var response = await Ollama.Generate(input);
            display.text = response;
        }
    }

    public void ToggleStream(bool val)
    {
        isStream = val;
    }
}
