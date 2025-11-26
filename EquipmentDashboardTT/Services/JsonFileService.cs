using EquipmentDashboardTT.Models;
using Newtonsoft.Json;
using System.IO;

namespace EquipmentDashboardTT.Services
{
    public interface IFileService
    {
        Task<List<Device>> ReadFromFileAsync(string filePath);
        Task WriteToFileAsync(string filePath, List<Device> data);
    }

    public class JsonFileService : IFileService
    {
        public async Task<List<Device>> ReadFromFileAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return [];
            }
            string json = await File.ReadAllTextAsync(filePath);
            return JsonConvert.DeserializeObject<List<Device>>(json) ?? [];
        }

        public async Task WriteToFileAsync(string filePath, List<Device> data)
        {
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            await File.WriteAllTextAsync(filePath, json);
        }
    }
}
