using MedicalSystem.DTOs;

namespace MedicalSystem.Utilities
{
    public static class Pagination
    {
        public static int _pageNumDefault = 1;
        public static int _perPageDefault = 5;

        public static PaginatedResultDto<T> GetPagedData<T>(List<T> paginatedList, int page, int perPage, int total) where T : class
        {
            page = page < 1 ? _pageNumDefault : page;
            perPage = perPage < 1 ? _perPageDefault : perPage;

            var total_pages = total % perPage == 0 ? total / perPage : total / perPage + 1;

            var pageMeta = new PageMetaData
            {
                Page = page,
                PerPage = perPage,
                Total = total,
                TotalPages = total_pages
            };

            return new PaginatedResultDto<T>
            {
                MetaData = pageMeta,
                PagedList = paginatedList
            };
        }

        public static IEnumerable<T> Paginate<T>(this IQueryable<T> source, int page, int perPage) where T : class
        {
            page = page < 1 ? _pageNumDefault : page;
            perPage = perPage < 1 ? _perPageDefault : perPage;

            return source.Skip((page - 1) * perPage).Take(perPage);
        }
    }
}
