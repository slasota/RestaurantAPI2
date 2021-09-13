using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantAPI2.Authorization
{
    public class MinimumCreatedRestaurantsRequirement : IAuthorizationRequirement
    {
        public int MinimumCreatedRestaurants { get; }

        public MinimumCreatedRestaurantsRequirement(int minimumCreatedRestaurants)
        {
            MinimumCreatedRestaurants = minimumCreatedRestaurants;
        }

        
    }
}
