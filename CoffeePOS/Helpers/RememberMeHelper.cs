using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;



namespace CoffeePOS.Helpers
{
    public class RememberMeData
    {
        public string Username
        {
            get; set;
        }
        public string Password
        {
            get; set;
        } // Hoặc chỉ lưu Username nếu không muốn lưu Password
    }

    public static class RememberMeHelper
    {
        private static readonly string FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "remember_me.json");

        public static async Task SaveAsync(RememberMeData data)
        {
            var json = JsonSerializer.Serialize(data);
            await File.WriteAllTextAsync(FilePath, json);
        }

        public static async Task<RememberMeData> LoadAsync()
        {
            if (!File.Exists(FilePath))
                return null;

            var json = await File.ReadAllTextAsync(FilePath);
            return JsonSerializer.Deserialize<RememberMeData>(json);
        }

        public static void Delete()
        {
            if (File.Exists(FilePath))
                File.Delete(FilePath);
        }
    }
}
