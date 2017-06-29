using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace Irixi_Aligner_Common.Configuration
{
    public class ConfigManager
    {
        public ConfigManager()
        {
            string json_string = string.Empty;

            #region Read the configration of motion controller
            // Read the JSON string from the config file
            StreamReader reader = File.OpenText(@"Configuration\motion_controller.json");
            json_string = reader.ReadToEnd();
            reader.Close();

            // Convert the string 
            MotionController = JsonConvert.DeserializeObject<ConfigurationMotionController>(json_string);

            #endregion

            #region GUI snapshot
            // Read the JSON string from the config file
            reader = File.OpenText(@"Configuration\gui_snapshot.json");
            json_string = reader.ReadToEnd();
            reader.Close();

            this.Snapshot = JsonConvert.DeserializeObject<SnapshotGUI>(json_string);
            #endregion
        }

        public ConfigurationMotionController MotionController { get; set; }

        public SnapshotGUI Snapshot { get; set; }

        /// <summary>
        /// Save the GUI snapshot to the json file
        /// </summary>
        public void WriteSnapshotGUI()
        {
           string _json_string = JsonConvert.SerializeObject(Snapshot);
            FileStream fs = File.Create(@"Configuration\gui_snapshot.json");
            StreamWriter sw = new StreamWriter(fs);
            sw.Write(_json_string);
            sw.Close();
            fs.Close();
            

        }
    }
}
