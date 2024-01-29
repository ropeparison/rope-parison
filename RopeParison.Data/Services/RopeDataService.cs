using RopeParison.Data;
using RopeParison.Data.Model;
using Microsoft.EntityFrameworkCore;
using RopeParison.Protocol;
using System.Runtime.ConstrainedExecution;

namespace RopeParison.Data.Services
{

    public interface IRopeDataService
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
        void UpdateRope(RopeDto rope);
        void AddRopeEditSuggestion(int ropeId, RopeEditSuggestionDto rESDto);
        void ImplementRopeEditSuggestion(int ropeEditSuggestionId);
        void DeleteRopeEditSuggestion(int ropeEditSuggestionId);
        void RemoveRope(int ropeId);
        void VerifyRope(int ropeId);

        //Other
        bool IsRopeNameUnique(int ropeId, string name);
    }

    public class RopeDataService : IRopeDataService
    {
        private IDbContextFactory<DataContext> _dbContextFactory;
        private IRopeCalculatedParameterSetDataService _ropeCalculatedParameterSetDataService;
        private IBrandDataService _brandDataService;
        private IRopeEditSuggestionDataService _ropeEditSuggestionDataService;

        public RopeDataService(IDbContextFactory<DataContext> dbContextFactory,
            IRopeCalculatedParameterSetDataService ropeCalculatedParameterSetDataService,
            IBrandDataService brandDataService,
            IRopeEditSuggestionDataService ropeEditSuggestionDataService)
        {
            _dbContextFactory = dbContextFactory;
            _ropeCalculatedParameterSetDataService = ropeCalculatedParameterSetDataService;
            _brandDataService = brandDataService;
            _ropeEditSuggestionDataService = ropeEditSuggestionDataService;
        }

        public List<RopeDto> GetRopes_All()
        {
            return GetRopes_All(false);
        }

        public List<RopeDto> GetRopes_All_IncEditSuggestions()
        {
            return GetRopes_All(true);
        }

        public List<RopeDto> GetRopes_Verified(bool verified)
        {
            using (var db = _dbContextFactory.CreateDbContext())
            {
                IQueryable<Rope> ropes = GetAllRopesQueryable(db);
                ropes = ropes.Where(r => r.Verified == verified);

                IList<Rope> ropesList = ropes.ToList(); //Realise ropes in memory to perform ToDto.

                var ropeDtos = ropesList.Select(x => ToDto(x)).ToList();

                return ropeDtos;
            }
        }

        public List<RopeDto> GetRopes_WithEditSuggestions()
        {
            using (var db = _dbContextFactory.CreateDbContext())
            {
                IQueryable<Rope> ropes = GetAllRopesQueryable(db);
                ropes = ropes.Include(r => r.RopeEditSuggestions).ThenInclude(x => x.Brand);

                ropes = ropes.Where(r => r.RopeEditSuggestions.Any());


                IList<Rope> ropesList = ropes.ToList(); //Realise ropes in memory to perform ToDto.

                var ropeDtos = ropesList.Select(x => ToDto(x)).ToList();

                return ropeDtos;
            }
        }

        public RopeDto GetRopeById(int ropeId)
        {
            using (var db = _dbContextFactory.CreateDbContext())
            {
                IQueryable<Rope> ropes = GetAllRopesQueryable(db);

                var rope = ropes.FirstOrDefault(r => r.RopeId == ropeId);

                if (rope == null)
                {
                    rope = new Rope();
                    rope.RopeId = 0;
                }
                return ToDto(rope);
            }
        }

        public RopeDto GetRopeById_IncEditSuggestions(int ropeId)
        {
            using (var db = _dbContextFactory.CreateDbContext())
            {
                IQueryable<Rope> ropes = GetAllRopesQueryable(db);
                ropes = ropes.Include(r => r.RopeEditSuggestions).ThenInclude(x => x.Brand);

                var rope = ropes.FirstOrDefault(r => r.RopeId == ropeId);

                if (rope == null)
                {
                    rope = new Rope();
                    rope.RopeId = 0;
                }
                return ToDto(rope);
            }
        }

        public List<RopeDto> GetRopes_FilteredA(RopeFilterADto filter)
        {
            var ropeDtos = new List<RopeDto>();

            using (var db = _dbContextFactory.CreateDbContext())
            {
                IQueryable<Rope> ropes = GetAllRopesQueryable(db);

                if (!filter.IncludeUnverified)
                {
                    ropes = ropes.Where(r => r.Verified == true);
                }

                if (filter.RopeIds != null && filter.RopeIds.Any())
                {
                    ropes = ropes.Where(r => filter.RopeIds.Any(x => r.RopeId == x));
                }
                if (filter.BrandIds != null && filter.BrandIds.Any())
                {
                    ropes = ropes.Where(r => filter.BrandIds.Any(x => r.Brand.BrandId == x));
                }
                if (filter.Categorys != null && filter.Categorys.Any())
                {
                    List<int> intCategorys = filter.Categorys.Select(x => (int)x).ToList(); //Have to do this because the EF Query Translator isn't yet capable of handling an enum inside an any query.
                    ropes = ropes.Where(r => intCategorys.Any(c => (int)(r.Category) == c));
                }

                ropes = ropes
                    .Where(r => filter.DiameterMin == null || filter.DiameterMin <= r.Diameter)
                    .Where(r => filter.DiameterMax == null || filter.DiameterMax >= r.Diameter)
                    .Where(r => filter.MassPerUnitLengthMin == null || filter.MassPerUnitLengthMin <= r.MassPerUnitLength)
                    .Where(r => filter.MassPerUnitLengthMax == null || filter.MassPerUnitLengthMax >= r.MassPerUnitLength)
                    .Where(r => filter.SheathPercentageMin == null || filter.SheathPercentageMin <= r.SheathPercentage)
                    .Where(r => filter.SheathPercentageMax == null || filter.SheathPercentageMax >= r.SheathPercentage);

                var ropeList = ropes.ToList();

                ropeDtos = ropeList.Select(x => ToDto(x)).ToList();
            }

            return ropeDtos;
        }

        public List<RopeDto> GetRopes_FilteredB(RopeFilterDto filter)
        {
            using (var db = _dbContextFactory.CreateDbContext())
            {

                IQueryable<Rope> query = db.Ropes.Include(r => r.Brand).Include(r => r.RopeCalculatedParameterSet);

                if (filter.Name != null)
                    query = query.Where(r => r.Name == filter.Name);

                if (filter.BrandName != null)
                    query = query.Where(r => r.Brand.Name == filter.BrandName);

                if (filter.CategoryName != null)
                    query = query.Where(r => r.Category.ToString() == filter.CategoryName);

                if (filter.DiameterMin.HasValue)
                    query = query.Where(r => r.Diameter >= filter.DiameterMin.Value);

                if (filter.DiameterMax.HasValue)
                    query = query.Where(r => r.Diameter <= filter.DiameterMax.Value);

                if (filter.MassPerUnitLengthMin.HasValue)
                    query = query.Where(r => r.MassPerUnitLength >= filter.MassPerUnitLengthMin.Value);

                if (filter.MassPerUnitLengthMax.HasValue)
                    query = query.Where(r => r.MassPerUnitLength <= filter.MassPerUnitLengthMax.Value);

                if (filter.AreaMin.HasValue)
                    query = query.Where(r => r.RopeCalculatedParameterSet.Area >= filter.AreaMin.Value);

                if (filter.AreaMax.HasValue)
                    query = query.Where(r => r.RopeCalculatedParameterSet.Area <= filter.AreaMax.Value);

                if (filter.SheathPercentageMin.HasValue)
                    query = query.Where(r => r.SheathPercentage >= filter.SheathPercentageMin.Value);

                if (filter.SheathPercentageMax.HasValue)
                    query = query.Where(r => r.SheathPercentage <= filter.SheathPercentageMax.Value);

                if (filter.SheathAreaMin.HasValue)
                    query = query.Where(r => r.RopeCalculatedParameterSet.SheathArea >= filter.SheathAreaMin.Value);

                if (filter.SheathAreaMax.HasValue)
                    query = query.Where(r => r.RopeCalculatedParameterSet.SheathArea <= filter.SheathAreaMax.Value);

                if (filter.SheathThicknessMin.HasValue)
                    query = query.Where(r => r.RopeCalculatedParameterSet.SheathThickness >= filter.SheathThicknessMin.Value);

                if (filter.SheathThicknessMax.HasValue)
                    query = query.Where(r => r.RopeCalculatedParameterSet.SheathThickness <= filter.SheathThicknessMax.Value);

                if (filter.CoreAreaMin.HasValue)
                    query = query.Where(r => r.RopeCalculatedParameterSet.CoreArea >= filter.CoreAreaMin.Value);

                if (filter.CoreAreaMax.HasValue)
                    query = query.Where(r => r.RopeCalculatedParameterSet.CoreArea <= filter.CoreAreaMax.Value);

                if (filter.CoreDiameterMin.HasValue)
                    query = query.Where(r => r.RopeCalculatedParameterSet.CoreDiameter >= filter.CoreDiameterMin.Value);

                if (filter.CoreDiameterMax.HasValue)
                    query = query.Where(r => r.RopeCalculatedParameterSet.CoreDiameter <= filter.CoreDiameterMax.Value);

                if (filter.ImpactForce55kgOneStrandMin.HasValue)
                    query = query.Where(r => r.ImpactForce55kgOneStrand >= filter.ImpactForce55kgOneStrandMin.Value);

                if (filter.ImpactForce55kgOneStrandMax.HasValue)
                    query = query.Where(r => r.ImpactForce55kgOneStrand <= filter.ImpactForce55kgOneStrandMax.Value);

                if (filter.ImpactForce80kgOneStrandMin.HasValue)
                    query = query.Where(r => r.ImpactForce80kgOneStrand >= filter.ImpactForce80kgOneStrandMin.Value);

                if (filter.ImpactForce80kgOneStrandMax.HasValue)
                    query = query.Where(r => r.ImpactForce80kgOneStrand <= filter.ImpactForce80kgOneStrandMax.Value);

                if (filter.ImpactForce80kgTwoStrandMin.HasValue)
                    query = query.Where(r => r.ImpactForce80kgTwoStrand >= filter.ImpactForce80kgTwoStrandMin.Value);

                if (filter.ImpactForce80kgTwoStrandMax.HasValue)
                    query = query.Where(r => r.ImpactForce80kgTwoStrand <= filter.ImpactForce80kgTwoStrandMax.Value);

                if (filter.StaticElongation80kgOneStrandMin.HasValue)
                    query = query.Where(r => r.StaticElongation80kgOneStrand >= filter.StaticElongation80kgOneStrandMin.Value);

                if (filter.StaticElongation80kgOneStrandMax.HasValue)
                    query = query.Where(r => r.StaticElongation80kgOneStrand <= filter.StaticElongation80kgOneStrandMax.Value);

                if (filter.StaticElongation80kgTwoStrandMin.HasValue)
                    query = query.Where(r => r.StaticElongation80kgTwoStrand >= filter.StaticElongation80kgTwoStrandMin.Value);

                if (filter.StaticElongation80kgTwoStrandMax.HasValue)
                    query = query.Where(r => r.StaticElongation80kgTwoStrand <= filter.StaticElongation80kgTwoStrandMax.Value);

                if (filter.DynamicElongation55kgOneStrandMin.HasValue)
                    query = query.Where(r => r.DynamicElongation55kgOneStrand >= filter.DynamicElongation55kgOneStrandMin.Value);

                if (filter.DynamicElongation55kgOneStrandMax.HasValue)
                    query = query.Where(r => r.DynamicElongation55kgOneStrand <= filter.DynamicElongation55kgOneStrandMax.Value);

                if (filter.DynamicElongation80kgOneStrandMin.HasValue)
                    query = query.Where(r => r.DynamicElongation80kgOneStrand >= filter.DynamicElongation80kgOneStrandMin.Value);

                if (filter.DynamicElongation80kgOneStrandMax.HasValue)
                    query = query.Where(r => r.DynamicElongation80kgOneStrand <= filter.DynamicElongation80kgOneStrandMax.Value);

                if (filter.DynamicElongation80kgTwoStrandMin.HasValue)
                    query = query.Where(r => r.DynamicElongation80kgTwoStrand >= filter.DynamicElongation80kgTwoStrandMin.Value);

                if (filter.DynamicElongation80kgTwoStrandMax.HasValue)
                    query = query.Where(r => r.DynamicElongation80kgTwoStrand <= filter.DynamicElongation80kgTwoStrandMax.Value);

                if (filter.DropsBeforeBreak55kgOneStrandMin.HasValue)
                    query = query.Where(r => r.DropsBeforeBreak55kgOneStrand >= filter.DropsBeforeBreak55kgOneStrandMin.Value);

                if (filter.DropsBeforeBreak55kgOneStrandMax.HasValue)
                    query = query.Where(r => r.DropsBeforeBreak55kgOneStrand <= filter.DropsBeforeBreak55kgOneStrandMax.Value);

                if (filter.DropsBeforeBreak80kgOneStrandMin.HasValue)
                    query = query.Where(r => r.DropsBeforeBreak80kgOneStrand >= filter.DropsBeforeBreak80kgOneStrandMin.Value);

                if (filter.DropsBeforeBreak80kgOneStrandMax.HasValue)
                    query = query.Where(r => r.DropsBeforeBreak80kgOneStrand <= filter.DropsBeforeBreak80kgOneStrandMax.Value);

                if (filter.DropsBeforeBreak80kgTwoStrandMin.HasValue)
                    query = query.Where(r => r.DropsBeforeBreak80kgTwoStrand >= filter.DropsBeforeBreak80kgTwoStrandMin.Value);

                if (filter.DropsBeforeBreak80kgTwoStrandMax.HasValue)
                    query = query.Where(r => r.DropsBeforeBreak80kgTwoStrand <= filter.DropsBeforeBreak80kgTwoStrandMax.Value);

                if (filter.SheathSlippageMin.HasValue)
                    query = query.Where(r => r.SheathSlippage >= filter.SheathSlippageMin.Value);

                if (filter.SheathSlippageMax.HasValue)
                    query = query.Where(r => r.SheathSlippage <= filter.SheathSlippageMax.Value);

                var queryList = query.ToList();
                var ropeDtos = queryList.Select(x => ToDto(x)).ToList();

                return ropeDtos;
            }
        }

        public void AddRope(RopeDto ropeDto)
        {
            using (var db = _dbContextFactory.CreateDbContext())
            {
                var rope = new Rope();

                UpdateRope(rope, ropeDto);
                db.Ropes.Add(rope);

                db.SaveChanges();
            }
        }

        public void UpdateRope(RopeDto ropeDto)
        {
            using (var db = _dbContextFactory.CreateDbContext())
            {
                var ropes = GetAllRopesQueryable(db);
                var rope = ropes.First(r => r.RopeId == ropeDto.RopeId);

                UpdateRope(rope, ropeDto);

                db.SaveChanges();
            }
        }

        public void AddRopeEditSuggestion(int ropeId, RopeEditSuggestionDto rESDto)
        {
            RopeEditSuggestion rES = new RopeEditSuggestion();
            UpdateRopeEditSuggestion(rES, rESDto);

            using (var db = _dbContextFactory.CreateDbContext())
            {
                var rope = db.Ropes.First(r => r.RopeId == ropeId);

                rope.RopeEditSuggestions.Add(rES);

                db.SaveChanges();
            }

        }

        public void RemoveRope(int ropeId)
        {
            using (var db = _dbContextFactory.CreateDbContext())
            {
                var rope = db.Ropes.FirstOrDefault(r => r.RopeId == ropeId);
                if (rope != null)
                {
                    var rES = db.RopeEditSuggestions.Where(x => x.RopeId == ropeId);
                    db.RopeEditSuggestions.RemoveRange(rES);

                    db.Ropes.Remove(rope);
                    db.SaveChanges();
                }
            }
        }

        public void VerifyRope(int ropeId)
        {
            using (var db = _dbContextFactory.CreateDbContext())
            {
                var rope = db.Ropes.FirstOrDefault(r => r.RopeId == ropeId);
                if (rope != null)
                {
                    rope.Verified = true;
                    db.SaveChanges();
                }
            }
        }

        public void ImplementRopeEditSuggestion(int ropeEditSuggestionId)
        {
            using (var db = _dbContextFactory.CreateDbContext())
            {
                var ropeEditSuggestion = db.RopeEditSuggestions.FirstOrDefault(r => r.RopeEditSuggestionId == ropeEditSuggestionId);
                if (ropeEditSuggestion != null)
                {
                    var rope = db.Ropes.FirstOrDefault(r => r.RopeId == ropeEditSuggestion.RopeId);
                    if (rope != null)
                    {
                        //Update rope with rES.
                        UpdateRope(rope, ropeEditSuggestion);

                        //Delete ropeEditSuggestion
                        db.Remove(ropeEditSuggestion);

                        //Delete all other rES for this parameter??

                        db.SaveChanges();
                    }
                }
            }
        }

        public void DeleteRopeEditSuggestion(int ropeEditSuggestionId)
        {
            using (var db = _dbContextFactory.CreateDbContext())
            {
                var ropeEditSuggestion = db.RopeEditSuggestions.FirstOrDefault(r => r.RopeEditSuggestionId == ropeEditSuggestionId);
                if (ropeEditSuggestion != null)
                {
                    db.Remove(ropeEditSuggestion);
                }

                db.SaveChanges();
            }
        }

        public bool IsRopeNameUnique(int ropeId, string name)
        {
            using (var db = _dbContextFactory.CreateDbContext())
            {
                bool test = db.Ropes.Where(r => r.RopeId != ropeId).Any(r => r.Name == name);
                return !test;
            }
        }


        //-------------------------------
        // Private
        //-------------------------------
        private IQueryable<Rope> GetAllRopesQueryable(DataContext db)
        {
            IQueryable<Rope> ropes = db.Ropes.Include(r => r.Brand).Include(r => r.RopeCalculatedParameterSet);
            return ropes;
        }

        private List<RopeDto> GetRopes_All(bool incRopeEditSuggestions)
        {
            using (var db = _dbContextFactory.CreateDbContext())
            {
                IQueryable<Rope> ropes = GetAllRopesQueryable(db);

                if (incRopeEditSuggestions)
                {
                    ropes = ropes.Include(r => r.RopeEditSuggestions).ThenInclude(x => x.Brand);
                }

                IList<Rope> ropesList = ropes.ToList(); //Realise ropes in memory to perform ToDto.

                var ropeDtos = ropesList.Select(x => ToDto(x)).ToList();

                return ropeDtos;
            }
        }

        private void UpdateRope(Rope rope, RopeDto dto)
        {
            //rope.RopeId = dto.RopeId;

            rope.Name = dto.Name;
            UpdateRopeBrand(rope, dto.Brand.BrandId);
            rope.Category = dto.Category.ToModel();

            rope.Diameter = dto.Diameter;
            rope.MassPerUnitLength = dto.MassPerUnitLength;
            rope.SheathPercentage = dto.SheathPercentage;

            rope.ImpactForce55kgOneStrand = dto.ImpactForce55kgOneStrand;
            rope.ImpactForce80kgOneStrand = dto.ImpactForce80kgOneStrand;
            rope.ImpactForce80kgTwoStrand = dto.ImpactForce80kgTwoStrand;

            rope.StaticElongation80kgOneStrand = dto.StaticElongation80kgOneStrand;
            rope.StaticElongation80kgTwoStrand = dto.StaticElongation80kgTwoStrand;

            rope.DynamicElongation55kgOneStrand = dto.DynamicElongation55kgOneStrand;
            rope.DynamicElongation80kgOneStrand = dto.DynamicElongation80kgOneStrand;
            rope.DynamicElongation80kgTwoStrand = dto.DynamicElongation80kgTwoStrand;

            rope.DropsBeforeBreak55kgOneStrand = dto.DropsBeforeBreak55kgOneStrand;
            rope.DropsBeforeBreak80kgOneStrand = dto.DropsBeforeBreak80kgOneStrand;
            rope.DropsBeforeBreak80kgTwoStrand = dto.DropsBeforeBreak80kgTwoStrand;

            rope.SheathSlippage = dto.SheathSlippage;

            //Calculate CalculatedParameterSet
            if (rope.RopeCalculatedParameterSet == null)
            {
                rope.RopeCalculatedParameterSet = new RopeCalculatedParameterSet();
            }
            _ropeCalculatedParameterSetDataService.UpdateRopeCalculatedParameterSet(rope.RopeCalculatedParameterSet, dto.RopeCalculatedParameterSet);
        }

        private void UpdateRope(Rope rope, RopeEditSuggestion rES)
        {
            if (rES.Name != null) { rope.Name = rES.Name; }
            if (rES.BrandId != null) { UpdateRopeBrand(rope, rES.BrandId.Value); }
            if (rES.Category != null) { rope.Category = rES.Category.Value; }

            if (rES.Diameter != null) { rope.Diameter = rES.Diameter.Value; }
            if (rES.MassPerUnitLength != null) { rope.MassPerUnitLength = rES.MassPerUnitLength.Value; }
            if (rES.SheathPercentage != null) { rope.SheathPercentage = rES.SheathPercentage.Value; }

            if (rES.ImpactForce55kgOneStrand != null) { rope.ImpactForce55kgOneStrand = rES.ImpactForce55kgOneStrand.Value; }
            if (rES.ImpactForce80kgOneStrand != null) { rope.ImpactForce80kgOneStrand = rES.ImpactForce80kgOneStrand.Value; }
            if (rES.ImpactForce80kgTwoStrand != null) { rope.ImpactForce80kgTwoStrand = rES.ImpactForce80kgTwoStrand.Value; }

            if (rES.StaticElongation80kgOneStrand != null) { rope.StaticElongation80kgOneStrand = rES.StaticElongation80kgOneStrand.Value; }
            if (rES.StaticElongation80kgTwoStrand != null) { rope.StaticElongation80kgTwoStrand = rES.StaticElongation80kgTwoStrand.Value; }

            if (rES.DynamicElongation55kgOneStrand != null) { rope.DynamicElongation55kgOneStrand = rES.DynamicElongation55kgOneStrand.Value; }
            if (rES.DynamicElongation80kgOneStrand != null) { rope.DynamicElongation80kgOneStrand = rES.DynamicElongation80kgOneStrand.Value; }
            if (rES.DynamicElongation80kgTwoStrand != null) { rope.DynamicElongation80kgTwoStrand = rES.DynamicElongation80kgTwoStrand.Value; }

            if (rES.DropsBeforeBreak55kgOneStrand != null) { rope.DropsBeforeBreak55kgOneStrand = rES.DropsBeforeBreak55kgOneStrand.Value; }
            if (rES.DropsBeforeBreak80kgOneStrand != null) { rope.DropsBeforeBreak80kgOneStrand = rES.DropsBeforeBreak80kgOneStrand.Value; }
            if (rES.DropsBeforeBreak80kgTwoStrand != null) { rope.DropsBeforeBreak80kgTwoStrand = rES.DropsBeforeBreak80kgTwoStrand.Value; }

            if (rES.SheathSlippage != null) { rope.SheathSlippage = rES.SheathSlippage.Value; }
        }

        private void UpdateRopeBrand(Rope rope, int brandId)
        {
            using (var db = _dbContextFactory.CreateDbContext())
            {
                if (rope.Brand?.BrandId == null || rope.Brand.BrandId != brandId)
                {
                    var brand = db.Brands.Where(b => b.BrandId == brandId).First();
                    //rope.Brand = brand;
                    rope.BrandId = brand.BrandId;
                }
            }
        }

        private void UpdateRopeEditSuggestion(RopeEditSuggestion rES, RopeEditSuggestionDto rESDto)
        {
            rES.RopeEditSuggestionId = rESDto.RopeEditSuggestionId;
            rES.Name = rESDto.Name;
            rES.BrandId = rESDto.BrandId;
            rES.Category = rESDto.Category?.ToModel();

            rES.Diameter = rESDto.Diameter;
            rES.MassPerUnitLength = rESDto.MassPerUnitLength;
            rES.SheathPercentage = rESDto.SheathPercentage;

            rES.ImpactForce55kgOneStrand = rESDto.ImpactForce55kgOneStrand;
            rES.ImpactForce80kgOneStrand = rESDto.ImpactForce80kgOneStrand;
            rES.ImpactForce80kgTwoStrand = rESDto.ImpactForce80kgTwoStrand;

            rES.StaticElongation80kgOneStrand = rESDto.StaticElongation80kgOneStrand;
            rES.StaticElongation80kgTwoStrand = rESDto.StaticElongation80kgTwoStrand;

            rES.DynamicElongation55kgOneStrand = rESDto.DynamicElongation55kgOneStrand;
            rES.DynamicElongation80kgOneStrand = rESDto.DynamicElongation80kgOneStrand;
            rES.DynamicElongation80kgTwoStrand = rESDto.DynamicElongation80kgTwoStrand;

            rES.DropsBeforeBreak55kgOneStrand = rESDto.DropsBeforeBreak55kgOneStrand;
            rES.DropsBeforeBreak80kgOneStrand = rESDto.DropsBeforeBreak80kgOneStrand;
            rES.DropsBeforeBreak80kgTwoStrand = rESDto.DropsBeforeBreak80kgTwoStrand;

            rES.SheathSlippage = rESDto.SheathSlippage;
        }

        private RopeDto ToDto(Rope rope)
        {
            var dto = new RopeDto();

            dto.RopeId = rope.RopeId;
            dto.Name = rope.Name;
            dto.Brand = _brandDataService.ToDto(rope.Brand);
            dto.Category = rope.Category.ToDto();

            dto.Diameter = rope.Diameter;
            dto.MassPerUnitLength = rope.MassPerUnitLength;
            dto.SheathPercentage = rope.SheathPercentage;

            dto.ImpactForce55kgOneStrand = rope.ImpactForce55kgOneStrand;
            dto.ImpactForce80kgOneStrand = rope.ImpactForce80kgOneStrand;
            dto.ImpactForce80kgTwoStrand = rope.ImpactForce80kgTwoStrand;

            dto.StaticElongation80kgOneStrand = rope.StaticElongation80kgOneStrand;
            dto.StaticElongation80kgTwoStrand = rope.StaticElongation80kgTwoStrand;

            dto.DynamicElongation55kgOneStrand = rope.DynamicElongation55kgOneStrand;
            dto.DynamicElongation80kgOneStrand = rope.DynamicElongation80kgOneStrand;
            dto.DynamicElongation80kgTwoStrand = rope.DynamicElongation80kgTwoStrand;

            dto.DropsBeforeBreak55kgOneStrand = rope.DropsBeforeBreak55kgOneStrand;
            dto.DropsBeforeBreak80kgOneStrand = rope.DropsBeforeBreak80kgOneStrand;
            dto.DropsBeforeBreak80kgTwoStrand = rope.DropsBeforeBreak80kgTwoStrand;

            dto.SheathSlippage = rope.SheathSlippage;

            dto.RopeCalculatedParameterSet = _ropeCalculatedParameterSetDataService.ToDto(rope.RopeCalculatedParameterSet);

            dto.RopeEditSuggestions = rope.RopeEditSuggestions.Any() ? rope.RopeEditSuggestions.Select(x => _ropeEditSuggestionDataService.ToDto(x)).ToList() : new List<RopeEditSuggestionDto>();

            return dto;
        }
    }

}
