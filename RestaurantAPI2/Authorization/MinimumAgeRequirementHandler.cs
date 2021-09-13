using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantAPI2.Authorization
{
    public class MinimumAgeRequirementHandler : AuthorizationHandler<MinimumAgeRequirement>
    {
        private readonly ILogger<MinimumAgeRequirementHandler> _logger;

        public MinimumAgeRequirementHandler(ILogger<MinimumAgeRequirementHandler> logger)
        {
            _logger = logger;
        }
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, MinimumAgeRequirement requirement)
        {
            var dateOfBirthClaim = context.User.FindFirst(c => c.Type == "DateOfBirth");

            if (dateOfBirthClaim == null)
            {
                return Task.CompletedTask;
            }

            var dateOfBirth = DateTime.Parse(dateOfBirthClaim.Value);

            var userId = context.User.FindFirst(c => c.Type == ClaimTypes.NameIdentifier).Value;

            _logger.LogInformation($"User: {userId} with date of birth: {dateOfBirth}");

            if(dateOfBirth.AddYears(requirement.MinimumAge) < DateTime.Today)
            {
                _logger.LogInformation("Authorization successed");
                context.Succeed(requirement);
            }
            else
            {
                _logger.LogInformation("Authorization failed");
            }

            return Task.CompletedTask;
        }
    }
}
