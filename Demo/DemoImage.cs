using ollama;
using TMPro;
using UnityEngine;

public class DemoImage : MonoBehaviour
{
    [Header("Models")]
    [SerializeField]
    private string visionModel = "gemma3:4b";

    [Header("UI")]
    [SerializeField]
    private TMP_Text llmOutput;

    [Header("UI")]
    [SerializeField]
    private Texture2D inputImage;

    void Awake() { Ollama.Launch(); }

    void Start()
    {
        if (!inputImage.isReadable)
            Debug.LogError("Please enable Read/Write permission for the image!");
    }

    /// <summary>Called by <b>TMP_InputField</b></summary>
    public async void OnSubmit(string input)
    {
        llmOutput.text = "processing...";
        var response = await Ollama.Generate(visionModel, input, images: new Texture2D[] { inputImage });
        llmOutput.text = response;
    }
}
