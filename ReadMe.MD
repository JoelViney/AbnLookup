
# AbnLookup
ABN Lookup is the public view of the Australian Business Register (ABR). It provides access to publicly available information supplied by businesses when they register for an Australian Business Number (ABN).

### Notes
You will need to request your own ABN Authentication Guid to access the web service from abr.business.gov.au then search for TODO in the project and add the guid there.

The webservice is pretty flakey so there are are some guards to assure we get a result. Their API also seems to change a bit so if something breaks it's not my fault. IF you are going to use this I would suggest adding it to a continuous integration sever.

### Technologies used:
* .Net Standard 2.1
* .Net Core 2.1
* NLog
* Microsoft.Extensions.Logging
* MSTest

### References

Australian Business Register: https://abr.business.gov.au

WSDL: https://abr.business.gov.au/abrxmlsearch/AbrXmlSearch.asmx?WSDL

ABN Format: https://abr.business.gov.au/Help/AbnFormat

Responses: https://abr.business.gov.au/Documentation/WebServiceResponse
