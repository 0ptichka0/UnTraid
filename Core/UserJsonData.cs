using Newtonsoft.Json;
using System;
using System.IO;
using UnTraid.DTO;

namespace UnTraid.Core
{
    public static class UserJsonData
    {
        private static string JsonFilePath { get; set; } = GetJsonDataPatch();


        static private string GetJsonDataPatch()
        {
            string appDir = AppDomain.CurrentDomain.BaseDirectory;
            string projectRootDir = Directory.GetParent(Directory.GetParent(Directory.GetParent(appDir).FullName).FullName).FullName;
            return Path.Combine(projectRootDir, "UserDataPropertis.json");
        }

        static public UserDataDTO GetJsonData()
        {
            string json = File.ReadAllText(JsonFilePath);
            return JsonConvert.DeserializeObject<UserDataDTO>(json);
        }
        static public void UpdateJsonData(UserDataDTO data)
        {
            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(JsonFilePath, json);
        }
    }
}
