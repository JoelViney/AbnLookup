using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AbnLookup
{
    public class LoggerTest
    {
        // This class just exists to test the logging below.
    }
    /// <summary>
    /// This is used to load-up the LoggerService.
    /// </summary>
    [TestClass]
    public static class Program
    {
        [AssemblyInitialize]
        public static void Configure(TestContext tc)
        {
            var logger = NLogFactory.GetLogger<LoggerTest>();
            logger.LogInformation("Application - Program.Configure was called.");
        }
    }
}
