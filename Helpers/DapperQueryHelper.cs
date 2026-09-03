using Dapper;
using sjam.Models;
using System.Data;
using System.Reflection;

namespace sjam.Helpers
{
    public static class DapperQueryHelper
    {
        public static async Task<PagedResult<IEnumerable<T>>> ExecuteAsync<T>(
            IDbConnection connection,
            string baseQuery,
            QueryRequest request)
        {
            // -------------------------
            // Defaults
            // -------------------------
            int pageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
            int pageSize = request.PageSize <= 0 ? 10 : request.PageSize;

            // -------------------------
            // Fetch data
            // -------------------------
            IEnumerable<T> data = await connection.QueryAsync<T>(baseQuery);

            // -------------------------
            // GLOBAL SEARCH
            // -------------------------
            if (!string.IsNullOrWhiteSpace(request.GlobalSearch))
            {
                string search = request.GlobalSearch.Trim();

                data = data.Where(item =>
                    ObjectValueAccessor.GetKeys(item).Any(key =>
                    {
                        var value = ObjectValueAccessor.GetValue(item, key);
                        return value != null &&
                               value.ToString()!
                                    .Contains(search, StringComparison.OrdinalIgnoreCase);
                    }));
            }

            // -------------------------
            // FILTERS
            // -------------------------
            foreach (var filter in request.Filters)
            {
                if (string.IsNullOrWhiteSpace(filter.Field))
                    continue;

                data = data.Where(item =>
                {
                    var value = ObjectValueAccessor.GetValue(item, filter.Field);

                    // New operators
                    switch ((filter.Operator ?? "").ToLower())
                    {
                        case "isnull":
                            return value == null;

                        case "isnotnull":
                            return value != null;

                        case "isempty":
                            return string.IsNullOrWhiteSpace(value?.ToString());

                        case "isnotempty":
                            return !string.IsNullOrWhiteSpace(value?.ToString());
                    }

                    if (value == null) return false;

                    string text = value.ToString()!;
                    string filterValue = filter.Value ?? "";

                    return filter.Operator.ToLower() switch
                    {
                        "eq" => text.Equals(filterValue, StringComparison.OrdinalIgnoreCase),
                        "neq" => !text.Equals(filterValue, StringComparison.OrdinalIgnoreCase),
                        "ilike" => text.Contains(filterValue, StringComparison.OrdinalIgnoreCase),
                        "in" => filterValue
                                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                    .Any(v => v.Trim()
                                        .Equals(text, StringComparison.OrdinalIgnoreCase)),
                        "range" => HandleRange(value, filterValue),
                        _ => true
                    };
                });
            }

            // -------------------------
            // SORTING
            // -------------------------
            if (request.Sorts.Any())
            {
                IOrderedEnumerable<T>? ordered = null;

                foreach (var sort in request.Sorts)
                {
                    Func<T, object?> keySelector =
                        x => ObjectValueAccessor.GetValue(x, sort.Field);

                    ordered = ordered == null
                        ? (sort.Order.Equals("desc", StringComparison.OrdinalIgnoreCase)
                            ? data.OrderByDescending(keySelector)
                            : data.OrderBy(keySelector))
                        : (sort.Order.Equals("desc", StringComparison.OrdinalIgnoreCase)
                            ? ordered.ThenByDescending(keySelector)
                            : ordered.ThenBy(keySelector));
                }

                if (ordered != null)
                    data = ordered;
            }

            // -------------------------
            // TOTAL COUNT
            // -------------------------
            long totalCount = data.LongCount();

            // -------------------------
            // PAGINATION
            // -------------------------
            data = data
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            // -------------------------
            // RETURN
            // -------------------------
            return new PagedResult<IEnumerable<T>>
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Data = data
            };
        }

        // -------------------------
        // RANGE HANDLER
        // -------------------------
        private static bool HandleRange(object value, string filterValue)
        {
            var parts = filterValue
                .Split(',', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 2)
                return false;

            var from = parts[0].Trim();
            var to = parts[1].Trim();

            if (value is DateTime dt &&
                DateTime.TryParse(from, out var fromDt) &&
                DateTime.TryParse(to, out var toDt))
                return dt >= fromDt && dt <= toDt;

            if (decimal.TryParse(value.ToString(), out var num) &&
                decimal.TryParse(from, out var fromNum) &&
                decimal.TryParse(to, out var toNum))
                return num >= fromNum && num <= toNum;

            return false;
        }
    }

    public static class ObjectValueAccessor
    {
        public static IEnumerable<string> GetKeys<T>(T item)
        {
            if (item is IDictionary<string, object> dict)
                return dict.Keys;

            return typeof(T)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(p => p.Name);
        }

        public static object? GetValue<T>(T item, string key)
        {
            if (item is IDictionary<string, object> dict)
            {
                dict.TryGetValue(key, out var value);
                return value;
            }

            var prop = typeof(T).GetProperty(
                key,
                BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            return prop?.GetValue(item);
        }
    }
}
