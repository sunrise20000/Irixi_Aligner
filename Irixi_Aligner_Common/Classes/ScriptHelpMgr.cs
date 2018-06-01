
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using Irixi_Aligner_Common.Alignment.BaseClasses;
using Irixi_Aligner_Common.Message;
using Irixi_Aligner_Common.ViewModel;
using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Irixi_Aligner_Common.Equipments.Instruments;
using Irixi_Aligner_Common.MotionControllers.Base;
using Irixi_Aligner_Common.Classes.BaseClass;
using Irixi_Aligner_Common.Equipments.Base;
using GalaSoft.MvvmLight.Messaging;
using System.IO;

namespace Irixi_Aligner_Common.Classes
{
    public class ScriptHelpMgr : SingleTon<ScriptHelpMgr>
    {


        public bool bCompile = false;
        public bool bCompileError = true;
        public string lastUnhandledError = "";
        private bool CheckArgs(string funcName, List<object> listPara = null)
        {
            bool bRet = true;
            if (listPara != null)
            {
                if (listPara.Contains(null))
                    bRet = false;
                else
                {
                    int n = listPara.Count();
                    if (n % 2 == 0)
                        for (int i = 0; i < n / 2; i++)
                            bRet &= (listPara[2 * i] as Type) == (listPara[2 * i + 1].GetType());
                }
            }
            if (!bRet)
            {
                bCompileError = true;
                lastUnhandledError = string.Format("Error argument used in function \"{0}]\"", funcName.Replace("LuaF_", ""));
                if (!bCompile)
                    Messenger.Default.Send<string>("", "ScriptStop");
            }
            return bRet && !bCompile;
        }
        private SystemService Systemservice = SimpleIoc.Default.GetInstance<SystemService>();    //封装Systemservice中的函数
        public Dictionary<string, InstrumentBase> InstrumentDic = new Dictionary<string, InstrumentBase>();
        public Object GetDynamicClassByDic(Dictionary<string, string> Dic = null)
        {
            dynamic d = new System.Dynamic.ExpandoObject();
            foreach (var dic in Dic)
            {
                var kp = new KeyValuePair<string, object>(dic.Key, dic.Value);
                (d as ICollection<KeyValuePair<string, object>>).Add(kp);
            }
            return d;
        }
        public KeyValuePair<string, List<KeyValuePair<string, int>>> GetEnumInfo(Type type, string strInstrumentCate)
        {
            Dictionary<string, List<KeyValuePair<string, int>>> dic = new Dictionary<string, List<KeyValuePair<string, int>>>();
            FieldInfo[] fis = type.GetFields();
            int i = 0;
            List<KeyValuePair<string, int>> kpList = new List<KeyValuePair<string, int>>();
            foreach (var fi in fis)
            {
                if (i++ > 0)
                    kpList.Add(new KeyValuePair<string, int>(fi.Name, Convert.ToInt32(fi.GetValue(null))));

            }
            return new KeyValuePair<string, List<KeyValuePair<string, int>>>(strInstrumentCate + "_" + type.Name, kpList);
        }
        public Dictionary<string, List<KeyValuePair<string, List<KeyValuePair<string, int>>>>> GetAllEnuminfo()
        {
            var dic_2 = new Dictionary<string, List<KeyValuePair<string, List<KeyValuePair<string, int>>>>>();
            var enumList = new List<KeyValuePair<string, List<KeyValuePair<string, int>>>>();
            //K2400
            enumList.Add(GetEnumInfo(typeof(Keithley2400.EnumInOutTerminal), "K2400"));
            enumList.Add(GetEnumInfo(typeof(Keithley2400.EnumMeasFunc), "K2400"));
            enumList.Add(GetEnumInfo(typeof(Keithley2400.EnumSourceMode), "K2400"));
            enumList.Add(GetEnumInfo(typeof(Keithley2400.EnumSourceWorkMode), "K2400"));
            enumList.Add(GetEnumInfo(typeof(Keithley2400.EnumSourceRange), "K2400"));
            enumList.Add(GetEnumInfo(typeof(Keithley2400.EnumComplianceLIMIT), "K2400"));
            enumList.Add(GetEnumInfo(typeof(Keithley2400.EnumReadCategory), "K2400"));
            enumList.Add(GetEnumInfo(typeof(Keithley2400.EnumMeasRangeAmps), "K2400"));
            enumList.Add(GetEnumInfo(typeof(Keithley2400.EnumMeasRangeVolts), "K2400"));
            enumList.Add(GetEnumInfo(typeof(Keithley2400.EnumDataStringElements), "K2400"));
            enumList.Add(GetEnumInfo(typeof(Keithley2400.EnumOperationStatus), "K2400"));
            enumList.Add(GetEnumInfo(typeof(Keithley2400.AmpsUnit), "K2400"));
            enumList.Add(GetEnumInfo(typeof(Keithley2400.VoltsUnit), "K2400"));

            //2832C
            enumList.Add(GetEnumInfo(typeof(Newport2832C.EnumChannel), "NP2832C"));
            enumList.Add(GetEnumInfo(typeof(Newport2832C.EnumRange), "NP2832C"));
            enumList.Add(GetEnumInfo(typeof(Newport2832C.EnumUnits), "NP2832C"));
            enumList.Add(GetEnumInfo(typeof(Newport2832C.EnumStatusFlag), "NP2832C"));



            dic_2.Add("ENUM", enumList);
            return dic_2;
        }
        public void InitAllInstrumentAndAxis()
        {
            foreach (var it in Systemservice.MeasurementInstrumentCollection)
                InstrumentDic.Add(it.Config.Caption.Replace(" ", "_"), it);
        }

        //Private
        private void StopScrip(string errorWhy)
        {
            try
            {
                Systemservice.LastMessage = new MessageItem(MessageType.Error, errorWhy);
                Systemservice.CommandStop.Execute(null);    //Stop all axis
                Systemservice.StopAllInstrument();
                Messenger.Default.Send<string>("", "ScriptStop");
            }
            catch (Exception e)
            {

            }

        }

       
        //Wrapper of enums for Lua
        //
   
        public string LuaE_AXIS { get { return "ABCDEFG"; } }
        public object LuaE_INST { get { return GetDynamicClassByDic(); }}
        public object LuaE_ENUM { get { return GetDynamicClassByDic(); } }

        //K2400  Enum      
        public object LuaE_K2400_EnumInOutTerminal { get { return GetDynamicClassByDic(); } }
        public object LuaE_K2400_EnumMeasFunc { get { return GetDynamicClassByDic(); } }
        public object LuaE_K2400_EnumSourceMode { get { return GetDynamicClassByDic(); } }
        public object LuaE_K2400_EnumSourceWorkMode { get { return GetDynamicClassByDic(); } }
        public object LuaE_K2400_EnumSourceRange { get { return GetDynamicClassByDic(); } }
        public object LuaE_K2400_EnumComplianceLIMIT { get { return GetDynamicClassByDic(); } }
        public object LuaE_K2400_EnumReadCategory { get { return GetDynamicClassByDic(); } }
        public object LuaE_K2400_EnumMeasRangeAmps { get { return GetDynamicClassByDic(); } }
        public object LuaE_K2400_EnumMeasRangeVolts { get { return GetDynamicClassByDic(); } }
        public object LuaE_K2400_EnumDataStringElements { get { return GetDynamicClassByDic(); } }
        public object LuaE_K2400_EnumOperationStatus { get { return GetDynamicClassByDic(); } }
        public object LuaE_K2400_AmpsUnit { get { return GetDynamicClassByDic(); } }
        public object LuaE_K2400_VoltsUnit { get { return GetDynamicClassByDic(); } }

        //NewPort2832  Enum
        public object LuaE_NP2832C_EnumChannel { get { return GetDynamicClassByDic(); } }
        public object LuaE_NP2832C_EnumRange { get { return GetDynamicClassByDic(); } }
        public object LuaE_NP2832C_EnumUnits { get { return GetDynamicClassByDic(); } }
        public object LuaE_NP2832C_EnumStatusFlag { get { return GetDynamicClassByDic(); } }


       
        #region Wrapper of functions for Lua
        //Axis
        public void LuaF_Stop()
        {
            if (CheckArgs("LuaF_Stop"))
                Systemservice.CommandStop.Execute(null);
        }
        public void LuaF_MoveAbs(string strAxisName, double fDistance, int nSpeed)
        {
            bool bResult = false;
            if (CheckArgs("LuaF_MoveAbs", new List<object> { typeof(string), strAxisName, typeof(double), fDistance, typeof(int), nSpeed }))
            {
                var axises = (from axis in Systemservice.LogicalAxisCollection where (axis.ToString().Replace(" ", "").Replace("*", "").Replace("@", "_").ToUpper()==strAxisName) select axis);
                if (axises.Count() > 0)
                {
                    var logicalAxis = axises.ElementAt(0) as LogicalAxis;
                    if (logicalAxis != null)
                    {
                        bResult = logicalAxis.PhysicalAxisInst.Move(MoveMode.ABS, nSpeed, fDistance);
                    }
                }
                if (!bResult)
                {
                    StopScrip(string.Format("Error ocurred when axis moving with agrs Name:{0},nSpeed:{1},fDistance:{2}", strAxisName, nSpeed, fDistance));

                }
            }
            
        }
        public void LuaF_MoveRel(string strAxisName, double fDistance, int nSpeed)
        {
            bool bResult = false;
            if (CheckArgs("LuaF_MoveAbs", new List<object> { typeof(string), strAxisName, typeof(double), fDistance, typeof(int), nSpeed }))
            {
                var axises = (from axis in Systemservice.LogicalAxisCollection where axis.ToString().Replace(" ", "").Replace("*", "").Replace("@", "_").ToUpper()== strAxisName select axis);
                if (axises.Count()>0)
                {
                    var logicalAxis = axises.ElementAt(0);
                    if (logicalAxis != null)
                    {
                        if (logicalAxis.PhysicalAxisInst.Move(MoveMode.REL, nSpeed, fDistance))
                            bResult = true;
                    }
                }
                if (!bResult)
                {
                    StopScrip(string.Format("Error ocurred when axis moving with agrs Name:{0},nSpeed:{1},fDistance:{2}", strAxisName, nSpeed, fDistance));
                }
            }
         
        }

        public void LuaF_DoAlignmentXD(string strArgFilePath)
        {
            if (CheckArgs("LuaF_DoAlignmentXD", new List<object> { typeof(string), strArgFilePath }))
            {
                if (File.Exists(strArgFilePath))
                {
                    Systemservice.PopWindow("panelAlignmentXD");
                    Systemservice.DoAlignmentXD(Systemservice.AlignmentXDArgs);
                }
                else
                {
                    StopScrip(string.Format("File:{0} is not exsit", strArgFilePath));
                }

            }
                
        }
        public void LuaF_DoBlindSearch(string strArgFilePath)
        {
            if (CheckArgs("LuaF_DoBlindSearch", new List<object> { typeof(string), strArgFilePath }))
            {
                if (File.Exists(strArgFilePath))
                {
                    Systemservice.PopWindow("panelBlindSearch");
                    Systemservice.DoBlindSearch(Systemservice.SpiralScanArgs);
                }
                else
                {
                    StopScrip(string.Format("File:{0} is not exsit", strArgFilePath));
                }
            }
        }
        public void LuaF_DoSnakeRouteScan(string strArgFilePath)
        {
            if (CheckArgs("LuaF_DoSnakeRouteScan", new List<object> { typeof(string), strArgFilePath }))
            {
                if (File.Exists(strArgFilePath))
                {
                    Systemservice.PopWindow("panelSnakeRouteScan");
                    Systemservice.DoSnakeRouteScan(Systemservice.SnakeRouteScanArgs);
                }
                else
                {
                    StopScrip(string.Format("File:{0} is not exsit", strArgFilePath));
                }
            }
        }
        public void LuaF_DoRotatingScan(string strArgFilePath)
        {
            if (CheckArgs("LuaF_DoRotatingScan", new List<object> { typeof(string), strArgFilePath }))
            {
                if (File.Exists(strArgFilePath))
                {
                    Systemservice.PopWindow("panelRotatingScan");
                    Systemservice.DoRotatingScan(Systemservice.RotatingScanArgs);
                }
                else
                {
                    StopScrip(string.Format("File:{0} is not exsit", strArgFilePath));
                }
            }
        }
        public void LuaF_DoCentralAlign(string strArgFilePath)
        {
            if (CheckArgs("LuaF_DoCentralAlign", new List<object> { typeof(string), strArgFilePath }))
            {
                if (File.Exists(strArgFilePath))
                {
                    Systemservice.PopWindow("panelCentralAlign");
                    Systemservice.DoCentralAlign(Systemservice.CentralAlignArgs);
                }
                else
                {
                    StopScrip(string.Format("File:{0} is not exsit", strArgFilePath));
                }
            }

        }
   
        //IO
        public void LuaF_FiberClampON()
        {
            if (CheckArgs("LuaF_FiberClampON"))
                Systemservice.FiberClampON();
        }
        public void LuaF_FiberClampOFF()
        {
            if (CheckArgs("LuaF_FiberClampOFF"))
                Systemservice.FiberClampOFF();
        }
        public void LuaF_SetFiberClampState(bool bEnable)
        {
            if (CheckArgs("LuaF_SetFiberClampState", new List<object> (){ typeof(bool),bEnable}))
            {
                Systemservice.CylinderController.SetFiberClampState(bEnable ? IrixiStepperControllerHelper.OutputState.Enabled : IrixiStepperControllerHelper.OutputState.Disabled);
                LogHelper.WriteLine(string.Format("Fiber Clamp is {0}", bEnable ? "Opened" : "Closed"), LogHelper.LogType.NORMAL);
            }
        }
        public void LuaF_LensVacuumON()
        {
            if (CheckArgs("LuaF_LensVacuumON"))
                Systemservice.LensVacuumON();
        }
        public void LuaF_LensVacuumOFF()
        {
            if (CheckArgs("LuaF_LensVacuumOFF"))
                Systemservice.LensVacuumOFF();
        }
        public void LuaF_SetLensVacuumState(bool bEnable)
        {
            if (CheckArgs("LuaF_SetLensVacuumState", new List<object>() { typeof(bool), bEnable}))
            {
                Systemservice.CylinderController.SetLensVacuumState(bEnable? IrixiStepperControllerHelper.OutputState.Enabled: IrixiStepperControllerHelper.OutputState.Disabled);
                LogHelper.WriteLine(string.Format("Lens Vacuum is {0}", bEnable ? "Opened":"Closed"), LogHelper.LogType.NORMAL);
            }
        }
        public void LuaF_PlcVacuumON()
        {
            if (CheckArgs("LuaF_PlcVacuumON"))
                Systemservice.PlcVacuumON();
        }
        public void LuaF_PlcVacuumOFF()
        {
            if (CheckArgs("LuaF_PlcVacuumOFF"))
                Systemservice.PlcVacuumOFF();
        }
        public void LuaF_SetPlcVacuumState(bool bEnable)
        {
            if (CheckArgs("LuaF_SetPlcVacuumState", new List<object> (){ typeof(bool), bEnable }))
            {
                Systemservice.CylinderController.SetPlcVacuumState(bEnable ? IrixiStepperControllerHelper.OutputState.Enabled : IrixiStepperControllerHelper.OutputState.Disabled);
                LogHelper.WriteLine(string.Format("Lens Vacuum is {0}", bEnable ? "Opened" : "Closed"), LogHelper.LogType.NORMAL);
            }
        }
        public void LuaF_PodVacuumON()
        {
            if (CheckArgs("LuaF_PodVacuumON"))
                Systemservice.PodVacuumON();
        }
        public void LuaF_PodVacuumOFF()
        {
            if (CheckArgs("LuaF_PodVacuumOFF"))
                Systemservice.PodVacuumOFF();
        }
        public void LuaF_SetPodVacuumState(bool bEnable)
        {
            if (CheckArgs("LuaF_SetPodVacuumState", new List<object>() { typeof(bool), bEnable }))
            {
                Systemservice.CylinderController.SetPodVacuumState(bEnable ? IrixiStepperControllerHelper.OutputState.Enabled : IrixiStepperControllerHelper.OutputState.Disabled);
                LogHelper.WriteLine(string.Format("Lens Vacuum is {0}", bEnable ? "Opened" : "Closed"), LogHelper.LogType.NORMAL);
            }
        }

        //System
        public void LuaF_Sleep(int nMs)
        {
            if (CheckArgs("LuaF_Sleep", new List<object> { typeof(int), nMs }))
            {
                Thread.Sleep(nMs);
            }
        }
        public void LuaF_Trace(string str)
        {
            if (CheckArgs("LuaF_Trace", new List<object> { typeof(string), str }))
                Application.Current.Dispatcher.Invoke(() => Systemservice.SetLastMessage(MessageType.Good, str));
        }
        public void LuaF_Error(string str)
        {
            if (CheckArgs("LuaF_Error", new List<object> { typeof(string), str }))
                Application.Current.Dispatcher.Invoke(() => Systemservice.SetLastMessage(MessageType.Error, str));
        }
        public void LuaF_Pause()
        {
            if (CheckArgs("LuaF_Pause"))
                Systemservice.Pause();
        }


        //Equipment 
        public void LuaF_K2400_SetToVoltageSource(string InstrumentName)
        {
            if (CheckArgs("LuaF_K2400_SetToVoltageSource", new List<object>() { typeof(string), InstrumentName }))
            {
                try
                {
                    if (InstrumentDic.Keys.Contains(InstrumentName))
                        (InstrumentDic[InstrumentName] as Keithley2400).SetToVoltageSource();
                    else
                    {
                        StopScrip(string.Format("Can't find instrument{0}", InstrumentName));
                    }
                }
                catch (Exception ex)
                {
                    StopScrip(string.Format("unable to set K2400 to voltage source mode, {0}", ex.Message));
                }
            }
                
        }
		public void LuaF_K2400_SetToCurrentSource(string InstrumentName)
        {
            if (CheckArgs("LuaF_K2400_SetToVoltageSource", new List<object>() { typeof(string), InstrumentName }))
            {
                try
                {
                    if (InstrumentDic.Keys.Contains(InstrumentName))
                        (InstrumentDic[InstrumentName] as Keithley2400).SetToCurrentSource();
                    else
                    {
                        StopScrip(string.Format("Can't find instrument{0}", InstrumentName));
                    }
                }
                catch (Exception ex)
                {
                    StopScrip(string.Format("unable to set set K2400 to current source mode, {0}", ex.Message));
                }
            }     
        }
        public void LuaF_K2400_SetCompliance(string InstrumentName,double target)
        {
            if (CheckArgs("LuaF_K2400_SetToVoltageSource", new List<object>() { typeof(string), InstrumentName, typeof(double), target }))
            {
                try
                {
                    if (InstrumentDic.Keys.Contains(InstrumentName))
                    {
                        var K2400 = (InstrumentDic[InstrumentName] as Keithley2400);
                        if (K2400.SourceMode == Keithley2400.EnumSourceMode.CURR)
                        {
                            K2400.SetComplianceVoltage(Keithley2400.EnumComplianceLIMIT.REAL, target);
                        }
                        else if (K2400.SourceMode == Keithley2400.EnumSourceMode.VOLT)
                        {
                            K2400.SetComplianceCurrent(Keithley2400.EnumComplianceLIMIT.REAL, target);
                        }
                    }
                    else
                    {
                        StopScrip(string.Format("Can't find instrument{0}", InstrumentName));
                    }
                }
                catch (Exception ex)
                {
                    StopScrip(string.Format("unable to set compliance, {0}", ex.Message));
                }
            }
        }
        public void LuaF_K2400_SetCurrentMeasurementRange(string InstrumentName,string K2400_EnumMeasRangeAmps)
        {
            if (CheckArgs("LuaF_K2400_SetToVoltageSource", new List<object>() { typeof(string), InstrumentName, typeof(string), K2400_EnumMeasRangeAmps }))
            {
                try
                {
                    if (InstrumentDic.Keys.Contains(InstrumentName))
                    {
                        var K2400 = (InstrumentDic[InstrumentName] as Keithley2400);
                        var enumInfo = GetEnumInfo(typeof(Keithley2400.EnumMeasRangeAmps), "K2400");
                        var values = from kp in enumInfo.Value where kp.Key == K2400_EnumMeasRangeAmps select kp.Value;
                        if (values.Count() > 0)
                        {
                            var enumValue = (Keithley2400.EnumMeasRangeAmps)values.ElementAt(0);
                            K2400.SetMeasRangeOfAmps(enumValue);
                        }
                    }
                    else
                    {
                        StopScrip(string.Format("Can't find instrument{0}", InstrumentName));
                    }
                }
                catch (Exception ex)
                {
                    StopScrip(string.Format("unable to set measurement range, {0}", ex.Message));
                }
            }
        }
        public void LuaF_K2400_SetVoltageMeasurementRange(string InstrumentName, string K2400_EnumMeasRangeVolts)
        {
            if (CheckArgs("LuaF_K2400_SetToVoltageSource", new List<object>() { typeof(string), InstrumentName, typeof(string), K2400_EnumMeasRangeVolts }))
            {
                
                try
                {
                    if (InstrumentDic.Keys.Contains(InstrumentName))
                    {
                        var K2400 = (InstrumentDic[InstrumentName] as Keithley2400);
                        var enumInfo = GetEnumInfo(typeof(Keithley2400.EnumMeasRangeVolts), "K2400");
                        var values = from kp in enumInfo.Value where kp.Key == K2400_EnumMeasRangeVolts select kp.Value;
                        if (values.Count() > 0)
                        {
                            var enumValue = (Keithley2400.EnumMeasRangeVolts)values.ElementAt(0);
                            K2400.SetMeasRangeOfVolts(enumValue);
                        }
                    }
                    else
                    {
                        StopScrip(string.Format("Can't find instrument{0}", InstrumentName));
                    }
                }
                catch (Exception ex)
                {
                    StopScrip(string.Format("unable to set measurement range, {0}", ex.Message));
                }

            }
        }
        public void LuaF_K2400_SetOutputLevel(string InstrumentName,double target)
        {
            if (CheckArgs("LuaF_K2400_SetOutputLevel", new List<object>() { typeof(string), InstrumentName, typeof(double), target }))
            { 
                try
                {
                    if (InstrumentDic.Keys.Contains(InstrumentName))
                    {
                        var K2400 = (InstrumentDic[InstrumentName] as Keithley2400);
                        if (K2400.SourceMode == Keithley2400.EnumSourceMode.CURR)
                        {
                            K2400.SetCurrentSourceLevel(target);
                        }
                        else if (K2400.SourceMode == Keithley2400.EnumSourceMode.VOLT)
                        {
                            K2400.SetVoltageSourceLevel(target);
                        }
                    }
                    else
                    {
                        StopScrip(string.Format("Can't find instrument{0}", InstrumentName));
                    }
                }
                catch (Exception ex)
                {
                    StopScrip((string.Format("unable to set compliance, {0}", ex.Message)));
                }
            }
        }
        public void LuaF_K2400_SetOutputEnabled(string InstrumentName,bool Enable)
        {
            if (CheckArgs("LuaF_K2400_SetOutputLevel", new List<object>() { typeof(string), InstrumentName, typeof(bool), Enable }))
            {
                try
                {
                    if (InstrumentDic.Keys.Contains(InstrumentName))
                    {
                        var K2400 = (InstrumentDic[InstrumentName] as Keithley2400);
                        if (Enable)
                        {
                            Systemservice.PopWindow(InstrumentName);
                            if (K2400.IsOutputEnabled == false)
                            {
                                K2400.SetOutputState(true);
                                K2400.StartAutoFetching();    //后台一直Fetch
                            }
                        }
                        else
                        {
                            //K2400.StopAutoFetching();
                            K2400.SetOutputState(false);
                        }
                    }
                    else
                    {
                        StopScrip(string.Format("Can't find instrument{0}", InstrumentName));
                    }
                }
                catch (Exception ex)
                {
                    StopScrip(string.Format("error occurred while setting output state, {0}", ex.Message));
                }
            }
        }
		public void LuaF_K2400_SetInOutTerminal(string InstrumentName,string K2400_EnumInOutTerminal)
        {
            if (CheckArgs("LuaF_K2400_SetOutputLevel", new List<object>() { typeof(string), InstrumentName, typeof(string), K2400_EnumInOutTerminal }))
            {
                try
                {
                    if (InstrumentDic.Keys.Contains(InstrumentName))
                    {
                        var K2400 = (InstrumentDic[InstrumentName] as Keithley2400);
                        var enumInfo = GetEnumInfo(typeof(Keithley2400.EnumInOutTerminal), "K2400");
                        var values = from kp in enumInfo.Value where kp.Key == K2400_EnumInOutTerminal select kp.Value;
                        if (values.Count() > 0)
                        {
                            var enumValue = (Keithley2400.EnumInOutTerminal)values.ElementAt(0);
                            K2400.SetInOutTerminal(enumValue);
                        }
                    }
                    else
                    {
                        StopScrip(string.Format("Can't find instrument{0}", InstrumentName));
                    }
                }
                catch (Exception ex)
                {
                    StopScrip(string.Format("unable to switch In/Out terminal, {0}", ex.Message));
                }
            }
           
        }
        public void LuaF_NP2832C_SetActiveChannel(string InstrumentName, string NP2832C_EnumChannel)
        {
            
            if (CheckArgs("LuaF_NP2832C_SetActiveChannel", new List<object>() { typeof(string), InstrumentName, typeof(string), NP2832C_EnumChannel }))
            {
                try
                {
                    if (InstrumentDic.Keys.Contains(InstrumentName))
                    {
                        var NP2832C = InstrumentDic[InstrumentName] as Newport2832C;
                        var enumInfo = GetEnumInfo(typeof(Newport2832C.EnumChannel), "NP2832C");
                        var values = from kp in enumInfo.Value where kp.Key == NP2832C_EnumChannel select kp.Value;
                        if (values.Count() > 0)
                        {
                            var enumValue = (Newport2832C.EnumChannel)values.ElementAt(0);
                            NP2832C.SetDisplayChannel(enumValue);
                        }
                    }
                    else
                    {
                        StopScrip(string.Format("Can't find instrument{0}", InstrumentName));
                    }
                }
                catch (Exception ex)
                {
                    StopScrip(string.Format("unable to set active channel to {0}, {1}", NP2832C_EnumChannel,ex.Message));
                }

            }
        }
        public void LuaF_NP2832C_SetLambda(string InstrumentName, string NP2832C_EnumChannel, int nLambda)
        {
            if (CheckArgs("LuaF_NP2832C_SetActiveChannel", new List<object>() { typeof(string), InstrumentName, typeof(string), NP2832C_EnumChannel, typeof(int), nLambda }))
            {
                try
                {
                    if (InstrumentDic.Keys.Contains(InstrumentName))
                    {
                        Newport2832C NP2832C = InstrumentDic[InstrumentName] as Newport2832C;
                        var enumInfo = GetEnumInfo(typeof(Newport2832C.EnumChannel), "NP2832C");
                        var values = from kp in enumInfo.Value where kp.Key == NP2832C_EnumChannel select kp.Value;
                        var enumValue = (Newport2832C.EnumChannel)values.ElementAt(0);
                        int snap = NP2832C.MetaProperties[values.ElementAt(0)].Lambda;
                        NP2832C.SetLambda(enumValue, nLambda);
                    }
                    else
                    {
                        StopScrip(string.Format("Can't find instrument{0}", InstrumentName));
                    }
                }
                catch (Exception ex)
                {
                    StopScrip(string.Format("Unable to set unit, {0}", ex.Message));
                }
            }
        }
        public void LuaF_NP2832C_SetRange(string InstrumentName, string NP2832C_EnumChannel,string NP2832C_EnumRange)
        {
            if (CheckArgs("LuaF_NP2832C_SetActiveChannel", new List<object>() { typeof(string), InstrumentName, typeof(string), NP2832C_EnumChannel, typeof(string), NP2832C_EnumRange }))
            {
                try
                {
                    if (InstrumentDic.Keys.Contains(InstrumentName))
                    {
                        var NP2832C = InstrumentDic[InstrumentName] as Newport2832C;
                        var enumChannel = GetEnumInfo(typeof(Newport2832C.EnumChannel), "NP2832C");
                        var enumUnit = GetEnumInfo(typeof(Newport2832C.EnumRange), "NP2832C");
                        var valuesChannel = from kp in enumChannel.Value where kp.Key == NP2832C_EnumChannel select kp.Value;
                        var valuesRange = from kp in enumUnit.Value where kp.Key == NP2832C_EnumRange select kp.Value;
                        if (valuesChannel.Count() > 0 && valuesRange.Count() > 0)
                        {
                            var nChannel = (Newport2832C.EnumChannel)valuesChannel.ElementAt(0);
                            var nRange = (Newport2832C.EnumRange)valuesRange.ElementAt(0);
                            NP2832C.SetMeasurementRange(nChannel, nRange);
                        }
                    }
                    else
                    {
                        StopScrip(string.Format("Can't find instrument{0}", InstrumentName));
                    }
                }
                catch (Exception ex)
                {
                    StopScrip(string.Format("Unable to set unit, {0}", ex.Message));
                }
            }
        }
        public void LuaF_NP2832C_SetUnit(string InstrumentName, string NP2832C_EnumChannel, string NP2832C_EnumUnits)
        {
            if (CheckArgs("LuaF_NP2832C_SetActiveChannel", new List<object>() { typeof(string), InstrumentName, typeof(string), NP2832C_EnumChannel, typeof(string), NP2832C_EnumUnits }))
            {
                try
                {
                    if (InstrumentDic.Keys.Contains(InstrumentName))
                    {
                        var NP2832C = InstrumentDic[InstrumentName] as Newport2832C;
                        var enumChannel = GetEnumInfo(typeof(Newport2832C.EnumChannel), "NP2832C");
                        var enumUnit = GetEnumInfo(typeof(Newport2832C.EnumRange), "NP2832C");
                        var valuesChannel = from kp in enumChannel.Value where kp.Key == NP2832C_EnumChannel select kp.Value;
                        var valuesUnit = from kp in enumUnit.Value where kp.Key == NP2832C_EnumUnits select kp.Value;
                        if (valuesChannel.Count() > 0 && valuesUnit.Count() > 0)
                        {
                            var nChannel = (Newport2832C.EnumChannel)valuesChannel.ElementAt(0);
                            var nUnit = (Newport2832C.EnumUnits)valuesUnit.ElementAt(0);
                            NP2832C.SetUnit(nChannel, nUnit);
                        }
                    }
                    else
                    {
                        StopScrip(string.Format("Can't find instrument{0}", InstrumentName));
                    }
                }
                catch (Exception ex)
                {
                    StopScrip(string.Format("Unable to set unit, {0}", ex.Message));
                }
            }
        }

        #endregion
    }
    public class SingleTon<T> where T : new()
    {
        private static readonly Lazy<T> _instance = new Lazy<T>(() => new T());
        public static T Instance
        {
            get { return _instance.Value; }
        }
        
    }
}
