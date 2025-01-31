using Catalog.API.Entities;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Catalog.API.Data
{
    public class CatalogContextSeed
    {
        public static void SeedData(IMongoCollection<Product> productCollection)
        {
            bool existProduct = productCollection.Find(p => true).Any();
            if (!existProduct)
            {
                var products = GetPreconfiguredProducts();
                try
                {
                    productCollection.InsertMany(products);
                }
                catch (Exception ex)
                {

                    throw ex;
                }
            }
        }


        /// <summary>
        /// GetPreconfiguredProducts
        /// </summary>
        /// <returns></returns>

        private static IEnumerable<Product> GetPreconfiguredProducts()
        {
            return new List<Product>()
    {
        new Product
        {
            Id = 1,
            Name = "Fjallraven - Foldsack No. 1 Backpack, Fits 15 Laptops",
            Price = 109.95m,
            Description = "Your perfect pack for everyday use and walks in the forest. Stash your laptop (up to 15 inches) in the padded sleeve, your everyday",
            Category = "men's clothing",
            Image = "https://fakestoreapi.com/img/81fPKd-2AYL._AC_SL1500_.jpg",
            Rating = new Rating
            {
                Rate = 3.9m,
                Count = 120
            }
        },
        new Product
        {
            Id = 2,
            Name = "Mens Casual Premium Slim Fit T-Shirts",
            Price = 22.3m,
            Description = "Slim-fitting style, contrast raglan long sleeve, three-button henley placket, light weight & soft fabric for breathable and comfortable wearing. And Solid stitched shirts with round neck made for durability and a great fit for casual fashion wear and diehard baseball fans. The Henley style round neckline includes a three-button placket.",
            Category = "men's clothing",
            Image = "https://fakestoreapi.com/img/71-3HjGNDUL._AC_SY879._SX._UX._SY._UY_.jpg",
            Rating = new Rating
            {
                Rate = 4.1m,
                Count = 259
            }
        },
        new Product
        {
            Id = 3,
            Name = "Mens Cotton Jacket",
            Price = 55.99m,
            Description = "Great outerwear jackets for Spring/Autumn/Winter, suitable for many occasions, such as working, hiking, camping, mountain/rock climbing, cycling, traveling or other outdoors. Good gift choice for you or your family member. A warm hearted love to Father, husband or son in this thanksgiving or Christmas Day.",
            Category = "men's clothing",
            Image = "https://fakestoreapi.com/img/71li-ujtlUL._AC_UX679_.jpg",
            Rating = new Rating
            {
                Rate = 4.7m,
                Count = 500
            }
        },
        new Product
        {
            Id = 4,
            Name = "Mens Casual Slim Fit",
            Price = 15.99m,
            Description = "The color could be slightly different between on the screen and in practice. / Please note that body builds vary by person, therefore, detailed size information should be reviewed below on the product description.",
            Category = "men's clothing",
            Image = "https://fakestoreapi.com/img/71YXzeOuslL._AC_UY879_.jpg",
            Rating = new Rating
            {
                Rate = 2.1m,
                Count = 430
            }
        },
        new Product
        {
            Id = 5,
            Name = "John Hardy Women's Legends Naga Gold & Silver Dragon Station Chain Bracelet",
            Price = 695m,
            Description = "From our Legends Collection, the Naga was inspired by the mythical water dragon that protects the ocean's pearl. Wear facing inward to be bestowed with love and abundance, or outward for protection.",
            Category = "jewelery",
            Image = "https://fakestoreapi.com/img/71pWzhdJNwL._AC_UL640_QL65_ML3_.jpg",
            Rating = new Rating
            {
                Rate = 4.6m,
                Count = 400
            }
        },
        new Product
        {
            Id = 6,
            Name = "Solid Gold Petite Micropave",
            Price = 168m,
            Description = "Satisfaction Guaranteed. Return or exchange any order within 30 days. Designed and sold by Hafeez Center in the United States. Satisfaction Guaranteed. Return or exchange any order within 30 days.",
            Category = "jewelery",
            Image = "https://fakestoreapi.com/img/61sbMiUnoGL._AC_UL640_QL65_ML3_.jpg",
            Rating = new Rating
            {
                Rate = 3.9m,
                Count = 70
            }
        },
        new Product
        {
            Id = 7,
            Name = "White Gold Plated Princess",
            Price = 9.99m,
            Description = "Classic Created Wedding Engagement Solitaire Diamond Promise Ring for Her. Gifts to spoil your love more for Engagement, Wedding, Anniversary, Valentine's Day...",
            Category = "jewelery",
            Image = "https://fakestoreapi.com/img/71YAIFU48IL._AC_UL640_QL65_ML3_.jpg",
            Rating = new Rating
            {
                Rate = 3.0m,
                Count = 400
            }
        },
        new Product
        {
            Id = 8,
            Name = "Pierced Owl Rose Gold Plated Stainless Steel Double",
            Price = 10.99m,
            Description = "Rose Gold Plated Double Flared Tunnel Plug Earrings. Made of 316L Stainless Steel.",
            Category = "jewelery",
            Image = "https://fakestoreapi.com/img/51UDEzMJVpL._AC_UL640_QL65_ML3_.jpg",
            Rating = new Rating
            {
                Rate = 1.9m,
                Count = 100
            }
        },
        new Product
        {
            Id = 9,
            Name = "WD 2TB Elements Portable External Hard Drive - USB 3.0",
            Price = 64m,
            Description = "USB 3.0 and USB 2.0 Compatibility Fast data transfers Improve PC Performance High Capacity; Compatibility Formatted NTFS for Windows 10, Windows 8.1, Windows 7; Reformatting may be required for other operating systems; Compatibility may vary depending on user’s hardware configuration and operating system",
            Category = "electronics",
            Image = "https://fakestoreapi.com/img/61IBBVJvSDL._AC_SY879_.jpg",
            Rating = new Rating
            {
                Rate = 3.3m,
                Count = 203
            }
        },
        new Product
        {
            Id = 10,
            Name = "SanDisk SSD PLUS 1TB Internal SSD - SATA III 6 Gb/s",
            Price = 109m,
            Description = "Easy upgrade for faster boot up, shutdown, application load and response (As compared to 5400 RPM SATA 2.5” hard drive; Based on published specifications and internal benchmarking tests using PCMark vantage scores) Boosts burst write performance, making it ideal for typical PC workloads The perfect balance of performance and reliability Read/write speeds of up to 535MB/s/450MB/s (Based on internal testing; Performance may vary depending upon drive capacity, host device, OS and application.)",
            Category = "electronics",
            Image = "https://fakestoreapi.com/img/61U7T1koQqL._AC_SX679_.jpg",
            Rating = new Rating
            {
                Rate = 2.9m,
                Count = 470
            }
        },
        new Product
        {
            Id = 11,
            Name = "Silicon Power 256GB SSD 3D NAND A55 SLC Cache Performance Boost SATA III 2.5",
            Price = 109m,
            Description = "3D NAND flash are applied to deliver high transfer speeds Remarkable transfer speeds that enable faster bootup and improved overall system performance. The advanced SLC Cache Technology allows performance boost and longer lifespan 7mm slim design suitable for Ultrabooks and Ultra-slim notebooks. Supports TRIM command, Garbage Collection technology, RAID, and ECC (Error Checking & Correction) to provide the optimized performance and enhanced reliability.",
            Category = "electronics",
            Image = "https://fakestoreapi.com/img/71kWymZ+c+L._AC_SX679_.jpg",
            Rating = new Rating
            {
                Rate = 4.8m,
                Count = 319
            }
        },
        new Product
        {
            Id = 12,
            Name = "WD 4TB Gaming Drive Works with Playstation 4 Portable External Hard Drive",
            Price = 114m,
            Description = "Expand your PS4 gaming experience, Play anywhere Fast and easy, setup Sleek design with high capacity, 3-year manufacturer's limited warranty",
            Category = "electronics",
            Image = "https://fakestoreapi.com/img/61mtL65D4cL._AC_SX679_.jpg",
            Rating = new Rating
            {
                Rate = 4.8m,
                Count = 400
            }
        },
        new Product
        {
            Id = 13,
            Name = "Acer SB220Q bi 21.5 inches Full HD (1920 x 1080) IPS Ultra-Thin",
            Price = 599m,
            Description = "21. 5 inches Full HD (1920 x 1080) widescreen IPS display And Radeon free Sync technology. No compatibility for VESA Mount Refresh Rate: 75Hz - Using HDMI port Zero-frame design | ultra-thin | 4ms response time | IPS panel Aspect ratio - 16: 9. Color Supported - 16. 7 million colors. Brightness - 250 nit Tilt angle -5 degree to 15 degree. Horizontal viewing angle-178 degree. Vertical viewing angle-178 degree 75 hertz",
            Category = "electronics",
            Image = "https://fakestoreapi.com/img/81QpkIctqPL._AC_SX679_.jpg",
            Rating = new Rating
            {
                Rate = 2.9m,
                Count = 250
            }
        },
        new Product
        {
            Id = 14,
            Name = "Samsung 49-Inch CHG90 32:9 Curved Gaming Monitor",
            Price = 999.99m,
            Description = "49-Inch Dual QHD (5120x1440) Super ultra-wide curved screen 144Hz refresh rate. 1ms response time HDR support. Customizable RGB lighting, Amazon exclusive - up to 32% off! Best for gaming, work & entertainment.",
            Category = "electronics",
            Image = "https://fakestoreapi.com/img/71g2p9sJf8L._AC_SX679_.jpg",
            Rating = new Rating
            {
                Rate = 2.5m,
                Count = 140
            }
        },
        new Product
        {
            Id = 15,
            Name = "Great Value 3-Pack Black Ballpoint Pens",
            Price = 4.99m,
            Description = "Perfect for everyday use, these black ballpoint pens provide a smooth writing experience.",
            Category = "office supplies",
            Image = "https://fakestoreapi.com/img/81I2r6NNZyL._AC_SY879_.jpg",
            Rating = new Rating
            {
                Rate = 4.3m,
                Count = 150
            }
        },


    new Product
    {
        Id = 16,
        Name = "Lock and Love Women's Removable Hooded Faux Leather Moto Biker Jacket",
        Price = 29.95m,
        Description = "100% POLYURETHANE(shell) 100% POLYESTER(lining) 75% POLYESTER 25% COTTON (SWEATER), Faux leather material for style and comfort / 2 pockets of front, 2-For-One Hooded denim style faux leather jacket, Button detail on waist / Detail stitching at sides, HAND WASH ONLY / DO NOT BLEACH / LINE DRY / DO NOT IRON",
        Category = "women's clothing",
        Image = "https://fakestoreapi.com/img/81XH0e8fefL._AC_UY879_.jpg",
        Rating = new Rating
        {
            Rate = 2.9m,
            Count = 340
        }
    },
    new Product
    {
        Id = 17,
        Name = "Rain Jacket Women Windbreaker Striped Climbing Raincoats",
        Price = 39.99m,
        Description = "Lightweight perfect for trip or casual wear---Long sleeve with hooded, adjustable drawstring waist design. Button and zipper front closure raincoat, fully stripes Lined and The Raincoat has 2 side pockets are a good size to hold all kinds of things, it covers the hips, and the hood is generous but doesn't overdo it. Attached Cotton Lined Hood with Adjustable Drawstrings give it a real styled look.",
        Category = "women's clothing",
        Image = "https://fakestoreapi.com/img/71HblAHs5xL._AC_UY879_-2.jpg",
        Rating = new Rating
        {
            Rate = 3.8m,
            Count = 679
        }
    },
    new Product
    {
        Id = 18,
        Name = "MBJ Women's Solid Short Sleeve Boat Neck V",
        Price = 9.85m,
        Description = "95% RAYON 5% SPANDEX, Made in USA or Imported, Do Not Bleach, Lightweight fabric with great stretch for comfort, Ribbed on sleeves and neckline / Double stitching on bottom hem",
        Category = "women's clothing",
        Image = "https://fakestoreapi.com/img/71z3kpMAYsL._AC_UY879_.jpg",
        Rating = new Rating
        {
            Rate = 4.7m,
            Count = 130
        }
    },
    new Product
    {
        Id = 19,
        Name = "Opna Women's Short Sleeve Moisture",
        Price = 7.95m,
        Description = "100% Polyester, Machine wash, 100% cationic polyester interlock, Machine Wash & Pre Shrunk for a Great Fit, Lightweight, roomy and highly breathable with moisture wicking fabric which helps to keep moisture away, Soft Lightweight Fabric with comfortable V-neck collar and a slimmer fit, delivers a sleek, more feminine silhouette and Added Comfort",
        Category = "women's clothing",
        Image = "https://fakestoreapi.com/img/51eg55uWmdL._AC_UX679_.jpg",
        Rating = new Rating
        {
            Rate = 4.5m,
            Count = 146
        }
    },
    new Product
    {
        Id = 20,
        Name = "DANVOUY Womens T Shirt Casual Cotton Short",
        Price = 12.99m,
        Description = "95%Cotton,5%Spandex, Features: Casual, Short Sleeve, Letter Print,V-Neck,Fashion Tees, The fabric is soft and has some stretch., Occasion: Casual/Office/Beach/School/Home/Street. Season: Spring, Summer, Autumn, Winter.",
        Category = "women's clothing",
        Image = "https://fakestoreapi.com/img/61pHAEJ4NML._AC_UX679_.jpg",
        Rating = new Rating
        {
            Rate = 3.6m,
            Count = 145
        }
    },


    };
        }

    }
}