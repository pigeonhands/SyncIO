namespace SocketNet.UI.Forms
{
    using System;
    using System.Windows.Forms;

    using SocketNet.UI.Controls;
    using SocketNet.UI.Dialogs;
    using SocketNet.UI.Extensions;

    public partial class GroupManager : Form
    {
        private readonly ListViewEx _listView;

        public GroupManager(ListViewEx listView)
        {
            InitializeComponent();

            _listView = listView;
            ListGroups();
        }

        private void BtnRefresh_Click(object sender, EventArgs e) => ListGroups();

        private void BtnNew_Click(object sender, EventArgs e)
        {
            var input = InputBoxDialog.GetInput("Enter a new group name for the selected clients...", "New Group Name", "Default");
            if (string.IsNullOrEmpty(input))
                return;

            _listView.NewGroup(input);
            ListGroups();
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (lvGroups.SelectedItems.Count <= 0)
                return;

            foreach (ListViewItem l in lvGroups.SelectedItems)
            {
                if (string.Compare(l.Text, LvGroupExtensions.DefaultGroup, true) != 0)
                {
                    _listView.DeleteGroup(l.Text);
                }
            }

            ListGroups();
        }

        private void ListGroups()
        {
            lvGroups.Items.Clear();
            foreach (ListViewGroup g in _listView.Groups)
            {
                var imageIndex = string.Compare(g.Header, LvGroupExtensions.DefaultGroup, true) == 0 ? 0 : 1;
                var l = lvGroups.Items.Add(g.Header, imageIndex);
                l.SubItems.Add(g.Items.Count.ToString());
            }
        }
    }
}
