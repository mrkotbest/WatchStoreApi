using WatchStoreApi.Application.Common;
using WatchStoreApi.Domain.Enums;

namespace WatchStoreApi.Application.DTOs.Products;

public class ProductFilterRequest : PagedRequest
{
    public string? Search { get; set; }
    public int? CategoryId { get; set; }
    public string? Material { get; set; }
    public Gender? Gender { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
}
