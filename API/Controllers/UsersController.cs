using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
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
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        private readonly IPhotoService _photoService;
        public UsersController(IUnitOfWork unitOfWork,
         IMapper mapper, IPhotoService photoService)
        {
            _photoService=photoService;
            _mapper = mapper;
            _unitOfWork=unitOfWork;
        }
        
       
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> Getusers([FromQuery]UserParams userParams)
        {
            var gender= await _unitOfWork.UserRepository.GetUserGender(User.GetUserName());
            userParams.CurrentUsername= User.GetUserName();
            if(string.IsNullOrEmpty(userParams.Gender))
            {
                userParams.Gender = gender=="male"? "female": "male";
            }
            var users= await _unitOfWork.UserRepository.GetMembersAsync(userParams);
            Response.AddPaginationHeader(users.CurrentPage, 
            users.PageSize,users.TotalCount, users.TotalPages);
            //var usersToReturn=  _mapper.Map<IEnumerable<MemberDto>>(users);
            return Ok(users); 
        }

       
        [HttpGet("{username}", Name ="GetUser")]
        public async Task<ActionResult<MemberDto>> Getuser(string username)
        {
            return  await _unitOfWork.UserRepository.GetMemberAsync(username);
        
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDTo)
        {
          var user= await _unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUserName());
          _mapper.Map(memberUpdateDTo,user);
          _unitOfWork.UserRepository.Update(user);

          if(await _unitOfWork.Complete()) return NoContent();
          return BadRequest("Failed to Update User");
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
          var user= await _unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUserName());
          var result= await _photoService.AddPhotoAsync(file);
          if(result.Error!=null)
          {
            return BadRequest(result.Error.Message);
          }

          var photo= new Photo{
              Url=result.SecureUrl.AbsoluteUri,
              PublicId=result.PublicId
          };

          if(user.Photos.Count == 0)
          {
              photo.IsMain= true;
          }

         user.Photos.Add(photo);
         if(await _unitOfWork.Complete())
         {
             
             //return CreatedAtRoute("GetUser",_mapper.Map<PhotoDto>(photo));
             return CreatedAtRoute("GetUser",new {username=user.UserName},_mapper.Map<PhotoDto>(photo));

         }
         return BadRequest("Error While Adding Photo");
        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var user= await _unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUserName());
            var photo= user.Photos.FirstOrDefault(b=>b.Id==photoId);
            if(photo.IsMain) return BadRequest("This is already your main Photo");
            var currentMain= user.Photos.FirstOrDefault(b=>b.IsMain);
            if(currentMain!=null)
            {
                   currentMain.IsMain=false;
            }
            photo.IsMain= true;

            if(await _unitOfWork.Complete())
            {
                return NoContent();
            }

            return BadRequest("Failed To Set Main photo");

        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
           var user= await _unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUserName());
           var photo= user.Photos.FirstOrDefault(b=>b.Id==photoId);
           if(photo==null) return NotFound();
           if(photo.IsMain) return BadRequest("You Cannot Delete your main Photo");
           if(photo.PublicId!= null) 
           {
            var result= await _photoService.DeletePhotoAsync(photo.PublicId);
            
            if(result.Error!=null)
            {
                return BadRequest(result.Error.Message);
            }
           }

            user.Photos.Remove(photo);

            if(await _unitOfWork.Complete())
            {
                return Ok();
            } 

            return BadRequest("Failed to Delete Photo");
          
        }

    }
}