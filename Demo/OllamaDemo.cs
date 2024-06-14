using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class OllamaDemo : MonoBehaviour
{
    [SerializeField]
    private Text display;
    [SerializeField]
    [Tooltip("Remember to:\n- Enable Read/Write\n- Disable Compression")]
    private Texture2D image;

    private bool isStream = false;
    private bool isSafe = true;

    void OnEnable()
    {
        Ollama.OnStreamFinished += () => isSafe = true;
    }

    async void Start()
    {
        var (mdl, mm_mdl) = await Ollama.ListCategorized();
        StringBuilder stringBuilder = new StringBuilder();

        stringBuilder.AppendLine("Regular Models:");
        for (int i = 0; i < mdl.Length; i++)
            stringBuilder.AppendLine($"  {i + 1:D2}. {mdl[i]}");

        stringBuilder.AppendLine(string.Empty);

        stringBuilder.AppendLine("Multimodal Models:");
        for (int i = 0; i < mm_mdl.Length; i++)
            stringBuilder.AppendLine($"  {i + 1:D2}. {mm_mdl[i]}");

        display.text = stringBuilder.ToString();
    }

    /// <summary> Called by UnityEngine.UI.InputField </summary>
    public async void OnSend(string input)
    {
        if (isStream)
        {
            if (!isSafe)
                return;

            display.text = string.Empty;
            isSafe = false;
            await Ollama.GenerateStream(input, (string text) =>
            {
                if (display != null)
                    display.text += text;
            });
        }
        else
        {
            display.text = "processing...";
            var response = await Task.Run(async () => await Ollama.Generate(input));
            display.text = response;
        }
    }

    /// <summary> Called by UnityEngine.UI.Button </summary>
    public async void OnSendImage()
    {
        string b64Image = Convert.ToBase64String(image.EncodeToJPG());

        if (isStream)
        {
            if (!isSafe)
                return;

            display.text = string.Empty;
            isSafe = false;
            await Ollama.GenerateWithImageStream("What is in this picture?", b64Image, (string text) =>
            {
                if (display != null)
                    display.text += text;
            });
        }
        else
        {
            display.text = "processing...";
            var response = await Task.Run(async () => await Ollama.GenerateWithImage("What is in this picture?", b64Image));
            display.text = response;
        }
    }

    public void ToggleStream(bool val)
    {
        isStream = val;
    }
}
