namespace EquipmentDashboardTT.Models.Helpers
{
    public class DeviceStatusWrapper : WrapperBase<Device.DeviceStatus?>
    {
        public DeviceStatusWrapper(Device.DeviceStatus? status, string displayName)
            : base(status, displayName)
        {
        }
    }
}
