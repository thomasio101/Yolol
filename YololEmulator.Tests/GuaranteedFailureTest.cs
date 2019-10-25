using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace YololEmulator.Tests
{
    [TestClass]
    public class GuaranteedFailureTest
    {
        [TestMethod]
        public void GuaranteedFailure() => Assert.IsTrue(false);
    }
}
