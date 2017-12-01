namespace Monifier.BusinessLogic.Model.Pagination
{
    public class PaginationArgs
    {
        public bool IncludeDeleted { get; set; }
        public int PageNumber { get; set; }
        public int ItemsPerPage { get; set; }
    }
}