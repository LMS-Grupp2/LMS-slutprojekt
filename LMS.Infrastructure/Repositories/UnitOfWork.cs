using Domain.Contracts;
using Domain.Contracts.Repositories;                       
using Domain.Models.Entities;                              
using LMS.Infrastructure.Data;                             
using Microsoft.AspNetCore.Identity;


namespace LMS.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public IUserRepository Users { get; }
    public ICourseUserRepository CourseUsers { get; }

    public UnitOfWork(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        Users = new UserRepository(userManager);
        CourseUsers = new CourseUserRepository(context);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
