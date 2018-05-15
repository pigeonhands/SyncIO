namespace ChatServer.UI.Extensions
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
    }
}