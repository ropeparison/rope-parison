using RopeParison.Data;
using RopeParison.Data.Model;
using RopeParison.Data.Services;
using RopeParison.Protocol;

namespace RopeParison.Logic.Services
{

    public interface ICategoryService
    {
        List<CategoryDto> GetAllCategorys(); //Deliberate incorrect pluralisation
    }

    public class CategoryService : ICategoryService
    {
        private readonly ICategoryDataService _categoryDataService;

        public CategoryService(ICategoryDataService categoryDataService)
        {
            _categoryDataService = categoryDataService;
        }
        //-------------------------------------------------------------------------

        public List<CategoryDto> GetAllCategorys()
        {
            return _categoryDataService.GetAllCategorys();
        }

    }
}
