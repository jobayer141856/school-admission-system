using SchoolAdmission.Domain.Exceptions;
using SchoolAdmission.Domain.SeatHolds;
using Xunit;

namespace SchoolAdmission.UnitTests;

public class StudentIdTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ThrowsDomainException_WhenEmptyOrWhitespace(string invalidValue)
    {
        var ex = Assert.Throws<DomainException>(() => new StudentId(invalidValue));
        Assert.Equal("Student ID cannot be empty.", ex.Message);
    }

    [Fact]
    public void Constructor_ThrowsDomainException_WhenExceedsMaxLength()
    {
        var longId = new string('1', StudentId.MaxLength + 1);
        var ex = Assert.Throws<DomainException>(() => new StudentId(longId));
        Assert.Contains("cannot exceed", ex.Message);
    }

    [Fact]
    public void Constructor_TrimsWhitespaceAndAllowsImplicitConversion()
    {
        var studentId = new StudentId("  20152691234567890  ");
        Assert.Equal("20152691234567890", studentId.Value);
        string stringVal = studentId;
        Assert.Equal("20152691234567890", stringVal);
    }
}

