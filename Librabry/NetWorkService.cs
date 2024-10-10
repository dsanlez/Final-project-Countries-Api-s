using System.Net;

namespace Librabry
{
    public class NetWorkService
    {

        /// <summary>
        /// Verifica se tem ligação à internet
        /// </summary>
        /// <returns></returns>
        public bool CheckConnection()
        {

            var client = new WebClient();
            try
            {
                using (client.OpenRead("http://clients3.google.com/generate_204"))
                {
                    return true;
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Sem ligação à internet");
                return false;
            }
        }
    }
}
