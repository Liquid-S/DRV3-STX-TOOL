using CLI.InputOutput;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Linq;

namespace CLI.ConfigFile
{
    public class AppConfig
    {
        public AppConfig(string fileName)
        {
            if (File.Exists(fileName))
            {
                configFile = LoadConfigFile(fileName);
            }
            else
            {
                configFile = CreateAppConfig(fileName);
            }

            STX_Folder = ReadValueFromConfigFile(in configFile, "STX");
            WRD_Folder = ReadValueFromConfigFile(in configFile, "WRD");
        }

        public string STX_Folder { get; private set; }
        public string WRD_Folder { get; private set; }

        private readonly XDocument configFile;

        /// <summary>
        /// Inizialize "App.config" and ask to the user the folders' path for the STX and WRD files.
        /// </summary>
        /// <returns>Returns a new "App.config" inizialited with the folder's path given by the user.</returns>
        private XDocument CreateAppConfig(string fileName)
        {
            XDocument xDoc = new XDocument(
                                       new XDeclaration("1.0", "utf-8", null),
                                       new XElement("configuration",
                                           new XElement("appSettings",
                                               new XElement("add",
                                               new XAttribute("key", "STX"),
                                               new XAttribute("value", ReadInput.WaitForPath("STX"))),
                                               new XElement("add",
                                               new XAttribute("key", "WRD"),
                                               new XAttribute("value", ReadInput.WaitForPath("WRD"))))));
            xDoc.Save(fileName);

            if (File.Exists(fileName))
            {
                ShowMessages.EventMessage($"{fileName} has been created.");
            }
            else
            {
                ShowMessages.ErrorMessage($"{fileName} cannot be created.");
            }

            Thread.Sleep(2000);
            return xDoc;
        }

        /// <summary>
        /// Load "App.Config" and check if the folders' path for STX and WRD files still exist.
        /// </summary>
        /// <returns>Returns "App.config" content.</returns>
        private XDocument LoadConfigFile(string fileName)
        {
            ShowMessages.EventMessage($"{fileName} has been found.");

            XDocument xDoc;

            try
            {
                xDoc = XDocument.Load(fileName);

                bool appConfigNeedToBeUpdated = CheckFolderPath(ref xDoc, "STX");
                appConfigNeedToBeUpdated = CheckFolderPath(ref xDoc, "WRD");

                if (appConfigNeedToBeUpdated)
                {
                    xDoc.Save(fileName);
                }
            }
            catch
            {
                ShowMessages.ErrorMessage($"{fileName} it's unreadable! Let's make a new one!");
                xDoc = CreateAppConfig(fileName);
            }

            //Thread.Sleep is required otherwise the user will not have the time to read the output.
            Thread.Sleep(2000);
            return xDoc;
        }

        /// <summary>
        /// Check if the folder exist, if it doesn't, then ask to the user to select a new one.
        /// </summary>
        /// <param name="xDoc">The xDocument that need to be read.</param>
        /// <param name="keyElementName">Parameter's name you want to read from the xDocument. E.g. STX</param>
        /// <returns></returns>
        private bool CheckFolderPath(ref XDocument xDoc, string keyElementName)
        {
            string path_folder = ReadValueFromConfigFile(in xDoc, keyElementName);

            if (path_folder == "" || !Directory.Exists(path_folder))
            {
                XAttribute target = xDoc.Descendants("add")
                                        .Where(e => e.Attribute("key").Value == keyElementName)
                                        .Single().Attribute("value");

                target.Value = ReadInput.WaitForPath(keyElementName);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Read the chosen parameter from "App.config".
        /// </summary>
        /// <param name="xDoc">The xDocument from which the value is going to be read.</param>
        /// <param name="keyElementName">The element you want to read.</param>
        /// <returns>Returns the element's value.</returns>
        private string ReadValueFromConfigFile(in XDocument xDoc, string keyElementName)
        {
            return xDoc.Descendants("add")
                       .Where(e => e.Attribute("key").Value == keyElementName)
                       .Single().Attribute("value").Value;
        }
    }
}
