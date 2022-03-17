using API.DTOs;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        public UsersController(IUserRepository userRepository, IMapper mapper)
        {
            _mapper = mapper;
            _userRepository=userRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> Getusers()
        {
            var users= await  _userRepository.GetMembersAsync();
            //var usersToReturn=  _mapper.Map<IEnumerable<MemberDto>>(users);
            return Ok(users); 
        }

       
        [HttpGet("{username}")]
        public async Task<ActionResult<MemberDto>> Getuser(string username)
        {
            return  await _userRepository.GetMemberAsync(username);
        
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDTo)
        {
          var username=User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
          var user= await _userRepository.GetUserByUsernameAsync(username);
          _mapper.Map(memberUpdateDTo,user);
          _userRepository.Update(user);

          if(await _userRepository.SaveAllAsync()) return NoContent();
          return BadRequest("Failed to Update User");
        }
    }
}