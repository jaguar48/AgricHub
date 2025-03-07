namespace AgricHub.Shared.DTO_s.Response;

public record PaginatedResult<T>(
    IEnumerable<T> Items,
    bool HasNextPage,
    DateTime? NextPageKey  // Last CreatedDate from current page
);
