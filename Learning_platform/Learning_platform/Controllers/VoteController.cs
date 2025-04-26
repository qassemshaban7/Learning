using Learning_platform.DTO;
using Learning_platform.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Learning_platform.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VoteController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public VoteController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("{courseId}/average-vote")]
        public IActionResult GetAverageVoteForCourse(int courseId)
        {
            var course = _context.Courses.Include(c => c.Votes).FirstOrDefault(c => c.Id == courseId);
            if (course == null)
            {
                return NotFound("Course not found.");
            }
            var votes = course.Votes;
            if (votes == null || votes.Count == 0)
            {
                return Ok(0);
            }
            int totalVotes = votes.Count;
            int totalValues = votes.Sum(v => v.Value);
            double averageVote = (double)totalValues / totalVotes;
            int roundedAverage = (int)Math.Round(averageVote);
            return Ok(roundedAverage);
        }
        
        [HttpPost("AddVoteForCourse")]
        public IActionResult AddVoteForCourse(int courseId, string userId, [FromBody] VoteDTO voteDTO)
        {
            if (voteDTO == null)
            {
                return BadRequest("Invalid vote data.");
            }

            var vot = _context.Votes.Where(x => x.UserId == userId && x.Course.Id == courseId).Count();

            if (vot != 0)
            {
                return BadRequest("Sorry You are Alread Voted this Coures.");
            }

            if (voteDTO.Value > 5)
            {
                return BadRequest("Vote value cannot exceed 5.");
            }

            var course = _context.Courses.Find(courseId);

            if (course == null)
            {
                return NotFound("Course not found.");
            }

            var vote = new Vote
            {
                UserId = userId,
                Value = voteDTO.Value,
                Course = course
            };

            _context.Votes.Add(vote);
            _context.SaveChanges();

            return Ok("Vote added successfully.");
        }

        [HttpPut("{id}")]
        public IActionResult UpdateVoteForCourse(int courseId, int id, [FromBody] VoteDTO voteDTO)
        {
            var existingVote = _context.Votes.FirstOrDefault(v => v.Id == id && v.Course.Id == courseId);

            if (existingVote == null)
            {
                return NotFound("Vote not found.");
            }

            if (voteDTO.Value > 5)
            {
                return BadRequest("Vote value cannot exceed 5.");
            }

            existingVote.Value = voteDTO.Value;

            _context.SaveChanges();

            return Ok("Vote updated successfully.");
        }
        
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public IActionResult DeleteVoteForCourse(int courseId, int id)
        {
            var voteToDelete = _context.Votes.FirstOrDefault(v => v.Id == id && v.Course.Id == courseId);

            if (voteToDelete == null)
            {
                return NotFound("Vote not found.");
            }

            _context.Votes.Remove(voteToDelete);
            _context.SaveChanges();

            return Ok("Vote deleted successfully.");
        }
    }
}
