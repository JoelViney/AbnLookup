using System;
using System.Threading.Tasks;
using AbrXmlSearch;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace AbnLookup
{
    /// <summary>
    /// ABN Lookup is the public view of the Australian Business Register (ABR). 
    /// It provides access to publicly available information supplied by 
    /// businesses when they register for an Australian Business Number (ABN).
    /// 
    /// This class provides the primary interface for those services.
    /// </summary>
    public class AbnLookupConnector
    {
        private readonly ILogger<AbnLookupConnector> _logger;

        private const string AuthenticationGuid = "TODO"; // Add your authentication guid here.

        private const int RetryAttempts = 10;
        private const int SleepBetweenCallsMilliseconds = 250; // This is multipled by the number of attempts.

        private const int SearchMinScore = 96;
        private const int SearchResults = 5;

        #region Constructors...

        public AbnLookupConnector(ILogger<AbnLookupConnector> logger)
        {
            this._logger = logger;
        }

        #endregion

        /// <summary>
        /// Performs a search against the ABNLookup webservice with the provided abn string.
        /// Returns the matching ABN or null if there is no match.
        /// </summary>
        public async Task<Business> SearchByAbnAsync(string abn)
        {
            _logger.LogDebug("AbnLookup.SearchByAbn({0}) called.", abn);
            var client = new ABRXMLSearchSoapClient(ABRXMLSearchSoapClient.EndpointConfiguration.ABRXMLSearchSoap12);
            var request = new SearchByABNv201408Request(abn, "Y", AuthenticationGuid);

            var attempts = 0;
            bool success = false;
            SearchByABNv201408Response response = null;
            do
            {
                response = await client.SearchByABNv201408Async(request);
                success = true;

                if (response.ABRPayloadSearchResults.response.Item is ResponseException socketException)
                {
                    // The AbnLookup seems to occasionally fail due to high load.
                    if (socketException.exceptionDescription.StartsWith("Error starting search : ssa.SSASocketException: unable to connect to host localhost port "))
                    {
                        attempts++;
                        Debug.WriteLine($"Failed to connect attempt #{attempts}");
                        if (attempts > RetryAttempts)
                        {
                            throw new AbnLookupException("The ABNLookup site returned a busy singnal.");
                        }
                        success = false;

                        _logger.LogWarning("Failed to connect attempt #{0}, trying again...", attempts);
                        Thread.Sleep(SleepBetweenCallsMilliseconds * attempts);
                    }
                }
            } while (success == false && attempts < RetryAttempts);

            if (response.ABRPayloadSearchResults.response.Item is ResponseException exception)
            {
                if (exception.exceptionDescription == "Search text is not a valid ABN or ACN")
                {
                    return null;
                }
                throw new AbnLookupException(exception.exceptionDescription);
            }

            var result = (ResponseBusinessEntity201408)response.ABRPayloadSearchResults.response.Item;

            // Suppressed ABNs won't return an item.
            string businessName = null;
            if (result.Items != null && result.Items.Length > 0)
            {
                if (result.Items[0] is IndividualName individualName)
                {
                    businessName = $"{individualName.givenName} {individualName.familyName}";
                }
                if (result.Items[0] is OrganisationName organisationName)
                {
                    businessName = organisationName.organisationName;
                }
            }
            var business = new Business()
            {
                Abn = result.ABN[0].identifierValue,
                Name = businessName,
                State = result.mainBusinessPhysicalAddress?[0]?.stateCode,
                Postcode = result.mainBusinessPhysicalAddress?[0]?.postcode,
                Score = 100
            };

            return business;
        }

        /// <summary>
        /// The Search by Name methods return a list of businesses that match the search criteria.
        /// </summary>
        public async Task<List<Business>> SearchByNameAsync(string name)
        {
            _logger.LogDebug("AbnLookup.SearchByNameAsync({0}) called.", name);
            var client = new ABRXMLSearchSoapClient(ABRXMLSearchSoapClient.EndpointConfiguration.ABRXMLSearchSoap12);

            var filters = new ExternalRequestFilters2012()
            {
                nameType = new ExternalRequestFilterNameType2012()
                {
                    legalName = "Y",
                    businessName = "N",
                    tradingName = "N",
                }
            };

            var search = new ExternalRequestNameSearchAdvanced2012()
            {
                name = name,
                filters = filters,
                maxSearchResults = SearchResults.ToString(),
                minimumScore = SearchMinScore
            };

            var request = new ABRSearchByNameAdvanced2012Request(search, AuthenticationGuid);

            var attempts = 0;
            bool success = false;
            ABRSearchByNameAdvanced2012Response response = null;
            do
            {
                response = await client.ABRSearchByNameAdvanced2012Async(request);
                success = true;

                if (response.ABRPayloadSearchResults.response.Item is ResponseException socketException)
                {
                    if (socketException.exceptionDescription.StartsWith("Error starting search : ssa.SSASocketException: unable to connect to host localhost port "))
                    {
                        // The AbnLookup seems to occasionally fail due to high load.
                        attempts++;
                        Debug.WriteLine($"Failed to connect attempt #{attempts}");
                        if (attempts > RetryAttempts)
                        {
                            throw new AbnLookupException("The ABNLookup site returned a busy singnal.");
                        }
                        success = false;
                        _logger.LogWarning("Failed to connect attempt #{0}, trying again...", attempts);
                        Thread.Sleep(SleepBetweenCallsMilliseconds * attempts);
                    }
                }
            } while (success == false && attempts < RetryAttempts);

            // Handle exceptions returned by the API
            if (response.ABRPayloadSearchResults.response.Item is ResponseException exception)
            {
                if (exception.exceptionDescription == "No records found")
                {
                    // 30th-Aug-2018: The ATO changed their API and seem to be returning this error on some situations.
                    _logger.LogTrace("'No records found' returned by the AbnLookup.");
                    return new List<Business>();
                }
                if (exception.exceptionDescription == "Search text is invalid for name search")
                {
                    _logger.LogTrace("'No records found' returned by the AbnLookup.");
                    return new List<Business>();
                }
                throw new AbnLookupException(exception.exceptionDescription);
            }

            var result = (ResponseSearchResultsList)response.ABRPayloadSearchResults.response.Item;
            var records = result.searchResultsRecord;


            // 30th Aug: The ATO changed their API so now that if there are no matching records the object may be null.
            if (records == null)
            {
                return new List<Business>();
            }

            var list = new List<Business>();
            foreach (var record in records)
                {
                    // Suppressed ABNs won't return an item.
                    string businessName = null;
                    int businessScore = 0;
                    if (record.Items != null && record.Items.Length > 0)
                    {
                        if (record.Items[0] is IndividualSimpleName individualName)
                        {
                            businessName = individualName.fullName;
                            businessScore = individualName.score;
                        }
                        if (record.Items[0] is OrganisationSimpleName organisationName)
                        {
                            businessName = organisationName.organisationName;
                            businessScore = organisationName.score;
                        }
                    }

                    var business = new Business()
                    {
                        Abn = record.ABN[0].identifierValue,
                        Name = businessName,
                        State = record.mainBusinessPhysicalAddress?[0]?.stateCode,
                        Postcode = record.mainBusinessPhysicalAddress?[0]?.postcode,
                        Score = businessScore,
                    };
                    list.Add(business);
                }

            return list;
        }
    }
}
