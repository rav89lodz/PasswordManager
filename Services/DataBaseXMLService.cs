using System.Xml.Linq;
using System.Xml;
using System.IO;
using System.Linq;
using PasswordManager.Interfaces;
using PasswordManager.Entities;
using System;

namespace PasswordManager.Services
{
    public class DataBaseXMLService : IDataBaseService
    {
        private IEncryptionService _encryption;

        public DataBaseXMLService(IEncryptionService encryption)
        {
            _encryption = encryption;
        }

        public DataBaseXMLService()
        {
        }

        public string GetFileName()
        {
            return "data.xml";
        }

        public void CreateFileOnStart()
        {
            if (!File.Exists(GetFileName()))
            {
                var settingsParam = new SettingsParams(_encryption);
                new EnvironmentService(settingsParam.Salt, settingsParam.Password, "xml");

                XmlDocument doc = new XmlDocument();
                doc.LoadXml("<item></item>");
                doc.Save(GetFileName());
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
            if (fileName == null)
            {
                fileName = GetFileName();
            }
            try
            {
                string data = "";
                var doc = XDocument.Load(fileName);
                foreach (XElement xe in doc.Descendants(descendants))
                {
                    data = xe.Element(value).Value;
                }
                return data;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public TableRow[] GetData(string fileName = null)
        {
            if(fileName == null)
            {
                fileName= GetFileName();
            }
            if (File.Exists(fileName))
            {
                var doc = XDocument.Load(fileName);
                if (doc == null)
                {
                    return null;
                }
                TableRow[] data = new TableRow[doc.Root.Elements().Count()];
                int i = 0;
                foreach (XElement xe in doc.Descendants("Data"))
                {
                    string inlinePass = _encryption.Decrypt(xe.Element("Value4").Value);
                    data[i] = new TableRow(inlinePass, _encryption.Decrypt(xe.Element("Value2").Value, inlinePass),
                                            _encryption.Decrypt(xe.Element("Value3").Value, inlinePass),
                                            _encryption.Decrypt(xe.Element("Value1").Value, inlinePass));
                    i++;
                }
                return data;
            }
            return null;
        }

        public void RemoveElement(TableRow tableRow)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(GetFileName());
            XmlNode root = doc.DocumentElement;
            XmlNode element = root.SelectSingleNode("descendant::Data [Value1='" + _encryption.Encrypt(tableRow.Description, tableRow.UUID) + "'][Value2='" +
                                   _encryption.Encrypt(tableRow.Login, tableRow.UUID) + "'][Value3='" + _encryption.Encrypt(tableRow.Password, tableRow.UUID) + "']");
            if(element != null)
            {
                root.RemoveChild(element);
                doc.Save(GetFileName());
            }
        }

        public string CreateBackup(string backupPassword)
        {
            string backupFileName = "password_generator_" + DateTime.Now.ToString("yyyy.MM.dd_HH-mm-ss.ms") + "_backup.xml";
            var settingsParam = new SettingsParams(_encryption);
            var environmentService = new EnvironmentService();
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml("<item></item>");
                XmlElement settingsElement = doc.CreateElement("Settings");
                settingsElement.InnerXml = "<Value1>" + environmentService.GetEnvironmentVariable(environmentService.PgSalt) + "</Value1><Value2>" +
                                            _encryption.Encrypt(environmentService.GetEnvironmentVariable(environmentService.PgPass), backupPassword) + "</Value2>";
                doc.DocumentElement.AppendChild(settingsElement);

                XmlElement paramsElement = doc.CreateElement("Params");
                paramsElement.InnerXml = "<Value1>" + _encryption.Encrypt(settingsParam.UserName, backupPassword) + "</Value1><Value2>" +
                                        _encryption.Encrypt(settingsParam.MachineName, backupPassword) + "</Value2><Value3>" + _encryption.Encrypt(backupPassword, backupPassword) + "</Value3>";
                doc.DocumentElement.AppendChild(paramsElement);

                doc.PreserveWhitespace = false;
                doc.Save(backupFileName);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return InsertDataToFile(null, backupFileName);
        }

        public string RestoreBackup(string filePath, string backupPassword)
        {
            string tempFileName = "password_generator_" + DateTime.Now.ToString("yyyy.MM.dd_HH-mm-ss.ms") + "_temp_restore.xml";
            try
            {
                string salt = GetValueFromDataBase("Settings", "Value1", filePath);
                new EnvironmentService(salt, null, "xml");
                string password = _encryption.Decrypt(GetValueFromDataBase("Settings", "Value2", filePath), backupPassword);
                new EnvironmentService(salt, password, "xml");

                XmlDocument doc = new XmlDocument();
                doc.LoadXml("<item></item>");
                doc.Save(tempFileName);

                SetUserCredentials(tempFileName);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            var result = InsertDataToFile(filePath, tempFileName);
            if(result != null)
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
            var document = XDocument.Load(fileName);
            XElement root = new XElement("Data",
                new XElement("Value1", _encryption.Encrypt(row.Description, row.UUID)),
                new XElement("Value2", _encryption.Encrypt(row.Login, row.UUID)),
                new XElement("Value3", _encryption.Encrypt(row.Password, row.UUID)),
                new XElement("Value4", _encryption.Encrypt(row.UUID))
                );
            document.Root.Add(root);
            document.Save(fileName);
        }

        private void SetUserCredentials(string fileName = null)
        {
            if(fileName == null)
            {
                fileName = GetFileName();
            }
            if (!File.Exists(fileName))
            {
                return;
            }
            var doc = XDocument.Load(fileName);
            if (doc.Root.Element("Params") == null)
            {
                var settingsParam = new SettingsParams(_encryption);
                XElement rootParams = new XElement("Params",
                    new XElement("Value1", _encryption.Encrypt(settingsParam.UserName)),
                    new XElement("Value2", _encryption.Encrypt(settingsParam.MachineName))
                    );
                doc.Root.Add(rootParams);
                doc.Save(fileName);
            }
        }
    }
}
