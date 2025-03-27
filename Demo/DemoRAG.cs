using ollama;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class DemoRAG : MonoBehaviour
{
    [Header("Models")]
    [SerializeField]
    private string embedModel = "nomic-embed-text";
    [SerializeField]
    private string askModel = "gemma3:4b";

    [Header("UI")]
    [SerializeField]
    private TMP_Text llmOutput;
    [SerializeField]
    private TMP_InputField userInput;

    [Header("Context")]
    [SerializeField]
    private TextAsset[] documents;

    void Awake() { Ollama.Launch(); }

    async void Start()
    {
        userInput.interactable = false;
        llmOutput.text = "Generating Embeddings for each Document...";

        Ollama.InitRAG(embedModel);
        foreach (var asset in documents)
            await Ollama.AppendData(asset.text);

        llmOutput.text = "RAG is ready!";
        userInput.interactable = true;

        // Ollama.DebugContext();
    }

    /// <summary>Called by <b>TMP_InputField</b></summary>
    public async void OnSubmit(string input)
    {
        llmOutput.text = "processing...";
        var response = await Task.Run(async () => await Ollama.Ask(askModel, input));
        llmOutput.text = response;
    }
}
