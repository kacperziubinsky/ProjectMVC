using Newtonsoft.Json;

namespace MVCProject.Models;

public class FileReadViewModel
{
    public IFormFile? UploadedFile { get; set; }

    public string FileContent { get; set; }
}