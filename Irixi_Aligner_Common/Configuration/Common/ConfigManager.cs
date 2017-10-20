
using Irixi_Aligner_Common.Configuration.Layout;
using Irixi_Aligner_Common.Message;
using Newtonsoft.Json;
using System;
using System.IO;

namespace Irixi_Aligner_Common.Configuration.Common
{
    public class ConfigManager
    {
        const string kCONF_MOTIONCONTROLLER = @"Configuration\system_setting.json";
        const string kPROF_SURUGASTAGES = @"Configuration\profile_motorized_stages.json";
        const string kPROF_LAYOUT = @"Configuration\layout.json";
        const string kPROF_DEFAULTLAYOUT = @"Configuration\defaultlayout.json";


        public ConfigManager()
        {
            string json_string = string.Empty;

            #region Read the configration of motion controller

            try
            {
                using (StreamReader reader = File.OpenText(kCONF_MOTIONCONTROLLER))
                {
                    json_string = reader.ReadToEnd();
                    reader.Close();
                }

                // Convert to object 
                this.ConfSystemSetting = JsonConvert.DeserializeObject<ConfigurationSystemSetting>(json_string);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLine("Unable to load config file {0}, {1}", kCONF_MOTIONCONTROLLER, ex.Message, LogHelper.LogType.ERROR);

                throw new Exception(ex.Message);
            }

            #endregion

            #region Read the profile of Suruage stages
            try
            {
                using (StreamReader reader = File.OpenText(kPROF_SURUGASTAGES))
                {
                    json_string = reader.ReadToEnd();
                    reader.Close();
                }

                // Convert to object 
                this.ProfileManager = JsonConvert.DeserializeObject<MotorizedStagesProfileManager>(json_string);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLine("Unable to load config file {0}, {1}", kPROF_SURUGASTAGES, ex.Message, LogHelper.LogType.ERROR);

                throw new Exception(ex.Message);
            }
            #endregion

            #region read layout
            string layout_file = "";
            try
            {
                if (StaticVariables.IsLoadDefaultLayout)
                {
                    // load default layout
                    layout_file = kPROF_DEFAULTLAYOUT;
                }
                else
                {
                    // load last layout
                    layout_file = kPROF_LAYOUT;
                }

                StreamReader reader = File.OpenText(layout_file);
                json_string = reader.ReadToEnd();
                reader.Close();

                // Convert to object
                this.ConfWSLayout = JsonConvert.DeserializeObject<LayoutManager>(json_string);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLine("Unable to load config file {0}, {1}", layout_file, ex.Message, LogHelper.LogType.ERROR);

                throw new Exception(ex.Message);
            }

            #endregion

            #region Bind actuator profile to physical axis
            bool allfound = true;
            foreach(var confcontroller in ConfSystemSetting.PhysicalMotionControllers)
            {
                foreach(var confaxis in confcontroller.AxisCollection)
                {
                    var profile = this.ProfileManager.FindProfile(confaxis.Vendor, confaxis.Model);

                    if (profile == null)
                    {
                        LogHelper.WriteLine("Unable to find the motorized stage profile of vendor:{0}/model:{1}", confaxis.Vendor, confaxis.Model, LogHelper.LogType.ERROR);

                        allfound = false;
                    }
                    else
                    {
                        confaxis.SetProfile(profile.Clone() as MotorizedStageProfile);
                    }
                }
            }

            if(allfound == false)
            {
                throw new Exception("Some of motorized stage profiles are not found.");
            }


            #endregion
        }

        public ConfigurationSystemSetting ConfSystemSetting { get; }

        public MotorizedStagesProfileManager ProfileManager { get; }

        public LayoutManager ConfWSLayout { get; set; }

        /// <summary>
        /// Save the layout of document group
        /// </summary>
        /// <param name="layout"></param>
        public void SaveLayout(dynamic layout)
        {
            string json_str = JsonConvert.SerializeObject(layout);

            using (FileStream fs = File.Open(kPROF_LAYOUT, FileMode.Create, FileAccess.Write))
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
