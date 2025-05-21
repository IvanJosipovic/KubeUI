using System.Linq.Expressions;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Experimental.Data;

namespace KubeUI
{
    /// <summary>
    /// A column in an <see cref="ITreeDataGridSource"/> which displays its values as text.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    /// <typeparam name="TValue">The column data type.</typeparam>
    public class MyCustomColumn<TModel, TValue> : ColumnBase<TModel, TValue>//, ITextSearchableColumn<TModel>
        where TModel : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TextColumn{TModel, TValue}"/> class.
        /// </summary>
        /// <param name="header">The column header.</param>
        /// <param name="getter">
        /// An expression which given a row model, returns a cell value for the column.
        /// </param>
        /// <param name="width">
        /// The column width. If null defaults to <see cref="GridLength.Auto"/>.
        /// </param>
        /// <param name="options">Additional column options.</param>
        //public MyCustomColumn(
        //    object? header,
        //    Expression<Func<TModel, TValue?>> getter,
        //    GridLength? width = null,
        //    TextColumnOptions<TModel>? options = null)
        //    : base(header, getter, null, width, options ?? new())
        //{
        //}

        //object? header,
        //    Func<TModel, TValue?> valueSelector,
        //    TypedBinding<TModel, TValue?> binding,
        //    GridLength? width,
        //    ColumnOptions<TModel>? options

        public MyCustomColumn(
            object? header,
            Func<TModel, TValue?> valueSelector,
            TypedBinding<TModel, TValue?> binding,
            GridLength? width,
            ColumnOptions<TModel>? options)
            : base(header, valueSelector, binding, width, options)
        {

        }

        //public new TextColumnOptions<TModel> Options => (TextColumnOptions<TModel>)base.Options;

        //bool ITextSearchableColumn<TModel>.IsTextSearchEnabled => Options?.IsTextSearchEnabled ?? false;

        public override ICell CreateCell(IRow<TModel> row)
        {
            return new TextCell<TValue?>(CreateBindingExpression(row.Model), Binding.Write is null);
        }

        //string? ITextSearchableColumn<TModel>.SelectValue(TModel model)
        //{
        //    return ValueSelector(model)?.ToString();
        //}
    }
}
