using Microsoft.AspNetCore.Authorization;
using RestaurantAPI2.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantAPI2.Authorization
{
    public class MinimumCreatedRestaurantsRequirementHandler : AuthorizationHandler<MinimumCreatedRestaurantsRequirement>
    {
        private readonly RestaurantDbContext _dbContext;

        public MinimumCreatedRestaurantsRequirementHandler(RestaurantDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, MinimumCreatedRestaurantsRequirement requirement)
        {
            var userId = int.Parse(context.User.FindFirst(c => c.Type == ClaimTypes.NameIdentifier).Value);

            var countOfCreatedRestaurants = _dbContext.Restaurants.Count(r => r.CreatedById == userId);

            if(countOfCreatedRestaurants >= requirement.MinimumCreatedRestaurants)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
