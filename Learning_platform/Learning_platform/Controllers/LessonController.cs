using Learning_platform.DTO;
using Learning_platform.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace Learning_platform.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LessonController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public LessonController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
            _context = context;
        }
        [HttpGet("PlayVideoForLesson/{lessonId}")]
        public IActionResult PlayVideoForLesson(int lessonId)
        {
            var lesson = _context.Lessons.Find(lessonId);

            if (lesson == null)
            {
                return NotFound("Lesson not found.");
            }

            return Ok(new
            {
                LessonName = lesson.Name,
                VideoUrl = $"videos/{lesson.Video}",
            });
        }


        [HttpGet("GetVideosForCourse/{courseId}")]
        public IActionResult GetVideosForCourse(int courseId)
        {
            var course = _context.Courses.Include(c => c.Lessons).FirstOrDefault(c => c.Id == courseId);

            if (course == null)
            {
                return NotFound("Course not found.");
            }

            var videos = course.Lessons.Select(lesson =>
            {
                //string videoUrl = GetVideoUrl(lesson.Video);
                return new
                {
                    LessonId = lesson.Id,
                    LessonName = lesson.Name,
                    VideoUrl = $"videos/{lesson.Video}",
                };
            }).ToList();

            return Ok(videos);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("addlesson")]
        public async Task<IActionResult> AddLesson([FromForm] LessonDTO lessonDTO)
        {
            if (lessonDTO == null)
            {
                return BadRequest("Invalid lesson data.");
            }

            var course = await _context.Courses.FindAsync(lessonDTO.CourseId);

            if (course == null)
            {
                return BadRequest($"Course with id '{lessonDTO.CourseId}' not found.");
            }

            if (lessonDTO.VideoFile != null)
            {
                string[] allowedExtensions = { ".mp4", ".avi", ".mkv" };
                string uploadsFolder = "videos";

                if (!allowedExtensions.Contains(Path.GetExtension(lessonDTO.VideoFile.FileName).ToLower()))
                {
                    return BadRequest("Only .mp4, .avi, and .mkv videos are allowed!");
                }

                var uniqueFileName = await SaveFile(lessonDTO.VideoFile, uploadsFolder);

                var lesson = new Lesson
                {
                    Name = lessonDTO.Name,
                    Description = lessonDTO.Description,
                    Video = uniqueFileName,
                    Course = course
                };

                _context.Lessons.Add(lesson);
                await _context.SaveChangesAsync();

                return Ok("Lesson added successfully.");
            }
            return BadRequest();
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("updatelesson/{id}")]
        public async Task<IActionResult> UpdateLesson(int id, [FromForm] LessonDTO lessonDTO)
        {
            var lessonToUpdate = await _context.Lessons.FindAsync(id);

            if (lessonToUpdate == null)
            {
                return NotFound($"Lesson with id '{id}' not found.");
            }

            if (lessonDTO.VideoFile != null)
            {
                string[] allowedExtensions = { ".mp4", ".avi", ".mkv" };
                string uploadsFolder = "videos";

                if (!allowedExtensions.Contains(Path.GetExtension(lessonDTO.VideoFile.FileName).ToLower()))
                {
                    return BadRequest("Only .mp4, .avi, and .mkv videos are allowed!");
                }

                var uniqueFileName = await SaveFile(lessonDTO.VideoFile, uploadsFolder);
                lessonToUpdate.Video = uniqueFileName;
            }

            lessonToUpdate.Name = lessonDTO.Name;
            lessonToUpdate.Description = lessonDTO.Description;

            await _context.SaveChangesAsync();

            return Ok("Lesson updated successfully.");
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("deletelesson/{id}")]
        public async Task<IActionResult> DeleteLesson(int id)
        {
            var lessonToDelete = await _context.Lessons.FindAsync(id);

            if (lessonToDelete == null)
            {
                return NotFound($"Lesson with id '{id}' not found.");
            }

            var videoPath = Path.Combine("wwwroot", "videos", lessonToDelete.Video);

            if (System.IO.File.Exists(videoPath))
            {
                System.IO.File.Delete(videoPath);
            }

            _context.Lessons.Remove(lessonToDelete);
            await _context.SaveChangesAsync();

            return Ok("Lesson deleted successfully.");
        }

        private async Task<string> SaveFile(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
            {
                return null;
            }

            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", folder);

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }
            return uniqueFileName;
        }
    }
}