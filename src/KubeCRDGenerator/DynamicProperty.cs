namespace KubeCRDGenerator;

public class DynamicProperty
{
    public DynamicProperty(string name, string fType, bool nullable = false, string? description = null, List<string> attributes = null)
    {
        Name = name;
        FType = fType;
        Nullable = nullable;
        Description = description;
        Attributes = attributes;
    }

    public string Name { get; set; }

    public string FType { get; set; }

    public bool Nullable { get; set; }

    public string? Description { get; set; }

    public List<string>? Attributes { get; set; }

    public override string ToString()
    {
        var output = "";

        if (Description != null)
        {
            output += "/// <summary>\n";

            foreach (var row in Description.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None))
            {
                output += $"/// {row}\n";
            }

            output += "/// </summary>\n";
        }

        if (Attributes != null)
        {
            foreach (var item in Attributes)
            {
                output += item + '\n';
            }
        }

        return output + "public " + FType + (Nullable ? "?" : "") + " " + Name + " {get; set;}\n";
    }
}
