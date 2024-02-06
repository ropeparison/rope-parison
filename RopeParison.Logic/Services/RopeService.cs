using RopeParison.Data;
using RopeParison.Data.Model;
using RopeParison.Data.Services;
using RopeParison.Protocol;
using System.Reflection;

namespace RopeParison.Logic.Services
{

    public interface IRopeService
    {
        //Gets
        List<RopeDto> GetRopes_All();
        List<RopeDto> GetRopes_All_IncEditSuggestions();
        List<RopeDto> GetRopes_Verified(bool verified);
        List<RopeDto> GetRopes_WithEditSuggestions();
        List<RopeDto> GetRopes_FilteredA(RopeFilterADto filter);
        List<RopeDto> GetRopes_FilteredB(RopeFilterDto filter);
        RopeDto GetRopeById(int ropeId);
        RopeDto GetRopeById_IncEditSuggestions(int ropeId);

        //Updates
        void AddRope(RopeDto rope);
        void SubmitRopeEditSuggestion(RopeDto rope, RopeDto ropeEdit);
        void ApproveRopeEditSuggestion(int ropeEditSuggestionId, int ropeId);
        void DeleteRopeEditSuggestion(int ropeEditSuggestionId);
        void UpdateRope(RopeDto rope);
        void RemoveRope(int ropeId);
        void VerifyRope(int ropeId);

        //Other
        bool IsRopeNameUnique(int ropeId, string name);

        
        RopePropertyInformationsDto GetRopePropertyInformations();
        List<RopePropertyInformationDto> GetRopePropertyInformationList();
        RopePropertyInformationsDto GetRopePropertyInformations(ColumnVisibilityFilterDto columnVisibilityFilter);



        /*List<string> GetRopePropertyNames_Id();
        List<string> GetRopePropertyNames_Category();
        List<string> GetRopePropertyNames_Basic();
        List<string> GetRopePropertyNames_ImpactForce();
        List<string> GetRopePropertyNames_StaticElongation();
        List<string> GetRopePropertyNames_DynamicElongation();
        List<string> GetRopePropertyNames_DropsBeforeBreak();
        List<string> GetRopePropertyNames_ExtraPhysicalProperties();

        List<string> GetRopePropertyNames_Data_Single();
        List<string> GetRopePropertyNames_Data_Half();
        List<string> GetRopePropertyNames_Data_Twin();
        List<string> GetRopePropertyNames_Data_HalfTwin();
        List<string> GetRopePropertyNames_Data_Triple();*/


    }

    public class RopeService : IRopeService
    {
        private readonly IRopeDataService _ropeDataService;
        private readonly ICategoryService _categoryService;

        public RopeService(IRopeDataService ropeDataService, ICategoryService categoryService)
        {
            _ropeDataService = ropeDataService;
            _categoryService = categoryService;
        }
        //-------------------------------------------------------------------------

        //#############################################################
        //GetRopes

        public List<RopeDto> GetRopes_All()
        {
            return _ropeDataService.GetRopes_All();
        }

        public List<RopeDto> GetRopes_All_IncEditSuggestions()
        {
            return _ropeDataService.GetRopes_All_IncEditSuggestions();
        }

        public List<RopeDto> GetRopes_Verified(bool verified)
        {
            return _ropeDataService.GetRopes_Verified(verified);
        }

        public List<RopeDto> GetRopes_WithEditSuggestions()
        {
            return _ropeDataService.GetRopes_WithEditSuggestions();
        }

        public List<RopeDto> GetRopes_FilteredA(RopeFilterADto filter)
        {
            var ropes = new List<RopeDto>();

            ropes = _ropeDataService.GetRopes_FilteredA(filter);

            return ropes;
        }

        public List<RopeDto> GetRopes_FilteredB(RopeFilterDto filter)
        {
            return _ropeDataService.GetRopes_FilteredB(filter);
        }

        public RopeDto GetRopeById(int ropeId)
        {
            return _ropeDataService.GetRopeById(ropeId);
        }

        public RopeDto GetRopeById_IncEditSuggestions(int ropeId)
        {
            return _ropeDataService.GetRopeById_IncEditSuggestions(ropeId);
        }

        //GetRopes END
        //#############################################################


        public void AddRope(RopeDto ropeDto)
        {
            ropeDto.Verified = false;

            SetRopeValuesNullBasedOnCategory(ropeDto);

            UpdateRopeCalculatedParameterSet(ropeDto);

            _ropeDataService.AddRope(ropeDto);
        }

        public void SubmitRopeEditSuggestion(RopeDto rope, RopeDto ropeEdit)
        {
            RopeEditSuggestionDto rES = new RopeEditSuggestionDto();

            //Strategy is a separate RopeEditSuggestion for each individual change.
            //This is not the most efficient way of doing things. (More efficent to have a separate list of suggestions for each rope property, but more faff.)
            //It is easy to maintain.
            //It is easy to look at the raw db data.
            
            if (rope.Name != ropeEdit.Name) { rES.Name = ropeEdit.Name; rES.RopeProperty = RopeProperty.Name; _ropeDataService.AddRopeEditSuggestion(rope.RopeId, rES); rES = new RopeEditSuggestionDto(); }
            if (rope.Brand.BrandId != ropeEdit.Brand.BrandId) { rES.BrandId = ropeEdit.Brand.BrandId; rES.RopeProperty = RopeProperty.Brand; _ropeDataService.AddRopeEditSuggestion(rope.RopeId, rES); rES = new RopeEditSuggestionDto(); }
            if (rope.Category != ropeEdit.Category) { rES.Category = ropeEdit.Category; rES.RopeProperty = RopeProperty.Category; _ropeDataService.AddRopeEditSuggestion(rope.RopeId, rES); rES = new RopeEditSuggestionDto(); }

            if (rope.Diameter != ropeEdit.Diameter) { rES.Diameter = ropeEdit.Diameter; rES.RopeProperty = RopeProperty.Diameter; _ropeDataService.AddRopeEditSuggestion(rope.RopeId, rES); rES = new RopeEditSuggestionDto(); }
            if (rope.MassPerUnitLength != ropeEdit.MassPerUnitLength) { rES.MassPerUnitLength = ropeEdit.MassPerUnitLength; rES.RopeProperty = RopeProperty.MassPerUnitLength; _ropeDataService.AddRopeEditSuggestion(rope.RopeId, rES); rES = new RopeEditSuggestionDto(); }
            if (rope.SheathPercentage != ropeEdit.SheathPercentage) { rES.SheathPercentage = ropeEdit.SheathPercentage; rES.RopeProperty = RopeProperty.SheathPercentage; _ropeDataService.AddRopeEditSuggestion(rope.RopeId, rES); rES = new RopeEditSuggestionDto(); }

            if (rope.ImpactForce55kgOneStrand != ropeEdit.ImpactForce55kgOneStrand) { rES.ImpactForce55kgOneStrand = ropeEdit.ImpactForce55kgOneStrand; rES.RopeProperty = RopeProperty.ImpactForce55kgOneStrand; _ropeDataService.AddRopeEditSuggestion(rope.RopeId, rES); rES = new RopeEditSuggestionDto(); }
            if (rope.ImpactForce80kgOneStrand != ropeEdit.ImpactForce80kgOneStrand) { rES.ImpactForce80kgOneStrand = ropeEdit.ImpactForce80kgOneStrand; rES.RopeProperty = RopeProperty.ImpactForce80kgOneStrand; _ropeDataService.AddRopeEditSuggestion(rope.RopeId, rES); rES = new RopeEditSuggestionDto(); }
            if (rope.ImpactForce80kgTwoStrand != ropeEdit.ImpactForce80kgTwoStrand) { rES.ImpactForce80kgTwoStrand = ropeEdit.ImpactForce80kgTwoStrand; rES.RopeProperty = RopeProperty.ImpactForce80kgTwoStrand; _ropeDataService.AddRopeEditSuggestion(rope.RopeId, rES); rES = new RopeEditSuggestionDto(); }

            if (rope.StaticElongation80kgOneStrand != ropeEdit.StaticElongation80kgOneStrand) { rES.StaticElongation80kgOneStrand = ropeEdit.StaticElongation80kgOneStrand; rES.RopeProperty = RopeProperty.StaticElongation80kgOneStrand; _ropeDataService.AddRopeEditSuggestion(rope.RopeId, rES); rES = new RopeEditSuggestionDto(); }
            if (rope.StaticElongation80kgTwoStrand != ropeEdit.StaticElongation80kgTwoStrand) { rES.StaticElongation80kgTwoStrand = ropeEdit.StaticElongation80kgTwoStrand; rES.RopeProperty = RopeProperty.StaticElongation80kgTwoStrand; _ropeDataService.AddRopeEditSuggestion(rope.RopeId, rES); rES = new RopeEditSuggestionDto(); }

            if (rope.DynamicElongation55kgOneStrand != ropeEdit.DynamicElongation55kgOneStrand) { rES.DynamicElongation55kgOneStrand = ropeEdit.DynamicElongation55kgOneStrand; rES.RopeProperty = RopeProperty.DynamicElongation55kgOneStrand; _ropeDataService.AddRopeEditSuggestion(rope.RopeId, rES); rES = new RopeEditSuggestionDto(); }
            if (rope.DynamicElongation80kgOneStrand != ropeEdit.DynamicElongation80kgOneStrand) { rES.DynamicElongation80kgOneStrand = ropeEdit.DynamicElongation80kgOneStrand; rES.RopeProperty = RopeProperty.DynamicElongation80kgOneStrand; _ropeDataService.AddRopeEditSuggestion(rope.RopeId, rES); rES = new RopeEditSuggestionDto(); }
            if (rope.DynamicElongation80kgTwoStrand != ropeEdit.DynamicElongation80kgTwoStrand) { rES.DynamicElongation80kgTwoStrand = ropeEdit.DynamicElongation80kgTwoStrand; rES.RopeProperty = RopeProperty.DynamicElongation80kgTwoStrand; _ropeDataService.AddRopeEditSuggestion(rope.RopeId, rES); rES = new RopeEditSuggestionDto(); }

            if (rope.DropsBeforeBreak55kgOneStrand != ropeEdit.DropsBeforeBreak55kgOneStrand) { rES.DropsBeforeBreak55kgOneStrand = ropeEdit.DropsBeforeBreak55kgOneStrand; rES.RopeProperty = RopeProperty.DropsBeforeBreak55kgOneStrand; _ropeDataService.AddRopeEditSuggestion(rope.RopeId, rES); rES = new RopeEditSuggestionDto(); }
            if (rope.DropsBeforeBreak80kgOneStrand != ropeEdit.DropsBeforeBreak80kgOneStrand) { rES.DropsBeforeBreak80kgOneStrand = ropeEdit.DropsBeforeBreak80kgOneStrand; rES.RopeProperty = RopeProperty.DropsBeforeBreak80kgOneStrand; _ropeDataService.AddRopeEditSuggestion(rope.RopeId, rES); rES = new RopeEditSuggestionDto(); }
            if (rope.DropsBeforeBreak80kgTwoStrand != ropeEdit.DropsBeforeBreak80kgTwoStrand) { rES.DropsBeforeBreak80kgTwoStrand = ropeEdit.DropsBeforeBreak80kgTwoStrand; rES.RopeProperty = RopeProperty.DropsBeforeBreak80kgTwoStrand; _ropeDataService.AddRopeEditSuggestion(rope.RopeId, rES); rES = new RopeEditSuggestionDto(); }

            if (rope.SheathSlippage != ropeEdit.SheathSlippage) { rES.SheathSlippage = ropeEdit.SheathSlippage; rES.RopeProperty = RopeProperty.SheathSlippage; _ropeDataService.AddRopeEditSuggestion(rope.RopeId, rES); rES = new RopeEditSuggestionDto(); }

            //_ropeDataService.AddRopeEditSuggestion(rope.RopeId, rES);
        }

        public void ApproveRopeEditSuggestion(int ropeEditSuggestionId, int ropeId)
        {
            _ropeDataService.ImplementRopeEditSuggestion(ropeEditSuggestionId);

            //Update calculated parameters.
            //This is not the most efficient way to do this. But is fast enough and easy to maintain.
            var ropeDto = _ropeDataService.GetRopeById(ropeId);
            if (ropeDto != null)
            {
                UpdateRope(ropeDto);
            }
        }

        public void DeleteRopeEditSuggestion(int ropeEditSuggestionId)
        {
            _ropeDataService.DeleteRopeEditSuggestion(ropeEditSuggestionId);
        }

        public void UpdateRope(RopeDto ropeDto)
        {
            UpdateRopeCalculatedParameterSet(ropeDto);

            _ropeDataService.UpdateRope(ropeDto);
        }

        public void RemoveRope(int ropeId)
        {
            _ropeDataService.RemoveRope(ropeId);
        }

        public void VerifyRope(int ropeId)
        {
            _ropeDataService.VerifyRope(ropeId);
        }



        public bool IsRopeNameUnique(int ropeId, string name)
        {
            return _ropeDataService.IsRopeNameUnique(ropeId, name);
        }
        
        public void SetRopeValuesNullBasedOnCategory(RopeDto rope)
        {
            if (!_categoryService.Has_ImpactForce55kgOneStrand(rope)) { rope.ImpactForce55kgOneStrand = null; }
            if (!_categoryService.Has_ImpactForce80kgOneStrand(rope)) { rope.ImpactForce80kgOneStrand = null; }
            if (!_categoryService.Has_ImpactForce80kgTwoStrand(rope)) { rope.ImpactForce80kgTwoStrand = null; }

            if (!_categoryService.Has_StaticElongation80kgOneStrand(rope)) { rope.StaticElongation80kgOneStrand = null; }
            if (!_categoryService.Has_StaticElongation80kgTwoStrand(rope)) { rope.StaticElongation80kgTwoStrand = null; }

            if (!_categoryService.Has_DynamicElongation55kgOneStrand(rope)) { rope.DynamicElongation55kgOneStrand = null; }
            if (!_categoryService.Has_DynamicElongation80kgOneStrand(rope)) { rope.DynamicElongation80kgOneStrand = null; }
            if (!_categoryService.Has_DynamicElongation80kgTwoStrand(rope)) { rope.DynamicElongation80kgTwoStrand = null; }

            if (!_categoryService.Has_DropsBeforeBreak55kgOneStrand(rope)) { rope.DropsBeforeBreak55kgOneStrand = null; }
            if (!_categoryService.Has_DropsBeforeBreak80kgOneStrand(rope)) { rope.DropsBeforeBreak80kgOneStrand = null; }
            if (!_categoryService.Has_DropsBeforeBreak80kgTwoStrand(rope)) { rope.DropsBeforeBreak80kgTwoStrand = null; }
        }

        public void UpdateRopeCalculatedParameterSet(RopeDto rope)
        {
            //Calculate CalculatedParameterSet
            if (rope.RopeCalculatedParameterSet != null)
            {
                var ropeArea = Math.PI * Math.Pow((rope.Diameter / 2), 2);

                rope.RopeCalculatedParameterSet.Area = ropeArea;

                if (rope.SheathPercentage.HasValue)
                {
                    rope.RopeCalculatedParameterSet.SheathArea = ropeArea * rope.SheathPercentage / 100;

                    var coreArea = ropeArea * (1 - rope.SheathPercentage / 100);
                    rope.RopeCalculatedParameterSet.CoreArea = coreArea;
                    var coreDiameter = Math.Sqrt(coreArea.Value / Math.PI) * 2;
                    rope.RopeCalculatedParameterSet.CoreDiameter = coreDiameter;

                    rope.RopeCalculatedParameterSet.SheathThickness = rope.Diameter - coreDiameter;
                }
                else
                {
                    rope.RopeCalculatedParameterSet.SheathArea = null;
                    rope.RopeCalculatedParameterSet.CoreArea = null;
                    rope.RopeCalculatedParameterSet.CoreDiameter = null;
                    rope.RopeCalculatedParameterSet.SheathThickness = null;
                }

                rope.RopeCalculatedParameterSet.Density = rope.MassPerUnitLength / (ropeArea * 1000);
            }
        }

        //------------------------------------------------------------------------------------------------------------
        //RopePropertyInformations
        //------------------------------------------------------------------------------------------------------------
        public List<RopePropertyInformationDto> GetRopePropertyInformationList()
        {
            var ropePropertyInformationsDto = new RopePropertyInformationsDto();

            List<RopePropertyInformationDto> ropePropertyInformationList = GetRopePropertyInformationList(ropePropertyInformationsDto);

            return ropePropertyInformationList;
        }
        public List<RopePropertyInformationDto> GetRopePropertyInformationList(RopePropertyInformationsDto ropePropertyInformationsDto)
        {
            var ropePropertyInformationList = new List<RopePropertyInformationDto>();

            ropePropertyInformationList.Add(ropePropertyInformationsDto.Name);
            ropePropertyInformationList.Add(ropePropertyInformationsDto.Brand);
            ropePropertyInformationList.Add(ropePropertyInformationsDto.Category);
            ropePropertyInformationList.Add(ropePropertyInformationsDto.Diameter);
            ropePropertyInformationList.Add(ropePropertyInformationsDto.MassPerUnitLength);
            ropePropertyInformationList.Add(ropePropertyInformationsDto.SheathPercentage);
            ropePropertyInformationList.Add(ropePropertyInformationsDto.ImpactForce55kgOneStrand);
            ropePropertyInformationList.Add(ropePropertyInformationsDto.ImpactForce80kgOneStrand);
            ropePropertyInformationList.Add(ropePropertyInformationsDto.ImpactForce80kgTwoStrand);
            ropePropertyInformationList.Add(ropePropertyInformationsDto.StaticElongation80kgOneStrand);
            ropePropertyInformationList.Add(ropePropertyInformationsDto.StaticElongation80kgTwoStrand);
            ropePropertyInformationList.Add(ropePropertyInformationsDto.DynamicElongation55kgOneStrand);
            ropePropertyInformationList.Add(ropePropertyInformationsDto.DynamicElongation80kgOneStrand);
            ropePropertyInformationList.Add(ropePropertyInformationsDto.DynamicElongation80kgTwoStrand);
            ropePropertyInformationList.Add(ropePropertyInformationsDto.DropsBeforeBreak55kgOneStrand);
            ropePropertyInformationList.Add(ropePropertyInformationsDto.DropsBeforeBreak80kgOneStrand);
            ropePropertyInformationList.Add(ropePropertyInformationsDto.DropsBeforeBreak80kgTwoStrand);
            ropePropertyInformationList.Add(ropePropertyInformationsDto.SheathSlippage);
            ropePropertyInformationList.Add(ropePropertyInformationsDto.RoCaPaSePrIn.Area);
            ropePropertyInformationList.Add(ropePropertyInformationsDto.RoCaPaSePrIn.SheathArea);
            ropePropertyInformationList.Add(ropePropertyInformationsDto.RoCaPaSePrIn.SheathThickness);
            ropePropertyInformationList.Add(ropePropertyInformationsDto.RoCaPaSePrIn.CoreArea);
            ropePropertyInformationList.Add(ropePropertyInformationsDto.RoCaPaSePrIn.CoreDiameter);
            ropePropertyInformationList.Add(ropePropertyInformationsDto.RoCaPaSePrIn.Density);

            return ropePropertyInformationList;
        }

        public RopePropertyInformationsDto GetRopePropertyInformations()
        {
            var ropePropertyInformationsDto = new RopePropertyInformationsDto();

            return ropePropertyInformationsDto;
        }

        public RopePropertyInformationsDto GetRopePropertyInformations(ColumnVisibilityFilterDto columnVisibilityFilter)
        {
            var ropePropertyInformationsDto = new RopePropertyInformationsDto();
            var roPrIn = ropePropertyInformationsDto;

            roPrIn.VisibleColCount = 10; //Default no cols.
            List<Category> categorys = GetColCategorys(columnVisibilityFilter);

            //Hide over-rides Show.

            //Show
            bool display = true;
            if (columnVisibilityFilter.ShowId)
                SetVisibility_Id(roPrIn, display);
            if (columnVisibilityFilter.ShowCategory)
                SetVisibility_Category(roPrIn, display);
            if (columnVisibilityFilter.ShowBasic)
                SetVisibility_Basic(roPrIn, display);
            if (columnVisibilityFilter.ShowImpactForce)
                SetVisibility_ShowImpactForce(roPrIn, display);
            if (columnVisibilityFilter.ShowStaticElongation)
                SetVisibility_StaticElongation(roPrIn, display);
            if (columnVisibilityFilter.ShowDynamicElongation)
                SetVisibility_DynamicElongation(roPrIn, display);
            if (columnVisibilityFilter.ShowDropsBeforeBreak)
                SetVisibility_DropsBeforeBreak(roPrIn, display);
            if (columnVisibilityFilter.ShowExtraPhysicalProperties)
                SetVisibility_ExtraPhysicalProperties(roPrIn, display);
            if (categorys.Any())
                SetVisibility_Category_SelectorPositive(roPrIn, categorys, display);

            //Hide
            display = false;
            if (!columnVisibilityFilter.ShowId)
                SetVisibility_Id(roPrIn, display);
            if (!columnVisibilityFilter.ShowCategory)
                SetVisibility_Category(roPrIn, display);
            if (!columnVisibilityFilter.ShowBasic)
                SetVisibility_Basic(roPrIn, display);
            if (!columnVisibilityFilter.ShowImpactForce)
                SetVisibility_ShowImpactForce(roPrIn, display);
            if (!columnVisibilityFilter.ShowStaticElongation)
                SetVisibility_StaticElongation(roPrIn, display);
            if (!columnVisibilityFilter.ShowDynamicElongation)
                SetVisibility_DynamicElongation(roPrIn, display);
            if (!columnVisibilityFilter.ShowDropsBeforeBreak)
                SetVisibility_DropsBeforeBreak(roPrIn, display);
            if (!columnVisibilityFilter.ShowExtraPhysicalProperties)
                SetVisibility_ExtraPhysicalProperties(roPrIn, display);
            SetVisibility_Category_SelectorNegative(roPrIn, categorys, display); //Note, process even when categorys list empty.



            return ropePropertyInformationsDto;
        }

        public List<Category> GetColCategorys(ColumnVisibilityFilterDto columnVisibilityFilter)
        {
            List<Category> categoryList = new List<Category>();
            if (columnVisibilityFilter.SelectedSingle)
                categoryList.Add(Category.Single);
            if (columnVisibilityFilter.SelectedHalf)
                categoryList.Add(Category.Half);
            if (columnVisibilityFilter.SelectedTwin)
                categoryList.Add(Category.Twin);

            return categoryList;
        }

        public void SetVisibility_Id(RopePropertyInformationsDto roPrIn, bool display)
        {
            roPrIn.VisibleColCount += roPrIn.Name.SetVisible(display);
            roPrIn.VisibleColCount += roPrIn.Brand.SetVisible(display);
        }
        public void SetVisibility_Category(RopePropertyInformationsDto roPrIn, bool display)
        {
            roPrIn.VisibleColCount += roPrIn.Category.SetVisible(display);
        }
        public void SetVisibility_Basic(RopePropertyInformationsDto roPrIn, bool display)
        {
            roPrIn.VisibleColCount += roPrIn.Diameter.SetVisible(display);
            roPrIn.VisibleColCount += roPrIn.MassPerUnitLength.SetVisible(display);
            roPrIn.VisibleColCount += roPrIn.SheathPercentage.SetVisible(display);
        }
        public void SetVisibility_ShowImpactForce(RopePropertyInformationsDto roPrIn, bool display)
        {
            roPrIn.VisibleColCount += roPrIn.ImpactForce55kgOneStrand.SetVisible(display);
            roPrIn.VisibleColCount += roPrIn.ImpactForce80kgOneStrand.SetVisible(display);
            roPrIn.VisibleColCount += roPrIn.ImpactForce80kgTwoStrand.SetVisible(display);
        }
        public void SetVisibility_StaticElongation(RopePropertyInformationsDto roPrIn, bool display)
        {
            roPrIn.VisibleColCount += roPrIn.StaticElongation80kgOneStrand.SetVisible(display);
            roPrIn.VisibleColCount += roPrIn.StaticElongation80kgTwoStrand.SetVisible(display);
        }
        public void SetVisibility_DynamicElongation(RopePropertyInformationsDto roPrIn, bool display)
        {
            roPrIn.VisibleColCount += roPrIn.DynamicElongation55kgOneStrand.SetVisible(display);
            roPrIn.VisibleColCount += roPrIn.DynamicElongation80kgOneStrand.SetVisible(display);
            roPrIn.VisibleColCount += roPrIn.DynamicElongation80kgTwoStrand.SetVisible(display);
        }
        public void SetVisibility_DropsBeforeBreak(RopePropertyInformationsDto roPrIn, bool display)
        {
            roPrIn.VisibleColCount += roPrIn.DropsBeforeBreak55kgOneStrand.SetVisible(display);
            roPrIn.VisibleColCount += roPrIn.DropsBeforeBreak80kgOneStrand.SetVisible(display);
            roPrIn.VisibleColCount += roPrIn.DropsBeforeBreak80kgTwoStrand.SetVisible(display);
        }
        public void SetVisibility_ExtraPhysicalProperties(RopePropertyInformationsDto roPrIn, bool display)
        {
            roPrIn.VisibleColCount += roPrIn.RoCaPaSePrIn.Area.SetVisible(display);
            roPrIn.VisibleColCount += roPrIn.RoCaPaSePrIn.CoreDiameter.SetVisible(display);
            roPrIn.VisibleColCount += roPrIn.RoCaPaSePrIn.CoreArea.SetVisible(display);
            roPrIn.VisibleColCount += roPrIn.RoCaPaSePrIn.SheathThickness.SetVisible(display);
            roPrIn.VisibleColCount += roPrIn.RoCaPaSePrIn.SheathArea.SetVisible(display);
            roPrIn.VisibleColCount += roPrIn.RoCaPaSePrIn.Density.SetVisible(display);
        }

        public void SetVisibility_Category_SelectorPositive(RopePropertyInformationsDto roPrIn, List<Category> categorys, bool display)
        {
            if (categorys.Contains(Category.Single))
            {
                SetVisibility_Category_Single(roPrIn, display);
            }
            if (categorys.Contains(Category.Half))
            {
                SetVisibility_Category_Half(roPrIn, display);
            }
            if (categorys.Contains(Category.Twin))
            {
                SetVisibility_Category_Twin(roPrIn, display);
            }
        }
        public void SetVisibility_Category_SelectorNegative(RopePropertyInformationsDto roPrIn, List<Category> categorys, bool display)
        {
            //Multiple ways to write this logic. This may not be the fastest, but it is quite clear.
            //StaticElongation80kg1Strand is a shared property for Single and Half ropes.
            if (!categorys.Contains(Category.Single))
            {
                if (categorys.Contains(Category.Half))
                    SetVisibility_Category_Single_ExcludeStaticElongation(roPrIn, display);
                else
                    SetVisibility_Category_Single(roPrIn, display);
            }
            if (!categorys.Contains(Category.Half))
            {
                if (categorys.Contains(Category.Single))
                    SetVisibility_Category_Half_ExcludeStaticElongation(roPrIn, display);
                else
                    SetVisibility_Category_Half(roPrIn, display);
            }
            if (!categorys.Contains(Category.Twin))
            {
                SetVisibility_Category_Twin(roPrIn, display);
            }
        }

        public void SetVisibility_Category_Single(RopePropertyInformationsDto roPrIn, bool display)
        {
            roPrIn.VisibleColCount += roPrIn.StaticElongation80kgOneStrand.SetVisible(display);

            SetVisibility_Category_Single_ExcludeStaticElongation(roPrIn, display);
        }
        public void SetVisibility_Category_Single_ExcludeStaticElongation(RopePropertyInformationsDto roPrIn, bool display)
        {
            //Single and Half share StaticElongation property.
            roPrIn.VisibleColCount += roPrIn.ImpactForce80kgOneStrand.SetVisible(display);
            roPrIn.VisibleColCount += roPrIn.DynamicElongation80kgOneStrand.SetVisible(display);
            roPrIn.VisibleColCount += roPrIn.DropsBeforeBreak80kgOneStrand.SetVisible(display);
        }
        public void SetVisibility_Category_Half(RopePropertyInformationsDto roPrIn, bool display)
        {
            roPrIn.VisibleColCount += roPrIn.StaticElongation80kgOneStrand.SetVisible(display);

            SetVisibility_Category_Half_ExcludeStaticElongation(roPrIn, display);
        }
        public void SetVisibility_Category_Half_ExcludeStaticElongation(RopePropertyInformationsDto roPrIn, bool display)
        {
            roPrIn.VisibleColCount += roPrIn.ImpactForce55kgOneStrand.SetVisible(display);
            roPrIn.VisibleColCount += roPrIn.DynamicElongation55kgOneStrand.SetVisible(display);
            roPrIn.VisibleColCount += roPrIn.DropsBeforeBreak55kgOneStrand.SetVisible(display);
        }
        public void SetVisibility_Category_Twin(RopePropertyInformationsDto roPrIn, bool display)
        {
            roPrIn.VisibleColCount += roPrIn.ImpactForce80kgTwoStrand.SetVisible(display);
            roPrIn.VisibleColCount += roPrIn.StaticElongation80kgTwoStrand.SetVisible(display);
            roPrIn.VisibleColCount += roPrIn.DynamicElongation80kgTwoStrand.SetVisible(display);
            roPrIn.VisibleColCount += roPrIn.DropsBeforeBreak80kgTwoStrand.SetVisible(display);
        }


        #region defunct
        /*
        public List<RopePropertyInformationDto> GetRopePropertyInformations_Id()
        {
            var roPrIn = GetRopePropertyInformations();

            var ropePropertyInformations = new List<RopePropertyInformationDto>();
            ropePropertyInformations.Add(roPrIn.Name);
            ropePropertyInformations.Add(roPrIn.Brand);

            return ropePropertyInformations;
        }

        public List<RopePropertyInformationDto> GetRopePropertyInformations_Category()
        {
            var roPrIn = GetRopePropertyInformations();

            var ropePropertyInformations = new List<RopePropertyInformationDto>();
            ropePropertyInformations.Add(roPrIn.Category);

            return ropePropertyInformations;
        }

        public List<RopePropertyInformationDto> GetRopePropertyInformations_Basic()
        {
            var roPrIn = GetRopePropertyInformations();

            var ropePropertyInformations_Basic = new List<RopePropertyInformationDto>();
            ropePropertyInformations_Basic.Add(roPrIn.Diameter);
            ropePropertyInformations_Basic.Add(roPrIn.MassPerUnitLength);
            ropePropertyInformations_Basic.Add(roPrIn.SheathPercentage);

            return ropePropertyInformations_Basic;
        }
        public List<RopePropertyInformationDto> GetRopePropertyInformations_ImpactForce()
        {
            var roPrIn = GetRopePropertyInformations();

            var ropePropertyInformations = new List<RopePropertyInformationDto>();
            ropePropertyInformations.Add(roPrIn.ImpactForce55kgOneStrand);
            ropePropertyInformations.Add(roPrIn.ImpactForce80kgOneStrand);
            ropePropertyInformations.Add(roPrIn.ImpactForce80kgTwoStrand);

            return ropePropertyInformations;
        }
        public List<RopePropertyInformationDto> GetRopePropertyInformations_StaticElongation()
        {
            var roPrIn = GetRopePropertyInformations();

            var ropePropertyInformations = new List<RopePropertyInformationDto>();
            ropePropertyInformations.Add(roPrIn.StaticElongation80kgOneStrand);
            ropePropertyInformations.Add(roPrIn.StaticElongation80kgTwoStrand);

            return ropePropertyInformations;
        }
        public List<RopePropertyInformationDto> GetRopePropertyInformations_DynamicElongation()
        {
            var roPrIn = GetRopePropertyInformations();

            var ropePropertyInformations = new List<RopePropertyInformationDto>();
            ropePropertyInformations.Add(roPrIn.DynamicElongation55kgOneStrand);
            ropePropertyInformations.Add(roPrIn.DynamicElongation80kgOneStrand);
            ropePropertyInformations.Add(roPrIn.DynamicElongation80kgTwoStrand);

            return ropePropertyInformations;
        }
        public List<RopePropertyInformationDto> GetRopePropertyInformations_DropsBeforeBreak()
        {
            var roPrIn = GetRopePropertyInformations();

            var ropePropertyInformations = new List<RopePropertyInformationDto>();
            ropePropertyInformations.Add(roPrIn.DropsBeforeBreak55kgOneStrand);
            ropePropertyInformations.Add(roPrIn.DropsBeforeBreak80kgOneStrand);
            ropePropertyInformations.Add(roPrIn.DropsBeforeBreak80kgTwoStrand);

            return ropePropertyInformations;
        }

        public List<RopePropertyInformationDto> GetRopePropertyInformations_ExtraPhysicalProperties()
        {
            var roPrIn = GetRopePropertyInformations();

            var ropePropertyInformations = new List<RopePropertyInformationDto>();
            ropePropertyInformations.Add(roPrIn.RoCaPaSePrIn.Area);
            ropePropertyInformations.Add(roPrIn.RoCaPaSePrIn.CoreDiameter);
            ropePropertyInformations.Add(roPrIn.RoCaPaSePrIn.CoreArea);
            ropePropertyInformations.Add(roPrIn.RoCaPaSePrIn.SheathArea);
            ropePropertyInformations.Add(roPrIn.RoCaPaSePrIn.SheathThickness);
            ropePropertyInformations.Add(roPrIn.RoCaPaSePrIn.Density);

            return ropePropertyInformations;
        }

        public List<RopePropertyInformationDto> GetRopePropertyInformations_Data_Single()
        {
            var roPrIn = GetRopePropertyInformations();

            var ropePropertyInformations = new List<RopePropertyInformationDto>();
            ropePropertyInformations.Add(roPrIn.ImpactForce80kgOneStrand);
            ropePropertyInformations.Add(roPrIn.StaticElongation80kgOneStrand);
            ropePropertyInformations.Add(roPrIn.DynamicElongation80kgOneStrand);
            ropePropertyInformations.Add(roPrIn.DropsBeforeBreak80kgOneStrand);

            return ropePropertyInformations;
        }
        public List<RopePropertyInformationDto> GetRopePropertyInformations_Data_Half()
        {
            var roPrIn = GetRopePropertyInformations();

            var ropePropertyInformations = new List<RopePropertyInformationDto>();
            ropePropertyInformations.Add(roPrIn.ImpactForce55kgOneStrand);
            ropePropertyInformations.Add(roPrIn.StaticElongation80kgOneStrand);
            ropePropertyInformations.Add(roPrIn.DynamicElongation55kgOneStrand);
            ropePropertyInformations.Add(roPrIn.DropsBeforeBreak55kgOneStrand);

            return ropePropertyInformations;
        }
        public List<RopePropertyInformationDto> GetRopePropertyInformations_Data_Twin()
        {
            var roPrIn = GetRopePropertyInformations();

            var ropePropertyInformations = new List<RopePropertyInformationDto>();
            ropePropertyInformations.Add(roPrIn.ImpactForce80kgTwoStrand);
            ropePropertyInformations.Add(roPrIn.StaticElongation80kgTwoStrand);
            ropePropertyInformations.Add(roPrIn.DynamicElongation80kgTwoStrand);
            ropePropertyInformations.Add(roPrIn.DropsBeforeBreak80kgTwoStrand);

            return ropePropertyInformations;
        }
        public List<RopePropertyInformationDto> GetRopePropertyInformations_Data_HalfTwin()
        {
            var roPrIn = GetRopePropertyInformations();

            var ropePropertyInformations = new List<RopePropertyInformationDto>();
            ropePropertyInformations.Add(roPrIn.ImpactForce55kgOneStrand);
            ropePropertyInformations.Add(roPrIn.ImpactForce80kgTwoStrand);

            ropePropertyInformations.Add(roPrIn.StaticElongation80kgOneStrand);
            ropePropertyInformations.Add(roPrIn.StaticElongation80kgTwoStrand);

            ropePropertyInformations.Add(roPrIn.DynamicElongation55kgOneStrand);
            ropePropertyInformations.Add(roPrIn.DynamicElongation80kgTwoStrand);

            ropePropertyInformations.Add(roPrIn.DropsBeforeBreak55kgOneStrand);
            ropePropertyInformations.Add(roPrIn.DropsBeforeBreak80kgTwoStrand);

            return ropePropertyInformations;
        }
        public List<RopePropertyInformationDto> GetRopePropertyInformations_Data_Triple()
        {
            var roPrIn = GetRopePropertyInformations();

            var ropePropertyInformations = new List<RopePropertyInformationDto>();
            ropePropertyInformations.Add(roPrIn.ImpactForce55kgOneStrand);
            ropePropertyInformations.Add(roPrIn.ImpactForce80kgOneStrand);
            ropePropertyInformations.Add(roPrIn.ImpactForce80kgTwoStrand);

            ropePropertyInformations.Add(roPrIn.StaticElongation80kgOneStrand);
            ropePropertyInformations.Add(roPrIn.StaticElongation80kgTwoStrand);

            ropePropertyInformations.Add(roPrIn.DynamicElongation55kgOneStrand);
            ropePropertyInformations.Add(roPrIn.DynamicElongation80kgOneStrand);
            ropePropertyInformations.Add(roPrIn.DynamicElongation80kgTwoStrand);

            ropePropertyInformations.Add(roPrIn.DropsBeforeBreak55kgOneStrand);
            ropePropertyInformations.Add(roPrIn.DropsBeforeBreak80kgOneStrand);
            ropePropertyInformations.Add(roPrIn.DropsBeforeBreak80kgTwoStrand);

            return ropePropertyInformations;
        }

        public List<string> GetRopePropertyNames_Id()
        {
            return GetRopePropertyNameUnitList(GetRopePropertyInformations_Id());
        }
        public List<string> GetRopePropertyNames_Category()
        {
            return GetRopePropertyNameUnitList(GetRopePropertyInformations_Category());
        }
        public List<string> GetRopePropertyNames_Basic()
        {
            return GetRopePropertyNameUnitList(GetRopePropertyInformations_Basic());
        }
        public List<string> GetRopePropertyNames_ImpactForce()
        {
            return GetRopePropertyNameUnitList(GetRopePropertyInformations_ImpactForce());
        }
        public List<string> GetRopePropertyNames_StaticElongation()
        {
            return GetRopePropertyNameUnitList(GetRopePropertyInformations_StaticElongation());
        }
        public List<string> GetRopePropertyNames_DynamicElongation()
        {
            return GetRopePropertyNameUnitList(GetRopePropertyInformations_DynamicElongation());
        }
        public List<string> GetRopePropertyNames_DropsBeforeBreak()
        {
            return GetRopePropertyNameUnitList(GetRopePropertyInformations_DropsBeforeBreak());
        }
        public List<string> GetRopePropertyNames_ExtraPhysicalProperties()
        {
            return GetRopePropertyNameUnitList(GetRopePropertyInformations_ExtraPhysicalProperties());
        }

        public List<string> GetRopePropertyNames_Data_Single()
        {
            return GetRopePropertyNameUnitList(GetRopePropertyInformations_Data_Single());
        }
        public List<string> GetRopePropertyNames_Data_Half()
        {
            return GetRopePropertyNameUnitList(GetRopePropertyInformations_Data_Half());
        }
        public List<string> GetRopePropertyNames_Data_Twin()
        {
            return GetRopePropertyNameUnitList(GetRopePropertyInformations_Data_Twin());
        }
        public List<string> GetRopePropertyNames_Data_HalfTwin()
        {
            return GetRopePropertyNameUnitList(GetRopePropertyInformations_Data_HalfTwin());
        }
        public List<string> GetRopePropertyNames_Data_Triple()
        {
            return GetRopePropertyNameUnitList(GetRopePropertyInformations_Data_Triple());
        }



        public List<string> GetRopePropertyNameUnitList(List<RopePropertyInformationDto> ropePropertyInformations)
        {
            List<string> roPrIn_Names = new List<string>();

            foreach (var ropeProperty in ropePropertyInformations)
            {
                roPrIn_Names.Add(ropeProperty.NameUnit);
            }

            return roPrIn_Names;
        }
        */
        #endregion

        //------------------------------------------------------------------------------------------------------------
        //RopePropertyInformationsEND
        //------------------------------------------------------------------------------------------------------------
    }
}
