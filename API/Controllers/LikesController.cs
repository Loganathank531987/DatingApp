using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class LikesController : BaseApiController
    {
        private readonly IUserRepository _userRepository; 
        private readonly ILikesRepository _likesRepository; 
        public LikesController(IUserRepository userRepository, ILikesRepository likesRepository)
        {
            _userRepository = userRepository;
            _likesRepository= likesRepository;
        }

        [HttpPost("{username}")]
        public async Task<ActionResult> AddLike(string username)
        {
            var sourceUserId= User.GetUserId();
            var likeduser= await _userRepository.GetUserByUsernameAsync(username);
            var sourceuser= await _likesRepository.GetUserWithLikes(sourceUserId);

            if(likeduser == null) return NotFound();
            if(sourceuser.UserName== username) return BadRequest("you Cannot Like Yourself");

            var userLike= await _likesRepository.GetUserLike(sourceUserId,likeduser.Id);

            if(userLike!= null) return BadRequest("You already liked this user");

            userLike= new UserLike 
            {
                SourceUserId= sourceUserId,
                LikeUserId= likeduser.Id
            };

            sourceuser.LikedUsers.Add(userLike);
            if(await _userRepository.SaveAllAsync()) return Ok();

            return BadRequest("Failed To Like User");
        
        }

        [HttpGet] 
        public async Task<ActionResult<IEnumerable<LikeDto>>> GetUserLikes([FromQuery]LikesParams likesParams)
        {   likesParams.UserId= User.GetUserId();
            var users=  await _likesRepository.GetUserLikes(likesParams);
            Response.AddPaginationHeader(users.CurrentPage
            ,users.PageSize, users.TotalCount,users.TotalPages);

            return Ok(users);
        }
    }
}