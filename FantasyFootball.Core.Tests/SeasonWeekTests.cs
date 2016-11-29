using System;
using Xunit;

namespace FantasyFootball.Core.Tests
{
    public class SeasonWeekTests
    {
        [Theory]
        [InlineData("September 6, 2016", "August 15, 2016", 1)]
        [InlineData("September 6, 2016", "September 8, 2016", 1)]
        [InlineData("September 6, 2016", "November 28, 2016 11:59 PM", 12)]
        [InlineData("September 6, 2016", "November 29, 2016 12:01 AM", 13)]
        [InlineData("September 6, 2016", "February 5, 2017", 17)]
        public void ComputeCurrentWeek(string start, string now, int expectedWeek)
        {
            var startDate = DateTime.Parse(start);
            var nowDate = DateTime.Parse(now);

            var computedWeek = SeasonWeek.ComputeCurrentWeek(startDate, nowDate);

            Assert.Equal(expectedWeek, computedWeek);
        }
    }
}
