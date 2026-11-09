using Microsoft.AspNetCore.Mvc;
using SchoolAdmission.Application.SeatHolds;
using SchoolAdmission.Application.SeatHolds.Dtos;

namespace SchoolAdmission.API.Controllers;

[ApiController]
[Route("me")]
public sealed class MeController : ControllerBase
{
    private readonly GetStudentSeatHoldsHandler _getStudentHoldsHandler;

    public MeController(GetStudentSeatHoldsHandler getStudentHoldsHandler)
    {
        _getStudentHoldsHandler = getStudentHoldsHandler;
    }

    [HttpGet("holds")]
    [ProducesResponseType(typeof(IReadOnlyList<StudentHoldItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetMyHolds(
        [FromHeader(Name = "X-Student-Id")] string? studentId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(studentId))
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Missing student identity",
                Detail = "The X-Student-Id header is required and cannot be empty."
            });
        }

        var holds = await _getStudentHoldsHandler.HandleAsync(studentId, cancellationToken);
        return Ok(holds);
    }
}

