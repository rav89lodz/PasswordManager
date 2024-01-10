using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PasswordManager.DTOs;
using PasswordManager.Entities;
using PasswordManager.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PasswordManager.Services
{
    public class DataBaseJSONService : IDataBaseService
    {
        private IEncryptionService _encryption;

        public DataBaseJSONService(IEncryptionService encryption)
        {
            _encryption = encryption;
        }

        public DataBaseJSONService()
        {
        }

        public string GetFileName()
        {
            return "data.json";
        }

        public void CreateFileOnStart()
        {
            if (!File.Exists(GetFileName()))
            {
                var settingsParam = new SettingsParams(_encryption);
                new EnvironmentService(settingsParam.Salt, settingsParam.Password, "json");

                var createStream = File.Create(@GetFileName());
                createStream.Close();
                var output = JsonConvert.SerializeObject(new JSONSchema(), Formatting.Indented);
                File.WriteAllText(@GetFileName(), output);                
            }
        }

        public void SaveToFile(TableRow tableRow)
        {
            if (File.Exists(GetFileName()))
            {
                SetUserCredentials();
                tableRow.UUID = _encryption.GetInlinePassValue();
                AddNewDataLine(tableRow, GetFileName());
            }
        }

        public string GetValueFromDataBase(string descendants, string value, string fileName = null)
        {
            if(fileName == null)
            {
                fileName = GetFileName();
            }
            try
            {
                var root = JObject.Parse(File.ReadAllText(fileName)).SelectToken(descendants);
                if(root.HasValues)
                {
                    return root.SelectToken(value).ToString();
                }
                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public TableRow[] GetData(string fileName = null)
        {
            if (fileName == null)
            {
                fileName = GetFileName();
            }
            if (File.Exists(fileName))
            {
                string json = File.ReadAllText(fileName);
                var jsonObj = JsonConvert.DeserializeObject<JSONSchema>(json);
                if (jsonObj == null || jsonObj.Data == null)
                {
                    return null;
                }
                    
                TableRow[] data = new TableRow[jsonObj.Data.Count];
                int i = 0;
                foreach (TableRowDTO row in jsonObj.Data)
                {
                    string inlinePass = _encryption.Decrypt(row.Value4);
                    data[i] = new TableRow(inlinePass, _encryption.Decrypt(row.Value2, inlinePass),
                                            _encryption.Decrypt(row.Value3, inlinePass),
                                            _encryption.Decrypt(row.Value1, inlinePass));
                    i++;
                }
                return data;
            }
            return null;
        }

        public void RemoveElement(TableRow tableRow)
        {
            string json = File.ReadAllText(GetFileName());
            var jsonObj = JsonConvert.DeserializeObject<JSONSchema>(json);
            if (jsonObj == null || jsonObj.Data == null)
            {
                return;
            }
            foreach (TableRowDTO row in jsonObj.Data.ToList())
            {
                if(row == null)
                {
                    continue;
                }
                if(row.Value1 == _encryption.Encrypt(tableRow.Description, tableRow.UUID) && row.Value2 == _encryption.Encrypt(tableRow.Login, tableRow.UUID) &&
                        row.Value3 == _encryption.Encrypt(tableRow.Password, tableRow.UUID))
                {
                    jsonObj.Data.Remove(row);
                }
            }
            var output = JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(GetFileName(), output);
        }

        public string CreateBackup(string backupPassword)
        {
            string backupFileName = "password_generator_" + DateTime.Now.ToString("yyyy.MM.dd_HH-mm-ss.ms") + "_backup.json";
            var settingsParam = new SettingsParams(_encryption);
            var environmentService = new EnvironmentService();
            try
            {
                var createStream = File.Create(backupFileName);
                createStream.Close();
                
                var settingsDTO = new SettingsDTO()
                {
                    Value1 = environmentService.GetEnvironmentVariable(environmentService.PgSalt),
                    Value2 = _encryption.Encrypt(environmentService.GetEnvironmentVariable(environmentService.PgPass), backupPassword)
                };                
                var paramsDTO = new ParamsDTO()
                {
                    Value1 = _encryption.Encrypt(settingsParam.UserName, backupPassword),
                    Value2 = _encryption.Encrypt(settingsParam.MachineName, backupPassword),
                    Value3 = _encryption.Encrypt(backupPassword, backupPassword)
                };
                var jsonSchema = new JSONSchema()
                {
                    Settings = settingsDTO,
                    Params = paramsDTO
                };
                var output = JsonConvert.SerializeObject(jsonSchema, Formatting.Indented);
                File.WriteAllText(backupFileName, output);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return InsertDataToFile(null, backupFileName);
        }

        public string RestoreBackup(string filePath, string backupPassword)
        {
            string tempFileName = "password_generator_" + DateTime.Now.ToString("yyyy.MM.dd_HH-mm-ss.ms") + "_temp_restore.json";
            try
            {
                string salt = GetValueFromDataBase("Settings", "Value1", filePath);
                new EnvironmentService(salt, null, "json");
                string password = _encryption.Decrypt(GetValueFromDataBase("Settings", "Value2", filePath), backupPassword);
                new EnvironmentService(salt, password, "json");

                var createStream = File.Create(tempFileName);
                createStream.Close();
                var output = JsonConvert.SerializeObject(new JSONSchema(), Formatting.Indented);
                File.WriteAllText(tempFileName, output);

                SetUserCredentials(tempFileName);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            var result = InsertDataToFile(filePath, tempFileName);
            if (result != null)
            {
                return result;
            }

            try
            {
                File.Delete(GetFileName());
                File.Move(tempFileName, GetFileName());
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return null;
        }

        private string InsertDataToFile(string filePath, string newFilePath)
        {
            try
            {
                TableRow[] tableRows = GetData(filePath);
                if (tableRows == null || tableRows.Length < 1)
                {
                    return "Obtaining data problem";
                }
                foreach (TableRow row in tableRows)
                {
                    if (row == null)
                    {
                        continue;
                    }
                    AddNewDataLine(row, newFilePath);
                }
                return null;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        private void AddNewDataLine(TableRow row, string fileName)
        {
            string json = File.ReadAllText(fileName);
            var jsonObj = JsonConvert.DeserializeObject<JSONSchema>(json);

            var tableRowDTO = new TableRowDTO()
            {
                Value1 = _encryption.Encrypt(row.Description, row.UUID),
                Value2 = _encryption.Encrypt(row.Login, row.UUID),
                Value3 = _encryption.Encrypt(row.Password, row.UUID),
                Value4 = _encryption.Encrypt(row.UUID)
            };

            if (jsonObj.Data == null)
            {
                jsonObj.Data = new List<TableRowDTO>() { tableRowDTO };
            }
            else
            {
                jsonObj.Data.Add(tableRowDTO);
            }
            var output = JsonConvert.SerializeObject(jsonObj, Formatting.Indented);
            File.WriteAllText(fileName, output);
        }

        private void SetUserCredentials(string fileName = null)
        {
            if (fileName == null)
            {
                fileName = GetFileName();
            }
            if (!File.Exists(fileName))
            {
                return;
            }
            string jsonFile = File.ReadAllText(fileName);
            if(!string.IsNullOrEmpty(jsonFile))
            {
                var root = JObject.Parse(jsonFile).SelectToken("Params");
                if (root != null && root.HasValues)
                {
                    return;
                }
            }
            var settingsParam = new SettingsParams(_encryption);
            var paramsDTO = new ParamsDTO()
            {
                Value1 = _encryption.Encrypt(settingsParam.UserName),
                Value2 = _encryption.Encrypt(settingsParam.MachineName)
            };
            var json = new JSONSchema()
            {
                Params = paramsDTO,
                Data = null
            };
            var output = JsonConvert.SerializeObject(json, Formatting.Indented);
            File.WriteAllText(fileName, output);
        }
    }
}
