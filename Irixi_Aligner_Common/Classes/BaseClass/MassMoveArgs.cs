using System.IO;
using GalaSoft.MvvmLight.Command;
using Newtonsoft.Json;

namespace Irixi_Aligner_Common.Classes.BaseClass
{
    public class MassMoveArgs : ObservableCollectionEx<AxisMoveArgs>
    {
        #region Variables

        const string PRESET_FOLDER = "PresetPosition";

        #endregion

        #region Properties

        /// <summary>
        /// Get or set which logical motion controller the args belongs to
        /// </summary>
        public string LogicalMotionController { get; set; }

        /// <summary>
        /// Get or set the file name of the preset position profile
        /// </summary>
        public string PresetPositionFileName { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Load the preset position file of the specified logical motion controller by the file name
        /// </summary>
        /// <param name="LogicalMotionController"></param>
        /// <param name="FileName"></param>
        public static MassMoveArgs LoadPresetPosition(string LogicalMotionController, string FileName)
        {
            // the full file path where we should find the preset profiles
            var fullFilePath = PRESET_FOLDER + "\\" + LogicalMotionController + "\\" + FileName + ".json";

            if(File.Exists(fullFilePath) == true)
            {
                var jsonString = File.ReadAllText(fullFilePath);
                var profile = JsonConvert.DeserializeObject<MassMoveArgs>(jsonString);
                return profile;
            }
            else
            {
                // the folder does not exist
                throw new FileNotFoundException(string.Format("the file {0} does not exist.", fullFilePath));
            }
        }

        /// <summary>
        /// Save the preset position profile
        /// </summary>
        /// <param name="Args"></param>
        /// <param name="FileName"></param>
        public static void SavePosition(MassMoveArgs Args, string FileName)
        {
            if (Args.LogicalMotionController == "")
                throw new InvalidDataException("the logical motion controller is empty.");

            // the full file path where we should find the preset profiles
            var fullFilePath = PRESET_FOLDER + "\\" + Args.LogicalMotionController + "\\" + FileName + ".json";

            var jsonString = JsonConvert.SerializeObject(Args);
            using (FileStream fs = File.Open(fullFilePath, FileMode.OpenOrCreate, FileAccess.Write))
            {
                using (StreamWriter wr = new StreamWriter(fs))
                {
                    wr.Write(jsonString);
                    wr.Close();
                }

                fs.Close();
            }

        }
        #endregion

        #region Commands

        

        #endregion

    }
}
