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
    public IModuleRepository Modules { get; }
    public ICourseUserRepository CourseUsers { get; }
    public ICourseRepository Courses { get; }

    public UnitOfWork(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        Users = new UserRepository(userManager);
        Modules = new ModuleRepository(context);
        CourseUsers = new CourseUserRepository(context);
        Courses = new CourseRepository(context);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
