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
                StreamReader reader = File.OpenText(@"Configuration\motion_controller.json");
                json_string = reader.ReadToEnd();
                reader.Close();

                // Convert the string 
                MotionControllerConfig = JsonConvert.DeserializeObject<ConfigurationMotionController>(json_string);
            }
            catch (Exception ex)
            {
                LogHelper.LogEnabled = true;
                LogHelper.WriteLine("Unable to parse config file of motion controller, {0}", ex.Message, LogHelper.LogType.ERROR);
                LogHelper.LogEnabled = false;
                throw new JsonException(ex.Message);
            }

            #endregion

            #region Workspace Layout
            try
            {
                // Read the JSON string from the config file
                StreamReader reader = File.OpenText(@"Configuration\ws_layout.json");
                json_string = reader.ReadToEnd();
                reader.Close();
                
                this.WsLayout = JsonConvert.DeserializeObject<WorkSpaceLayout>(json_string);
            }
            catch (Exception ex)
            {
                LogHelper.LogEnabled = true;
                LogHelper.WriteLine("Unable to parse config file of workspace layout, {0}", ex.Message, LogHelper.LogType.ERROR);
                LogHelper.LogEnabled = false;
                throw new JsonException(ex.Message);
            }
            #endregion
        }

        public ConfigurationMotionController MotionControllerConfig { get; set; }

        public WorkSpaceLayout WsLayout { get; set; }

        /// <summary>
        /// Save the GUI snapshot to the json file
        /// </summary>
        public void WriteWsLayout()
        {
            string _json_string = JsonConvert.SerializeObject(WsLayout);
            FileStream fs = File.Create(@"Configuration\ws_layout.json");
            StreamWriter sw = new StreamWriter(fs);
            sw.Write(_json_string);
            sw.Close();
            fs.Close();
        }
    }
}
