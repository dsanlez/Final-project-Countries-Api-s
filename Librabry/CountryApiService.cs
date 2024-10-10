using Newtonsoft.Json;

namespace Librabry
{
    public class CountryApiService
    {

        public async Task<List<Country>?> GetCountriesAsync(string urlBase, string controller)
        {

            try
            {
                var client = new HttpClient();

                client.BaseAddress = new Uri(urlBase);

                var response = await client.GetAsync(controller);

                var result = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var countries = JsonConvert.DeserializeObject<List<Country>>(result);

                return countries;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");

                return null;
            }
        }
    }
}
