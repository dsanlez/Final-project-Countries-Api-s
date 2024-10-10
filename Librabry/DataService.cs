using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Data.SQLite;

namespace Librabry
{
    public class DataService
    {
        private SQLiteConnection connection;
        private SQLiteCommand command;

        /// <summary>
        /// Cria a pasta de dados se não existir e configura a conexão à base de dados SQLite.
        /// Se a base de dados ainda não existir, cria uma nova tabela para armazenar informações sobre países.
        /// Abre a conexão à base de dados
        /// </summary>
        public DataService()
        {
            if (!Directory.Exists("Data"))
            {
                Directory.CreateDirectory("Data");
            }

            var path = @"Data\Countries.sqlite";

            try
            {
                connection = new SQLiteConnection("Data Source=" + path);
                connection.Open();

                string sqlCommand = @"
                    CREATE TABLE IF NOT EXISTS Paises (
                    Name TEXT,
                    Capital TEXT,
                    Region TEXT,
                    Subregion TEXT,
                    Population INTEGER,
                    Gini TEXT,
                    Languages TEXT,
                    Area REAL,
                    Independent TEXT,
                    Status TEXT,
                    UnMember TEXT
                )";

                command = new SQLiteCommand(sqlCommand, connection);
                command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Erro a inicar a base de dados", e.Message);
            }
        }

        /// <summary>
        /// Guarda os países na base de dados
        /// </summary>
        /// <param name="pais"></param>
        public void SaveData(Country pais)
        {
            try
            {
                string giniJson = JsonConvert.SerializeObject(pais.Gini);
                string languagesJson = JsonConvert.SerializeObject(pais.Languages);
                string capital = string.Join(";", pais.Capital);

                string sql = @"
                    INSERT INTO Paises (Name, Capital, Region, Subregion, Population, Gini, Languages, Area, Independent, Status, UnMember)
                    VALUES (@Name, @Capital, @Region, @Subregion, @Population, @Gini, @Languages, @Area, @Independent, @Status, @UnMember)";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Name", pais.Name.Common);
                    command.Parameters.AddWithValue("@Capital", capital);
                    command.Parameters.AddWithValue("@Region", pais.Region);
                    command.Parameters.AddWithValue("@Subregion", pais.Subregion);
                    command.Parameters.AddWithValue("@Population", pais.Population);
                    command.Parameters.AddWithValue("@Gini", giniJson);
                    command.Parameters.AddWithValue("@Languages", languagesJson);
                    command.Parameters.AddWithValue("@Area", pais.Area);
                    command.Parameters.AddWithValue("@Independent", pais.Independent);
                    command.Parameters.AddWithValue("@Status", pais.Status);
                    command.Parameters.AddWithValue("@UnMember", pais.UnMember);

                    command.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Erro ao gravar os dados", e.Message);
            }

        }

        /// <summary>
        /// Carrega os países da base de dados
        /// </summary>
        /// <returns></returns>
        public List<Country> getData()
        {
            List<Country> Paises = new List<Country>();
            try
            {
                string sql = "SELECT Name, Capital, Region, Subregion, Population, Gini, Languages, Area, Independent, Status, UnMember FROM Paises";
                using (var command = new SQLiteCommand(sql, connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Paises.Add(new Country
                            {
                                Name = new CountryName { Common = reader["Name"].ToString() },
                                Capital = reader["Capital"] != DBNull.Value ? reader["Capital"].ToString().Split(';').ToList() : new List<string>(),
                                Region = reader["Region"] != DBNull.Value ? reader["Region"].ToString() : "N/A",
                                Subregion = reader["Subregion"] != DBNull.Value ? reader["Subregion"].ToString() : "N/A",
                                Population = reader["Population"] != DBNull.Value ? Convert.ToInt32(reader["Population"]) : 0,
                                Gini = reader["Gini"] != DBNull.Value ? JsonConvert.DeserializeObject<Dictionary<string, double>>(reader["Gini"].ToString()) : new Dictionary<string, double>(),
                                Languages = reader["Languages"] != DBNull.Value ? JsonConvert.DeserializeObject<Dictionary<string, string>>(reader["Languages"].ToString()) : new Dictionary<string, string>(),
                                Area = reader["Area"] != DBNull.Value ? Convert.ToDouble(reader["Area"]) : 0.0,
                                Independent = reader["Independent"] != DBNull.Value ? reader["Independent"].ToString() == "1" : null,
                                Status = reader["Status"] != DBNull.Value ? reader["Status"].ToString() : "N/A",
                                UnMember = reader["UnMember"] != DBNull.Value ? reader["UnMember"].ToString() == "1" : null
                            });
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Erro ao carregar dados: {e.Message}");
            }

            return Paises;
        }

        /// <summary>
        /// Apaga os dados da tabela 'Paises' na base de dados.
        /// </summary>
        public void DeleteData()
        {
            try
            {
                string sql = "delete from Paises";

                command = new SQLiteCommand(sql, connection);
                command.ExecuteNonQuery();
            }

            catch (Exception e)
            {
                Console.WriteLine("Erro ao apagar os dados", e.Message);
            }
        }

        /// <summary>
        /// Descarrega as imagens da API e guarda
        /// </summary>
        /// <returns></returns>
        public async Task DowloadImagens()
        {
            // URL da API
            string apiUrl = "https://restcountries.com/v3.1/all";

            // Pasta onde as imagens serão salvas
            string outputFolder = "Bandeiras";

            // Criar a pasta se não existir
            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }

            using (HttpClient client = new HttpClient())
            {
                // Fazer uma requisição GET para obter a lista de bandeiras
                HttpResponseMessage response = await client.GetAsync(apiUrl);

                // Verificar se a requisição foi bem-sucedida
                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();

                    // Imprimir a resposta completa para depuração
                    Console.WriteLine("Resposta da API:");
                    Console.WriteLine(responseBody);

                    JArray paises = JArray.Parse(responseBody);

                    // Iterar sobre a lista de países
                    foreach (JObject pais in paises)
                    {
                        // Verificar se os campos 'flags' e 'alt' existem
                        if (pais["flags"]?["png"] != null && pais["name"]?["common"] != null)
                        {
                            // Obter o URL da imagem e o nome do país
                            string imageUrl = pais["flags"]["png"].ToString();
                            string imageName = pais["name"]["common"].ToString() + ".png";

                            // Fazer o download da imagem
                            byte[] imgData = await client.GetByteArrayAsync(imageUrl);

                            // Salvar a imagem na pasta de saída
                            string filePath = Path.Combine(outputFolder, imageName);
                            await File.WriteAllBytesAsync(filePath, imgData);

                            Console.WriteLine($"Baixado {imageName}");
                        }
                        else
                        {
                            Console.WriteLine("Os campos 'flags.png' ou 'name.common' não estão presentes em um dos objetos.");
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"Erro ao acessar a API: {response.StatusCode}");
                }
            }
        }

        /// <summary>
        /// Fecha a conexão com a base de dados se estiver aberta.
        /// </summary>
        public void CloseConnection()
        {
            try
            {
                if (connection != null && connection.State != ConnectionState.Closed)
                {
                    connection.Close();
                    Console.WriteLine("Conexão com a base de dados fechada.");

                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Erro ao fechar a conexão com a base de dados: {e.Message}");
            }
        }

    }
}





