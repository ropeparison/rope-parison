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
        void UpdateRopeCalculatedParameterSet(Rope rope);

        RopeCalculatedParameterSetDto ToDto(RopeCalculatedParameterSet ropeCalculatedParameterSet);
    }

    public class RopeCalculatedParameterSetDataService : IRopeCalculatedParameterSetDataService
    {
        private IDbContextFactory<Context> _dbContextFactory;

        public RopeCalculatedParameterSetDataService(IDbContextFactory<Context> dbContextFactory)
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

        public void UpdateRopeCalculatedParameterSet(Rope rope)
        {
            //Calculate CalculatedParameterSet
            if (rope.RopeCalculatedParameterSet != null)
            {
                var ropeArea = Math.PI * Math.Pow((rope.Diameter / 2), 2);

                rope.RopeCalculatedParameterSet.Area = ropeArea;
                rope.RopeCalculatedParameterSet.SheathArea = ropeArea * rope.SheathPercentage / 100;

                var coreArea = ropeArea * (1 - rope.SheathPercentage / 100);
                rope.RopeCalculatedParameterSet.CoreArea = coreArea;
                var coreDiameter = Math.Sqrt(coreArea / Math.PI) * 2;
                rope.RopeCalculatedParameterSet.CoreDiameter = coreDiameter;

                rope.RopeCalculatedParameterSet.SheathThickness = rope.Diameter - coreDiameter;
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

                dto.RopeID = ropeCalculatedParameterSet.RopeId;
            }

            return dto;
        }

    }

}
