using RopeParison.Data;
using RopeParison.Data.Model;
using RopeParison.Data.Services;
using RopeParison.Protocol;

namespace RopeParison.Logic.Services
{

    public interface IBrandService
    {
        List<BrandDto> GetAllBrands();
        BrandDto GetBrandById(int brandId);
        BrandDto GetBrandByName(string brandName);
        void AddBrand(BrandDto brand);
        void UpdateBrand(BrandDto brand);
        void RemoveBrand(int brandId);
        bool IsBrandNameUnique(string name);
    }

    public class BrandService : IBrandService
    {
        private readonly IBrandDataService _brandDataService;

        public BrandService(IBrandDataService brandDataService)
        {
            _brandDataService = brandDataService;
        }
        //-------------------------------------------------------------------------

        public List<BrandDto> GetAllBrands()
        {
            return _brandDataService.GetAllBrands();
        }

        public BrandDto GetBrandById(int brandId)
        {
           return _brandDataService.GetBrandById(brandId);
        }

        public BrandDto GetBrandByName(string brandName)
        {
            return _brandDataService.GetBrandByName(brandName);
        }

        public void AddBrand(BrandDto brandDto)
        {
            //Do calcs
            _brandDataService.AddBrand(brandDto);
        }

        public void UpdateBrand(BrandDto brandDto)
        {
            //Do calcs
            _brandDataService.UpdateBrand(brandDto);
        }

        public void RemoveBrand(int brandId)
        {
            _brandDataService.RemoveBrand(brandId);
        }

        public bool IsBrandNameUnique(string name)
        {
            return _brandDataService.IsBrandNameUnique(name);
        }

    }
}
