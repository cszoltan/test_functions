using System.Data.SqlClient;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace HeroApi
{
    public static class Heroes
    {
        private static string ConnectionString = null;

        [FunctionName("getHeroes")]
        public static async Task<HttpResponseMessage> GetData([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "heroes/{id?}")]HttpRequest req, string id, ILogger log)
        {
            if (ConnectionString == null)
            {
                ConnectionString = await GetConnString();
            }

            try
            {
                string json;
                string name = req.Query["name"];
                if (id != null)
                {
                    json = GetHeroes(ConnectionString, $" WHERE id={id}").Result;
                }
                else if (name != null)
                {
                    json = GetHeroes(ConnectionString, $" WHERE name LIKE '{name}%'").Result;
                } else
                {
                    json = GetHeroes(ConnectionString).Result;
                }
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
            }
            catch (SqlException e)
            {
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }

        [FunctionName("UpdateHeroes")]
        public static async Task<HttpResponseMessage> UpdateData([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "heroes/{id}")]HttpRequestMessage req, string id, ILogger log)
        {
            if (ConnectionString == null)
            {
                ConnectionString = await GetConnString();
            }

            try
            {
                Hero hero = await req.Content.ReadAsAsync<Hero>();
                UpdateHero(ConnectionString, hero, id);
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (SqlException e)
            {
                return req.CreateResponse(HttpStatusCode.InternalServerError, e.Message);
            }
        }

        [FunctionName("PostHeroes")]
        public static async Task<HttpResponseMessage> PostData([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "heroes")]HttpRequestMessage req, ILogger log)
        {
            if (ConnectionString == null)
            {
                ConnectionString = await GetConnString();
            }

            try
            {
                Hero hero = await req.Content.ReadAsAsync<Hero>();
                SaveHero(ConnectionString, hero);
                return new HttpResponseMessage(HttpStatusCode.Created);
            }
            catch (SqlException e)
            {
                return req.CreateResponse(HttpStatusCode.InternalServerError, e.Message);
            }
        }

        [FunctionName("DeleteHeroes")]
        public static async Task<HttpResponseMessage> DeleteData([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "heroes/{id}")]HttpRequestMessage req, string id, ILogger log)
        {
            if (ConnectionString == null)
            {
                ConnectionString = await GetConnString();
            }

            try
            {
                DeleteHero(ConnectionString, id);
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (SqlException e)
            {
                return req.CreateResponse(HttpStatusCode.InternalServerError, e.Message);
            }
        }

        private static async void DeleteHero(string connectionS, string id)
        {
            try
            {
                using (var connection = new SqlConnection(connectionS))
                {
                    await connection.OpenAsync();

                    string query = $"DELETE FROM hero_details WHERE id={id};";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException e)
            {
                throw e;
            }
        }

        private static async void SaveHero(string connectionS, Hero hero)
        {
            try
            {
                using (var connection = new SqlConnection(connectionS))
                {
                    await connection.OpenAsync();

                    string query = $"INSERT INTO hero_details (name, role) VALUES ('{hero.name}', '{hero.role}');";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException e)
            {
                throw e;
            }
        }

        private static async void UpdateHero(string connectionS, Hero hero, string id)
        {
            try
            {
                using (var connection = new SqlConnection(connectionS))
                {
                    await connection.OpenAsync();

                    string query = $"UPDATE hero_details SET name='{hero.name}', role='{hero.role}' WHERE id={id};";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException e)
            {
                throw e;
            }
        }

        private static async Task<string> GetHeroes(string connectionS, string queryParam = "")
        {
            try
            {
                using (var connection = new SqlConnection(connectionS))
                {
                    await connection.OpenAsync();
                    List<Hero> heroes = new List<Hero>();

                    string query = "SELECT * FROM hero_details" + queryParam + ";";

                    SqlCommand command = new SqlCommand(query, connection);
                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            int id = reader.GetInt32(0);
                            string name = reader.GetString(1);
                            string role = reader.GetString(2);
                            heroes.Add(new Hero(id, name, role));
                        }
                    }
                    if (queryParam.Contains("id"))
                    {
                        return JsonConvert.SerializeObject(heroes[0]);
                    }
                    else
                    {
                        return JsonConvert.SerializeObject(heroes);
                    }
                }
            }
            catch (SqlException e)
            {
                throw e;
            }
        }

        private static async Task<string> GetConnString()
        {
            AzureServiceTokenProvider azureServiceTokenProvider = new AzureServiceTokenProvider();
            KeyVaultClient kvClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
            string secretUri = "https://authkey.vault.azure.net/secrets/herosecret";
            string ConnectionString = (await kvClient.GetSecretAsync(secretUri)).Value;
            return ConnectionString;
        }
    }
}
