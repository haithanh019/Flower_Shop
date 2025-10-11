namespace BusinessLogic.DTOs
{
    public class QueryParameters
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        // mở rộng
        public Guid? FilterID { get; set; }
        public Guid? FilterCategoryId { get; set; }
        public Guid? FilterProductId { get; set; }
        public bool? FilterBool { get; set; }
        public string? Search { get; set; }
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; } = false;
    }
}
