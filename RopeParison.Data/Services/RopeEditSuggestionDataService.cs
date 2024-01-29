using RopeParison.Data;
using RopeParison.Data.Model;
using Microsoft.EntityFrameworkCore;
using RopeParison.Protocol;

namespace RopeParison.Data.Services
{

    public interface IRopeEditSuggestionDataService
    {
        void AddRopeEditSuggestion(RopeEditSuggestionDto ropeEditSuggestionDto);
        void RemoveRopeEditSuggestion(int ropeCalculatedParameterSetId);

        RopeEditSuggestionDto ToDto(RopeEditSuggestion ropeEditSuggestion);
    }

    public class RopeEditSuggestionDataService : IRopeEditSuggestionDataService
    {
        private IDbContextFactory<DataContext> _dbContextFactory;
        private IBrandDataService _brandDataService;

        public RopeEditSuggestionDataService(IDbContextFactory<DataContext> dbContextFactory,
            IBrandDataService brandDataService)
        {
            _dbContextFactory = dbContextFactory;

            _brandDataService = brandDataService;
        }

        public void AddRopeEditSuggestion(RopeEditSuggestionDto ropeEditSuggestionDto)
        {
            using(var db = _dbContextFactory.CreateDbContext())
            {
                var ropeEditSuggestion = new RopeEditSuggestion();
                UpdateRopeEditSuggestion(ropeEditSuggestion, ropeEditSuggestionDto);

                db.RopeEditSuggestions.Add(ropeEditSuggestion);
                db.SaveChanges();
            }
        }

        public void RemoveRopeEditSuggestion(int ropeEditSuggestionId)
        {
            using(var db =_dbContextFactory.CreateDbContext())
            {
                var ropeEditSuggestion = db.RopeEditSuggestions.FirstOrDefault(r => r.RopeEditSuggestionId == ropeEditSuggestionId);
                if (ropeEditSuggestion != null)
                {
                    db.RopeEditSuggestions.Remove(ropeEditSuggestion);
                    db.SaveChanges();
                }
            }
        }

        public void UpdateRopeEditSuggestion(RopeEditSuggestion model, RopeEditSuggestionDto dto)
        {
            //Calculate CalculatedParameterSet
            if (model != null && dto != null)
            {
                model.Name = dto.Name;
                model.BrandId = dto.BrandId;
                //model.Brand = dto.Brand != null ? dto.Brand. : null;
                model.Category = dto.Category != null ? dto.Category.ToModel() : null;

                model.Diameter = dto.Diameter;
                model.MassPerUnitLength = dto.MassPerUnitLength;
                model.SheathPercentage = dto.SheathPercentage;

                model.ImpactForce55kgOneStrand = dto.ImpactForce55kgOneStrand;
                model.ImpactForce80kgOneStrand = dto.ImpactForce80kgOneStrand;
                model.ImpactForce80kgTwoStrand = dto.ImpactForce80kgTwoStrand;

                model.StaticElongation80kgOneStrand = dto.StaticElongation80kgOneStrand;
                model.StaticElongation80kgTwoStrand = dto.StaticElongation80kgTwoStrand;

                model.DynamicElongation55kgOneStrand = dto.DynamicElongation55kgOneStrand;
                model.DynamicElongation80kgOneStrand = dto.DynamicElongation80kgOneStrand;
                model.DynamicElongation80kgTwoStrand = dto.DynamicElongation80kgTwoStrand;

                model.DropsBeforeBreak55kgOneStrand = dto.DropsBeforeBreak55kgOneStrand;
                model.DropsBeforeBreak80kgOneStrand = dto.DropsBeforeBreak80kgOneStrand;
                model.DropsBeforeBreak80kgTwoStrand = dto.DropsBeforeBreak80kgTwoStrand;

                model.SheathSlippage = dto.SheathSlippage;
            }
        }

        public RopeEditSuggestionDto ToDto(RopeEditSuggestion model)
        {
            var dto = new RopeEditSuggestionDto();

            if (model != null)
            {
                dto.RopeEditSuggestionId = model.RopeEditSuggestionId;

                dto.Name = model.Name;
                dto.BrandId = model.BrandId;
                dto.Brand = model.Brand != null ? _brandDataService.ToDto(model.Brand) : null;
                dto.Category = model.Category.ToDto();

                dto.Diameter = model.Diameter;
                dto.MassPerUnitLength = model.MassPerUnitLength;
                dto.SheathPercentage = model.SheathPercentage;

                dto.ImpactForce55kgOneStrand = model.ImpactForce55kgOneStrand;
                dto.ImpactForce80kgOneStrand = model.ImpactForce80kgOneStrand;
                dto.ImpactForce80kgTwoStrand = model.ImpactForce80kgTwoStrand;

                dto.StaticElongation80kgOneStrand = model.StaticElongation80kgOneStrand;
                dto.StaticElongation80kgTwoStrand = model.StaticElongation80kgTwoStrand;

                dto.DynamicElongation55kgOneStrand = model.DynamicElongation55kgOneStrand;
                dto.DynamicElongation80kgOneStrand = model.DynamicElongation80kgOneStrand;
                dto.DynamicElongation80kgTwoStrand = model.DynamicElongation80kgTwoStrand;

                dto.DropsBeforeBreak55kgOneStrand = model.DropsBeforeBreak55kgOneStrand;
                dto.DropsBeforeBreak80kgOneStrand = model.DropsBeforeBreak80kgOneStrand;
                dto.DropsBeforeBreak80kgTwoStrand = model.DropsBeforeBreak80kgTwoStrand;

                dto.SheathSlippage = model.SheathSlippage;

            }

            return dto;
        }

    }

}
