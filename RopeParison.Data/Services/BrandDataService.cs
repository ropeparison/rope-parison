using RopeParison.Data;
using RopeParison.Data.Model;
using Microsoft.EntityFrameworkCore;
using RopeParison.Protocol;

namespace RopeParison.Data.Services
{

    public interface IBrandDataService
    {
        List<BrandDto> GetAllBrands();
        BrandDto GetBrandById(int brandId);
        BrandDto GetBrandByName(string brandName);
        void AddBrand(BrandDto brand);
        void UpdateBrand(BrandDto brand);
        void RemoveBrand(int brandId);
        bool IsBrandNameUnique(string name);
        BrandDto ToDto(Brand brand);
        void UpdateBrand(Brand brand, BrandDto brandDto);

    }

    public class BrandDataService : IBrandDataService
    {
        private IDbContextFactory<DataContext> _dbContextFactory;

        public BrandDataService(IDbContextFactory<DataContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public List<BrandDto> GetAllBrands()
        {
            using (var db = _dbContextFactory.CreateDbContext())
            {
                IQueryable<Brand> brands = db.Brands;
                brands = brands.OrderBy(b => b.Name); //Default order alphabetically.

                var brandList = brands.ToList();
                
                return brandList.Select(x => ToDto(x)).ToList();
            }
        }

        public BrandDto GetBrandById(int brandId)
        {
            using (var db = _dbContextFactory.CreateDbContext())
            {
                var brand = db.Brands.FirstOrDefault(r => r.BrandId == brandId);
                if (brand == null)
                {
                    brand = new Brand();
                    brand.BrandId = 0;
                }
                return ToDto(brand);
            }
        }

        public BrandDto GetBrandByName(string brandName)
        {
            using (var db = _dbContextFactory.CreateDbContext())
            {
                var brand = db.Brands.FirstOrDefault(r => r.Name == brandName);
                if (brand == null)
                {
                    brand = new Brand();
                    brand.BrandId = 0;
                }
                return ToDto(brand);
            }
        }

        public void AddBrand(BrandDto brandDto)
        {
            using(var db = _dbContextFactory.CreateDbContext())
            {
                var brand = new Brand();
                UpdateBrand(brand, brandDto);

                db.Brands.Add(brand);
                db.SaveChanges();
            }
        }

        public void UpdateBrand(BrandDto brandDto)
        {
            using (var db = _dbContextFactory.CreateDbContext())
            {
                var brand = db.Brands.First(r => r.BrandId == brandDto.BrandId);

                UpdateBrand(brand, brandDto);

                db.SaveChanges();
            }
        }

        public void RemoveBrand(int brandId)
        {
            using(var db =_dbContextFactory.CreateDbContext())
            {
                var brand = db.Brands.FirstOrDefault(r => r.BrandId == brandId);
                if (brand != null)
                {
                    db.Brands.Remove(brand);
                    db.SaveChanges();
                }
            }
        }



        

        public bool IsBrandNameUnique(string name)
        {
            using (var db = _dbContextFactory.CreateDbContext())
            {
                bool test = db.Brands.Any(r => r.Name == name);
                return !test;
            }
        }


        public void UpdateBrand(Brand brand, BrandDto dto)
        {
            brand.BrandId = dto.BrandId;
            brand.Name = dto.Name;
        }

        public Protocol.BrandDto ToDto(Brand brand)
        {
            var dto = new Protocol.BrandDto();

            dto.BrandId = brand.BrandId;
            dto.Name = brand.Name;

            return dto;
        }


    }

}
