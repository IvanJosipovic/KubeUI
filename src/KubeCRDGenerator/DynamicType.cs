namespace KubeCRDGenerator;

public class DynamicType
{
    public string? Name { get; set; }

    public string? Namespace { get; set; }

    public string? Implements { get; set; }

    public string? Description { get; set; }

    public bool AddUsing { get; set; }

    public List<string> Constant { get; set; } = new List<string>();

    public List<DynamicProperty> Fields { get; set; } = new List<DynamicProperty>();

    public List<string> Attributes { get; set; } = new List<string>();

    public override string ToString()
    {
        var result = "";

        if (AddUsing)
        {
            result += "using System;\n";
            result += "using System.Runtime;\n";
            result += "using k8s;\n";
            result += "using k8s.Models;\n";
            result += "using System.ComponentModel;\n";
            result += "using System.Collections.Generic;\n";
            result += "using System.Text.Json;\n";
            result += "using System.Text.Json.Nodes;\n";
            result += "using System.Text.Json.Serialization;\n";
        }

        if (!string.IsNullOrEmpty(Namespace))
        {
            result += $"namespace {Namespace};\n";
        }

        foreach (var attribute in Attributes)
            result += attribute + "\n";

        if (Description != null)
        {
            result += "/// <summary>\n";

            foreach (var row in Description.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None))
            {
                result += $"/// {row}\n";
            }

            result += "/// </summary>\n";
        }

        result += "public class " + Name + (Implements != null ? ": " + Implements : "");
        result += "\n{\n";

        foreach (var constant in Constant)
            result += constant + "\n";

        foreach (var dynamicField in Fields)
            result += dynamicField;

        result += "}";

        return result;
    }
}
