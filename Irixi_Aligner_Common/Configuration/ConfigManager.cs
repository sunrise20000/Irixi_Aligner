
using Irixi_Aligner_Common.Message;
using Newtonsoft.Json;
using System;
using System.IO;

namespace Irixi_Aligner_Common.Configuration
{
    public class ConfigManager
    {
        public ConfigManager()
        {
            string json_string = string.Empty;

            #region Read the configration of motion controller

            try
            {
                // Read the JSON string from the config file
                using (StreamReader reader = File.OpenText(@"Configuration\motion_controller.json"))
                {
                    json_string = reader.ReadToEnd();
                    reader.Close();
                }

                // Convert to object 
                MotionControllerConfig = JsonConvert.DeserializeObject<ConfigurationMotionController>(json_string);
            }
            catch (Exception ex)
            {
                LogHelper.LogEnabled = true;
                LogHelper.WriteLine("Unable to read config file of motion controller, {0}", ex.Message, LogHelper.LogType.ERROR);
                LogHelper.LogEnabled = false;
                throw new Exception(ex.Message);
            }

            #endregion

            #region read layout

            try
            {
                string filename = "";
                if (StaticVariables.DefaultLayout)
                    filename = @"Configuration\defaultlayout.json";
                else
                    filename = @"Configuration\layout.json";

                // Read the JSON string from the config file
                StreamReader reader = File.OpenText(filename);
                json_string = reader.ReadToEnd();
                reader.Close();

                // Convert to object
                WorkspaceLayoutHelper = JsonConvert.DeserializeObject<LayoutManager>(json_string);
            }
            catch (Exception ex)
            {
                LogHelper.LogEnabled = true;
                LogHelper.WriteLine("Unable to read config file of layout, {0}", ex.Message, LogHelper.LogType.ERROR);
                LogHelper.LogEnabled = false;
                throw new Exception(ex.Message);
            }

            #endregion
        }

        public ConfigurationMotionController MotionControllerConfig { get; set; }

        public LayoutManager WorkspaceLayoutHelper { get; set; }

        /// <summary>
        /// Save the layout of document group
        /// </summary>
        /// <param name="layout"></param>
        public void SaveLayout(LayoutManager layout)
        {
            string json_str = JsonConvert.SerializeObject(layout);

            using (FileStream fs = File.Open(@"Configuration\layout.json", FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter wr = new StreamWriter(fs))
                {
                    wr.Write(json_str);
                    wr.Close();
                }

                fs.Close();
            }
        }

    }
}
