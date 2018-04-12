using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace Irixi_Aligner_Common.Classes.BaseClass
{
    [Serializable]
    public class MassMoveArgs : ObservableCollection<AxisMoveArgs>
    {
        #region Variables
        int[] moveOrderList = null;
        #endregion

        #region Constructors

        public MassMoveArgs()
        {

        }

        public MassMoveArgs(IEnumerable<AxisMoveArgs> Collection) : base(Collection)
        {
            CreateMoveOrderList();

            foreach (var item in this)
            {
                ((AxisMoveArgs)item).Container = this;
            }
        }
        
        #endregion

        #region Properties

        public string LogicalMotionComponent { get; set; }
        
        public string HashString { get; set; }

        public int[] MoveOrderList
        {
            private set
            {
                moveOrderList = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("MoveOrderList"));
                //UpdateProperty(ref moveOrderList, value);
            }
            get
            {
                return moveOrderList;
            }
        }

        #endregion

        #region Methods
        
        private void CreateMoveOrderList()
        {
            int[] list = new int[this.Count];
            if (this.Count == 0)
                this.MoveOrderList = null;
            else
            {
                for (int i = 0; i < this.Count; i++)
                    list[i] = i + 1;
                this.MoveOrderList = list;
            }
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CreateMoveOrderList();

            if(e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach(var item in e.NewItems)
                {
                    ((AxisMoveArgs)item).Container = this;
                }
            }

            base.OnCollectionChanged(e);
        }

        public int[] GetDistinctMoveOrder()
        {
            if (this.Count <= 0)
                return null;
            else
            {
                var ret = this.GroupBy(arg => arg.MoveOrder).Select(grp => grp.First().MoveOrder).OrderBy(i => i);
                return ret.ToArray();
            }
        }


        public string GetHashString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(LogicalMotionComponent);

            foreach (var axisArgs in this)
            {
                sb.Append(axisArgs.GetHashString());
            }

            return HashGenerator.GetHashSHA256(sb.ToString());
        }

        public override int GetHashCode()
        {
            return GetHashString().GetHashCode();
        }
        
        #endregion

        #region Commands



        #endregion

    }
}
