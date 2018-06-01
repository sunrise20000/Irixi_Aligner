using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HalconDotNet;

namespace Irixi_Aligner_Common.Vision
{
    public class Vision
    {
        #region constructor
        private Vision() { }
        private static readonly Lazy<Vision> _instance = new Lazy<Vision>(() => new Vision());
        public static Vision Instance
        {
            get { return _instance.Value; }
        }
        #endregion

        #region  var
        public enum IMAGEPROCESS_STEP
        {
            T1,
            T2,
            T3,
            T4           
        }
        public delegate void ProcessDe(int nCamID);
        private List<HObject> HoImageList = new List<HObject>();    //Image
        private List<HTuple> HAque = new List<HTuple>();            //Aqu
        private List<HTuple> HWindow = new List<HTuple>();          //Hwindow
        #endregion

        #region public method 
        public bool OpenCam(int nCamID)
        {
            return true;
        }
        public bool CloseCam(int nCamID)
        {
            return true;
        }
        public bool IsCamOpen(int nCamID)
        {
            return false;
        }     
        public bool ProcessImage(IMAGEPROCESS_STEP nStep,int nCamID,object para,out object result)
        {
            try
            {
                switch (nStep)
                {
                    case IMAGEPROCESS_STEP.T1:
                        break;
                    case IMAGEPROCESS_STEP.T2:
                        break;
                    default:
                        break;
                }
                result = null;
                return true;
            }
            catch (Exception ex)
            {
                result = null;
                return false;
            }
        }
        #endregion

        #region private method

        #endregion

    }
}
