using Learning_platform.DTO;
using Learning_platform.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Learning_platform.Controllers
{
    public class FavController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FavController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("AddCourseTOFavList")]
        public IActionResult AddTOFav( [FromBody]FavDTO voteDTO)
        {
            if (voteDTO == null)
            {
                return BadRequest("Invalid fav data.");
            }

            var fav = _context.Favorites.Where(x => x.UserId == voteDTO.UserId && x.CourseId == voteDTO.CourseId).Count();
            if(fav != 0)
            {
                return BadRequest("This Course is  Alread in Faivorate List.");
            }
            var course = _context.Courses.Find(voteDTO.CourseId);

            var faivorate = new Favorite
            {
                UserId = voteDTO.UserId,
                CourseId = voteDTO.CourseId,
                Course = course,
            };

            _context.Favorites.Add(faivorate);
            _context.SaveChanges();

            return Ok("Add TO Faivorate succeded.");
        }


        [HttpDelete("deletefromFav")]
        public IActionResult DeleteVoteForCourse(int courseId, string userId)
        {
            var fav = _context.Favorites.Where(x => x.UserId == userId && x.CourseId == courseId).FirstOrDefault();
            if (fav == null)
            {
                return BadRequest("This Course Don't in Faivorate List.");
            }


            _context.Favorites.Remove(fav);
            _context.SaveChanges();

            return Ok("Deleted From Faivorate List Succeded.");
        }


    }
}
