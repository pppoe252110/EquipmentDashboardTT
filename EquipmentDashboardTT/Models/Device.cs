using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.ComponentModel;

namespace EquipmentDashboardTT.Models
{
    public class Device : IDataErrorInfo
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum DeviceStatus
        {
            Active,
            Inactive,
            Maintenance,
            Working,
            Faulty,
            Decommissioned
        }

        public Guid Id { get; set; } = Guid.NewGuid();
        public string? Name { get; set; }
        public string? Category { get; set; }
        public string? SerialNumber { get; set; }
        public DateTime InstallationDate { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public DeviceStatus Status { get; set; }
        public string Error => string.Empty;

        public string this[string columnName]
        {
            get
            {
                string error = string.Empty;
                switch (columnName)
                {
                    case nameof(Name):
                        if (string.IsNullOrWhiteSpace(Name))
                            error = "Название устройства не может быть пустым";
                        else if (Name.Trim().Length < 2)
                            error = "Название устройства должно содержать 2 символа";
                        break;

                    case nameof(SerialNumber):
                        if (string.IsNullOrWhiteSpace(SerialNumber))
                            error = "Серийный номер не может быть пустым";
                        else if (SerialNumber.Trim().Length < 3)
                            error = "Серийный номер должен содержать 3 символа";
                        break;

                    case nameof(Category):
                        if (string.IsNullOrWhiteSpace(Category))
                            error = "Категория не может быть пустой";
                        else if (Category.Trim().Length < 2)
                            error = "Категория должна содержать 2 символа";
                        break;

                    case nameof(InstallationDate):
                        if (InstallationDate > DateTime.Today)
                            error = "Дата установки не может быть в будущем";
                        else if (InstallationDate < DateTime.Today.AddYears(-50))
                            error = "Дата установки не может быть более 50 лет назад";
                        break;
                }
                return error;
            }
        }

        public bool IsValid()
        {
            return string.IsNullOrEmpty(this[nameof(Name)]) &&
                   string.IsNullOrEmpty(this[nameof(SerialNumber)]) &&
                   string.IsNullOrEmpty(this[nameof(Category)]) &&
                   string.IsNullOrEmpty(this[nameof(InstallationDate)]);
        }
    }
}