using API.Context;
using API.Models;
using FleetAPI.DTOs;
using FleetAPI.Enums;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Runtime.InteropServices;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FleetAPIController : ControllerBase
    {
        private readonly FleetContext _context;

        public FleetAPIController(FleetContext context)
        {
            _context = context;
        }

        [HttpGet("GetAllVessels")]
        public async Task<ActionResult<List<Vessel>>> GetAllVessels()
        {
            List<Vessel> vessels = await _context.Vessels.ToListAsync();

            return vessels;
        }

        [HttpGet("{VesselId:int}", Name = "GetVessel")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Vessel>> GetVessel([FromQuery] int id)
        {
            var vessel = await _context.Vessels.FirstOrDefaultAsync(x => x.Id == id);
            //If vessel doesn't exist send back 404 code
            if (vessel == null)
            {
                return NotFound();
            }
            return Ok(vessel);
        }

        [HttpPost("CreateVessel", Name ="CreateVessel")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Vessel?>> CreateVessel([FromBody] VesselDTO dto)
        {
            //User should provide the capacity
            if (dto is null || dto.Capacity <= 0)
            {
                //Add an error to the ModelState, with a custom name and message
                ModelState.AddModelError("CapacityError", "Provide a larger than zero capacity");
                //Send a 400 bad response with the above error
                return BadRequest(ModelState);
            }

            Vessel vessel;

            if (dto.Fleet is null)
            {
                vessel = new Vessel() {Capacity = dto.Capacity };
            }
            else
            {
                vessel = new Vessel() {Capacity = dto.Capacity, Fleet = dto.Fleet };
            }


            _context.Vessels.Add(vessel);
            await _context.SaveChangesAsync();

            return Ok(vessel);
        }

        [HttpGet("{OnlyIds:bool}", Name = "GetContainer")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<Container>>> GetVesselContainersIds(int vesselId)
        {
            if (vesselId <= 0) {
                return BadRequest(ModelState);
            }
            var containers = await _context.Containers.Where(x => x.Vessel.Id == vesselId).Select(x =>x.Id).ToListAsync();
            //If vessel doesn't exist send back 404 code
            if (containers == null)
            {
                return NotFound();
            }
            
            return Ok(containers);
            
        }

        [HttpPost("CreateContainer")]
        public async Task<ActionResult<Container>> CreateContainer()
        {
            var container = new Container();
            _context.Containers.Add(container);
            await _context.SaveChangesAsync();

            return Ok(container);
        }

        [HttpPatch("LoadContainerToVessel")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<bool>> LoadContainerToVessel([FromBody] LoadContainerDTO dto)
        {

            if (dto is null || dto.ContainerId <= 0 || dto.VesselId <= 0)
            {
                return BadRequest();
            }

            var container = await _context.Containers.SingleOrDefaultAsync(x => x.Id == dto.ContainerId);
            var vessel = await _context.Vessels.SingleOrDefaultAsync(x => x.Id == dto.VesselId);

            if (container is null || vessel is null)
            {
                return NotFound();
            }
            else if (vessel.Containers.Contains(container))
            {
                return BadRequest("Vessel already contains container");
            }
            else if (vessel.Containers.Count >= vessel.Capacity)
            {
                return BadRequest("Vessel is already full");
            }

            vessel.Containers.Add(container);
            container.Vessel = vessel;
            await _context.SaveChangesAsync();
            return Ok(vessel.Containers.Last());
        }

        [HttpPatch("RestrictVesselCapacity")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<bool>> RestrictVesselCapacity([FromBody] RestrictCapacityDTO dto)
        {


            if (dto is null || dto.VesselId <= 0 || dto.NewCapacity < 0)
            {
                return BadRequest();
            }

            var vessel = await _context.Vessels.SingleOrDefaultAsync(x => x.Id == dto.VesselId);

            if (vessel is null)
            {
                return NotFound();
            }
            else if (vessel.Containers.Count > dto.NewCapacity)
            {
                return BadRequest("Vessel already contains too many containers");
            }
            else
            {
                vessel.Capacity = dto.NewCapacity;
                await _context.SaveChangesAsync();
                return true;
            }
        }

        [HttpPatch("TransferContainer")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<bool>> TransferContainer([FromBody] TransferContainerDTO dto)
        {
            if (dto is null || dto.FromVesselId <= 0 || dto.ToVesselId <= 0 || dto.ContainerId <= 0)
            {
                return BadRequest();
            }

            var fromVessel = await _context.Vessels.SingleOrDefaultAsync(x => x.Id == dto.FromVesselId);
            var toVessel = await _context.Vessels.SingleOrDefaultAsync(x => x.Id == dto.ToVesselId);
            var container = await _context.Containers.SingleOrDefaultAsync(x => x.Id == dto.ContainerId);

            if (fromVessel is null || toVessel is null || container is null)
            {
                return NotFound("Vessel(s) or container not found");
            }
            else if (!fromVessel.Containers.Contains(container))
            {
                return BadRequest("First Vessel does not contain container");
            }
            else if (toVessel.Containers.Contains(container) || toVessel.Containers.Count == toVessel.Capacity)
            {
                return BadRequest("Second Vessel already contains the container or has reached full capacity");
            }
            else
            {
               
                    fromVessel.Containers.Remove(container);

                    toVessel.Containers.Add(container);
 
                    container.Vessel = toVessel;

                    await _context.SaveChangesAsync();
                    return Ok(container);
            }
        }

        [HttpPatch("AssignFleetToVessel")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<bool>> AssignFleetToVessel([FromBody] AssignFleetDTO dto)
        {
            if (dto is null || dto.VesselId <= 0 || dto.Fleet is null)
            {
                return BadRequest();
            }

            var vessel = await _context.Vessels.SingleOrDefaultAsync(x => x.Id == dto.VesselId);

            if (vessel is null)
            {
                return NotFound("Vessel not found");
            }

            vessel.Fleet = dto.Fleet;
            await _context.SaveChangesAsync();
            return Ok(vessel);
        }
    }
}
