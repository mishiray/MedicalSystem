using System.Collections.Generic;

namespace MedicalSystem.DTOs
{
    public class PaginatedResultDto<T>
    {
        public PageMetaData MetaData { get; set; }
        public IEnumerable<T> PagedList { get; set; }

        public PaginatedResultDto()
        {
            PagedList = new List<T>();
        }
    }

    public class PageMetaData
    {
        public int Page { get; set; }
        public int PerPage { get; set; }
        public int Total { get; set; }
        public int TotalPages { get; set; }
    }
}
