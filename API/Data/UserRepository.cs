using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using API.Helpers;

namespace API.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        public UserRepository(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context=context;
        }

        public async Task<MemberDto> GetMemberAsync(string username)
        {
            return await _context.Users.Where(b=>b.UserName==username)
            .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
            .SingleOrDefaultAsync();
        }

        public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams)
        {
            var query=  _context.Users.AsQueryable();
           // .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
            //.AsNoTracking()
            //.AsQueryable();

            query= query.Where(b=>b.UserName!=userParams.CurrentUsername);
            query=query.Where(d=>d.Gender==userParams.Gender);

            var minDob= DateTime.Today.AddYears(-userParams.MaxAge-1);
            var maxDob= DateTime.Today.AddYears(-userParams.MinAge);

            query= query.Where(e=>e.DateOfBirth >= minDob && e.DateOfBirth <= maxDob);

            query= userParams.OrderBy switch
            {
                "createdAt" => query.OrderByDescending(u=>u.Created),
                _=> query.OrderByDescending(u=>u.LastActive)
            };
            
            return await PagedList<MemberDto>
            .CreateAsync(query.ProjectTo<MemberDto>(_mapper
            .ConfigurationProvider).AsNoTracking()
            ,userParams.PageNumber,userParams.PageSize);
            
        }

        public async Task<AppUser> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<AppUser> GetUserByUsernameAsync(string username)
        {
            return await _context.Users.Include(c=>c.Photos).SingleOrDefaultAsync(b=>b.UserName==username);
        }

        public async Task<string> GetUserGender(string username)
        {
            return await _context.Users
            .Where(b=>b.UserName== username)
            .Select(c=>c.Gender).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<AppUser>> GetUsersAsync()
        {
            return await _context.Users.
            Include(b=>b.Photos).ToListAsync();
        } 

        public void Update(AppUser user)
        {
            _context.Entry(user).State= EntityState.Modified; 
        }
    }
}