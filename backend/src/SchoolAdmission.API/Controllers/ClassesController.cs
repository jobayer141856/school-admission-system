using Microsoft.AspNetCore.Mvc;
using SchoolAdmission.Application.AdmissionClasses;
using SchoolAdmission.Application.AdmissionClasses.Dtos;

namespace SchoolAdmission.API.Controllers;

[ApiController]
[Route("classes")]
public sealed class ClassesController : ControllerBase
{
    private readonly CreateAdmissionClassHandler _createHandler;
    private readonly GetAdmissionClassesHandler _getAllHandler;
    private readonly GetAdmissionClassByIdHandler _getByIdHandler;

    public ClassesController(
        CreateAdmissionClassHandler createHandler,
        GetAdmissionClassesHandler getAllHandler,
        GetAdmissionClassByIdHandler getByIdHandler)
    {
        _createHandler = createHandler;
        _getAllHandler = getAllHandler;
        _getByIdHandler = getByIdHandler;
    }

    [HttpPost]
    [ProducesResponseType(typeof(AdmissionClassResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateClass(
        [FromBody] CreateAdmissionClassRequest request,
        CancellationToken cancellationToken)
    {
        if (request == null)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid request",
                Detail = "Request body cannot be null."
            });
        }

        var response = await _createHandler.HandleAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetClassById), new { classId = response.Id }, response);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AdmissionClassDetailResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetClasses(CancellationToken cancellationToken)
    {
        var classes = await _getAllHandler.HandleAsync(cancellationToken);
        return Ok(classes);
    }

    [HttpGet("{classId}")]
    [ProducesResponseType(typeof(AdmissionClassDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetClassById(
        [FromRoute] string classId,
        CancellationToken cancellationToken)
    {
        var admissionClass = await _getByIdHandler.HandleAsync(classId, cancellationToken);
        if (admissionClass == null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Admission class not found",
                Detail = $"No admission class was found with ID '{classId}'."
            });
        }

        return Ok(admissionClass);
    }
}

