using RopeParison.Data;
using RopeParison.Data.Model;
using Microsoft.EntityFrameworkCore;
using RopeParison.Protocol;

namespace RopeParison.Data.Services
{

    public interface IRopeCalculatedParameterSetDataService
    {
        void AddRopeCalculatedParameterSet(int ropeId);
        //void AddRopeCalculatedParameterSet(RopeCalculatedParameterSetDto ropeCalculatedParameterSetDto);
        void RemoveRopeCalculatedParameterSet(int ropeCalculatedParameterSetId);
        //void UpdateRopeCalculatedParameterSet(RopeDto rope);
        void UpdateRopeCalculatedParameterSet(RopeCalculatedParameterSet ropeSet, RopeCalculatedParameterSetDto dtoSet);

        RopeCalculatedParameterSetDto ToDto(RopeCalculatedParameterSet ropeCalculatedParameterSet);
    }

    public class RopeCalculatedParameterSetDataService : IRopeCalculatedParameterSetDataService
    {
        private IDbContextFactory<DataContext> _dbContextFactory;

        public RopeCalculatedParameterSetDataService(IDbContextFactory<DataContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public void AddRopeCalculatedParameterSet(int ropeId)
        {
            using (var db = _dbContextFactory.CreateDbContext())
            {
                var ropeCalculatedParameterSet = new RopeCalculatedParameterSet();
                ropeCalculatedParameterSet.RopeId = ropeId;

                db.RopeCalculatedParameterSets.Add(ropeCalculatedParameterSet);
                db.SaveChanges();
            }
        }

        /*public void AddRopeCalculatedParameterSet(RopeCalculatedParameterSetDto ropeCalculatedParameterSetDto)
        {
            using(var db = _dbContextFactory.CreateDbContext())
            {
                var ropeCalculatedParameterSet = new RopeCalculatedParameterSet();
                UpdateRopeCalculatedParameterSet(ropeCalculatedParameterSet, ropeCalculatedParameterSetDto);

                db.RopeCalculatedParameterSets.Add(ropeCalculatedParameterSet);
                db.SaveChanges();
            }
        }*/

        public void RemoveRopeCalculatedParameterSet(int ropeCalculatedParameterSetId)
        {
            using(var db =_dbContextFactory.CreateDbContext())
            {
                var ropeCalculatedParameterSet = db.RopeCalculatedParameterSets.FirstOrDefault(r => r.RopeCalculatedParameterSetId == ropeCalculatedParameterSetId);
                if (ropeCalculatedParameterSet != null)
                {
                    db.RopeCalculatedParameterSets.Remove(ropeCalculatedParameterSet);
                    db.SaveChanges();
                }
            }
        }

        public void UpdateRopeCalculatedParameterSet(RopeCalculatedParameterSet ropeSet, RopeCalculatedParameterSetDto dtoSet)
        {
            //Calculate CalculatedParameterSet
            if (ropeSet != null && dtoSet != null)
            {
                ropeSet.Area = dtoSet.Area;
                ropeSet.SheathArea = dtoSet.SheathArea;
                ropeSet.CoreArea = dtoSet.CoreArea;
                ropeSet.CoreDiameter = dtoSet.CoreDiameter;
                ropeSet.SheathThickness = dtoSet.SheathThickness;
                ropeSet.Density = dtoSet.Density;
            }
        }

        public RopeCalculatedParameterSetDto ToDto(RopeCalculatedParameterSet ropeCalculatedParameterSet)
        {
            var dto = new RopeCalculatedParameterSetDto();

            if (ropeCalculatedParameterSet != null)
            {

                dto.RopeCalculatedParameterSetId = ropeCalculatedParameterSet.RopeCalculatedParameterSetId;

                dto.Area = ropeCalculatedParameterSet.Area;
                dto.SheathArea = ropeCalculatedParameterSet.SheathArea;
                dto.SheathThickness = ropeCalculatedParameterSet.SheathThickness;
                dto.CoreArea = ropeCalculatedParameterSet.CoreArea;
                dto.CoreDiameter = ropeCalculatedParameterSet.CoreDiameter;
                dto.CoreDiameter = ropeCalculatedParameterSet.CoreDiameter;
                dto.Density = ropeCalculatedParameterSet.Density;

                dto.RopeID = ropeCalculatedParameterSet.RopeId;
            }

            return dto;
        }

    }

}
