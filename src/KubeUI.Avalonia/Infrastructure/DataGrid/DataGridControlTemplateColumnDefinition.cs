// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#nullable disable

using Avalonia.Controls.Templates;

namespace Avalonia.Controls
{
#if !DATAGRID_INTERNAL
    public
#else
    internal
#endif
    sealed class DataGridControlTemplateColumnDefinition : DataGridColumnDefinition
    {
        private IDataTemplate _cellTemplate;
        private bool? _reuseCellContent;

        public IDataTemplate CellTemplate
        {
            get => _cellTemplate;
            set => SetProperty(ref _cellTemplate, value);
        }

        public bool? ReuseCellContent
        {
            get => _reuseCellContent;
            set => SetProperty(ref _reuseCellContent, value);
        }

        protected override DataGridColumn CreateColumnCore()
        {
            return new DataGridResourceTemplateColumn();
        }

        protected override void ApplyColumnProperties(DataGridColumn column, DataGridColumnDefinitionContext context)
        {
            if (column is DataGridTemplateColumn templateColumn)
            {
                if (ReuseCellContent.HasValue)
                {
                    templateColumn.ReuseCellContent = ReuseCellContent.Value;
                }
                else
                {
                    templateColumn.ClearValue(DataGridTemplateColumn.ReuseCellContentProperty);
                }

                templateColumn.CellTemplate = CellTemplate;
            }

        }

        protected override bool ApplyColumnPropertyChange(
            DataGridColumn column,
            DataGridColumnDefinitionContext context,
            string propertyName)
        {
            if (column is not DataGridTemplateColumn templateColumn)
            {
                return false;
            }

            switch (propertyName)
            {
                case nameof(CellTemplate):
                    templateColumn.CellTemplate = CellTemplate;
                    return true;
                case nameof(ReuseCellContent):
                    if (ReuseCellContent.HasValue)
                    {
                        templateColumn.ReuseCellContent = ReuseCellContent.Value;
                    }
                    else
                    {
                        templateColumn.ClearValue(DataGridTemplateColumn.ReuseCellContentProperty);
                    }

                    templateColumn.CellTemplate = CellTemplate;
                    return true;
            }

            return false;
        }

    /// <summary>
    /// Template column that refreshes the data context of reused cell content.
    /// </summary>
    sealed class DataGridResourceTemplateColumn : DataGridTemplateColumn
    {
        protected override Control GenerateElement(DataGridCell cell, object dataItem)
        {
            var control = base.GenerateElement(cell, dataItem);
            control.DataContext = dataItem;
            return control;
        }
    }
    }
}
