using EquipmentDashboardTT.Commands;
using EquipmentDashboardTT.Models;
using EquipmentDashboardTT.Models.Helpers;
using EquipmentDashboardTT.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace EquipmentDashboardTT.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IFileService _fileService;
        private readonly string _filePath = "devices.json";

        public ObservableCollection<Device> Devices
        {
            get => _devices;
            set => Set(ref _devices, value);
        }
        private ObservableCollection<Device> _devices;

        public ICollectionView DevicesView
        {
            get => _devicesView;
            set => Set(ref _devicesView, value);
        }
        private ICollectionView _devicesView;

        private Device _selectedDevice;
        public Device SelectedDevice
        {
            get => _selectedDevice;
            set
            {
                if (Set(ref _selectedDevice, value))
                {

                    if (_selectedDevice != null)
                    {
                        _originalSerialNumber = _selectedDevice.SerialNumber;
                        _selectedDeviceId = _selectedDevice.Id;
                    }
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (Set(ref _searchText, value))
                {
                    DevicesView?.Refresh();
                }
            }
        }
        private string _searchText;

        public DeviceStatusWrapper SelectedStatusFilter
        {
            get => _selectedStatusFilter;
            set
            {
                if (Set(ref _selectedStatusFilter, value))
                {
                    DevicesView?.Refresh();
                }
            }
        }
        private DeviceStatusWrapper _selectedStatusFilter;

        private string _originalSerialNumber;
        private Guid _selectedDeviceId;

        public ObservableCollection<DeviceStatusWrapper> StatusFilters { get; } = new()
        {
            new DeviceStatusWrapper(null, "Все статусы"),
            new DeviceStatusWrapper(Device.DeviceStatus.Working, "Работает"),
            new DeviceStatusWrapper(Device.DeviceStatus.Faulty, "Сломан"),
            new DeviceStatusWrapper(Device.DeviceStatus.Maintenance, "На обслуживании"),
            new DeviceStatusWrapper(Device.DeviceStatus.Decommissioned, "Списан"),
            new DeviceStatusWrapper(Device.DeviceStatus.Active, "Активен"),
            new DeviceStatusWrapper(Device.DeviceStatus.Inactive, "Неактивен")
        };


        public ObservableCollection<DeviceStatusWrapper> DeviceStatuses { get; } = new()
        {
            new DeviceStatusWrapper(Device.DeviceStatus.Working, "Работает"),
            new DeviceStatusWrapper(Device.DeviceStatus.Faulty, "Сломан"),
            new DeviceStatusWrapper(Device.DeviceStatus.Maintenance, "На обслуживании"),
            new DeviceStatusWrapper(Device.DeviceStatus.Decommissioned, "Списан"),
            new DeviceStatusWrapper(Device.DeviceStatus.Active, "Активен"),
            new DeviceStatusWrapper(Device.DeviceStatus.Inactive, "Неактивен")
        };

        public ICommand AddDeviceCommand { get; }
        public ICommand DeleteDeviceCommand { get; }
        public ICommand SaveDeviceCommand { get; }

        public MainViewModel()
        {
            _fileService = new JsonFileService();

            Devices = [];
            DevicesView = CollectionViewSource.GetDefaultView(Devices);
            DevicesView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(Device.Category)));
            DevicesView.Filter = FilterDevices;
            DevicesView.SortDescriptions.Add(new SortDescription(nameof(Device.Name), ListSortDirection.Ascending));


            SelectedStatusFilter = StatusFilters[0];

            AddDeviceCommand = new RelayCommand(_ => ShowAddDeviceDialog());
            DeleteDeviceCommand = new RelayCommand(_ => DeleteDevice(), _ => SelectedDevice != null);
            SaveDeviceCommand = new RelayCommand(async _ => await SaveDevicesAsync(), _ => CanSaveDevices());

            LoadDataAsync();
        }

        private async void LoadDataAsync()
        {
            try
            {
                var devices = await _fileService.ReadFromFileAsync(_filePath);
                foreach (var device in devices)
                {
                    Devices.Add(device);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task SaveDevicesAsync()
        {
            try
            {
                var duplicateSerialNumbers = Devices
                    .GroupBy(d => d.SerialNumber?.ToLowerInvariant())
                    .Where(g => !string.IsNullOrEmpty(g.Key) && g.Count() > 1)
                    .ToList();

                if (duplicateSerialNumbers.Any())
                {
                    var errorMessage = "Обнаружены дубликаты серийных номеров:\n";
                    foreach (var group in duplicateSerialNumbers)
                    {
                        var deviceNames = string.Join(", ", group.Select(d => d.Name));
                        errorMessage += $"\nСерийный номер '{group.Key}': {deviceNames}";
                    }

                    MessageBox.Show(errorMessage, "Ошибка валидации",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }


                var invalidDevices = Devices.Where(d => !d.IsValid()).ToList();
                if (invalidDevices.Any())
                {
                    var invalidDeviceNames = string.Join(", ", invalidDevices.Select(d => d.Name));
                    MessageBox.Show($"Следующие устройства содержат ошибки: {invalidDeviceNames}\nПожалуйста, исправьте ошибки перед сохранением.",
                        "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                await _fileService.WriteToFileAsync(_filePath, [.. Devices]);
                MessageBox.Show("Изменения сохранены!", "Успех",
                    MessageBoxButton.OK);

                if (SelectedDevice != null)
                {
                    _originalSerialNumber = SelectedDevice.SerialNumber;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanSaveDevices()
        {
            return Devices.All(d => d.IsValid());
        }

        private void DeleteDevice()
        {
            if (SelectedDevice == null) return;

            var result = MessageBox.Show($"Вы уверены, что хотите удалить устройство '{SelectedDevice.Name}'?",
                                         "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Devices.Remove(SelectedDevice);
                SaveDevicesAsync();
            }
        }

        public bool IsSerialNumberUnique(string serialNumber, Guid? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(serialNumber))
                return true;

            return !Devices.Any(d =>
                d.SerialNumber?.Equals(serialNumber, StringComparison.OrdinalIgnoreCase) == true &&
                (excludeId == null || d.Id != excludeId));
        }

        public bool ValidateSerialNumberForSelectedDevice()
        {
            if (SelectedDevice == null || string.IsNullOrWhiteSpace(SelectedDevice.SerialNumber))
                return true;

            if (!IsSerialNumberUnique(SelectedDevice.SerialNumber, _selectedDeviceId))
            {
                SelectedDevice.SerialNumber = _originalSerialNumber;
                OnPropertyChanged(nameof(SelectedDevice));

                MessageBox.Show("Устройство с таким серийным номером уже существует", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void ShowAddDeviceDialog()
        {
            var addViewModel = new AddDeviceViewModel();
            var addWindow = new Views.AddDeviceWindow
            {
                DataContext = addViewModel,
                Owner = Application.Current.MainWindow
            };

            addViewModel.CloseAction = (result) =>
            {
                if (result && addViewModel.NewDevice != null)
                {

                    if (IsSerialNumberUnique(addViewModel.NewDevice.SerialNumber))
                    {
                        Devices.Add(addViewModel.NewDevice);
                        SelectedDevice = addViewModel.NewDevice;
                        _ = SaveDevicesAsync();
                    }
                    else
                    {
                        MessageBox.Show("Устройство с таким серийным номером уже существует", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                addWindow.Close();
            };

            addWindow.ShowDialog();
        }

        private bool FilterDevices(object item)
        {
            if (item is not Device device) return false;

            var matchesSearch = string.IsNullOrWhiteSpace(SearchText) ||
                               (device.Name?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true) ||
                               (device.SerialNumber?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true) ||
                               (device.Category?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true);

            var matchesStatus = SelectedStatusFilter?.Value == null ||
                               device.Status == SelectedStatusFilter.Value;

            return matchesSearch && matchesStatus;
        }
    }
}