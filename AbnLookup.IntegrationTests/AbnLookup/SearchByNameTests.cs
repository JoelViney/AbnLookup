using AbnLookup;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;

namespace AbnLookup
{
    [TestClass]
    public class SearchByNameTests
    { 
        // 28th Aug 2018: This test has been a bit flakey and returned 'There was a problem completing your request.'
        [TestMethod]
        public async Task SearchByNameNoMatch()
        {
            // Arrange
            var logger = NLogFactory.GetLogger<AbnLookupConnector>();
            var connector = new AbnLookupConnector(logger);

            // Act
            var list = await connector.SearchByNameAsync("This shouldnt match any records.");

            // Assert
            Assert.IsTrue(list.Count == 0);
        }

        [TestMethod]
        public async Task SearchByNameAllNumbersNoMatch()
        {
            // Arrange
            var logger = NLogFactory.GetLogger<AbnLookupConnector>();
            var connector = new AbnLookupConnector(logger);

            // Act
            var list = await connector.SearchByNameAsync("0000000000");

            // Assert
            Assert.IsTrue(list.Count == 0);
        }

        [TestMethod]
        public async Task SearchByNameReplacedAbn()
        {
            // Arrange
            var logger = NLogFactory.GetLogger<AbnLookupConnector>();
            var connector = new AbnLookupConnector(logger);

            // Act
            var list = await connector.SearchByNameAsync("KADECO AUSTRALASIA PTY LTD");
            
            // Assert
            Assert.IsTrue(list.Count == 2);
            // This will sound crazy. But sometimes the list changes its sort order 
            // as the results both have a score of 100.
            Assert.IsTrue(list.Any(x => x.Abn == "77104439054"));
            Assert.IsTrue(list.Any(x => x.Abn == "30613501612"));
        }

        [TestMethod]
        public async Task SearchByNameReissuedAbn()
        {
            // Arrange
            var logger = NLogFactory.GetLogger<AbnLookupConnector>();
            var connector = new AbnLookupConnector(logger);

            // Act
            var list = await connector.SearchByNameAsync("RECON SERVICES PTY LTD");

            // Assert
            Assert.IsTrue(list.Count > 0);
            Assert.AreEqual("49093669660", list[0].Abn);
        }

        [TestMethod]
        public async Task SearchByNameMultipleAddresses()
        {
            // Arrange
            var logger = NLogFactory.GetLogger<AbnLookupConnector>();
            var connector = new AbnLookupConnector(logger);

            // Act
            var list = await connector.SearchByNameAsync("MANOLIS MAHLIS");

            // Assert
            Assert.IsTrue(list.Count > 0);
            Assert.AreEqual("33531321789", list[0].Abn);
        }

        [TestMethod]
        public async Task SearchByNameMultipleGstStatuses()
        {
            // Arrange
            var logger = NLogFactory.GetLogger<AbnLookupConnector>();
            var connector = new AbnLookupConnector(logger);

            // Act
            var list = await connector.SearchByNameAsync("TOTAL QUALITY MILK PTY. LTD.");

            // Assert
            Assert.IsTrue(list.Count > 0);
            Assert.AreEqual("76093555992", list[0].Abn);
        }

        [TestMethod]
        public async Task SearchByNameMultipleAbnStatuses()
        {
            // Arrange
            var logger = NLogFactory.GetLogger<AbnLookupConnector>();
            var connector = new AbnLookupConnector(logger);

            // Act
            var list = await connector.SearchByNameAsync("ANTONINA RIVITUSO");

            // Assert
            // n.b. This abn returns an individual instead of an organisation
            Assert.IsTrue(list.Count > 0);
            Assert.AreEqual("53772093958", list[0].Abn);
        }

        [TestMethod]
        public async Task SearchByNameManyNameTypes()
        {
            // Arrange
            var logger = NLogFactory.GetLogger<AbnLookupConnector>();
            var connector = new AbnLookupConnector(logger);

            // Act
            var list = await connector.SearchByNameAsync("UNITING CARE-SHOALHAVEN AGEING AND DISABILITY SERVICE");

            // Assert
            // n.b. this is defaulting to the first business name
            Assert.IsTrue(list.Count > 0);
            Assert.AreEqual("85832766990", list[0].Abn);
        }

        [TestMethod]
        public async Task SearchByNameMainDgrStatus()
        {
            // Arrange
            var logger = NLogFactory.GetLogger<AbnLookupConnector>();
            var connector = new AbnLookupConnector(logger);

            // Act
            var list = await connector.SearchByNameAsync("THE BIONICS INSTITUTE OF AUSTRALIA");

            // Assert
            Assert.IsTrue(list.Count > 0);
            Assert.AreEqual("56006580883", list[0].Abn);
        }

        [TestMethod]
        public async Task SearchByNameDgrFundsWithHistoricalNames()
        {
            // Arrange
            var logger = NLogFactory.GetLogger<AbnLookupConnector>();
            var connector = new AbnLookupConnector(logger);

            // Act
            var list = await connector.SearchByNameAsync("JEWISH CARE (VICTORIA) INC");

            // Assert
            // n.b. this is defaulting to the first business name
            Assert.IsTrue(list.Count > 0);
            Assert.AreEqual("78345431247", list[0].Abn);
        }

        [TestMethod]
        public async Task SearchByNameTaxConcessionInformation()
        {
            // Arrange
            var logger = NLogFactory.GetLogger<AbnLookupConnector>();
            var connector = new AbnLookupConnector(logger);

            // Act
            var list = await connector.SearchByNameAsync("TASMANIAN ABORIGINAL CORPORATION");

            // Assert
            Assert.IsTrue(list.Count > 0);
            Assert.AreEqual("48212321102", list[0].Abn);
        }

        [TestMethod]
        public async Task SearchByNameSuperannuationFund()
        {
            // Arrange
            var logger = NLogFactory.GetLogger<AbnLookupConnector>();
            var connector = new AbnLookupConnector(logger);

            // Act
            var list = await connector.SearchByNameAsync("The Trustee for Ramsays Superfund");

            // Assert
            // n.b. This returns the latest version of the Ramsays Superfund as the first result.
            // While the test case in the SearchByAbn uses the lower case business which is 'The trustee for ramsays superfund'
            Assert.IsTrue(list.Count > 0);
            Assert.AreEqual("24127614077", list[0].Abn);
        }
    }
}
