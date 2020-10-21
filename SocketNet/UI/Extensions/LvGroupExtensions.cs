namespace SocketNet.UI.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Forms;

    using SocketNet.UI.Controls;

    static class LvGroupExtensions
    {
        public static string DefaultGroup = "Default"; // "Unclassed Clients";

        public static void GroupItem(this ListViewEx lv, ListViewItem l, string colHeader)
        {
            // This flag will tell us if proper group already exists
            bool groupExists = false;
            // Check each group if it fits to the item
            foreach (ListViewGroup g in lv.Groups)
            {
                // Compare group's header to selected subitem's text
                if (g.Header == colHeader)
                {
                    // Add item to the group.
                    // Alternative is: group.Items.Add(item);
                    l.Group = g;
                    groupExists = true;
                    break;
                }
            }
            // Create new group if no proper group was found
            if (!groupExists)
            {
                ListViewGroup g = new ListViewGroup(colHeader);
                // We need to add the group to the ListView first
                lv.Groups.Add(g);
                l.Group = g;
            }

            //lv.SetGroupState(ListViewGroupState.Collapsible);
        }

        public static ListViewGroup GetGroup(this ListViewEx lv, string gHeader)
        {
            foreach (ListViewGroup g in lv.Groups)
            {
                if (g.Header == gHeader)
                    return g;
            }

            return null;
        }

        public static void NewGroup(this ListViewEx lv, string gHeader)
        {
            ListViewGroup g = new ListViewGroup(gHeader);
            if (!lv.Groups.Contains(g) && g.Header != DefaultGroup) lv.Groups.Add(g);
        }

        public static void DeleteGroup(this ListViewEx lv, string gHeader)
        {
            List<ListViewItem> removeList = new List<ListViewItem>();
            foreach (ListViewItem l in GetGroup(lv, gHeader).Items)
            {
                removeList.Add(l);
            }

            foreach (ListViewItem l in removeList)
            {
                GroupItem(lv, l, DefaultGroup);
            }
            lv.Groups.Remove(GetGroup(lv, gHeader));
        }
    }
}