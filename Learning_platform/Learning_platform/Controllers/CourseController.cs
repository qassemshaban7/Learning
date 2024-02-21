using Learning_platform.DTO;
using Learning_platform.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
namespace Learning_platform.Controllers
{
    
    [ApiController]
    [Route("api/courses")]
    public class CourseController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        IWebHostEnvironment webHostEnvironment;
        public CourseController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            this.webHostEnvironment = webHostEnvironment;
        }

        [HttpGet("search")]
        public IActionResult SearchCoursesByName([FromQuery] string courseName)
        {
            if (string.IsNullOrWhiteSpace(courseName))
            {
                return BadRequest("No result.");
            }

            var courses = _context.Courses
                .Where(c => c.Name.Contains(courseName, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (courses.Count == 0)
            {
                return NotFound("No result.");
            }

            var courseNames = courses.Select(c => c.Name).ToList();
            return Ok(courseNames);
        }
        [HttpGet("getallcourses")]
        public IActionResult GetCourses()
        {
            var courses = _context.Courses.ToList();
            var courseDTOs = courses.Select(c => new getcoursedto
            {
                Id = c.Id,
                Name = c.Name, 
                Description = c.Description, 
                Price = c.Price,
                ImageOfCertificate = c.ImageOfCertificate,
            }).ToList();
            return Ok(courseDTOs);
        }
        [HttpGet("getcoursebID/{id}")]
        public IActionResult GetCourseById(int id)
        {
            var course = _context.Courses.Find(id);

            if (course == null)
            {
                return NotFound();
            }

            var courseDTO = new getcoursedto
            {
                Id = course.Id,
                Name = course.Name,
                Description = course.Description ,
                Price = course.Price,
                ImageOfCertificate = course.ImageOfCertificate,
            };
            return Ok(courseDTO);
        }
        [HttpGet("getallinstructorinsideonecourse/{courseId}")]
        public IActionResult GetInstructorsByCourse(int courseId)
        {
            var course = _context.Courses.Include(c => c.Instructors).FirstOrDefault(c => c.Id == courseId);

            if (course == null)
            {
                return NotFound("Course not found.");
            }

            var instructors = course.Instructors.Select(i => new InstructorDetailsDTO
            {
                Name = i.Name,
                Photo = i.Image,
                Description = i.Description,
            }).ToList();

            return Ok(instructors);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost("addcourse")]
        public async Task<IActionResult> AddCourse([FromForm] AddCourseDTO courseDTO)
        {
            if (courseDTO == null)
            {
                return BadRequest("Invalid course data.");
            }

            var category = await _context.Category.FindAsync(courseDTO.Category_Id);
            var instructor = await _context.Instructors.FindAsync(courseDTO.Instructor_Id);

            if (category == null || instructor == null)
            {
                return BadRequest("Invalid category or instructor data.");
            }

            if (_context.Courses.Any(c => c.Name == courseDTO.Name))
            {
                return BadRequest($"Course with name '{courseDTO.Name}' already exists.");
            }

            var uniqueFileName = await SaveFile(courseDTO.ImageOfCertificate, "certificates");

            var course = new Course
            {
                Name = courseDTO.Name,
                Description = courseDTO.Description,
                ImageOfCertificate = uniqueFileName,
                Price = courseDTO.Price,
                Category = category,
                Instructors = new List<Instructor> { instructor }
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            return Ok("Course added successfully.");
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("updatecourse/{id}")]
        public async Task<IActionResult> UpdateCourse(int id, [FromForm] AddCourseDTO courseUpdateDTO)
        {
            var existingCourse = await _context.Courses.FindAsync(id);

            if (existingCourse == null)
            {
                return NotFound();
            }

            var category = await _context.Category.FindAsync(courseUpdateDTO.Category_Id);
            var instructor = await _context.Instructors.FindAsync(courseUpdateDTO.Instructor_Id);

            if (category == null || instructor == null)
            {
                return BadRequest("Invalid category or instructor data.");
            }

            if (_context.Courses.Any(c => c.Name == courseUpdateDTO.Name && c.Id != id))
            {
                return BadRequest($"Course with name '{courseUpdateDTO.Name}' already exists.");
            }

            var uniqueFileName = await SaveFile(courseUpdateDTO.ImageOfCertificate, "certificates");

            existingCourse.Name = courseUpdateDTO.Name;
            existingCourse.Description = courseUpdateDTO.Description;
            existingCourse.Price = courseUpdateDTO.Price;
            existingCourse.ImageOfCertificate = uniqueFileName;
            existingCourse.Category = category;
            existingCourse.Instructors = new List<Instructor> { instructor };

            await _context.SaveChangesAsync();

            return Ok("Course Updated successfully.");
        }

        private async Task<string> SaveFile(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
            {
                return null; // Handle the case where no file is provided
            }

            string uploadsFolder = Path.Combine(webHostEnvironment.WebRootPath, folder);

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return uniqueFileName;
        }

        // DELETE: api/courses/1
        [Authorize(Roles = "Admin")]
        [HttpDelete("deletecourse/{id}")]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var course = await _context.Courses.FindAsync(id);

            if (course == null)
            {
                return NotFound();
            }

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();

            return Ok("Course delete successfully.");
        }
    }
}
