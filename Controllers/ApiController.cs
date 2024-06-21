using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SecureFileStorageProvider.Models;
using System.Globalization;
using System.Security.Claims;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace SecureFileStorageProvider.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class ApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webEnv;
        private readonly IConfiguration _config;
        public ApiController(ApplicationDbContext context, IConfiguration config, IWebHostEnvironment webEnv)
        {
            _context = context;
            _config = config;
            _webEnv = webEnv;
        }
        [HttpPost("UploadFile")]
        public async Task<IActionResult> UploadFile(IFormFile INPUT_FILE)
        {
            // Verification
            //var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            //if (userIdClaim == null) return StatusCode(401, new { Success = false, Error = "User ID not found in token" });
            //if (!int.TryParse(userIdClaim, out int UserId)) return StatusCode(400, new { Success = false, Error = "Invalid user ID" });
            var UserId = 1;
            if (!INPUT_FILE.FileName.EndsWith(".zip")) return StatusCode(400, new { Success = false, Error = "Invalid file type" });
            
            // Storage Id
            string storageId = Guid.NewGuid().ToString("N");
            string storageName = INPUT_FILE.FileName.Split(".zip")[0];

            // File Path Handling
            string basePath = _config["StorageSettings:basePath"];
            string destPath = Path.Combine(_webEnv.ContentRootPath, basePath, storageId);
            Directory.CreateDirectory(destPath);
            string destRootPath = Path.Combine(_webEnv.ContentRootPath, basePath, storageId, storageName);
            Directory.CreateDirectory(destRootPath);
            string filePath = Path.Combine(destPath, INPUT_FILE.FileName);
            string folderPath = Path.Combine(destPath, storageName);
            try
            {
                // Save the input file to the destPath
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await INPUT_FILE.CopyToAsync(fileStream);
                }

                // Extract the zip file to the destination folder
                ZipFile.ExtractToDirectory(filePath, destPath);

                // Move contents inside unzipped root folder outside
                foreach (var file in Directory.GetFiles(destRootPath, "*", SearchOption.AllDirectories))
                {
                    var relativePath = Path.GetRelativePath(destRootPath, file);
                    var destinationFile = Path.Combine(destPath, relativePath);
                    var destinationDir = Path.GetDirectoryName(destinationFile);
                    Directory.CreateDirectory(destinationDir);
                    System.IO.File.Move(file, destinationFile);
                }

                // Delete the extracted subdirectory and the ZIP file
                Directory.Delete(destRootPath, true);
                System.IO.File.Delete(filePath);

                // Calculate the size of the extracted files
                int size = Directory.EnumerateFiles(destPath, "*", SearchOption.AllDirectories)
                                    .Sum(file => (int)new FileInfo(file).Length);

                // Save To Database
                var storageItem = new Storage
                {
                    StorageId = storageId,
                    Name = storageName,
                    Size = size,
                    UserId = UserId
                };

                await _context.Storages.AddAsync(storageItem);
                await _context.SaveChangesAsync();

                return Ok(new {Success = true});
            } catch (Exception ex)
            {
                // Directory.Delete(destPath, true);
                return StatusCode(500, new { Success = false, Error = ex.Message });
            }
        }
        [HttpGet("UserList")]
        public IActionResult GetUserList()
        {
            try
            {
                return Ok(new { Success = true, User = _context.Users.ToList() });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Error = ex.Message });
            }
        }
        [HttpGet("StorageList")]
        public async Task<IActionResult> GetStorageList()
        {
            try
            {
                return Ok(new { Success = true, Storage = await _context.Storages.ToListAsync() });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Error = ex.Message });
            }
        }
    }
}