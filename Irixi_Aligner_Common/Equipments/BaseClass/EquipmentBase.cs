using Irixi_Aligner_Common.Configuration;
using Irixi_Aligner_Common.Interfaces;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Irixi_Aligner_Common.Equipments
{
    public class EquipmentBase : IDisposable, IEquipmentBase, INotifyPropertyChanged
    {
        #region Variables

        #endregion  

        public EquipmentBase(ConfigurationBase Config)
        {
            this.Config = Config;
            this.DeviceClass = Config.DeviceClass;
            this.Description = Config.Desc;
            this.Port = Config.Port;
            this.IsEnabled = Config.Enabled;
            this.IsInitialized = false;
        }

        #region Properties
        public ConfigurationBase Config { private set; get; }

        Guid _devclass;
        public Guid DeviceClass
        {
            protected set
            {
                UpdateProperty<Guid>(ref _devclass, value);
            }
            get
            {
                return _devclass;
            }
        }
        
        public string Description
        {
            protected set;
            get;
        }

        string _port;
        public string Port
        {
            protected set
            {
                UpdateProperty<string>(ref _port, value);
            }
            get
            {
                return _port;
            }
        }

        bool _enabled;
        public bool IsEnabled
        {
            protected set
            {
                UpdateProperty<bool>(ref _enabled, value);
            }
            get
            {
                return _enabled;
            }
        }

        bool _is_init;
        public bool IsInitialized
        {
            protected set
            {
                UpdateProperty<bool>(ref _is_init, value);
            }
            get
            {
                return _is_init;
            }
        }
        
        public string LastError
        {
            protected set;
            get;
        }

        #endregion

        #region Methods
        public virtual bool Init()
        {
            throw new NotImplementedException();
        }

        #region RaisePropertyChangedEvent

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="OldValue"></param>
        /// <param name="NewValue"></param>
        /// <param name="PropertyName"></param>
        protected void UpdateProperty<T>(ref T OldValue, T NewValue, [CallerMemberName]string PropertyName = "")
        {
            //if (object.Equals(OldValue, NewValue))  // To save resource, if the value is not changed, do not raise the notify event
            //    return;

            OldValue = NewValue;                // Set the property value to the new value
            OnPropertyChanged(PropertyName);    // Raise the notify event
        }

        protected void OnPropertyChanged([CallerMemberName]string PropertyName = "")
        {
            //PropertyChangedEventHandler handler = PropertyChanged;
            //if (handler != null)
            //    handler(this, new PropertyChangedEventArgs(PropertyName));
            //RaisePropertyChanged(PropertyName);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));

        }

        #endregion

        #region IDisposable Support
        protected bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)。
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。

                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~CylinderController() {
        //   // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
        //   Dispose(false);
        // }

        // 添加此代码以正确实现可处置模式。
        public void Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            // TODO: 如果在以上内容中替代了终结器，则取消注释以下行。
            // GC.SuppressFinalize(this);
        }
        #endregion

        public override string ToString()
        {
            return string.Format("*{0}@{1}*", this.Description, this.Port);
        }

        public override int GetHashCode()
        {
            return this.DeviceClass.GetHashCode();
        }

        #endregion
    }
}
