using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using SchoolAdmission.API.Middleware;
using Xunit;

namespace SchoolAdmission.UnitTests;

public class ProblemDetailsTests
{
    [Theory]
    [InlineData(typeof(KeyNotFoundException), 404)]
    [InlineData(typeof(ArgumentException), 400)]
    [InlineData(typeof(InvalidOperationException), 409)]
    [InlineData(typeof(Exception), 500)]
    public async Task TryHandleAsync_MapsExceptionToCorrectStatusCode(Type exceptionType, int expectedStatusCode)
    {
        var handler = new ProblemDetailsExceptionHandler(NullLogger<ProblemDetailsExceptionHandler>.Instance);
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();

        var exception = (Exception)Activator.CreateInstance(exceptionType, "Test error message")!;
        var handled = await handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(expectedStatusCode, httpContext.Response.StatusCode);
        Assert.StartsWith("application/problem+json", httpContext.Response.ContentType);
    }
}
