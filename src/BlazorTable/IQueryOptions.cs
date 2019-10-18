namespace BlazorTable
{
    public interface IQueryOptions
    {
        int PageNumber { get; set; }
        int PageSize { get; set; }
        bool SortDescending { get; set; }
        string SortColumn { get; set; }
    }
}