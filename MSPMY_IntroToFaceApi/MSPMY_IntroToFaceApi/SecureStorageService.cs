using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace FacialRecognitionLogin
{
    public static class SecureStorageService
    {
        public static Task SaveLogin(string username, string password) => SecureStorage.SetAsync(username, password);

        public static async Task<bool> IsLoginCorrect(string username, string password)
        {
            try
            {
                string savedPassword = await SecureStorage.GetAsync(username);
                return password.Equals(savedPassword);
            }
            catch
            {
                return false;
            }
        }
    }
}
