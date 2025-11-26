namespace EquipmentDashboardTT.Models.Helpers
{
    public abstract class WrapperBase<T>
    {
        public T? Value { get; set; }
        public string DisplayName { get; set; }

        protected WrapperBase(T? value, string displayName)
        {
            Value = value;
            DisplayName = displayName;
        }

        public override string ToString()
        {
            return DisplayName;
        }
    }
}
