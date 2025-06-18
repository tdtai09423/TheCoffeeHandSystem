using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace Interfracture.PaggingItems
{
    public class PaginatedList<T>
    {
        public IReadOnlyCollection<T> Items { get; private set; } = new List<T>();
        public int PageNumber { get; private set; }
        public int TotalPages { get; private set; }
        public int TotalCount { get; private set; }
        public int PageSize { get; private set; }

        // Parameterless constructor for deserialization
        public PaginatedList() { }

        // Constructor with [JsonConstructor] to help System.Text.Json bind values properly
        [JsonConstructor]
        public PaginatedList(IReadOnlyCollection<T> items, int totalCount, int pageNumber, int pageSize)
        {
            PageNumber = pageNumber;
            TotalPages = (totalCount == 0) ? 1 : (int)Math.Ceiling(totalCount / (double)pageSize);
            TotalCount = totalCount;
            Items = items;
            PageSize = pageSize;
        }

        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;

        public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
        {
            var totalCount = await source.CountAsync();
            var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PaginatedList<T>(items, totalCount, pageNumber, pageSize);
        }

        public static PaginatedList<T> Create(List<T> source, int pageNumber, int pageSize)
        {
            var totalCount = source.Count;
            var items = source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            return new PaginatedList<T>(items, totalCount, pageNumber, pageSize);
        }
    }
}
