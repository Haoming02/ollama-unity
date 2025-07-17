using ollama;
using TMPro;
using UnityEngine;

public class DemoTool : MonoBehaviour
{
    [Header("Models")]
    [SerializeField]
    private string toolModel = "qwen3:4b";

    [Header("UI")]
    [SerializeField]
    private TMP_Text llmOutput;

    private Ollama.ToolConstructor[] tools;

    void Awake() { Ollama.Launch(); }

    void Start()
    {
        var param = new Ollama.ToolConstructor.Parameter[]
        {
            new Ollama.ToolConstructor.Parameter("R", "int", "The value for the Red channel"),
            new Ollama.ToolConstructor.Parameter("G", "int", "The value for the Green channel"),
            new Ollama.ToolConstructor.Parameter("B", "int", "The value for the Blue channel")
        };

        var colorGenerator = new Ollama.ToolConstructor(
            "ColorGenerator",
            "Generate a Color from given RGB values",
            param,
            new string[] { "R", "G", "B" }
        );

        tools = new Ollama.ToolConstructor[] { colorGenerator };
    }

    private static Color32 ColorGenerator(byte R, byte G, byte B) => new Color32(R, G, B, 255);

    /// <summary>Called by <b>TMP_InputField</b></summary>
    public async void OnSubmit(string input)
    {
        llmOutput.color = Color.white;
        llmOutput.text = "processing...";

        Ollama.InitChat(4, "Use the given tools to fulfill the user's request, with simple and brief thinking and response");

        var (response, calls) = await Ollama.ChatWithTool(toolModel, input, tools: tools);

        llmOutput.text = response;

        var call = calls[0].function;
        if (call.name == "ColorGenerator")
        {
            var color = ColorGenerator(
                byte.Parse(call.arguments["R"]),
                byte.Parse(call.arguments["G"]),
                byte.Parse(call.arguments["B"])
            );

            llmOutput.color = color;
        }
        else
            Debug.LogWarning($"Unrecognized Function: {call.name}...");
    }
}
