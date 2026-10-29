using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using LMS.Shared.Constants;
using Microsoft.IdentityModel.Tokens;

namespace LMS.Tests.Integration;

/// <summary>
/// US16, criteria 1 and 2: the API itself says no, whatever the UI shows. These requests go through
/// the real pipeline (JWT bearer authentication, then [Authorize]), which a unit test never runs.
/// No token is 401, the wrong role is 403, a valid token is accepted.
/// </summary>
public sealed class ApiAuthorizationTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Theory]
    [InlineData("api/courses")]
    [InlineData("api/users")]
    [InlineData("api/student/dashboard")]
    [InlineData("api/notifications")]
    public async Task WithoutToken_Is401(string url)
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync(url);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task TokenSignedWithAnotherKey_Is401()
    {
        // A forged token: right claims, wrong signature. The API must not trust the role inside it.
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", CreateToken(UserRoles.Teacher, "another-secret-key-that-is-also-32-bytes-long!"));

        var response = await client.GetAsync("api/courses");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("api/courses")]           // CourseController: teacher only
    [InlineData("api/users")]             // UsersController: teacher only
    [InlineData("api/teacher/dashboard")] // TeacherDashboardController: teacher only
    public async Task Student_OnTeacherEndpoint_Is403(string url)
    {
        using var client = ClientAs(UserRoles.Student);

        var response = await client.GetAsync(url);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Theory]
    [InlineData("api/student/course")]      // StudentCourseController: student only
    [InlineData("api/student/dashboard")]   // StudentDashboardController: student only
    [InlineData("api/student/assignments")] // AssignmentsController: student only
    public async Task Teacher_OnStudentEndpoint_Is403(string url)
    {
        using var client = ClientAs(UserRoles.Teacher);

        var response = await client.GetAsync(url);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ValidToken_IsAccepted()
    {
        // api/auth/me only reads the claims, so it works without a database
        using var client = ClientAs(UserRoles.Student);

        var response = await client.GetAsync("api/auth/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    /* Helpers */

    private HttpClient ClientAs(string role)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateToken(role));
        return client;
    }

    // Same claim types and signing as AuthService, with the settings from ApiFactory
    private static string CreateToken(string role, string secretKey = ApiFactory.SecretKey)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

        var token = new JwtSecurityToken(
            issuer: ApiFactory.Issuer,
            audience: ApiFactory.Audience,
            claims:
            [
                new Claim(ClaimTypes.NameIdentifier, "user-1"),
                new Claim(ClaimTypes.Name, "user@lms.com"),
                new Claim(ClaimTypes.Role, role)
            ],
            expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}