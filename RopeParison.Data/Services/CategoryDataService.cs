using RopeParison.Data;
using RopeParison.Data.Model;
using Microsoft.EntityFrameworkCore;
using RopeParison.Protocol;
using RopeParison.Helpers;

namespace RopeParison.Data.Services
{

    public interface ICategoryDataService
    {
        List<CategoryDto> GetAllCategorys(); //Deliberate incorrect pluralisation
    }

    public class CategoryDataService : ICategoryDataService
    {
        private IDbContextFactory<DataContext> _dbContextFactory;

        public CategoryDataService(Microsoft.EntityFrameworkCore.IDbContextFactory<DataContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public List<CategoryDto> GetAllCategorys()
        {
            var categorys = EnumHelper.GetEnumAsIEnumerable<Category>();
            var categorysList = categorys.Select(x => x.ToDto()).ToList();
            return categorysList;
        }

    }
}
