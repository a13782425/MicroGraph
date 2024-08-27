namespace MicroGraph.Editor
{
    /// <summary>
    /// 记录的数据
    /// </summary>
    public class MicroRecordOperateData
    {
        /// <summary>
        /// 记录指令链表
        /// </summary>
        public class RecordCommandLinked
        {
            public readonly IMicroGraphRecordCommand RecordCommand;
            public RecordCommandLinked Next;
            public RecordCommandLinked(IMicroGraphRecordCommand recordCommand)
            {
                this.RecordCommand = recordCommand;
            }
        }
        public BaseMicroGraphView View = default;

        public RecordCommandLinked Record { get; private set; }

        public int RecordId { get; internal set; }

        internal void AddCommand(IMicroGraphRecordCommand command)
        {
            RecordCommandLinked linked = new RecordCommandLinked(command);
            if (Record == null)
            {
                Record = linked;
                return;
            }
            if (Record.RecordCommand.Priority < linked.RecordCommand.Priority)
            {
                linked.Next = Record;
                Record = linked;
                return;
            }
            RecordCommandLinked temp = Record;
            while (temp != null)
            {
                if (temp.Next == null)
                {
                    temp.Next = linked;
                    break;
                }
                if (temp.Next.RecordCommand.Priority < linked.RecordCommand.Priority)
                {
                    linked.Next = temp.Next;
                    temp.Next = linked;
                    break;
                }
                else
                {
                    temp = temp.Next;
                }
            }
        }
    }
}
