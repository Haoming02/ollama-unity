using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class OllamaGenerateDemo : MonoBehaviour
{
    [SerializeField]
    private string demoModel = "gemma2:2b";

    [SerializeField]
    private Text display;

    async void Start()
    {
        var models = await Ollama.List();
        foreach (var model in models)
            Debug.Log($"{model.name}: [{string.Join(',', model.details.families)}]");
    }

    /// <summary>Called by <b>UnityEngine.UI.InputField</b></summary>
    public async void OnSubmit(string input)
    {
        display.text = "processing...";
        var response = await Task.Run(async () => await Ollama.Generate(demoModel, input));
        display.text = response;
    }

    /// <summary>Called by <b>UnityEngine.UI.Button</b></summary>
    public async void OnMath()
    {
        string input = "Calculate the result of 1 + 1, return in a JSON format with a key `answer` and an `int` value.";

        display.text = "calculating...";
        var response = await Task.Run(async () => await Ollama.GenerateJson<MathResult>(demoModel, input));
        display.text = $"Answer: {response.answer}";
    }

    /// <summary>
    /// Called by <b>UnityEngine.UI.Button</b> <br/>
    /// You can send an empty string with <b>unload_immediately</b> to free the memory
    /// </summary>
    public async void Unload()
    {
        await Ollama.Generate(demoModel, "", null, Ollama.KeepAlive.unload_immediately);
    }

    private struct MathResult
    {
        public int answer;
    }
}
