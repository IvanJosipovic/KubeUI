using System.ComponentModel;

public enum StringFilters
{
    [Description("Contains")]
    Contains,

    [Description("Does not contain")]
    Does_not_contain,

    [Description("Starts with")]
    Starts_with,

    [Description("Ends with")]
    Ends_with,

    [Description("Is equal to")]
    Is_equal_to,

    [Description("Is not equal to")]
    Is_not_equal_to,

    [Description("Is null")]
    Is_null,

    [Description("Is not null")]
    Is_not_null,

    [Description("Is empty")]
    Is_empty,

    [Description("Is not empty")]
    Is_not_empty,
}