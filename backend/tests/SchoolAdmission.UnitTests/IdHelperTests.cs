using SchoolAdmission.Application.Common;
using Xunit;

namespace SchoolAdmission.UnitTests;

public class IdHelperTests
{
    [Fact]
    public void ToClassId_PrefixesWithC_AndHex()
    {
        var guid = Guid.NewGuid();
        var classId = IdHelper.ToClassId(guid);

        Assert.StartsWith("c_", classId);
        Assert.Equal($"c_{guid:N}", classId);
    }

    [Fact]
    public void ToHoldId_PrefixesWithH_AndHex()
    {
        var guid = Guid.NewGuid();
        var holdId = IdHelper.ToHoldId(guid);

        Assert.StartsWith("h_", holdId);
        Assert.Equal($"h_{guid:N}", holdId);
    }

    [Fact]
    public void TryParseClassId_ParsesPrefixedAndBareGuids()
    {
        var guid = Guid.NewGuid();

        Assert.True(IdHelper.TryParseClassId($"c_{guid:N}", out var parsed1));
        Assert.Equal(guid, parsed1);

        Assert.True(IdHelper.TryParseClassId($"C_{guid:D}", out var parsed2));
        Assert.Equal(guid, parsed2);

        Assert.True(IdHelper.TryParseClassId(guid.ToString(), out var parsed3));
        Assert.Equal(guid, parsed3);

        Assert.False(IdHelper.TryParseClassId("invalid-id", out _));
        Assert.False(IdHelper.TryParseClassId("", out _));
    }

    [Fact]
    public void TryParseHoldId_ParsesPrefixedAndBareGuids()
    {
        var guid = Guid.NewGuid();

        Assert.True(IdHelper.TryParseHoldId($"h_{guid:N}", out var parsed1));
        Assert.Equal(guid, parsed1);

        Assert.True(IdHelper.TryParseHoldId($"H_{guid:D}", out var parsed2));
        Assert.Equal(guid, parsed2);

        Assert.True(IdHelper.TryParseHoldId(guid.ToString(), out var parsed3));
        Assert.Equal(guid, parsed3);

        Assert.False(IdHelper.TryParseHoldId("invalid-hold-id", out _));
        Assert.False(IdHelper.TryParseHoldId("", out _));
    }
}

