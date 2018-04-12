using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
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
                if (value != null)
                {
                    SelectedProfile = null;

                    MoveArgsCollection = LoadCurrentPositions(value);
                    
                    // load profiles belong to the selected motion component
                    LoadProfilesList(value);

                    motionComponent = value;
                    RaisePropertyChanged();
                }
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
                try
                {
                    if (value == null || value == "")
                        MoveArgsCollection = null;
                    else
                        MoveArgsCollection = LoadProfile(SelectedMotionComponent, value);

                    selectedProfile = value;
                }
                catch(Exception ex)
                {
                    PostErrorMessage($"Unable to load profile {value}, {ex.Message}");
                    MoveArgsCollection = null;
                    selectedProfile = null;
                }

                
                RaisePropertyChanged();
            }
            get
            {
                return selectedProfile;
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
                //a.MaxMoveOrder = MotionComponent.Count;
                a.MoveOrder = 1;
                arg.Add(a);
            }

            arg.LogicalMotionComponent = MotionComponent.GetHashString();

            return arg;
        }

        /// <summary>
        /// Load the profiles which belong to the selected logical motion component
        /// </summary>
        /// <returns></returns>
        private void LoadProfilesList(LogicalMotionComponent MotionComponent)
        {
            var dir = PRESET_FOLDER + "\\" + MotionComponent.GetHashString();

            if(Directory.Exists(dir))
            {
                List<string> profiles = new List<string>();

                DirectoryInfo info = new DirectoryInfo(dir);
                foreach(var file in info.GetFiles())
                {
                    if(file.Extension == ".json")
                    {
                        profiles.Add(Path.GetFileNameWithoutExtension(file.FullName));
                    }
                }

                ProfileList = profiles.ToArray();
            }
            else
            {
                ProfileList = null;
            }
        }

        /// <summary>
        /// Load the preset position file of the specified logical motion controller by the file name
        /// </summary>
        /// <param name="LogicalMotionController"></param>
        /// <param name="FileName"></param>
        public MassMoveArgs LoadProfile(LogicalMotionComponent MotionComponent, string FileName)
        {
            // the full file path where we should find the preset profiles
            var fullFilePath = PRESET_FOLDER + "\\" + MotionComponent.GetHashString() + "\\" + FileName + ".json";

            if (File.Exists(fullFilePath) == true)
            {
                var json = File.ReadAllText(fullFilePath);

                JsonSerializerSettings settings = new JsonSerializerSettings();
                settings.Converters.Add(new MassMoveArgsConverter());
                var args = JsonConvert.DeserializeObject<MassMoveArgs>(json, new MassMoveArgsConverter());

                if (args.LogicalMotionComponent != MotionComponent.GetHashString())
                {
                    throw new FormatException("it does not match with the selected motion component.");
                }
                else if (args.HashString != args.GetHashString())
                {
                    throw new FormatException("it might be modified unexpectedly.");
                }
                else
                {
                    return args;
                }                                                                                                                                 
            }
            else
            {
                // the folder does not exist
                throw new FileNotFoundException($"{fullFilePath} does not exist.");
            }
        }

        /// <summary>
        /// Save the preset position profile
        /// </summary>
        /// <param name="Args"></param>
        /// <param name="FileName"></param>
        private void SaveProfile(LogicalMotionComponent MotionComponent, MassMoveArgs Args, string FileName)
        {
            if (MotionComponent == null)
                throw new InvalidDataException("the logical motion controller is empty.");

            if (FileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                throw new InvalidDataException("the name of profile contains invalid chars.");

            // if the directory does not exist, create it.
            var dir = PRESET_FOLDER + "\\" + MotionComponent.GetHashString();
            if (Directory.Exists(dir) == false)
                Directory.CreateDirectory(dir);

            // the full file path where we should find the preset profiles
            var fullFilePath = PRESET_FOLDER + "\\" + MotionComponent.GetHashString() + "\\" + FileName + ".json";

            // calculate the hash string which is used to check whether the profile is modified 
            Args.HashString = Args.GetHashString();

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Converters.Add(new MassMoveArgsConverter());
            var json = JsonConvert.SerializeObject(Args, settings);

            using (FileStream fs = File.Open(fullFilePath, FileMode.OpenOrCreate, FileAccess.Write))
            {
                //BinaryFormatter formatter = new BinaryFormatter();
                //formatter.Serialize(fs, Args);

                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(json);
                    sw.Close();
                }

                    fs.Close();
            }

            // reload the position preset profile list
            LoadProfilesList(MotionComponent);

            new DialogService.DialogService().ShowMessage($"{FileName} has been saved!", "Success");

        }

        private bool CheckProfileExistance(LogicalMotionComponent MotionComponent, string FileName)
        {
            // the full file path where we should find the preset profiles
            var fullFilePath = PRESET_FOLDER + "\\" + MotionComponent.GetHashString() + "\\" + FileName + ".json";

            return File.Exists(fullFilePath);
        }

        #endregion

        #region Commands

        public RelayCommand ReloadCurrentPositions
        {
            get
            {
                return new RelayCommand(() =>
                {
                    SelectedProfile = null;

                    if (SelectedMotionComponent == null)
                    {
                        PostErrorMessage("You should select the motion component.");
                        MoveArgsCollection = null;
                    }
                    else
                    { 
                        MoveArgsCollection = LoadCurrentPositions(SelectedMotionComponent);
                        
                    }
                });
            }
        }

        public RelayCommand Save
        {
            get
            {
                return new RelayCommand(() =>
                {
                    try
                    {
                        DialogService.DialogService ds = new DialogService.DialogService();
                        ds.OpenInputDialog("Position Preset", "Please input the position preset profile name:", null, new Action<string>(filename => 
                        {
                            if (CheckProfileExistance(SelectedMotionComponent, filename) == true) // if the file has existed
                            {
                                if(MessageBox.Show($"The profile {filename} has existed, overwrite it?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                                {
                                    SaveProfile(SelectedMotionComponent, MoveArgsCollection, filename);
                                }
                            }
                            else
                            {
                                SaveProfile(SelectedMotionComponent, MoveArgsCollection, filename);
                            }

                        }));                        
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
