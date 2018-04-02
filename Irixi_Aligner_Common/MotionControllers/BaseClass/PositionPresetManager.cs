using System;
using System.Collections.Generic;
using System.IO;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Irixi_Aligner_Common.Classes.BaseClass;
using Newtonsoft.Json;

namespace Irixi_Aligner_Common.MotionControllers.Base
{
    public class PositionPresetManager : ViewModelBase
    {
        #region Variables
        const string PRESET_FOLDER = "PresetPositionProfiles";

        MassMoveArgs moveArgsCollection;
        LogicalMotionComponent motionComponent;
        string[] profileList = null;
        string selectedProfile = "";

        #endregion

        #region Properties

        /// <summary>
        /// Get or set the active logical motion controller
        /// </summary>
        public LogicalMotionComponent SelectedMotionComponent
        {
            set
            {
               
                MoveArgsCollection = LoadCurrentPositions(value);

                // load profiles belong to the selected motion component
                ProfileList = LoadPresetPositionProfiles(value);

                motionComponent = value;
                RaisePropertyChanged();
            }
            get
            {
                return motionComponent;
            }
        }

        /// <summary>
        /// Get position preset profile list when the motion controller changed
        /// <see cref="SelectedMotionComponent"/>
        /// </summary>
        public string[] ProfileList
        {
            get
            {
                return profileList;
            }
            private set
            {
                profileList = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Set the selected profile and load the preset positions
        /// </summary>
        public string SelectedProfile
        {
            set
            {
                selectedProfile = value;

                //TODO Load preset positions
            }
        }

        /// <summary>
        /// Get the mass move arguments, it's set when the motion controller changed
        /// <see cref="SelectedMotionComponent"/>
        /// </summary>
        public MassMoveArgs MoveArgsCollection
        {
            private set
            {
                moveArgsCollection = value;
                RaisePropertyChanged();
            }
            get
            {
                return moveArgsCollection;
            }
        }

        #endregion

        #region Methods
        /// <summary>
        /// Post the error message to the UI
        /// </summary>
        /// <param name="Message"></param>
        private void PostErrorMessage(string Message)
        {
            Messenger.Default.Send<NotificationMessage<string>>(new NotificationMessage<string>(
                        this,
                        Message,
                        "ERROR"));
        }

        /// <summary>
        /// load the current positions of the specified logical motion component
        /// </summary>
        /// <param name="MotionComponent"></param>
        /// <returns></returns>
        private MassMoveArgs LoadCurrentPositions(LogicalMotionComponent MotionComponent)
        {
            // generate the move args list to bind to the window
            MassMoveArgs arg = new MassMoveArgs()
            {
                
            };

            foreach (var laxis in MotionComponent)
            {
                var a = laxis.MoveArgs.Clone() as AxisMoveArgs;
                a.IsMoveable = true;
                a.MaxMoveOrder = MotionComponent.Count;


                arg.Add(a);
            }

            return arg;
        }

        /// <summary>
        /// Load the profiles which belong to the selected logical motion component
        /// </summary>
        /// <returns></returns>
        private string[] LoadPresetPositionProfiles(LogicalMotionComponent MotionComponent)
        {
            var dir = PRESET_FOLDER + "\\" + MotionComponent.GetHashString();

            if(Directory.Exists(dir))
            {
                List<string> profiles = new List<string>();

                DirectoryInfo info = new DirectoryInfo(dir);
                foreach(var file in info.GetFiles())
                {
                    if(file.Extension == "json")
                    {
                        profiles.Add(file.Name);
                    }
                }

                return profiles.ToArray();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Load the preset position file of the specified logical motion controller by the file name
        /// </summary>
        /// <param name="LogicalMotionController"></param>
        /// <param name="FileName"></param>
        public MassMoveArgs LoadPresetPositionProfile(LogicalMotionComponent MotionComponent, string FileName)
        {
            // the full file path where we should find the preset profiles
            var fullFilePath = PRESET_FOLDER + "\\" + MotionComponent.GetHashString() + "\\" + FileName + ".json";

            if (File.Exists(fullFilePath) == true)
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
        private void SavePresetPositionProfile(LogicalMotionComponent MotionComponent, MassMoveArgs Args, string FileName)
        {
            if (MotionComponent == null)
                throw new InvalidDataException("the logical motion controller is empty.");

            // the full file path where we should find the preset profiles
            var fullFilePath = PRESET_FOLDER + "\\" + MotionComponent.GetHashString() + "\\" + FileName + ".json";

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

        private bool CheckProfileExistance(LogicalMotionComponent MotionComponent, string FileName)
        {
            // the full file path where we should find the preset profiles
            var fullFilePath = PRESET_FOLDER + "\\" + MotionComponent.GetHashString() + "\\" + FileName + ".json";

            return File.Exists(fullFilePath);
        }

        #endregion

        #region Commands

        public RelayCommand<LogicalMotionComponent> GetCurrentPositions
        {
            get
            {
                return new RelayCommand<LogicalMotionComponent>(lmc =>
                {
                    if (lmc == null)
                        MoveArgsCollection = null;
                    else
                        MoveArgsCollection = LoadCurrentPositions(lmc);
                });
            }
        }

        public RelayCommand<string> Save
        {
            get
            {
                return new RelayCommand<string>(filename =>
                {
                    try
                    {
                        if (CheckProfileExistance(SelectedMotionComponent, filename))
                        {
                        }
                        else
                        {
                            SavePresetPositionProfile(SelectedMotionComponent, MoveArgsCollection, filename);
                        }
                    }
                    catch(Exception ex)
                    {
                        PostErrorMessage(string.Format("Unable to save the profile, {0}.", ex.Message));
                    }
                });
            }
        }

        #endregion
    }
}
