using Learning_platform.DTO;
using Learning_platform.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Learning_platform.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InstructorController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public InstructorController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("instructors")]
        public IActionResult GetAllInstructorsDetails()
        {
            var instructors = _context.Instructors.ToList();

            if (instructors == null || !instructors.Any())
            {
                return NotFound("No instructors found.");
            }

            var instructorDetails = instructors.Select(instructor =>
            {
                var imagePath = Path.Combine( "instructorImage", instructor.Image);
                    return new
                    {
                        Id = instructor.Id,
                        Name = instructor.Name,
                        ImageUrl = $"https://learningplatformv1.runasp.net/instructorImage//{imagePath}",
                        Description = instructor.Description,
                        Courses = instructor.Courses
                    };
            }).ToList();

            return Ok(instructorDetails);
        }


        [HttpGet("{instructorId}/instructor")]
        public IActionResult GetInstructorDetails(int instructorId)
        {
            var instructor = _context.Instructors.Find(instructorId);

            if (instructor == null)
            {
                return NotFound("Instructor not found.");
            }

            var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "instructorImage", instructor.Image);

            var imageDetails = new
            {
                Url = $"https://learningplatformv1.runasp.net/instructorImage//{instructor.Image}",
                Name = instructor.Name,
            };

            return Ok(imageDetails);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("add")]
        public async Task<IActionResult> AddInstructor([FromForm] InstructorDTO instructorDTO)
        {
            if (instructorDTO == null)
            {
                return BadRequest("Invalid instructor data.");
            }

            if (instructorDTO.Imageprofile != null)
            {
                string[] allowedExtensions = { ".png", ".jpg" };
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "instructorImage");

                if (!allowedExtensions.Contains(Path.GetExtension(instructorDTO.Imageprofile.FileName).ToLower()))
                {
                    return BadRequest("Only .png and .jpg images are allowed!");
                }

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + instructorDTO.Imageprofile.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await instructorDTO.Imageprofile.CopyToAsync(fileStream);
                }

                instructorDTO.Photo = Path.Combine("instructorImage", uniqueFileName);
            }
            else
            {
                return BadRequest("Profile image is required.");
            }

            var instructor = new Instructor
            {
                Name = instructorDTO.Name,
                Image = instructorDTO.Photo,
                Description = instructorDTO.Description,
            };

            _context.Instructors.Add(instructor);
            await _context.SaveChangesAsync();

            return Ok("Instructor added successfully.");
        }
        
        [Authorize(Roles = "Admin")]
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateInstructor(int id, [FromForm] InstructorDTO instructorDTO)
        {
            var instructorToUpdate = await _context.Instructors.FindAsync(id);

            if (instructorToUpdate == null)
            {
                return NotFound();
            }

            if (instructorDTO.Imageprofile != null)
            {
                if (!string.IsNullOrEmpty(instructorToUpdate.Image))
                {
                    var existingFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", instructorToUpdate.Image);
                    if (System.IO.File.Exists(existingFilePath))
                    {
                        System.IO.File.Delete(existingFilePath);
                    }
                }

                string[] allowedExtensions = { ".png", ".jpg" };
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "instructorImage");

                if (!allowedExtensions.Contains(Path.GetExtension(instructorDTO.Imageprofile.FileName).ToLower()))
                {
                    return BadRequest("Only .png and .jpg images are allowed!");
                }

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + instructorDTO.Imageprofile.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await instructorDTO.Imageprofile.CopyToAsync(fileStream);
                }

                instructorDTO.Photo = Path.Combine("instructorImage", uniqueFileName);
            }

            instructorToUpdate.Name = instructorDTO.Name;
            instructorToUpdate.Image = instructorDTO.Photo;
            instructorToUpdate.Description = instructorDTO.Description;

            await _context.SaveChangesAsync();

            return Ok("Instructor updated successfully.");
        }
        
        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteInstructor(int id)
        {
            var instructorToDelete = await _context.Instructors.FindAsync(id);

            if (instructorToDelete == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(instructorToDelete.Image))
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", instructorToDelete.Image);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            _context.Instructors.Remove(instructorToDelete);
            await _context.SaveChangesAsync();

            return Ok("Instructor deleted successfully.");
        }
    
    }
}