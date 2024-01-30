using RopeParison.Data;
using RopeParison.Data.Model;
using RopeParison.Data.Services;
using RopeParison.Protocol;

namespace RopeParison.Logic.Services
{

    public interface ICategoryService
    {
        List<CategoryDto> GetAllCategorys(); //Deliberate incorrect pluralisation

        //Category checks
        bool IsHalfTwinOrTriple(RopeDto rope);
        bool IsHalfTwin(RopeDto rope);
        bool IsTriple(RopeDto rope);

        bool Has_ImpactForce55kgOneStrand(RopeDto rope);
        bool Has_ImpactForce80kgOneStrand(RopeDto rope);
        bool Has_ImpactForce80kgTwoStrand(RopeDto rope);

        bool Has_StaticElongation80kgOneStrand(RopeDto rope);
        bool Has_StaticElongation80kgTwoStrand(RopeDto rope);

        bool Has_DynamicElongation55kgOneStrand(RopeDto rope);
        bool Has_DynamicElongation80kgOneStrand(RopeDto rope);
        bool Has_DynamicElongation80kgTwoStrand(RopeDto rope);

        bool Has_DropsBeforeBreak55kgOneStrand(RopeDto rope);
        bool Has_DropsBeforeBreak80kgOneStrand(RopeDto rope);
        bool Has_DropsBeforeBreak80kgTwoStrand(RopeDto rope);

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



        //--------------------------------------------------------------------------------------------------
        // Category Checks
        //--------------------------------------------------------------------------------------------------

        public bool IsHalfTwinOrTriple(RopeDto rope)
        {
            Enum[] categories = new Enum[] { Category.Triple, Category.HalfTwin };
            return RopeCategoryCheck(rope, categories);
        }
        public bool IsHalfTwin(RopeDto rope)
        {
            Enum[] categories = new Enum[] { Category.HalfTwin };
            return RopeCategoryCheck(rope, categories);
        }
        public bool IsTriple(RopeDto rope)
        {
            Enum[] categories = new Enum[] { Category.Triple };
            return RopeCategoryCheck(rope, categories);
        }


        public bool Has_ImpactForce55kgOneStrand(RopeDto rope)
        {
            Enum[] categories = new Enum[] { Category.Triple, Category.Half, Category.HalfTwin };
            return RopeCategoryCheck(rope, categories);
        }

        public bool Has_ImpactForce80kgOneStrand(RopeDto rope)
        {
            Enum[] categories = new Enum[] { Category.Triple, Category.Single };
            return RopeCategoryCheck(rope, categories);
        }

        public bool Has_ImpactForce80kgTwoStrand(RopeDto rope)
        {
            Enum[] categories = new Enum[] { Category.Triple, Category.Twin, Category.HalfTwin };
            return RopeCategoryCheck(rope, categories);
        }


        public bool Has_StaticElongation80kgOneStrand(RopeDto rope)
        {
            Enum[] categories = new Enum[] { Category.Triple, Category.Single, Category.Half, Category.HalfTwin };
            return RopeCategoryCheck(rope, categories);
        }

        public bool Has_StaticElongation80kgTwoStrand(RopeDto rope)
        {
            Enum[] categories = new Enum[] { Category.Triple, Category.Twin, Category.HalfTwin };
            return RopeCategoryCheck(rope, categories);
        }


        public bool Has_DynamicElongation55kgOneStrand(RopeDto rope)
        {
            Enum[] categories = new Enum[] { Category.Triple, Category.Half, Category.HalfTwin };
            return RopeCategoryCheck(rope, categories);
        }

        public bool Has_DynamicElongation80kgOneStrand(RopeDto rope)
        {
            Enum[] categories = new Enum[] { Category.Triple, Category.Single };
            return RopeCategoryCheck(rope, categories);
        }

        public bool Has_DynamicElongation80kgTwoStrand(RopeDto rope)
        {
            Enum[] categories = new Enum[] { Category.Triple, Category.Twin, Category.HalfTwin };
            return RopeCategoryCheck(rope, categories);
        }


        public bool Has_DropsBeforeBreak55kgOneStrand(RopeDto rope)
        {
            Enum[] categories = new Enum[] { Category.Triple, Category.Half, Category.HalfTwin };
            return RopeCategoryCheck(rope, categories);
        }

        public bool Has_DropsBeforeBreak80kgOneStrand(RopeDto rope)
        {
            Enum[] categories = new Enum[] { Category.Triple, Category.Single };
            return RopeCategoryCheck(rope, categories);
        }

        public bool Has_DropsBeforeBreak80kgTwoStrand(RopeDto rope)
        {
            Enum[] categories = new Enum[] { Category.Triple, Category.Twin, Category.HalfTwin };
            return RopeCategoryCheck(rope, categories);
        }

        public bool RopeCategoryCheck(RopeDto rope, Enum[] categories)
        {
            var contains = false;

            if (categories.Contains(rope?.Category?.Category))
            {
                contains = true;
            }
            //else if (categories.Contains(ropeEdit?.Category?.Category))
            //{
            //    contains = true;
            //}

            return contains;
        }

    }
}
