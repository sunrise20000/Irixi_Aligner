using GalaSoft.MvvmLight.Ioc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Irixi_Aligner_Common.Configuration.Common;

namespace Irixi_Aligner_Common.Configuration.ScriptCfg
{
    public class VM_TreeView
    {
        private Node _treeViewItemSource = null;
        public Node TreeViewItemSource { get; set; }
        private Dictionary<string,List<String>> rawDataDic = new Dictionary<string, List<String>>();
        public VM_TreeView()
        {
            ConfigManager cfg =SimpleIoc.Default.GetInstance<ConfigManager>();
            //Script
                //Cat
                    //Name
            _treeViewItemSource = new Node() { Name = "Script" };

            foreach (var it in cfg.FuncManager.Funcs)
            {
                if (!rawDataDic.Keys.Contains(it.Category))
                    rawDataDic.Add(it.Category, new List<string>() { it.FunctionName });
                else
                    rawDataDic[it.Category].Add(it.FunctionName);
            }
            foreach (var dic in rawDataDic)
            {
                List<Node> nl = new List<Node>();
                foreach (string str in dic.Value)
                    nl.Add(new Node() { Name = str });

                _treeViewItemSource.NodeList.Add(new Node() {
                    Name = dic.Key,
                    NodeList = nl
                });
            }

        }
    }

    public class Node
    {
        public string Name { set; get; }
        public List<Node> NodeList{set;get;}
    }
}
