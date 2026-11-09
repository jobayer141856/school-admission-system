using SchoolAdmission.Domain.AdmissionClasses;
using SchoolAdmission.Domain.Exceptions;
using Xunit;

namespace SchoolAdmission.UnitTests;

public class AdmissionClassTests
{
    private readonly DateTimeOffset _now = DateTimeOffset.UtcNow;

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Create_ThrowsDomainException_WhenNameIsEmptyOrWhitespace(string invalidName)
    {
        var ex = Assert.Throws<DomainException>(() =>
            AdmissionClass.Create(invalidName, 50, _now));

        Assert.Equal("Admission class name is required.", ex.Message);
    }

    [Fact]
    public void Create_ThrowsDomainException_WhenNameExceedsMaxLength()
    {
        var longName = new string('A', AdmissionClass.MaxNameLength + 1);

        var ex = Assert.Throws<DomainException>(() =>
            AdmissionClass.Create(longName, 50, _now));

        Assert.Contains("cannot exceed", ex.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Create_ThrowsDomainException_WhenTotalSeatsIsNotPositive(int invalidSeats)
    {
        var ex = Assert.Throws<DomainException>(() =>
            AdmissionClass.Create("Class 6 - Morning", invalidSeats, _now));

        Assert.Equal("Total seats must be a positive integer.", ex.Message);
    }

    [Fact]
    public void Create_Succeeds_WhenValidParametersProvided()
    {
        var admissionClass = AdmissionClass.Create("  Class 6 - Morning  ", 60, _now);

        Assert.NotEqual(Guid.Empty, admissionClass.Id);
        Assert.Equal("Class 6 - Morning", admissionClass.Name);
        Assert.Equal(60, admissionClass.TotalSeats);
        Assert.Equal(_now, admissionClass.CreatedAtUtc);
    }
}

