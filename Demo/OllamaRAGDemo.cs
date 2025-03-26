using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using ollama;

public class OllamaRAGDemo : MonoBehaviour
{
    [SerializeField]
    private string demoModel = "llama3.1";

    [SerializeField]
    private Text display;
    [SerializeField]
    private InputField field;

    [SerializeField]
    private TextAsset[] context;

    void Awake() { Ollama.Launch(); }

    async void Start()
    {
        field.interactable = false;
        Ollama.InitRAG(demoModel);

        foreach (var asset in context)
            await Ollama.AppendData(asset.text);

        display.text = "ready";
        field.interactable = true;

        // Ollama.DebugContext();
    }

    /// <summary>Called by <b>UnityEngine.UI.InputField</b></summary>
    public async void OnSubmit(string input)
    {
        display.text = "processing...";
        var response = await Task.Run(async () => await Ollama.Ask(demoModel, input));
        display.text = response;
    }
}
