using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.ServiceModel.Security;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using GatewayService.DTOs;
using GatewayService.Helpers;
using GatewayService.Helpers.Nira;
using GatewayService.Repositories.Contracts;
using GatewayService.Services.Nira.Xml;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NiraWebService;
using static GatewayService.Services.Nira.NiraRequest;

namespace GatewayService.Services.Nira
{
    public class NiraCoreService : INiraCoreService
    {
        private readonly string _serviceUrl;
        private readonly ILogger<INiraCoreService> _logger;
        private readonly INiraXmlService _niraXmlService;
        private readonly IConfiguration _configuration;

        public NiraCoreService(INiraXmlService niraXmlService, IConfiguration configuration, ILogger<INiraCoreService> logger)
        {
            var settings = configuration.GetNiraSettings();
            _serviceUrl = settings.Url;
            _logger = logger;
            _niraXmlService = niraXmlService;
            _configuration = configuration;
        }

        /// <summary>
        /// Verify person information
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<PersonInfoVerificationResponse> VerifyPersonInformation(
            string username, string password,
            verifyPersonInformationRequest request)
        {
            try
            {
                _logger.LogInformation("Preparing XML request");
                var xml = _niraXmlService.PrepareXmlRequest(username, password, request);

                var xmlBytes = StringExtensions.GetAsciiBytes(xml);

                var xmlBytesLength = xmlBytes.Length;

                var req = PrepareWebRequest(_serviceUrl, xmlBytesLength);

                foreach (string key in req.Headers.AllKeys)
                {
                    _logger.LogDebug(string.Concat(key, " = ", req.Headers[key]));
                }

                var reqStream = await req.GetRequestStreamAsync();
                _logger.LogInformation("Got a response from nira for the request with national Id: {NationalId}", request.nationalId);

                reqStream.Write(xmlBytes, 0, xmlBytesLength);
                reqStream.Close();

                var res = (HttpWebResponse)req.GetResponse();
                if (res.StatusCode == HttpStatusCode.OK)
                {
                    var resStream = res.GetResponseStream();
                    if (resStream != null)
                    {
                        var resStr = new StreamReader(resStream).ReadToEnd();

                        _logger.LogTrace(resStr);

                        _logger.LogDebug(resStr);

                        return _niraXmlService.PrepareVerifyPersonInfoResponse(resStr);
                    }
                }

                return new PersonInfoVerificationResponse
                {
                    Status = "Error",
                    Error = new ResponseError
                    {
                        Code = res.StatusCode.ToString()
                    }
                };

            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to verify person information for request with national Id: {NationalId}", request.nationalId);

                throw;
            }
        }


        public async Task<ChangePasswordResponse> ChangePasswordAsync(string username, string password, changePasswordRequest request)
        {
            try
            {
                var xml = _niraXmlService.PrepareXmlRequest(username, password, request);

                _logger.LogTrace(xml);

                _logger.LogDebug(xml);

                var xmlBytes = StringExtensions.GetAsciiBytes(xml);

                var xmlBytesLength = xmlBytes.Length;

                var req = PrepareWebRequest(_serviceUrl, xmlBytesLength);

                var reqStream = await req.GetRequestStreamAsync();

                _logger.LogInformation("Got a response from nira for the new password request for {NiraUsername}", username);

                reqStream.Write(xmlBytes, 0, xmlBytesLength);
                reqStream.Close();

                var res = (HttpWebResponse)req.GetResponse();

                if (res.StatusCode == HttpStatusCode.OK)
                {
                    var resStream = res.GetResponseStream();
                    if (resStream != null)
                    {
                        var resStr = new StreamReader(resStream).ReadToEnd();

                        _logger.LogTrace(resStr);

                        _logger.LogDebug(resStr);

                        return _niraXmlService.PrepareChangePasswordResponse(resStr);
                    }
                }

                return new ChangePasswordResponse
                {
                    Status = "Error",
                    Error = new ResponseError
                    {
                        Code = res.StatusCode.ToString()
                    }
                };

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set a new password for {NiraUsername}", username);

                throw;
            }
        }

        /// <summary>
        /// Prepare an HTTP web request
        /// </summary>
        /// <param name="url"></param>
        /// <param name="contentLength"></param>
        /// <returns></returns>
        private HttpWebRequest PrepareWebRequest(string url, int contentLength)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);

            request.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
            request.ContentType = "application/xml; encoding='utf-8'";
            request.ContentLength = contentLength;
            request.Method = HttpMethod.Post.Method;
            var proxySettings = _configuration.GetProxySettings();
            if (proxySettings.Enabled)
            {
                request.Proxy = new WebProxy
                {
                    Address = new Uri(proxySettings.Url),
                    BypassProxyOnLocal = proxySettings.BypassLocal
                };
                _logger.LogInformation("Using proxy Url:{Url}", proxySettings.Url);
            }
            else
            {
                _logger.LogInformation("Proxy is disabled");
            }
            return request;
        }

    }
}