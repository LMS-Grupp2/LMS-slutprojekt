using Domain.Contracts;
using Domain.Contracts.Repositories;
using Domain.Models.Entities;
using LMS.Services;
using LMS.Shared.Paging;
using Moq;

namespace LMS.Tests;

public class UserServicePagingTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly UserService _service;

    public UserServicePagingTests()
    {
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(u => u.Users).Returns(_users.Object);
        _service = new UserService(unitOfWork.Object);
    }

    [Fact]
    public async Task GetPageAsync_PassesTheQueryToTheRepository()
    {
        var query = new UserListQuery { Search = "anna", Role = "Student", Page = 2, PageSize = 10 };
        _users.Setup(r => r.GetPageAsync(query))
            .ReturnsAsync(new PagedResult<ApplicationUser>([], 2, 10, 0));
        _users.Setup(r => r.GetRoleNamesByUserIdAsync())
            .ReturnsAsync(new Dictionary<string, string>());

        await _service.GetPageAsync(query);

        _users.Verify(r => r.GetPageAsync(query), Times.Once);
    }

    [Fact]
    public async Task GetPageAsync_MapsUsersToDtos_AndKeepsTheMetadata()
    {
        var anna = GivenUser("u1", "Anna", "anna@lms.local", courseName: ".NET 2026");
        var bo = GivenUser("u2", "Bo", "bo@lms.local", courseName: null);
        var query = new UserListQuery { Page = 2, PageSize = 2 };

        _users.Setup(r => r.GetPageAsync(query))
            .ReturnsAsync(new PagedResult<ApplicationUser>([anna, bo], 2, 2, 5));
        _users.Setup(r => r.GetRoleNamesByUserIdAsync())
            .ReturnsAsync(new Dictionary<string, string> { ["u1"] = "Student" }); // Bo has no role row

        var result = await _service.GetPageAsync(query);

        Assert.Equal(2, result.Page);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(3, result.TotalPages);

        var first = result.Items[0];
        Assert.Equal("Anna", first.Name);
        Assert.Equal("Student", first.Role);
        Assert.Equal([".NET 2026"], first.CourseNames);

        Assert.Equal("Role not assigned", result.Items[1].Role);
        Assert.Empty(result.Items[1].CourseNames);
    }

    [Fact]
    public async Task GetPageAsync_LooksUpRolesOnce_NotPerUser()
    {
        var query = new UserListQuery();
        _users.Setup(r => r.GetPageAsync(query))
            .ReturnsAsync(new PagedResult<ApplicationUser>(
                [GivenUser("u1", "A", "a@x"), GivenUser("u2", "B", "b@x"), GivenUser("u3", "C", "c@x")], 1, 20, 3));
        _users.Setup(r => r.GetRoleNamesByUserIdAsync())
            .ReturnsAsync(new Dictionary<string, string>());

        await _service.GetPageAsync(query);

        _users.Verify(r => r.GetRoleNamesByUserIdAsync(), Times.Once);
        _users.Verify(r => r.GetRolesAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    private static ApplicationUser GivenUser(string id, string name, string email, string? courseName = null)
    {
        var user = new ApplicationUser { Id = id, Name = name, Email = email, UserName = email };

        if (courseName is not null)
            user.CourseUsers.Add(new CourseUser
            {
                UserId = id,
                Course = new Course { Id = Guid.NewGuid(), Name = courseName }
            });

        return user;
    }
}
