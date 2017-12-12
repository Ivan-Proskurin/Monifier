using System;

namespace Monifier.BusinessLogic.Model.Pagination
{
    public class PaginationInfo
    {
        public PaginationInfo(PaginationArgs args, int totalItemsCount)
        {
            if (args.ItemsPerPage == 0)
                throw new ArgumentException("Количество записей на страницу должно быть больше нуля", nameof(args.ItemsPerPage));
            if (args.PageNumber < 1 && args.PageNumber != -1)
                throw new ArgumentException("Номер страницы должно быть больше нуля", nameof(args.PageNumber));

            var totalPageCount = totalItemsCount / args.ItemsPerPage;
            if (totalItemsCount % args.ItemsPerPage > 0) totalPageCount++;

            var pageNumber = args.PageNumber == -1 ? totalPageCount : args.PageNumber;

            ItemsPerPage = args.ItemsPerPage;
            PageNumber = pageNumber;
            TotalPageCount = totalPageCount;
            Skipped = (pageNumber - 1) * args.ItemsPerPage;
            Taken = args.ItemsPerPage;
        }
        
        public int PageNumber { get; }
        public int ItemsPerPage { get; }
        public int TotalPageCount { get; }
        public int Skipped { get; }
        public int Taken { get; }

    }
}