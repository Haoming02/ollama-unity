using ollama;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DemoGenerate : MonoBehaviour
{
    [Header("UI")]
    [SerializeField]
    private TMP_Text llmOutput;
    [SerializeField]
    private TMP_Dropdown modelSelection;

    private List<string> modelNames;

    void Awake() { Ollama.Launch(); }

    async void Start()
    {
        var models = await Ollama.List();

        modelNames = new List<string>();
        foreach (var model in models)
            modelNames.Add(model.name);

        modelSelection.AddOptions(modelNames);
        modelSelection.value = 0;
    }

    /// <summary>Called by <b>TMP_InputField</b></summary>
    public async void OnSubmit(string input)
    {
        llmOutput.text = "processing...";
        var response = await Ollama.Generate(modelNames[modelSelection.value], input);
        llmOutput.text = response;
    }

    /// <summary>Called by <b>UnityEngine.UI.Button</b></summary>
    public async void OnMath()
    {
        string input = "Calculate the result of 1 + 1, return in a JSON format with a key `answer` and an `int` value.";

        llmOutput.text = "calculating...";
        var response = await Ollama.GenerateJson<MathResult>(modelNames[modelSelection.value], input);
        llmOutput.text = $"Answer: {response.answer}";
    }

    private struct MathResult { public int answer; }
}
