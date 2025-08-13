using Microsoft.AspNetCore.Mvc;

namespace EcoFashionBackEnd.Common.Payloads.Requests
{
    public class CreateBlogRequest
    {
        public required string Title { get; set; }
        public required string Content { get; set; }
        [FromForm]
        public List<IFormFile> ImageFiles { get; set; } = new();
    }
}
