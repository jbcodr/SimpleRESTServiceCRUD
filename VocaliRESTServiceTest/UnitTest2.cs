using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VocaliRESTServiceTest
{
    [TestClass]
    public class UnitTest2
    {
        [TestMethod]
        public void TestMethodMidnightProcess()
        {
            VocaliRESTService.MidnightProcess midnight = new VocaliRESTService.MidnightProcess();
        }
    }
}
