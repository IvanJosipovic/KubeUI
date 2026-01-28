<<<<<<< HEAD
﻿namespace KubeUI.Controls;

/// <summary>
/// Custom DataGridTextColumn that binds the tooltip of the generated element to the column's binding.
/// </summary>
public class MyDataGridTextColumn : DataGridTextColumn
{
    protected override Control GenerateElement(DataGridCell cell, object dataItem)
    {
        var control = base.GenerateElement(cell, dataItem);

        if (Binding != null)
        {
            control.Bind(ToolTip.TipProperty, Binding);
        }

        return control;
    }
}
=======
﻿namespace KubeUI.Controls;

/// <summary>
/// Custom DataGridTextColumn that binds the tooltip of the generated element to the column's binding.
/// </summary>
public class MyDataGridTextColumn : DataGridTextColumn
{
    protected override Control GenerateElement(DataGridCell cell, object dataItem)
    {
        var control = base.GenerateElement(cell, dataItem);

        if (Binding != null)
        {
            control.Bind(ToolTip.TipProperty, Binding);
        }

        return control;
    }
}
>>>>>>> f1d4d8c68404aae64f8db01f52a60c2a3f3320b2
