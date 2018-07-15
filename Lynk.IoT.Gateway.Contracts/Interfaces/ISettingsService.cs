namespace Lynk.IoT.Gateway.Contracts.Interfaces
{
    public interface ISettingsService
    {
        int GetInt(string key);
        void SetInt(string key, int value);

        decimal GetDecimal(string key);
        void SetDecimal(string key, decimal value);

        string GetString(string key);
        void SetString(string key, string value);

        T GetObject<T>(string key) where T : class;
        void SetObject<T>(string key, T value) where T : class;
    }
}
