using Microsoft.AspNetCore.Mvc;
using SchoolAdmission.Application.SeatHolds;
using SchoolAdmission.Application.SeatHolds.Dtos;

namespace SchoolAdmission.API.Controllers;

[ApiController]
public sealed class HoldsController : ControllerBase
{
    private readonly CreateSeatHoldHandler _createHandler;
    private readonly GetSeatHoldHandler _getHandler;
    private readonly ConfirmSeatHoldHandler _confirmHandler;
    private readonly CancelSeatHoldHandler _cancelHandler;

    public HoldsController(
        CreateSeatHoldHandler createHandler,
        GetSeatHoldHandler getHandler,
        ConfirmSeatHoldHandler confirmHandler,
        CancelSeatHoldHandler cancelHandler)
    {
        _createHandler = createHandler;
        _getHandler = getHandler;
        _confirmHandler = confirmHandler;
        _cancelHandler = cancelHandler;
    }

    [HttpPost("classes/{classId}/holds")]
    [ProducesResponseType(typeof(SeatHoldResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(SeatHoldResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateHold(
        [FromRoute] string classId,
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

        var result = await _createHandler.HandleAsync(classId, studentId, cancellationToken);

        return result.Outcome switch
        {
            CreateHoldOutcome.Created => CreatedAtAction(
                nameof(GetHold),
                new { holdId = result.Hold!.HoldId },
                result.Hold),

            CreateHoldOutcome.ExistingActiveHoldReturned => Ok(result.Hold),

            CreateHoldOutcome.ClassNotFound => NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Admission class not found",
                Detail = result.Message ?? "Admission class not found."
            }),

            CreateHoldOutcome.ClassFull => Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Seat is no longer available",
                Detail = result.Message ?? "No available seats remain for this admission class."
            }),

            CreateHoldOutcome.HasActiveHoldInAnotherClass => Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Active application exists",
                Detail = result.Message ?? "Student already has an active application in another class."
            }),

            _ => StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Unexpected error",
                Detail = "Failed to create hold."
            })
        };
    }

    [HttpGet("holds/{holdId}")]
    [ProducesResponseType(typeof(SeatHoldDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetHold(
        [FromRoute] string holdId,
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

        var hold = await _getHandler.HandleAsync(holdId, studentId, cancellationToken);
        if (hold == null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Hold not found",
                Detail = $"No hold was found with ID '{holdId}' for this student."
            });
        }

        return Ok(hold);
    }

    [HttpPost("holds/{holdId}/confirm")]
    [ProducesResponseType(typeof(ConfirmHoldResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ConfirmHold(
        [FromRoute] string holdId,
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

        var result = await _confirmHandler.HandleAsync(holdId, studentId, cancellationToken);

        return result.Outcome switch
        {
            ConfirmHoldOutcome.Confirmed or ConfirmHoldOutcome.AlreadyConfirmed => Ok(result.Response),

            ConfirmHoldOutcome.NotFound => NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Hold not found",
                Detail = result.Message ?? "Hold not found for this student."
            }),

            ConfirmHoldOutcome.Expired => Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Hold expired",
                Detail = result.Message ?? "Hold has expired and cannot be confirmed."
            }),

            ConfirmHoldOutcome.Conflict => Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Conflict",
                Detail = result.Message ?? "Hold cannot be confirmed in its current status."
            }),

            _ => StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Unexpected error",
                Detail = "Failed to confirm hold."
            })
        };
    }

    [HttpDelete("holds/{holdId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CancelHold(
        [FromRoute] string holdId,
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

        var result = await _cancelHandler.HandleAsync(holdId, studentId, cancellationToken);

        return result.Outcome switch
        {
            CancelHoldOutcome.Cancelled or CancelHoldOutcome.AlreadyCancelled => NoContent(),

            CancelHoldOutcome.NotFound => NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Hold not found",
                Detail = result.Message ?? "Hold not found for this student."
            }),

            CancelHoldOutcome.AlreadyConfirmed => Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Cannot cancel confirmed admission",
                Detail = result.Message ?? "Confirmed admission cannot be cancelled."
            }),

            CancelHoldOutcome.Expired => Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Hold expired",
                Detail = result.Message ?? "Expired hold cannot be cancelled."
            }),

            CancelHoldOutcome.Conflict => Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Conflict",
                Detail = result.Message ?? "Hold cannot be cancelled."
            }),

            _ => StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Unexpected error",
                Detail = "Failed to cancel hold."
            })
        };
    }
}

