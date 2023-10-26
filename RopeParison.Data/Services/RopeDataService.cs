using RopeParison.Data;
using RopeParison.Data.Model;
using Microsoft.EntityFrameworkCore;
using RopeParison.Protocol;

namespace RopeParison.Data.Services
{

    public interface IRopeDataService
    {
        List<RopeDto> GetAllRopes();
        RopeDto GetRopeById(int ropeId);
        RopeDto GetRopeByName(string ropeName);
        List<RopeDto> GetFilteredRopes(RopeFilterDto filter);
        void AddRope(RopeDto rope);
        void UpdateRope(RopeDto rope);
        void RemoveRope(int ropeId);
        bool IsRopeNameUnique(string name);
        RopeDto ToDto(Rope rope);
        void UpdateRope(Rope rope, RopeDto ropeDto);
    }

    public class RopeDataService : IRopeDataService
    {
        private IDbContextFactory<Context> _dbContextFactory;
        private IRopeCalculatedParameterSetDataService _ropeCalculatedParameterSetDataService;
        private IBrandDataService _brandDataService;

        public RopeDataService(IDbContextFactory<Context> dbContextFactory,
            IRopeCalculatedParameterSetDataService ropeCalculatedParameterSetDataService,
            IBrandDataService brandDataService)
        {
            _dbContextFactory = dbContextFactory;
            _ropeCalculatedParameterSetDataService = ropeCalculatedParameterSetDataService;
            _brandDataService = brandDataService;
        }

        public List<RopeDto> GetAllRopes()
        {
            using (var db = _dbContextFactory.CreateDbContext())
            {
                IQueryable<Rope> ropes = db.Ropes.Include(r => r.Brand).Include(r => r.RopeCalculatedParameterSet);
                IList<Rope> ropesList = ropes.ToList(); //Realise ropes in memory to perform ToDto.

                var ropeDtos = ropesList.Select(x => ToDto(x)).ToList();

                return ropeDtos;
            }
        }

        public RopeDto GetRopeById(int ropeId)
        {
            using (var db = _dbContextFactory.CreateDbContext())
            {
                var rope = db.Ropes.Include(r => r.Brand).FirstOrDefault(r => r.RopeId == ropeId);
                if (rope == null)
                {
                    rope = new Rope();
                    rope.RopeId = 0;
                }
                return ToDto(rope);
            }
        }

        public RopeDto GetRopeByName(string ropeName)
        {
            using (var db = _dbContextFactory.CreateDbContext())
            {
                var rope = db.Ropes.Include(r => r.Brand).FirstOrDefault(r => r.Name == ropeName);
                if (rope == null)
                {
                    rope = new Rope();
                    rope.RopeId= 0;
                }
                return ToDto(rope);
            }
        }

        public void AddRope(RopeDto ropeDto)
        {
            using(var db = _dbContextFactory.CreateDbContext())
            {
                var rope = new Rope();
                //UpdateRopeBrand(rope, ropeDto); //Rope requires brand to satisfy foreign key.

                //db.Ropes.Add(rope);
                //db.SaveChanges();

                //_ropeCalculatedParameterSetDataService.AddRopeCalculatedParameterSet(rope.RopeId);

                UpdateRope(rope, ropeDto);
                db.Ropes.Add(rope);

                db.SaveChanges();
            }
        }

        public void UpdateRope(RopeDto ropeDto)
        {
            using (var db = _dbContextFactory.CreateDbContext())
            {
                var rope = db.Ropes.First(r => r.RopeId == ropeDto.RopeId);

                UpdateRope(rope, ropeDto);

                db.SaveChanges();
            }
        }

        public void RemoveRope(int ropeId)
        {
            using(var db =_dbContextFactory.CreateDbContext())
            {
                var rope = db.Ropes.FirstOrDefault(r => r.RopeId == ropeId);
                if (rope != null)
                {
                    db.Ropes.Remove(rope);
                    db.SaveChanges();
                }
            }
        }



        public List<RopeDto> GetFilteredRopes(RopeFilterDto filter)
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

        public bool IsRopeNameUnique(string name)
        {
            using (var db = _dbContextFactory.CreateDbContext())
            {
                bool test = db.Ropes.Include(r => r.Brand).Any(r => r.Name == name);
                return !test;
            }
        }


        public void UpdateRope(Rope rope, RopeDto dto)
        {
            //rope.RopeId = dto.RopeId;

            rope.Name = dto.Name;
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
            _ropeCalculatedParameterSetDataService.UpdateRopeCalculatedParameterSet(rope);

            UpdateRopeBrand(rope, dto);

        }

        public void UpdateRopeBrand(Rope rope, RopeDto dto)
        {
            using (var db = _dbContextFactory.CreateDbContext())
            {
                if (rope.Brand?.BrandId == null || rope.Brand.BrandId != dto.Brand.BrandId)
                {
                    var brand = db.Brands.Where(b => b.BrandId == dto.Brand.BrandId).First();
                    //rope.Brand = brand;
                    rope.BrandId = brand.BrandId;
                }
            }

        }

        public RopeDto ToDto(Rope rope)
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

            return dto;
        }

    }

}
