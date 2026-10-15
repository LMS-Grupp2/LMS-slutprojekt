using System.Security.Claims;
using LMS.Shared.Constants;
using LMS.Shared.DTOs.DocumentDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using Swashbuckle.AspNetCore.Annotations;

namespace LMS.Presentation.Controllers;

/// <summary>
/// Document endpoints. The controller only handles HTTP, all rules live in DocumentService.
/// Teachers and students can list and download, only teachers can upload and delete.
/// </summary>
[Route("api")]
[ApiController]
[Authorize(Roles = UserRoles.Teacher + "," + UserRoles.Student)]
public class DocumentsController(IServiceManager serviceManager) : ControllerBase
{
    private readonly IServiceManager _serviceManager = serviceManager;

    /// <summary>GET api/courses/{courseId}/documents - every document in the course (course, modules, activities).
    /// 404 if the course does not exist or a student is not enrolled in it.</summary>
    [HttpGet("courses/{courseId:guid}/documents")]
    [SwaggerOperation(Summary = "List the documents of a course",
        Description = "Returns all documents in the course, newest first. Students only get their own course.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<DocumentDto>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<DocumentDto>>> GetCourseDocuments(Guid courseId)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        var documents = await _serviceManager.DocumentService
            .GetByCourseAsync(courseId, userId, IsTeacher());

        return Ok(documents);
    }

    /// <summary>GET api/modules/{moduleId}/documents - the documents attached to a module.</summary>
    [HttpGet("modules/{moduleId:guid}/documents")]
    [SwaggerOperation(Summary = "List the documents of a module",
        Description = "Returns the documents attached to the module, newest first.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<DocumentDto>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<DocumentDto>>> GetModuleDocuments(Guid moduleId)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        var documents = await _serviceManager.DocumentService
            .GetByModuleAsync(moduleId, userId, IsTeacher());

        return Ok(documents);
    }

    /// <summary>GET api/activities/{activityId}/documents - the documents attached to an activity.</summary>
    [HttpGet("activities/{activityId:guid}/documents")]
    [SwaggerOperation(Summary = "List the documents of an activity",
        Description = "Returns the documents attached to the activity, newest first.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<DocumentDto>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<DocumentDto>>> GetActivityDocuments(Guid activityId)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        var documents = await _serviceManager.DocumentService
            .GetByActivityAsync(activityId, userId, IsTeacher());

        return Ok(documents);
    }

    /// <summary>POST api/documents - uploads a file (multipart/form-data) and attaches it to exactly one
    /// of course, module or activity. 400 for a bad file or target, 404 if the target does not exist.</summary>
    [HttpPost("documents")]
    [Authorize(Roles = UserRoles.Teacher)]
    [Consumes("multipart/form-data")]
    [SwaggerOperation(Summary = "Upload a document",
        Description = "Uploads a file and attaches it to a course, a module or an activity (set exactly one).")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(DocumentDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DocumentDto>> UploadDocument(
        [FromForm] UploadDocumentRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        if (request.File is null)
            return BadRequest(new ProblemDetails { Title = "Bad Request", Detail = "No file was uploaded." });

        // The stream is disposed here, the service has finished copying it to disk by then
        await using var stream = request.File.OpenReadStream();

        var created = await _serviceManager.DocumentService.UploadAsync(
            stream,
            request.File.FileName,
            request.File.ContentType,
            request.File.Length,
            request.CourseId,
            request.ModuleId,
            request.ActivityId,
            userId,
            cancellationToken);

        // The download endpoint is the way to get the file back, so the Location header points there
        return Created($"api/documents/{created.Id}/download", created);
    }

    /// <summary>GET api/documents/{id}/download - the file itself. 404 if it does not exist or a student
    /// is not enrolled in the document's course.</summary>
    [HttpGet("documents/{id:guid}/download")]
    [SwaggerOperation(Summary = "Download a document",
        Description = "Returns the file as an attachment. Students only get documents from their own course.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadDocument(Guid id)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        var download = await _serviceManager.DocumentService.DownloadAsync(id, userId, IsTeacher());

        // Tell the browser not to guess a different type than the one we send
        Response.Headers["X-Content-Type-Options"] = "nosniff";

        // File() sends the stream and disposes it when the response is done.
        // Passing a file name makes it a download (Content-Disposition: attachment).
        return File(download.Content, download.ContentType, download.FileName);
    }

    /// <summary>DELETE api/documents/{id} - deletes a document and its file, or 404 if it does not exist.</summary>
    [HttpDelete("documents/{id:guid}")]
    [Authorize(Roles = UserRoles.Teacher)]
    [SwaggerOperation(Summary = "Delete a document", Description = "Deletes a document and its file by id.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteDocument(Guid id)
    {
        await _serviceManager.DocumentService.DeleteAsync(id);
        return NoContent();
    }


    /* Helpers */

    private bool IsTeacher() => User.IsInRole(UserRoles.Teacher);

    // The user id comes from the login token, never from the request
    private bool TryGetUserId(out string userId)
    {
        userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        return userId.Length > 0;
    }
}

/// <summary>
/// The form fields of the upload request. Lives here, not in LMS.Shared, because it uses
/// IFormFile from ASP.NET. Set exactly one of CourseId, ModuleId or ActivityId.
/// </summary>
public class UploadDocumentRequest
{
    public IFormFile? File { get; set; }
    public Guid? CourseId { get; set; }
    public Guid? ModuleId { get; set; }
    public Guid? ActivityId { get; set; }
}