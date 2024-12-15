namespace FleetAPI.DTOs
{
    public class TransferContainerDTO
    {
        public int FromVesselId { get; set; }
        public int ToVesselId { get; set; }
        public int ContainerId { get; set; }
    }
}
