using System;

namespace Monifier.BusinessLogic.Model.Pagination
{
    public class PaginationInfo
    {
        public int PageNumber { get; set; }
        public int ItemsPerPage { get; set; }
        public int TotalPageCount { get; set; }
        public int Skipped { get; set; }
        public int Taken { get; set; }

        public static PaginationInfo FromArgs(PaginationArgs args, int totalItemsCount)
        {
            if (args.ItemsPerPage == 0)
                throw new ArgumentException("Количество записей на страницу должно быть больше нуля", nameof(args.ItemsPerPage));
            if (args.PageNumber < 1 && args.PageNumber != -1)
                throw new ArgumentException("Номер страницы должно быть больше нуля", nameof(args.PageNumber));

            var totalPageCount = totalItemsCount / args.ItemsPerPage;
            if (totalItemsCount % args.ItemsPerPage > 0) totalPageCount++;

            var pageNumber = args.PageNumber == -1 ? totalPageCount : args.PageNumber;

            var info = new PaginationInfo
            {
                ItemsPerPage = args.ItemsPerPage,
                PageNumber = pageNumber,
                TotalPageCount = totalPageCount,
                Skipped = (pageNumber - 1) * args.ItemsPerPage,
                Taken = args.ItemsPerPage
            };
            return info;
        }
    }
}