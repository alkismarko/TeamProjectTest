using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using TeamProjectTest.Models;

namespace TeamProjectTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImagesUploadsController : ControllerBase
    {
        public static IWebHostEnvironment _webHostEnvironment;

        public ImagesUploadsController(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        //[Consumes("multipart/form-data")]
        [Authorize(Roles = "Vendor,Admin")]
        [HttpPost]
        public string Post([FromForm] ImageUpload fileUpload)
        {
            try
            {
                if (fileUpload.Image.Length > 0)
                {
                    //  string path = _webHostEnvironment.WebRootPath + "\\uploads\\"; //To default root
                    string path = Path.Combine(_webHostEnvironment.WebRootPath, "Images"); //To allagameno root gia na ftasw sto path pou thelw
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }

                    // using FileStream fileStream = System.IO.File.Create(path + fileUpload.Name + fileUpload.Image.FileName);
                    using FileStream fileStream = System.IO.File.Create(path + fileUpload.Name);
                    fileUpload.Image.CopyTo(fileStream);
                    fileStream.Flush();

                    return "Upload Done.";
                }
                else
                {
                    return "Failed.";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        [Authorize(Roles = "Vendor,Admin")]
        [HttpGet("{fileName}")]
        public IActionResult Get([FromRoute] string fileName)
        {
            // string path = _webHostEnvironment.WebRootPath + "\\uploads\\"; //To default root
            string path = Path.Combine(_webHostEnvironment.WebRootPath, "Images"); //To allagameno root gia na ftasw sto path pou thelw

            // var filePath = path + fileName + ".png";
            var filePath = path + fileName;
            if (System.IO.File.Exists(filePath))
            {
                byte[] b = System.IO.File.ReadAllBytes(filePath);
                return File(b, "image/png");
            }
            return null;
        }
    }
}
