namespace CleanroomMonitoring.Web.ViewModels
{
    public class CleanRoomDetailsViewModel
    {
        public int RoomID { get; set; }
        public string RoomName { get; set; }
        public string CleanRoomClass { get; set; }
        public string Area { get; set; }
        public string Description { get; set; }
        public List<SensorViewModel> Sensors { get; set; } = new List<SensorViewModel>();
    }
}
