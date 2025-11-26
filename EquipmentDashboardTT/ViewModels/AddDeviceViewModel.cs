using EquipmentDashboardTT.Commands;
using EquipmentDashboardTT.Models;
using EquipmentDashboardTT.Models.Helpers;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace EquipmentDashboardTT.ViewModels
{
    public class AddDeviceViewModel : ViewModelBase
    {
        private Device? _newDevice;

        public Device? NewDevice
        {
            get => _newDevice;
            set => Set(ref _newDevice, value);
        }

        public ObservableCollection<DeviceStatusWrapper> DeviceStatuses { get; } = new()
        {
            new DeviceStatusWrapper(Device.DeviceStatus.Working, "Работает"),
            new DeviceStatusWrapper(Device.DeviceStatus.Faulty, "Сломан"),
            new DeviceStatusWrapper(Device.DeviceStatus.Maintenance, "На обслуживании"),
            new DeviceStatusWrapper(Device.DeviceStatus.Decommissioned, "Списан"),
            new DeviceStatusWrapper(Device.DeviceStatus.Active, "Активен"),
            new DeviceStatusWrapper(Device.DeviceStatus.Inactive, "Неактивен")
        };

        private DeviceStatusWrapper _selectedStatus;
        public DeviceStatusWrapper SelectedStatus
        {
            get => _selectedStatus;
            set
            {
                if (Set(ref _selectedStatus, value) && NewDevice != null)
                {
                    NewDevice.Status = value.Value ?? Device.DeviceStatus.Working;
                }
            }
        }

        public Action<bool>? CloseAction { get; set; }

        public ICommand OkCommand { get; }
        public ICommand CancelCommand { get; }

        public AddDeviceViewModel()
        {
            NewDevice = new Device
            {
                InstallationDate = DateTime.Today,
                Status = Device.DeviceStatus.Working
            };


            SelectedStatus = DeviceStatuses.FirstOrDefault(x => x.Value == Device.DeviceStatus.Working)
                          ?? DeviceStatuses[0];

            OkCommand = new RelayCommand(
                execute: _ => ExecuteOkCommand(),
                canExecute: _ => CanExecuteOkCommand()
            );
            CancelCommand = new RelayCommand(_ => CloseWindow(false));


            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(NewDevice) ||
                    e.PropertyName == nameof(SelectedStatus))
                {
                    CommandManager.InvalidateRequerySuggested();
                }
            };
        }

        private void ExecuteOkCommand()
        {
            if (ValidateDevice())
            {
                CloseWindow(true);
            }
        }

        private bool ValidateDevice()
        {
            if (NewDevice == null) return false;

            if (!NewDevice.IsValid())
            {
                var error = NewDevice[nameof(NewDevice.Name)] ??
                           NewDevice[nameof(NewDevice.SerialNumber)] ??
                           NewDevice[nameof(NewDevice.Category)] ??
                           NewDevice[nameof(NewDevice.InstallationDate)];

                if (!string.IsNullOrEmpty(error))
                {
                    MessageBox.Show(error, "Ошибка валидации",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
            }

            if (!IsSerialNumberUnique(NewDevice.SerialNumber))
            {
                MessageBox.Show("Устройство с таким серийным номером уже существует", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private bool CanExecuteOkCommand()
        {
            return NewDevice != null &&
                   !string.IsNullOrWhiteSpace(NewDevice.Name?.Trim()) &&
                   !string.IsNullOrWhiteSpace(NewDevice.SerialNumber?.Trim()) &&
                   !string.IsNullOrWhiteSpace(NewDevice.Category?.Trim()) &&
                   SelectedStatus != null;
        }

        private bool IsSerialNumberUnique(string serialNumber)
        {
            var mainWindow = Application.Current.Windows.OfType<Window>()
                .FirstOrDefault(w => w is MainWindow);

            if (mainWindow?.DataContext is MainViewModel mainViewModel)
            {
                return mainViewModel.IsSerialNumberUnique(serialNumber);
            }

            return true;
        }

        private void CloseWindow(bool result)
        {
            CloseAction?.Invoke(result);
        }
    }
}