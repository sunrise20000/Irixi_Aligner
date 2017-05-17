using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Irixi_Aligner_Common.Message
{
    public enum MessageType
    {
        Normal,
        Error,
        Good,
        Warning
    }

    public class CMessageHelper : ObservableCollection<CMessageItem>
    {
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            
            base.OnCollectionChanged(e);
            if(e.Action == NotifyCollectionChangedAction.Add)
            {
                var msg = e.NewItems[0] as CMessageItem;
                LogHelper.WriteLine(msg.Message, msg.Type == MessageType.Error ? LogHelper.LogType.ERROR : LogHelper.LogType.NORMAL);
            }
        }
    }

    public class CMessageItem
    {
        #region Constructors

        public CMessageItem(MessageType Type, string Message)
        {
            Ctor(Type, Message);
        }

        public CMessageItem(MessageType Type, string Message, object Arg0)
        {
            Ctor(Type, string.Format(Message, Arg0));
        }

        public CMessageItem(MessageType Type, string Message, object Arg0, object Arg1)
        {
            Ctor(Type, string.Format(Message, Arg0, Arg1));
        }

        public CMessageItem(MessageType Type, string Message, object Arg0, object Arg1, object Arg2)
        {
            Ctor(Type, string.Format(Message, Arg0, Arg1, Arg2));
        }

        public CMessageItem(MessageType Type, string Message, object[] Args)
        {
            Ctor(Type, string.Format(Message, Args));
        }

        #endregion

        #region Properties

        public string Message
        {
            private set;
            get;
        }

        public DateTime Time
        {
            private set;
            get;
        }

        public MessageType Type
        {
            private set;
            get;
        }
        #endregion

        #region Methods

        private void Ctor(MessageType Type, string Message)
        {
            this.Message = Message;
            this.Type = Type;
            this.Time = DateTime.Now;
        }

        public override string ToString()
        {
            return string.Format("{0:HH:mm:ss} {1}", this.Time, this.Message);
        }

        #endregion
    }
}
