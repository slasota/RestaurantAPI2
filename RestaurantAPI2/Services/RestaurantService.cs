using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantAPI2.Authorization;
using RestaurantAPI2.Entities;
using RestaurantAPI2.Exceptions;
using RestaurantAPI2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantAPI2.Services
{
    public interface IRestaurantService
    {
        int Create(CreateRestaurantDto dto);
        PagedResult<RestaurantDto> GetAll(RestaurantQuery query);
        RestaurantDto GetById(int id);
        void Delete(int id);
        void Update(int id, UpdateRestaurantDto dto);
        void GenerateRestaurantsWithRandomData(int count);
    }

    public class RestaurantService : IRestaurantService
    {
        private readonly RestaurantDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly ILogger<RestaurantService> _logger;
        private readonly IAuthorizationService _authorizationService;
        private readonly IUserContextService _userContextService;

        public RestaurantService(RestaurantDbContext dbContext, IMapper mapper, ILogger<RestaurantService> logger, IAuthorizationService authorizationService, IUserContextService userContextService)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _logger = logger;
            _authorizationService = authorizationService;
            _userContextService = userContextService;
        }

        public void Update(int id, UpdateRestaurantDto dto)
        {


            var restaurant = _dbContext
            .Restaurants
            .FirstOrDefault(r => r.Id == id);

            if (restaurant is null)
                throw new NotFoundException("Restaurant not found");

            var authorizationResult = _authorizationService.AuthorizeAsync(_userContextService.User, restaurant, new ResourceOperationRequirement(ResourceOperation.Update)).Result;

            if (!authorizationResult.Succeeded)
            {
                throw new ForbidException();
            }

            restaurant.Name = dto.Name;
            restaurant.Description = dto.Description;
            restaurant.HasDelivery = dto.HasDelivery;
            _dbContext.SaveChanges();

        }
        public void Delete(int id)
        {
            _logger.LogError($"Restaurant with id: {id} DELETE action invoked");

            var restaurant = _dbContext
                .Restaurants
                .FirstOrDefault(r => r.Id == id);

            if (restaurant is null)
                throw new NotFoundException("Restaurant not found");

            var authorizationResult = _authorizationService.AuthorizeAsync(_userContextService.User, restaurant, new ResourceOperationRequirement(ResourceOperation.Delete)).Result;

            if (!authorizationResult.Succeeded)
            {
                throw new ForbidException();
            }


            _dbContext.Restaurants.Remove(restaurant);
            _dbContext.SaveChanges();
        }

        public RestaurantDto GetById(int id)
        {
            var restaurant = _dbContext
            .Restaurants
            .Include(r => r.Address)
            .Include(r => r.Dishes)
            .FirstOrDefault(r => r.Id == id);

            if (restaurant is null)
                throw new NotFoundException("Restaurant not found");

            var result = _mapper.Map<RestaurantDto>(restaurant);

            return result;
        }

        public PagedResult<RestaurantDto> GetAll(RestaurantQuery query)
        {

            var baseQuery = _dbContext
            .Restaurants
            .Include(r => r.Address)
            .Include(r => r.Dishes)
            .Where(r => query.searchPhrase == null || (r.Name.ToLower().Contains(query.searchPhrase.ToLower())
                                                || r.Description.ToLower().Contains(query.searchPhrase.ToLower())));

            var restaurants = baseQuery
            .Skip(query.PageSize * (query.PageNumber - 1))
            .Take(query.PageSize)
            .ToList();
            
            var restaurantsDtos = _mapper.Map<List<RestaurantDto>>(restaurants);

            var totalItemsCount = baseQuery.Count();

            var result = new PagedResult<RestaurantDto>(restaurantsDtos,totalItemsCount, query.PageSize, query.PageNumber);

            return result;
        }

        public int Create(CreateRestaurantDto dto)
        {
            var restaurant = _mapper.Map<Restaurant>(dto);
            restaurant.CreatedById = _userContextService.GetUserId;
            _dbContext.Restaurants.Add(restaurant);
            _dbContext.SaveChanges();
            return restaurant.Id;
        }

        public void GenerateRestaurantsWithRandomData(int count)
        {
            var restaurants = new List<Restaurant>();

            for (int i = 0; i < count; i++)
            {
                var newRestaurant = new Restaurant()
                {
                    Name = String.Join(" ", Faker.Lorem.Words(2)),
                    Category = Faker.Lorem.GetFirstWord(),
                    ContactEmail = Faker.Internet.Email(),
                    ContactNumber = Faker.Phone.Number(),
                    HasDelivery = true,
                    Description = Faker.Lorem.Sentence(15),
                    Address = new Address()
                    {
                        City = Faker.Address.City(),
                        Street = Faker.Address.StreetAddress(),
                        PostalCode = Faker.Address.UkPostCode()
                    },
                    CreatedById = _userContextService.GetUserId,
                    Dishes = new List<Dish>()
                    {
                        new Dish()
                        {
                            Name = Faker.Name.FullName(),
                            Description = Faker.Lorem.Sentence(),
                            Price = Faker.RandomNumber.Next(100)
                        },
                        new Dish()
                        {
                            Name = Faker.Name.FullName(),
                            Description = Faker.Lorem.Sentence(),
                            Price = Faker.RandomNumber.Next(100)
                        },
                    }
                };
                restaurants.Add(newRestaurant);
            }

            _dbContext.Restaurants.AddRange(restaurants);
            _dbContext.SaveChanges();
        }
    }
}
