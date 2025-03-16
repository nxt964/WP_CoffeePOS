using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CoffeePOS.Services;

public class FirebaseService
{
    private readonly HttpClient _httpClient;
    private readonly string _databaseUrl = "https://coffeepos-2fe38-default-rtdb.asia-southeast1.firebasedatabase.app/";

    public FirebaseService()
    {
        _httpClient = new HttpClient();
    }

    // Write data to Firebase (PUT)
    public async Task<bool> WriteDataAsync(string path, object data)
    {
        try
        {
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var url = $"{_databaseUrl}/{path}.json";

            var response = await _httpClient.PutAsync(url, content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error writing to Firebase: {ex.Message}");
            return false;
        }
    }

    // Read data from Firebase (GET)
    public async Task<T> ReadDataAsync<T>(string path)
    {
        try
        {
            var url = $"{_databaseUrl}/{path}.json";
            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(json);
            }
            return default;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading from Firebase: {ex.Message}");
            return default;
        }
    }
}