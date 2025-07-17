using System.Collections.Generic;

namespace ollama
{
    public static partial class Ollama
    {
        // https://github.com/ollama/ollama/blob/main/docs/api.md#tool-calling

        public class ToolConstructor
        {
            public string functionName;
            public string functionDescription;
            public Parameter[] functionParameters;
            public string[] requiredParameters;

            public class Parameter
            {
                public string paramName;
                public string paramType;
                public string paramDescription;

                public Parameter(string paramName, string paramType, string paramDescription)
                {
                    this.paramName = paramName;
                    this.paramType = paramType;
                    this.paramDescription = paramDescription;
                }
            }

            public ToolConstructor(string functionName, string functionDescription, Parameter[] functionParameters, string[] requiredParameters)
            {
                this.functionName = functionName;
                this.functionDescription = functionDescription;
                this.functionParameters = functionParameters;
                this.requiredParameters = requiredParameters;
            }
        }

        public class ToolCall
        {
            public Function function;

            public class Function
            {
                public string name;
                public Dictionary<string, string> arguments;
            }
        }


        private class Tool
        {
            public string type = "function";
            public Function function;
        }

        private class Function
        {
            public string name;
            public string description;
            public Parameters parameters;
        }

        private class Parameters
        {
            public string type = "object";
            public Dictionary<string, Dictionary<string, string>> properties;
            public string[] required;
        }


        private static List<Tool> ConstructTools(ToolConstructor[] args)
        {
            if (args == null || args.Length == 0)
                return null;

            var result = new List<Tool>();

            foreach (var arg in args)
            {
                var props = new Dictionary<string, Dictionary<string, string>>();

                foreach (var p in arg.functionParameters)
                {
                    props.Add(p.paramName, new Dictionary<string, string>() {
                        {"type", p.paramName},
                        {"description", p.paramDescription}
                    });
                }

                var param = new Parameters()
                {
                    properties = props,
                    required = arg.requiredParameters
                };

                var func = new Function()
                {
                    name = arg.functionName,
                    description = arg.functionDescription,
                    parameters = param
                };

                result.Add(new Tool() { function = func });
            }

            return result;
        }
    }
}
