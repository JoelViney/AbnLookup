using AbnLookup;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace AbnLookup
{
    /*
       Test ABNs from: https://abr.business.gov.au/Documentation/WebServiceResponse

        Suppressed ABN	34 241 177 887
        Replaced ABN	30 613 501 612
        Re-issued ABN	49 093 669 660

        Multiple addresses	33 531 321 789
        Multiple GST status	76 093 555 992
        Multiple ABN status	53 772 093 958
        Many name types	85 832 766 990
        Main DGR status	56 006 580 883
        DGR funds with historical names	78 345 431 247
        Tax concession information	48 212 321 102
        Superannuation fund	12 586 695 715
    */

    [TestClass]
    public class SearchByAbnTests
    {
        [TestMethod]
        public async Task SearchByAbnNoMatch()
        {
            // Arrange
            var logger = NLogFactory.GetLogger<AbnLookupConnector>();
            var connector = new AbnLookupConnector(logger);

            // Act
            var business = await connector.SearchByAbnAsync("00000000000");

            // Assert
            Assert.IsNull(business);
        }

        [TestMethod]
        public async Task SearchByAbnInvalidAbn()
        {
            // Arrang
            var logger = NLogFactory.GetLogger<AbnLookupConnector>();
            var connector = new AbnLookupConnector(logger);

            // Act
            var business = await connector.SearchByAbnAsync("this won't work");

            // Assert
            Assert.IsNull(business);
        }

        [TestMethod]
        public async Task SearchByAbnSuppressedAbn()
        {
            // Arrange
            var logger = NLogFactory.GetLogger<AbnLookupConnector>();
            var connector = new AbnLookupConnector(logger);

            // Act
            var business = await connector.SearchByAbnAsync("34 241 177 887");

            // Assert
            // This will return a business object without any values.
            Assert.AreEqual("34241177887", business.Abn);
            Assert.AreEqual(null, business.Name);
        }

        [TestMethod]
        public async Task SearchByAbnReplacedAbn()
        {
            // Arrange
            var logger = NLogFactory.GetLogger<AbnLookupConnector>();
            var connector = new AbnLookupConnector(logger);

            // Act
            var business = await connector.SearchByAbnAsync("77 104 439 054");

            // Assert
            // n.b. This will return the specified ABN not the replaced ABN
            Assert.AreEqual("KADECO AUSTRALASIA PTY LTD", business.Name);
        }

        [TestMethod]
        public async Task SearchByAbnReissuedAbn()
        {
            // Arrange
            var logger = NLogFactory.GetLogger<AbnLookupConnector>();
            var connector = new AbnLookupConnector(logger);

            // Act
            var business = await connector.SearchByAbnAsync("49 093 669 660");

            // Assert
            Assert.AreEqual("RECON SERVICES PTY LTD", business.Name);
        }

        [TestMethod]
        public async Task SearchByAbnMultipleAddresses()
        {
            // Arrange
            var logger = NLogFactory.GetLogger<AbnLookupConnector>();
            var connector = new AbnLookupConnector(logger);

            // Act
            var business = await connector.SearchByAbnAsync("33 531 321 789");

            // Assert
            Assert.AreEqual("6233", business.Postcode); // Assure it selects the first address.
            Assert.AreEqual("MANOLIS MAHLIS", business.Name);
        }

        [TestMethod]
        public async Task SearchByAbnMultipleGstStatuses()
        {
            // Arrange
            var logger = NLogFactory.GetLogger<AbnLookupConnector>();
            var connector = new AbnLookupConnector(logger);

            // Act
            var business = await connector.SearchByAbnAsync("76 093 555 992");

            // Assert
            Assert.AreEqual("TOTAL QUALITY MILK PTY. LTD.", business.Name);
        }

        [TestMethod]
        public async Task SearchByAbnMultipleAbnStatuses()
        {
            // Arrange
            var logger = NLogFactory.GetLogger<AbnLookupConnector>();
            var connector = new AbnLookupConnector(logger);

            // Act
            var business = await connector.SearchByAbnAsync("53 772 093 958");

            // Assert
            // n.b. This abn returns an individual instead of an organisation
            Assert.AreEqual("ANTONINA RIVITUSO", business.Name);
        }

        [TestMethod]
        public async Task SearchByAbnManyNameTypes()
        {
            // Arrange
            var logger = NLogFactory.GetLogger<AbnLookupConnector>();
            var connector = new AbnLookupConnector(logger);

            // Act
            var business = await connector.SearchByAbnAsync("85 832 766 990");

            // Assert
            // n.b. this is defaulting to the first business name
            Assert.AreEqual("UNITING CARE-SHOALHAVEN AGEING AND DISABILITY SERVICE", business.Name);
        }

        [TestMethod]
        public async Task SearchByAbnMainDgrStatus()
        {
            // Arrange
            var logger = NLogFactory.GetLogger<AbnLookupConnector>();
            var connector = new AbnLookupConnector(logger);

            // Act
            var business = await connector.SearchByAbnAsync("56 006 580 883");

            // Assert
            Assert.AreEqual("THE BIONICS INSTITUTE OF AUSTRALIA", business.Name);
        }

        [TestMethod]
        public async Task SearchByAbnDgrFundsWithHistoricalNames()
        {
            // Arrange
            var logger = NLogFactory.GetLogger<AbnLookupConnector>();
            var connector = new AbnLookupConnector(logger);

            // Act
            var business = await connector.SearchByAbnAsync("78 345 431 247");

            // Assert
            // n.b. this is defaulting to the first business name
            Assert.AreEqual("JEWISH CARE (VICTORIA) INC", business.Name);
        }

        [TestMethod]
        public async Task SearchByAbnTaxConcessionInformation()
        {
            // Arrange
            var logger = NLogFactory.GetLogger<AbnLookupConnector>();
            var connector = new AbnLookupConnector(logger);

            // Act
            var business = await connector.SearchByAbnAsync("48 212 321 102");

            // Assert
            Assert.AreEqual("TASMANIAN ABORIGINAL CORPORATION", business.Name);
        }

        [TestMethod]
        public async Task SearchByAbnSuperannuationFund()
        {
            // Arrange
            var logger = NLogFactory.GetLogger<AbnLookupConnector>();
            var connector = new AbnLookupConnector(logger);

            // Act
            var business = await connector.SearchByAbnAsync("12 586 695 715");

            // Assert
            Assert.AreEqual("The trustee for ramsays superfund", business.Name);
        }
    }
}
