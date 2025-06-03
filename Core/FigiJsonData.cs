using Newtonsoft.Json;
using System;
using System.IO;
using UnTraid.DTO;

namespace UnTraid.Core
{
    public static class FigiJsonData
    {
        private static string JsonFilePath { get; set; } = GetJsonDataPatch();


        static private string GetJsonDataPatch()
        {
            string appDir = AppDomain.CurrentDomain.BaseDirectory;
            string projectRootDir = Directory.GetParent(Directory.GetParent(Directory.GetParent(appDir).FullName).FullName).FullName;
            return Path.Combine(projectRootDir, "Core", "Figi.json");
        }

        static public FigiDataDTO GetJsonData()
        {
            string json = File.ReadAllText(JsonFilePath);
            return JsonConvert.DeserializeObject<FigiDataDTO>(json);
        }
        static public void UpdateJsonData(FigiDataDTO data)
        {
            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(JsonFilePath, json);
        }
    }
}
