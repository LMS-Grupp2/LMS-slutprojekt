using LMS.Shared.Paging;

namespace LMS.Tests;

public class PagedResultTests
{
    [Theory]
    [InlineData(0, 20, 1)]    // empty list is still "page 1 of 1"
    [InlineData(1, 20, 1)]
    [InlineData(20, 20, 1)]   // exactly one full page
    [InlineData(21, 20, 2)]   // one item over → a second page
    [InlineData(134, 20, 7)]
    public void TotalPages_RoundsUp_AndIsAtLeastOne(int totalCount, int pageSize, int expected)
    {
        var result = new PagedResult<int>([], 1, pageSize, totalCount);

        Assert.Equal(expected, result.TotalPages);
    }

    [Fact]
    public void HasPrevious_AndHasNext_FollowThePageNumber()
    {
        var first = new PagedResult<int>([], 1, 10, 25);
        var middle = new PagedResult<int>([], 2, 10, 25);
        var last = new PagedResult<int>([], 3, 10, 25);

        Assert.False(first.HasPrevious);
        Assert.True(first.HasNext);
        Assert.True(middle.HasPrevious);
        Assert.True(middle.HasNext);
        Assert.True(last.HasPrevious);
        Assert.False(last.HasNext);
    }
}