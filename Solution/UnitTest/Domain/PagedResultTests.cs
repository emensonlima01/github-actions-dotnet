using Domain.Models;

namespace UnitTest.Domain;

public class PagedResultTests
{
    [Fact]
    public void TotalPages_CalculatesCeilingDivision()
    {
        var result = new PagedResult<int>
        {
            PageSize = 10,
            TotalCount = 25
        };

        Assert.Equal(3, result.TotalPages);
    }

    [Fact]
    public void HasPreviousAndHasNext_AreBasedOnPageNumber()
    {
        var result = new PagedResult<int>
        {
            PageNumber = 2,
            PageSize = 10,
            TotalCount = 25
        };

        Assert.True(result.HasPreviousPage);
        Assert.True(result.HasNextPage);
    }
}
