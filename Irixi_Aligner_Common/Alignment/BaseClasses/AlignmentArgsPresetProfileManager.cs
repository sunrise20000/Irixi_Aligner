using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Threading;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Irixi_Aligner_Common.Alignment.AlignmentXD;
using Irixi_Aligner_Common.Alignment.Interfaces;
using Irixi_Aligner_Common.Interfaces;
using Newtonsoft.Json;

namespace Irixi_Aligner_Common.Alignment.BaseClasses
{
    public class AlignmentArgsPresetProfileManager<T, P> : ViewModelBase
        where T: AlignmentArgsBase where P: AlignmentArgsPresetProfileBase, new()
    {
        private const string PROFILE_PATH = "AlignmentProfiles";

        #region Constructors

        public AlignmentArgsPresetProfileManager(T arg)
        {
            this.Arg = arg;
            LoadPresetProfileList();
        }

        #endregion

        #region Properties

        public T Arg
        {
            get;
        }

        private string[] profileList = null;
        public string[] PresetProfileList
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

        private string _selectedPresetProfile = "";

        public string SelectedPresetProfile
        {
            get
            {
                return _selectedPresetProfile;
            }
            set
            {
                if (value != "")
                {
                    var olditem = _selectedPresetProfile;
                    _selectedPresetProfile = value;
                    RaisePropertyChanged();

                    // try to load the content of the specified preset profile
                    try
                    {
                        var profile = LoadContent(value);
                        ((IAlignmentArgsProfile)profile).ToArgsInstance(Arg);
                    }
                    catch (Exception ex)
                    {
                        PostErrorMessage(ex.Message);
                        Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
                        {
                            SelectedPresetProfile = olditem;
                        }), DispatcherPriority.ApplicationIdle);
                    }
                }
            }
        }


        #endregion


        #region Methods

        /// <summary>
        /// Post the error message to the UI
        /// </summary>
        /// <param name="message"></param>
        private void PostErrorMessage(string message)
        {
            Messenger.Default.Send<NotificationMessage<string>>(new NotificationMessage<string>(
                        this,
                        message,
                        "ERROR"));
        }

        private void LoadPresetProfileList()
        {
            var dir = String.Join("\\", new object[]
                {
                    PROFILE_PATH,
                    ((IAlignmentArgs)Arg).SubPath
                });

            if (Directory.Exists(dir))
            {
                List<string> list = new List<string>();

                DirectoryInfo info = new DirectoryInfo(dir);
                foreach (var file in info.GetFiles())
                {
                    if (file.Extension == ".json")
                    {
                        list.Add(Path.GetFileNameWithoutExtension(file.FullName));
                    }
                }

                this.PresetProfileList = list.ToArray();
            }
            else
            {
                this.PresetProfileList = null;
            }
        }

        private void SavePresetProfile(string filename)
        {
            if (filename == null || filename == "")
                throw new ArgumentException("the name of profile can not be empty.");

            // check the validity of the file name
            if (filename.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                throw new InvalidDataException("the name of profile contains invalid chars.");

            // validate the arguments, save is not allowed if some of the arguments are in error format.
            Arg.Validate();
                

            // create the full path
            var dir = String.Join("\\", new object[]
            {
                PROFILE_PATH,
                ((IAlignmentArgs)Arg).SubPath
            });

            // if the directory does not exist, create it.
            if (Directory.Exists(dir) == false)
                Directory.CreateDirectory(dir);

            var profile = new P();
            profile.FromArgsInstance(this.Arg as AlignmentArgsBase);

            var jsonstr = JsonConvert.SerializeObject(profile);
            File.WriteAllText(
                $"{dir}\\{filename}.json",
                jsonstr,
                new UTF8Encoding());

            // reload the preset profile list
            LoadPresetProfileList();
        }


        private P LoadContent(string filename)
        {
            var fullpath = String.Join("\\", new object[]
            {
                PROFILE_PATH,
                ((IAlignmentArgs)Arg).SubPath,
                $"{filename}.json"
            });

            if (File.Exists(fullpath))
            {
                var jsonstr = File.ReadAllText(fullpath);

                var profile = JsonConvert.DeserializeObject<P>(jsonstr);

                // validate the content of profile by hash string
                if (profile.Validate())
                    return profile;
                else
                    throw new Exception("the profile were modified unexpectedly.");
            }
            else
            {
                throw new FileNotFoundException($"the preset profile {filename} was not found.");
            }
        }

        #endregion

        #region Commands

        public RelayCommand CommandSave
        {
            get
            {
                return new RelayCommand(() =>
                {
                    DialogService.DialogService ds = new DialogService.DialogService();

                    if (SelectedPresetProfile != null && SelectedPresetProfile != "")
                    {
                        ds.ShowYesNoMessage($"Are you sure to overwrite the preset profile {SelectedPresetProfile}?", "Overwrite", () =>
                        {
                            try
                            {
                                this.SavePresetProfile(SelectedPresetProfile);
                            }
                            catch (Exception ex)
                            {
                                PostErrorMessage($"Unable to save the preset profile, {ex.Message}");
                            }
                        });

                    }
                    else
                    {
                        CommandSaveAs.Execute(null);
                    }
                });
            }
        }


        public RelayCommand CommandSaveAs
        {
            get
            {
                return new RelayCommand(() =>
                {
                    DialogService.DialogService ds = new DialogService.DialogService();
                    ds.OpenInputDialog("Alignment Parameters Preset", "Please input the name of the preset profile.", null, new Action<string>(filename =>
                    {
                        try
                        {
                            this.SavePresetProfile(filename);

                            // select the profile last saved
                            this.SelectedPresetProfile = filename;
                        }
                        catch (Exception ex)
                        {
                            PostErrorMessage($"Unable to save the preset profile, {ex.Message}");
                        }
                    }));
                });
            }
        
        }
        #endregion


    }
}
