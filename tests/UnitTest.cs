using System;
using Xunit;

namespace dotnet_demoapp_tests
{
    public class LameUnitTest1
    {
        [Fact]
        public void TestAThingFalse()
        {
            bool result = false;
            Assert.False(result, $"{result} should not be true"); 
        }
    }

    public class LameUnitTest2
    {
        [Fact]
        public void TestAThingTrue()
        {
            bool result = true;
            Assert.True(result, $"{result} should not be false"); 
        }        
    }
}
