namespace sjam.Models
{
    public class QueryRequest
    {
        public string? GlobalSearch { get; set; }
        public List<QueryFilter> Filters { get; set; } = new();
        public List<QuerySort> Sorts { get; set; } = new();
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
    public class QueryFilter
    {
        public string? Field { get; set; }        // Column name
        public string? Value { get; set; }       // Value or "v1,v2" or "from,to"
        public string? Operator { get; set; }     // eq, neq, gt, lt, gtet, ltet, ilike, btw, in, isnull, notnull
    }
    public class QuerySort
    {
        public string? Field { get; set; }
        public string Order { get; set; } = "asc"; // asc / desc
    }
    public class PagedResult<T>
    {
        public long TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public T Data { get; set; }
        public Dictionary<string, object?> MetaData { get; set; } = new Dictionary<string, object?>();
    }
}
