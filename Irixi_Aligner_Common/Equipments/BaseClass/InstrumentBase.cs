using Irixi_Aligner_Common.Configuration.Base;
using Irixi_Aligner_Common.Interfaces;
using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;

namespace Irixi_Aligner_Common.Equipments.Base
{
    public class InstrumentBase : EquipmentBase, IInstrument
    {
        #region Variables

        protected SerialPort serialport;
        protected CancellationTokenSource cts_fetching;
        protected Progress<EventArgs> prgchg_fetching;
        protected Task task_fetch_loop = null;
        int activeChannel;

        #endregion

        #region Constructors

        public InstrumentBase(ConfigurationBase Config) : base(Config)
        {
            IsMultiChannel = false;
            ActiveChannel = 0;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Get whether this instrument contains multiple channel
        /// </summary>
        public bool IsMultiChannel
        {
            protected set;
            get;
        }

        /// <summary>
        /// Get which channel is the active one that represents the return value of fetch() function
        /// </summary>
        public int ActiveChannel
        {
            protected set
            {
                UpdateProperty(ref activeChannel, value);
            }
            get
            {
                return activeChannel;
            }
        }

        /// <summary>
        /// Get what is the unit of the active channel
        /// </summary>
        public int ActiveUnit { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        #endregion

        #region Methods

        public virtual string GetDescription()
        {
            return Read("*IDN?");
        }

        public virtual void Reset()
        {
            Send("*CLS");
            Thread.Sleep(50);

            Send("*RST"); // reset device to default setting
            Thread.Sleep(200);
        }
        
        public override bool Init()
        {
            try
            {
                serialport.Open();

                Task.Delay(100);

                // run user init process function
                UserInitProc();

                this.IsInitialized = true;
                return true;
            }
            catch(Exception ex)
            {
                try
                {
                    serialport.Close();
                }
                catch
                {

                }

                LastError = ex.StackTrace;
                return false;
            }
        }
        
        public virtual double Fetch()
        {
            throw new NotImplementedException();
        }

        public virtual double Fetch(int Channel)
        {
            throw new NotImplementedException();
        }

        public virtual Task<double> FetchAsync()
        {
            return new Task<double>(() =>
            {
                return Fetch();
            });
        }
        
        public virtual Task<double> FetchAsync(int Channel)
        {
            throw new NotImplementedException();
        }

        public virtual void StartAutoFetching()
        {
            // check whether the task had been started
            if (task_fetch_loop == null || task_fetch_loop.IsCompleted)
            {
                // token source to cancel the task
                cts_fetching = new CancellationTokenSource();
                prgchg_fetching = new Progress<EventArgs>(AutoFetchingProgressChanged);

                var token = cts_fetching.Token;

                // start the loop task
                task_fetch_loop = Task.Run(() =>
                {
                    DoAutoFetching(token, prgchg_fetching);
                });

                // if error, throw it
                task_fetch_loop.ContinueWith(t =>
                {
                   throw t.Exception;
                }, TaskContinuationOptions.OnlyOnFaulted);
            }
        }

        public virtual void PauseAutoFetching()
        {
            if (task_fetch_loop != null && task_fetch_loop.IsCompleted == false)
            {
                // cancel the task of fetching loop
                cts_fetching.Cancel();

                TimeSpan ts = TimeSpan.FromMilliseconds(2000);
                if (!task_fetch_loop.Wait(ts))
                    throw new TimeoutException("unable to stop the fetching loop task");

            }
        }
        
        public virtual void ResumeAutoFetching()
        {
            if (task_fetch_loop != null)
                StartAutoFetching();
        }

        public virtual void StopAutoFetching()
        {
            if (task_fetch_loop != null)
            {
                if (task_fetch_loop.IsCompleted == false)
                {
                    PauseAutoFetching();
                }

                // destory the objects
                task_fetch_loop = null;
                cts_fetching = null;
            }
        }
        
        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    try
                    {
                        //! Remember to run the stop function, otherwise the app may NOT exit currectly
                        StopAutoFetching();

                        // run user's dispose process
                        UserDisposeProc();

                        // close serial port and destory the object
                        serialport.Close();
                    }
                    catch
                    {
                        serialport = null;
                    }
                }
                
                disposedValue = true;
            }
        }

        #endregion

        #region Methods implemented by user

        protected virtual void UserInitProc()
        {
            throw new NotImplementedException();
        }

        protected virtual void DoAutoFetching(CancellationToken token, IProgress<EventArgs> progress)
        {
            throw new NotImplementedException();
        }

        protected virtual void AutoFetchingProgressChanged(EventArgs Args)
        {
            throw new NotImplementedException();
        }

        protected virtual void UserDisposeProc()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Private Methods

        protected virtual void Send(string Command)
        {
            try
            {
                lock (serialport)
                {
                    serialport.WriteLine(Command);

                    Thread.Sleep(10);

                    // check if error occured
                    serialport.WriteLine("*ERR?");
                    var ret = serialport.ReadLine();
                    var errornum = ret.Split(',')[0];

                    if (int.TryParse(errornum, out int err_count))
                    {
                        if (err_count != 0) // error occured
                        {
                            // read all errors occured
                            var err = ret.Split(',')[1];
                            throw new InvalidOperationException(err);
                        }
                    }
                    else
                    {
                        throw new InvalidCastException(string.Format("unable to convert error count {0} to number", err_count));
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        protected virtual string Read(string Command)
        {

            try
            {
                lock (serialport)
                {
                    serialport.WriteLine(Command);
                    return serialport.ReadLine().Replace("\r", "").Replace("\n", "");
                }
            }
            catch (TimeoutException)
            {
                // read all errors occured
                serialport.WriteLine("*ERR?");
                this.LastError = serialport.ReadLine();
                throw new InvalidOperationException(this.LastError);
            }
            catch (Exception ex)
            {
                this.LastError = ex.Message;
                throw ex;
            }
        }
        
        #endregion
    }
}
