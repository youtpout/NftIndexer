using System;
namespace NftIndexer.Dtos
{
    public class PaginationDto
    {
        public PaginationDto()
        {
            Tokens = new List<TokenDto>();
        }

        public int Skip { get; set; }
        public int Take { get; set; }
        public int Total { get; set; }

        public List<TokenDto> Tokens { get; set; }
    }
}

