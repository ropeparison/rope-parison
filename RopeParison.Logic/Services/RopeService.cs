using RopeParison.Data;
using RopeParison.Data.Model;
using RopeParison.Data.Services;
using RopeParison.Protocol;

namespace RopeParison.Logic.Services
{

    public interface IRopeService
    {
        List<RopeDto> GetAllRopes();
        RopeDto GetRopeById(int ropeId);
        RopeDto GetRopeByName(string ropeName);
        List<RopeDto> GetFilteredRopes(RopeFilterDto filter);
        void AddRope(RopeDto rope);
        void UpdateRope(RopeDto rope);
        void RemoveRope(int ropeId);
        bool IsRopeNameUnique(string name);
    }

    public class RopeService : IRopeService
    {
        private readonly IRopeDataService _ropeDataService;

        public RopeService(IRopeDataService ropeDataService)
        {
            _ropeDataService = ropeDataService;
        }
        //-------------------------------------------------------------------------

        public List<RopeDto> GetAllRopes()
        {
            return _ropeDataService.GetAllRopes();
        }

        public RopeDto GetRopeById(int ropeId)
        {
           return _ropeDataService.GetRopeById(ropeId);
        }

        public RopeDto GetRopeByName(string ropeName)
        {
            return _ropeDataService.GetRopeByName(ropeName);
        }

        public void AddRope(RopeDto ropeDto)
        {
            //Do calcs
            _ropeDataService.AddRope(ropeDto);
        }

        public void UpdateRope(RopeDto ropeDto)
        {
            //Do calcs
            _ropeDataService.UpdateRope(ropeDto);
        }

        public void RemoveRope(int ropeId)
        {
            _ropeDataService.RemoveRope(ropeId);
        }


        public List<RopeDto> GetFilteredRopes(RopeFilterDto filter)
        {
            return _ropeDataService.GetFilteredRopes(filter);
        }

        public bool IsRopeNameUnique(string name)
        {
            return _ropeDataService.IsRopeNameUnique(name);
        }

    }
}
