using Learning_platform.DTO;
using Learning_platform.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;

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
        public async Task<IActionResult> PlayVideoForLesson(int lessonId, string userId)
        {
            var lesson = await _context.Lessons.FindAsync(lessonId);

            if (lesson == null)
            {
                return NotFound("Lesson not found.");
            }

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not found.");
            }

            var courseLessons = await _context.Lessons
                .Where(l => l.CourseId == lesson.CourseId)
                .OrderBy(l => l.CreationDate)
                .ToListAsync();

            var currentLessonIndex = courseLessons.FindIndex(l => l.Id == lessonId);

            if (currentLessonIndex > 0)
            {
                var previousLessons = courseLessons.Take(currentLessonIndex).Select(l => l.Id).ToList();

                var watchedLessons = await _context.StudentLessons
                    .Where(sl => sl.StudentId == userId && previousLessons.Contains(sl.LessonId))
                    .Select(sl => sl.LessonId)
                    .ToListAsync();

                if (watchedLessons.Count != previousLessons.Count)
                {
                    return BadRequest("You must watch previous lessons before accessing this one.");
                }
            }

            var studentWatch = await _context.StudentLessons
                .Where(sl => sl.StudentId == userId && sl.LessonId == lessonId)
                .FirstOrDefaultAsync();

            if (studentWatch == null)
            {
                var studLesson = new StudentLessons
                {
                    StudentId = userId,
                    ApplicationUser = await _context.Users.FindAsync(userId),
                    LessonId = lessonId
                };

                _context.StudentLessons.Add(studLesson);
                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                LessonName = lesson.Name,
                VideoUrl = $"https://learningplatformv1.runasp.net/videos//{lesson.Video}",
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

            var videos = course.Lessons.OrderBy(c => c.CreationDate).Select(lesson =>
            {
                //string videoUrl = GetVideoUrl(lesson.Video);
                return new
                {
                    LessonId = lesson.Id,
                    LessonName = lesson.Name,
                    VideoUrl = $"https://learningplatformv1.runasp.net/videos//{lesson.Video}",
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
                    CourseId = course.Id,
                    Course = course,
                    CreationDate = DateTime.Now,
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