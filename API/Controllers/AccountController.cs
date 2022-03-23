using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController: BaseApiController
    {
        private readonly DataContext _context;
        private readonly ITokenService _tokenservice;

        private readonly IMapper _mapper;
       public AccountController(DataContext context, ITokenService tokenservice, IMapper mapper)
       {
           _mapper = mapper;
           _tokenservice= tokenservice;
           _context= context;
       }

       [HttpPost("register")] 
       public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
       {
           if(await UserExists(registerDto.Username))
           {
               return BadRequest("Username already exists");
           }

           var user= _mapper.Map<AppUser>(registerDto);
          using var hmac= new HMACSHA512();
         
              user.UserName= registerDto.Username.ToLower();
              user.PasswordHash= hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password));
              user.PasswordSalt= hmac.Key;
          

           _context.Users.Add(user);
           await _context.SaveChangesAsync();
           return new UserDto{
               Username= user.UserName,
               Token= _tokenservice.CreateToken(user),
               KnownAs= user.KnownAs
           };
       }
           
      [HttpPost("login")]
      public async Task<ActionResult<UserDto>> Login(LoginDto loginDto )
      {
          var user= await _context.Users
          .Include(b=>b.Photos)
          .SingleOrDefaultAsync(b=>b.UserName==loginDto.Username.ToLower());
          if(user == null) return Unauthorized("Invalid User Name");

          using var hmac= new HMACSHA512(user.PasswordSalt);
          var computedHash= hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));
          for (int i=0 ; i< computedHash.Length; i++)
          {
              if (computedHash[i] != user.PasswordHash[i])
              return Unauthorized("Invalid Password");
          }

          return new UserDto{
               Username= user.UserName,
               Token= _tokenservice.CreateToken(user),
               PhotoUrl= user.Photos.FirstOrDefault(b=>b.IsMain)?.Url,
               KnownAs=user.KnownAs
           };
      }
      private async Task<bool> UserExists(string username)
      {
          return await _context.Users.AnyAsync(c=>c.UserName==username.ToLower());
      }
    }
}