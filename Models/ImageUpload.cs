using Microsoft.AspNetCore.Http;

namespace TeamProjectTest.Models
{
    public class ImageUpload
    {
        public IFormFile Image { get; set; }

        public string Name { get; set; }
    }
}
