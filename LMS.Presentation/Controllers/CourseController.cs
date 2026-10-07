using LMS.Shared.DTOs.CourseDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace LMS.Presentation.Controllers;

[Route("api/student/course")]
[ApiController]
[Authorize(Roles = "Teatcher, Student")]
internal class CourseController(IServiceManager serviceManager) : ControllerBase
{
    private readonly IServiceManager _serviceManager = serviceManager;

    [HttpGet]
    [Authorize(Roles = "Teatcher")]
    public async Task<ActionResult<IEnumerable<CourseDto>>> GetCourses()
    {
        var courses = await _serviceManager.CourseService.GetCourses();

        return Ok(courses);
    }



}
