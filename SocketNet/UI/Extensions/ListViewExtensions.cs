namespace SocketNet.UI.Extensions
{
    using System;
    using System.Windows.Forms;

    public static class ListViewExtensions
    {
        public static int IndexFromKey(this ListView lv, string key)
        {
            for (var i = 0; i < lv.Columns.Count; i++)
            {
                if (string.Compare(lv.Columns[i].Name, key, true) == 0)
                {
                    return i;
                }
            }
            return -1;
        }

        public static int ColumnIndex(this ListView lv, string colName)
        {
            try
            {
                return lv.Columns[colName].Index;
            }
            catch
            {
                return -1;
            }
        }
    }
}