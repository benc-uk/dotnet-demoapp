using Microsoft.AspNetCore.Http;
using Microsoft.FeatureManagement.FeatureFilters;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotnetDemoapp
{
    public class TestTargetingContextAccessor : ITargetingContextAccessor
    {
        private const string TargetingContextLookup = "TestTargetingContextAccessor.TargetingContext";
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TestTargetingContextAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public ValueTask<TargetingContext> GetContextAsync()
        {
            HttpContext httpContext = _httpContextAccessor.HttpContext;
            if (httpContext.Items.TryGetValue(TargetingContextLookup, out object value))
            {
                return new ValueTask<TargetingContext>((TargetingContext)value);
            }
            List<string> groups = new List<string>();
            if (httpContext.User.Identity.Name != null)
            {
                groups.Add(httpContext.User.Identity.Name.Split("@", StringSplitOptions.None)[1]);
            }
            TargetingContext targetingContext = new TargetingContext
            {
                UserId = httpContext.User.Identity.Name,
                Groups = groups
            };
            httpContext.Items[TargetingContextLookup] = targetingContext;
            return new ValueTask<TargetingContext>(targetingContext);
        }
    }
}