namespace Irixi_Aligner_Common.Classes.BaseClass
{
    public class PresetPosition
    {
        public string Name { set; get; }
        public int MotionComponentHashCode { set; get; }
        public PresetPositionItem[] Items { set; get; }
    }

    public class PresetPositionItem
    {
        public int Id { set; get; }
        public int HashCode { set; get; }
        public int MoveOrder { set; get; }
        public int Speed { set; get; }
        public bool IsAbsMode { set; get; }
        public double Position { set; get; }

    }
}
