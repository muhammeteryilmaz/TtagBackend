using AutoMapper;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Features.Categories.Queries.GetAllCategories;
using CleanArchitecture.Core.Features.Products.Commands.CreateProduct;
using CleanArchitecture.Core.Features.Products.Queries.GetAllProducts;
using CleanArchitecture.Core.DTOs.Car;
using CleanArchitecture.Core.DTOs.Driver;
using CleanArchitecture.Infrastructure.Entities;

namespace CleanArchitecture.Core.Mappings
{
    public class GeneralProfile : Profile
    {
        public GeneralProfile()
        {
            CreateMap<Product, GetAllProductsViewModel>().ReverseMap();
            CreateMap<CreateProductCommand, Product>();
            CreateMap<GetAllProductsQuery, GetAllProductsParameter>();
            CreateMap<GetAllCategoriesQuery, GetAllCategoriesParameter>();
            CreateMap<Category, GetAllCategoriesViewModel>().ReverseMap();
            CreateMap<Car, CarResponse>();
            CreateMap<ApplicationDriver, DriverResponse>();
        }
    }
}