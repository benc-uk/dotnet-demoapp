using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Graph;

#pragma warning disable CA1848 
#pragma warning disable CA2254 

namespace DotnetDemoapp.Pages
{
    [Authorize]
    public class UserInfoModel : PageModel
    {
        private readonly ILogger<UserInfoModel> _logger;
        private readonly GraphServiceClient _graphServiceClient;

        public string Username { get; private set; } = "";
        public string Oid { get; private set; } = "";
        public string Name { get; private set; } = "";
        public string TenantId { get; private set; } = "";
        public string PreferredUsername { get; private set; } = "";
        internal Dictionary<string, string> GraphData = new();
        internal byte[] GraphPhoto;

        public UserInfoModel(ILogger<UserInfoModel> logger, GraphServiceClient graphServiceClient)
        {
            _logger = logger;
            _graphServiceClient = graphServiceClient;
        }

        public async Task<IActionResult> OnGet()
        {
            foreach (var claim in User.Claims)
            {
                if (claim.Type.Contains("objectidentifier") || claim.Type.Contains("oid"))
                {
                    Oid = claim.Value;
                }
                if (claim.Type.Contains("tenant") || claim.Type.Contains("tid"))
                {
                    TenantId = claim.Value;
                }
                if (claim.Type == "name")
                {
                    Name = claim.Value;
                }
                if (claim.Type == "preferred_username")
                {
                    PreferredUsername = claim.Value;
                }
            }

            Username = User.Identity.Name;

            // Graph stuff
            // Acquire the access token
            try
            {
                // Fetch user details from Graph API
                var graphDetails = await _graphServiceClient.Me
                .Request()
                .GetAsync();

                GraphData.Add("UPN", graphDetails.UserPrincipalName);
                GraphData.Add("Given Name", graphDetails.GivenName);
                GraphData.Add("Display Name", graphDetails.DisplayName);
                GraphData.Add("Office", graphDetails.OfficeLocation);
                GraphData.Add("Mobile", graphDetails.MobilePhone);
                GraphData.Add("Other Phone", graphDetails.BusinessPhones.Any() ? graphDetails.BusinessPhones.First() : "");
                GraphData.Add("Job Title", graphDetails.JobTitle);

                // Fetch user photo, this used to fail with MSA accounts hence the extra try/catch
                try
                {
                    var pictureStream = await _graphServiceClient
                    .Me
                    .Photos["432x432"]
                    .Content
                    .Request()
                    .GetAsync();

                    // Convert to bytes
                    GraphPhoto = ToByteArray(pictureStream);
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e.ToString());
                }
            }
            catch (Exception)
            {
                // Cookie seems to get out of sync with the token cache when hotreloading the page
                // This is a horrible hack
                foreach (var cookie in Request.Cookies.Keys)
                {
                    Response.Cookies.Delete(cookie);
                }
                return Redirect("/");
            }
            return Page();
        }

        private static byte[] ToByteArray(Stream stream)
        {
            var length = stream.Length > int.MaxValue ? int.MaxValue : Convert.ToInt32(stream.Length);
            var buffer = new byte[length];
            _ = stream.Read(buffer, 0, length);
            return buffer;
        }
    }
}
