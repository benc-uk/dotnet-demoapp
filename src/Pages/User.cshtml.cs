using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Graph=Microsoft.Graph;

using Microsoft.Identity.Web;

namespace dotnet_demoapp.Pages
{
    // [Authorize]
    public class UserInfoModel : PageModel
    {
        private readonly ILogger<UserInfoModel> _logger;
        readonly ITokenAcquisition _tokenAcquisition;
        public string username { get; private set; } = "";
        public string oid { get; private set; } = "";
        public string name { get; private set; } = "";
        public string tenantId { get; private set; } = "";
        public string preferredUsername { get; private set; } = "";
        public Dictionary<string, string> graphData = new Dictionary<string, string>();
        public byte[] graphPhoto;

        public UserInfoModel(ILogger<UserInfoModel> logger, ITokenAcquisition tokenAcquisition)
        {
            _logger = logger;
            _tokenAcquisition = tokenAcquisition;
        }

        public async Task<IActionResult> OnGet()
        {
            foreach(Claim claim in User.Claims) {
                if (claim.Type.Contains("objectidentifier") || claim.Type.Contains("oid")) {
                    oid = claim.Value;
                }
                if (claim.Type.Contains("tenant") || claim.Type.Contains("tid")) {
                    tenantId = claim.Value;
                }                
                if (claim.Type == "name") {
                    name = claim.Value;
                }         
                if (claim.Type == "preferred_username") {
                    preferredUsername = claim.Value;
                }                           
            }

            username = User.Identity.Name; 

            // Graph stuff
            // Acquire the access token
            try {
                string[] scopes = new string[]{"user.read"};
                string accessToken = await _tokenAcquisition.GetAccessTokenForUserAsync(scopes);

                // Create a client
                var graphClient = new GraphServiceClient(
                new DelegateAuthenticationProvider(
                (requestMessage) =>
                {
                    requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", accessToken);

                    return Task.FromResult(0);
                }));

                // Fetch user details
                var user = await graphClient.Me
                .Request()
                .GetAsync();

                graphData.Add("UPN", user.UserPrincipalName);
                graphData.Add("Given Name", user.GivenName);
                graphData.Add("Display Name", user.DisplayName);
                graphData.Add("Office", user.OfficeLocation);
                graphData.Add("Mobile", user.MobilePhone);
                graphData.Add("Other Phone", user.BusinessPhones.Count() > 0 ? user.BusinessPhones.First() : "");
                graphData.Add("Job Title", user.JobTitle);

                // Fetch user photo
                try {
                    Stream pictureStream = await graphClient
                    .Me
                    .Photos["432x432"]
                    .Content
                    .Request()
                    .GetAsync();

                    // Convert to bytes
                    graphPhoto = ToByteArray(pictureStream);
                } catch(Exception e) {
                    _logger.LogWarning(e.ToString());
                }
            } catch(Exception) {
                return Redirect("/");
            }
            return Page();
        }

        private Byte[] ToByteArray(Stream stream) {
            Int32 length = stream.Length > Int32.MaxValue ? Int32.MaxValue : Convert.ToInt32(stream.Length);
            Byte[] buffer = new Byte[length];
            stream.Read(buffer, 0, length);
            return buffer;
        }
    }
}
