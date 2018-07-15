using Lynk.IoT.Gateway.Contracts.Interfaces;
using Lynk.IoT.Gateway.Persistent.Models;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace Lynk.IoT.Gateway.Services
{
    public class SettingsService : ISettingsService
    {
        private IRepository _repository;

        public SettingsService(IRepository repository)
        {
            _repository = repository;
        }

        public decimal GetDecimal(string key)
        {
            var setting = GetSetting(key);
            decimal value = 0M;
            decimal.TryParse(setting?.Value, out value);
            return value;
        }

        public int GetInt(string key)
        {
            var setting = GetSetting(key);
            int value = 0;
            int.TryParse(setting?.Value, out value);
            return value;
        }

        public T GetObject<T>(string key) where T : class
        {

            var value = JsonConvert.DeserializeObject<T>(GetSetting(key)?.Value ?? "{}");
            return value;
        }

        Setting GetSetting(string key)
        {
            var setting = _repository.Get<Setting>().Where(x => x.Key.Equals(key, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (setting == null)
                return null;
            return setting;
        }

        public string GetString(string key)
        {
            return GetSetting(key)?.Value;
        }

        public async void SetDecimal(string key, decimal value)
        {
            await AddOrUpdateSettingsAsync(key, value.ToString());
        }

        public async void SetInt(string key, int value)
        {
            await AddOrUpdateSettingsAsync(key, value.ToString());
        }

        private async System.Threading.Tasks.Task AddOrUpdateSettingsAsync(string key, string value)
        {
            var setting = GetSetting(key);
            if (setting == null)
                await _repository.AddEntityAsync(new Setting { Key = key, Value = value.ToString() });
            else
            {
                setting.Value = value;
                await _repository.UpdateEntityAsync(setting);
            }
        }

        public async void SetObject<T>(string key, T value) where T : class
        {
            await AddOrUpdateSettingsAsync(key, JsonConvert.SerializeObject(value));
        }

        public async void SetString(string key, string value)
        {
            await AddOrUpdateSettingsAsync(key, value);
        }
    }
}
