using Microsoft.EntityFrameworkCore;
using WatchStoreApi.Domain.Entities;
using WatchStoreApi.Domain.Enums;

namespace WatchStoreApi.Infrastructure.Persistence;

internal static class SeedData
{
    public static void Apply(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Classic" },
            new Category { Id = 2, Name = "Smart" },
            new Category { Id = 3, Name = "Premium" },
            new Category { Id = 4, Name = "Luxury" }
        );

        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1,  Name = "Doroly",       Description = "Stainless steel gents watch with a 38mm dial in a luxury case.",                                       Material = "Steel",    Gender = Gender.Male,   Price = 799.99m, CategoryId = 1 },
            new Product { Id = 2,  Name = "Aristos",      Description = "Stainless steel gents watch with a 38mm dial in a luxury case.",                                       Material = "Steel",    Gender = Gender.Male,   Price = 749.99m, CategoryId = 1 },
            new Product { Id = 3,  Name = "Modicci",      Description = "Stainless steel gents watch with a 39mm dial in a luxury case.",                                       Material = "Steel",    Gender = Gender.Male,   Price = 849.99m, CategoryId = 1 },
            new Product { Id = 4,  Name = "Aurum",        Description = "Automatic stainless steel watch for men with a 36mm dial in a luxury case.",                            Material = "Steel",    Gender = Gender.Male,   Price = 949.99m, CategoryId = 1 },
            new Product { Id = 5,  Name = "Aeri",         Description = "Stainless steel ladies watch with a 29mm dial in a luxury case.",                                       Material = "Steel",    Gender = Gender.Female, Price = 649.99m, CategoryId = 1 },
            new Product { Id = 6,  Name = "Hope",         Description = "Stainless steel ladies watch with a 21mm dial in a luxury case.",                                       Material = "Steel",    Gender = Gender.Female, Price = 699.99m, CategoryId = 1 },
            new Product { Id = 7,  Name = "Torque Chain", Description = "Stainless steel smart watch with a 21mm dial in a luxury case.",                                        Material = "Steel",    Gender = Gender.Male,   Price = 499.99m, CategoryId = 2 },
            new Product { Id = 8,  Name = "Force Pro",    Description = "Smart watch with a 21mm dial and rubber straps.",                                                       Material = "Steel",    Gender = Gender.Male,   Price = 399.99m, CategoryId = 2 },
            new Product { Id = 9,  Name = "Maxfit",       Description = "Unisex smart watch with a 21mm dial and rubber straps.",                                                Material = "Steel",    Gender = Gender.Unisex, Price = 299.99m, CategoryId = 2 },
            new Product { Id = 10, Name = "Valentina",    Description = "Feminine smart watch with a 21mm dial and rubber straps.",                                              Material = "Steel",    Gender = Gender.Female, Price = 249.99m, CategoryId = 2 },
            new Product { Id = 11, Name = "Ace",          Description = "Feminine smart watch with a 21mm dial and rubber straps.",                                              Material = "Steel",    Gender = Gender.Female, Price = 349.99m, CategoryId = 2 },
            new Product { Id = 12, Name = "Favous",       Description = "Distinguished men's watch with a 46mm dial and a genuine leather strap in a luxury case.",              Material = "Steel",    Gender = Gender.Male,   Price = 899.99m, CategoryId = 3 },
            new Product { Id = 13, Name = "Highlight",    Description = "Sophisticated men's stainless steel watch with a 41mm dial in a luxury case.",                          Material = "Steel",    Gender = Gender.Male,   Price = 999.99m, CategoryId = 3 },
            new Product { Id = 14, Name = "Vanesio",      Description = "Sophisticated stainless steel men's watch with a 39mm dial and dual time function.",                    Material = "Steel",    Gender = Gender.Male,   Price = 999.99m, CategoryId = 3 },
            new Product { Id = 15, Name = "Diesel",       Description = "Robust stainless steel men's watch with a 41mm dial and dual time function.",                           Material = "Steel",    Gender = Gender.Male,   Price = 799.99m, CategoryId = 3 },
            new Product { Id = 16, Name = "Spectrum",     Description = "Sophisticated men's gold watch with a 41mm dial in a titanium case.",                                   Material = "Gold",     Gender = Gender.Male,   Price = 999.99m, CategoryId = 4 },
            new Product { Id = 17, Name = "Wondrous",     Description = "Platinum men's watch with a 37mm dial and dual time function in a stainless steel case.",               Material = "Platinum", Gender = Gender.Male,   Price = 999.99m, CategoryId = 4 },
            new Product { Id = 18, Name = "Chrono",       Description = "High-performance gold men's watch with a 34mm dial.",                                                   Material = "Gold",     Gender = Gender.Male,   Price = 999.99m, CategoryId = 4 },
            new Product { Id = 19, Name = "Ironman",      Description = "Stainless steel men's watch with a 37mm dial in a luxury case.",                                        Material = "Steel",    Gender = Gender.Male,   Price = 899.99m, CategoryId = 4 },
            new Product { Id = 20, Name = "Royal",        Description = "Bronze men's watch in a luxury case.",                                                                  Material = "Bronze",   Gender = Gender.Male,   Price = 599.99m, CategoryId = 4 }
        );
    }
}
