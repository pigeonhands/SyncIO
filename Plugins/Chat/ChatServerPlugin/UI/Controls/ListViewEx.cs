namespace ChatServerPlugin.UI.Controls
{
    /*
        Uses new Windows visual styles
        Movable column headers
        Collapsible listviewgroups
        Completely header and sub header sortable
    */

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using System.Windows.Forms.VisualStyles;

    public class ListViewEx : ListView
    {
        #region Constants

        private const int LVIF_IMAGE = 0x2;
        private const int HDF_SORTUP = 0x400;
        private const int HDF_SORTDOWN = 0x200;
        private const int HDM_GETITEM = 0x120b;
        private const int HDM_SETITEM = 0x120c;
        private const int HDI_FORMAT = 4;

        private const int WM_HSCROLL = 0x114;
        private const int WM_VSCROLL = 0x115;

        private const string user32 = "user32.dll";
        private const string uxtheme = "uxtheme.dll";

        #endregion

        #region Variables

        private int _columnIndexCurrent = -1;
        private int _columnIndexPrevious = -1;
        private bool _drawSubItemIcons = true;
        private bool _drawCheckBoxes;
        private SortOrder sortOrder = SortOrder.None;

        private readonly Dictionary<string, List<string>> _comboList;
        private readonly Dictionary<string, ComboBox> _combos;
        private ListViewItem _item;
        private int _selectedSubItem;
        private int _x;
        private int _y;

        private bool _clicked;
        private CheckBoxState _state;

        #endregion

        #region Properties

        public int ColumnIndex
        {
            get { return _columnIndexCurrent; }
            set { _columnIndexCurrent = value; }
        }

        public int PreviousColumnIndex
        {
            get { return _columnIndexPrevious; }
        }

        public bool DrawCheckBoxes
        {
            get { return _drawCheckBoxes; }
            set
            {
                _drawCheckBoxes = value;
                ShowCheckBoxes(_drawCheckBoxes);
                Invalidate();
            }
        }

        public bool DrawSubItemIcons
        {
            get { return _drawSubItemIcons; }
            set
            {
                _drawSubItemIcons = value;
                ShowSubItemIcons(_drawSubItemIcons);
                Invalidate();
            }
        }

        #endregion

        #region Constructors

        public ListViewEx()
        {
            DoubleBuffered = true;
            OwnerDraw = true;
            View = View.Details;
            FullRowSelect = true;

            ContextMenuStrip = SelectedItems.Count > 0 ? ContextMenuStrip : null;
            _comboList = new Dictionary<string, List<string>>();
            _combos = new Dictionary<string, ComboBox>();
        }

        #endregion

        #region Enumerations

        public enum GroupState
        {
            COLLAPSIBLE = 8,
            COLLAPSED = 1,
            EXPANDED = 0
        }

        public enum ListViewGroupMask : uint
        {
            None = 0x00000,
            Header = 0x00001,
            Footer = 0x00002,
            State = 0x00004,
            Align = 0x00008,
            GroupId = 0x00010,
            SubTitle = 0x00100,
            Task = 0x00200,
            DescriptionTop = 0x00400,
            DescriptionBottom = 0x00800,
            TitleImage = 0x01000,
            ExtendedImage = 0x02000,
            Items = 0x04000,
            Subset = 0x08000,
            SubsetItems = 0x10000
        }

        [Flags]
        public enum LVS_EX : uint
        {
            GRIDLINES = 0x00000001,
            SUBITEMIMAGES = 0x00000002,
            CHECKBOXES = 0x00000004,
            TRACKSELECT = 0x00000008,
            HEADERDRAGDROP = 0x00000010,
            FULLROWSELECT = 0x00000020,
            ONECLICKACTIVATE = 0x00000040,
            TWOCLICKACTIVATE = 0x00000080,
            FLATSB = 0x00000100,
            REGIONAL = 0x00000200,
            INFOTIP = 0x00000400,
            UNDERLINEHOT = 0x00000800,
            UNDERLINECOLD = 0x00001000,
            MULTIWORKAREAS = 0x00002000,
            LABELTIP = 0x00004000,
            BORDERSELECT = 0x00008000,
            DOUBLEBUFFER = 0x00010000,
            HIDELABELS = 0x00020000,
            SINGLEROW = 0x00040000,
            SNAPTOGRID = 0x00080000,
            SIMPLESELECT = 0x00100000
        }

        public enum LVM : uint
        {
            FIRST = 0x1000,
            SETITEM = FIRST + 6,
            SETGROUPINFO = FIRST + 147,
            GETHEADER = 0x101f,
            SETEXTENDEDLISTVIEWSTYLE = (FIRST + 54),
            GETEXTENDEDLISTVIEWSTYLE = (FIRST + 55),
        }

        #endregion

        #region Delegates

        private delegate void CallbackSetGroupString(ListViewGroup lstvwgrp, string value);

        #endregion

        #region Protected Overrides

        protected override void CreateHandle()
        {
            base.CreateHandle();
            // Change the style of listview to accept image on subitems
            Message m = new Message()
            {
                HWnd = Handle,
                Msg = (int)LVM.SETEXTENDEDLISTVIEWSTYLE,
                LParam = (IntPtr)
                (
                    LVS_EX.SUBITEMIMAGES |
                    LVS_EX.DOUBLEBUFFER |
                    LVS_EX.FULLROWSELECT |
                    LVS_EX.GRIDLINES |
                    LVS_EX.HEADERDRAGDROP |
                    LVS_EX.LABELTIP |
                    LVS_EX.INFOTIP /*
                    LVS_EX.REPORT   */
                ),
                WParam = IntPtr.Zero
            };

            SetWindowTheme(Handle, "explorer", null);
            WndProc(ref m);

        }

        protected override void OnColumnClick(ColumnClickEventArgs e)
        {
            if (e.Column == _columnIndexCurrent)
            {
                sortOrder = sortOrder == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
            }
            else
            {
                _columnIndexCurrent = e.Column;
                sortOrder = SortOrder.Ascending;
            }

            ListViewItemSorter = new Comparer(_columnIndexCurrent, sortOrder);
            Sort();
            SortColumns(e.Column);

            base.OnColumnClick(e);

            if (!DrawCheckBoxes)
                return;

            if (!_clicked)
            {
                _clicked = true;
                _state = CheckBoxState.CheckedPressed;

                foreach (ListViewItem item in Items)
                {
                    item.Checked = true;
                }

                Invalidate();
            }
            else
            {
                _clicked = false;
                _state = CheckBoxState.UncheckedNormal;
                Invalidate();

                foreach (ListViewItem item in Items)
                {
                    item.Checked = false;
                }
            }
        }

        protected override void OnDoubleClick(EventArgs e)
        {
            base.OnDoubleClick(e);

            if (!DrawCheckBoxes)
                return;

            // Check whether the subitem was clicked
            var start = _x;
            var position = 0;
            var end = Columns[0].Width;
            for (int i = 0; i < Columns.Count; i++)
            {
                if (start > position && start < end)
                {
                    _selectedSubItem = i;
                    break;
                }

                position = end;
                end += Columns[i].Width;
            }

            var selectedText = _item.SubItems[_selectedSubItem].Text;

            var column = Columns[_selectedSubItem].Text;
            if (_comboList.ContainsKey(column))
            {
                var r = new Rectangle(position, _item.Bounds.Top, end, _item.Bounds.Bottom);
                //_combos[column].Size = new Size(end - position, _item.Bounds.Bottom - _item.Bounds.Top);
                _combos[column].Location = new Point(position, _item.Bounds.Y);
                _combos[column].Show();
                _combos[column].Text = selectedText;
                _combos[column].SelectAll();
                _combos[column].Focus();
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            _item = GetItemAt(e.X, e.Y);
            _x = e.X;
            _y = e.Y;
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case 0x1000 + 145:
                    // LVM_INSERTGROUP = LVM_FIRST + 145;
                    // Add the collapsible feature id needed
                    LVGROUP lvgroup = (LVGROUP)Marshal.PtrToStructure(m.LParam, typeof(LVGROUP));
                    lvgroup.state = (int)GroupState.COLLAPSIBLE;
                    // LVGS_COLLAPSIBLE
                    lvgroup.mask ^= 4;
                    // LVGF_STATE
                    Marshal.StructureToPtr(lvgroup, m.LParam, true);
                    break;
                case 0x202:
                    //WM_LBUTTONUP
                    base.DefWndProc(ref m);
                    break;
                /*case 0x83: // WM_NCCALCSIZE
                    int style = (int)GetWindowLong(this.Handle, GwlStyle);
                    if ((style & WsHscroll) == WsHscroll)
                        SetWindowLong(this.Handle, GwlStyle, style & ~WsHscroll);
                    break;*/
                case WM_VSCROLL:
                case WM_HSCROLL:
                    Focus();
                    break;
            }

            base.WndProc(ref m);
        }

        protected override void OnDrawColumnHeader(DrawListViewColumnHeaderEventArgs e)
        {
            if (DrawCheckBoxes)
            {
                CheckBoxRenderer.DrawCheckBox(e.Graphics, new Point(ClientRectangle.Location.X + 4, ClientRectangle.Location.Y + 5), _state);
            }
            //e.DrawText(TextFormatFlags.LeftAndRightPadding);
            e.DrawDefault = true;
        }

        protected override void OnDrawItem(DrawListViewItemEventArgs e) => e.DrawDefault = true;

        protected override void OnDrawSubItem(DrawListViewSubItemEventArgs e) => e.DrawDefault = true;

        #endregion

        #region Public Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lvg"></param>
        /// <param name="footerText"></param>
        public void SetGroupFooter(ListViewGroup lvg, string footerText)
        {
            SetGrpFooter(lvg, footerText);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="imageIndex"></param>
        public void AddSubItemIcon(ListViewItem row, int col, int imageIndex)
        {
            AddSubItemIcon(row.Index, col, imageIndex);
        }

        public void AddSubItemIcon(int row, int col, int imageIndex)
        {
            var lvi = new LV_ITEM
            {
                iItem = row,          // Row.
                iSubItem = col,       // Column.
                uiMask = LVIF_IMAGE,  // We're setting the image.
                iImage = imageIndex    // The image index in the ImageList.
            };
            SendMessage(Handle, (uint)LVM.SETITEM, 0, ref lvi);
        }

        public void AddComboBox(string columnName, List<string> items)
        {
            if (!_comboList.ContainsKey(columnName))
            {
                _comboList.Add(columnName, items);

                var cb = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
                cb.SelectedIndexChanged += ComboBoxSelected;
                cb.LostFocus += ComboBoxFocusExit;
                cb.KeyPress += ComboBoxKeyPress;
                cb.Hide();
                cb.Items.AddRange(items.ToArray());
                Controls.Add(cb);

                _combos.Add(columnName, cb);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="show"></param>
        private void ShowSubItemIcons(bool show)
        {
            // Get the current style.
            int style = SendMessage(Handle, (int)LVM.GETEXTENDEDLISTVIEWSTYLE, 0, IntPtr.Zero);
            // Show or hide sub-item icons.
            if (show)
                style |= (int)LVS_EX.SUBITEMIMAGES;
            else
                style &= ~((int)LVS_EX.SUBITEMIMAGES);

            SendMessage(Handle, (int)LVM.SETEXTENDEDLISTVIEWSTYLE, 0, style);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="show"></param>
        private void ShowCheckBoxes(bool show)
        {
            // Get the current style.
            int style = SendMessage(Handle, (int)LVM.GETEXTENDEDLISTVIEWSTYLE, 0, IntPtr.Zero);

            // Show or hide sub-item icons.
            if (show)
                style |= (int)LVS_EX.CHECKBOXES;
            else
                style &= ~((int)LVS_EX.CHECKBOXES);

            SendMessage(Handle, (int)LVM.SETEXTENDEDLISTVIEWSTYLE, 0, style);
        }

        private void SortColumns(int columnIndex)
        {
            Console.WriteLine(columnIndex);
            var ptrHeader = SendMessage(Handle, (int)LVM.GETHEADER, 0, 0);
            var ptrCurrentColumnIndex = new IntPtr(_columnIndexCurrent);
            var ptrPreviousColumnIndex = new IntPtr(_columnIndexPrevious);
            if ((_columnIndexPrevious != -1) && (_columnIndexPrevious != _columnIndexCurrent))
            {
                var hdiPreviousItem = new HDITEM
                {
                    mask = HDI_FORMAT
                };
                SendMessage(ptrHeader, HDM_GETITEM, ptrPreviousColumnIndex, ref hdiPreviousItem);
                hdiPreviousItem.fmt = (hdiPreviousItem.fmt & ~HDF_SORTDOWN) & ~HDF_SORTUP;
                SendMessage(ptrHeader, HDM_SETITEM, ptrPreviousColumnIndex, ref hdiPreviousItem);
            }
            var hdiCurrentItem = new HDITEM
            {
                mask = HDI_FORMAT
            };
            SendMessage(ptrHeader, HDM_GETITEM, ptrCurrentColumnIndex, ref hdiCurrentItem);
            if (sortOrder == SortOrder.Ascending)
            {
                hdiCurrentItem.fmt &= ~HDF_SORTDOWN;
                hdiCurrentItem.fmt |= HDF_SORTUP;
            }
            else
            {
                hdiCurrentItem.fmt &= ~HDF_SORTUP;
                hdiCurrentItem.fmt |= HDF_SORTDOWN;
            }
            SendMessage(ptrHeader, HDM_SETITEM, ptrCurrentColumnIndex, ref hdiCurrentItem);
            _columnIndexPrevious = _columnIndexCurrent;
        }

        private int GetWindowLong(IntPtr hWnd, int nIndex)
        {
            if (!Is64bit)
                return (int)GetWindowLong32(hWnd, nIndex);
            else
                return (int)GetWindowLongPtr64(hWnd, nIndex).ToInt64();
        }
        private int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong)
        {
            if (!Is64bit)
                return (int)SetWindowLongPtr32(hWnd, nIndex, dwNewLong);
            else
                return (int)SetWindowLongPtr64(hWnd, nIndex, dwNewLong).ToInt64();
        }
        private bool Is64bit
        {
            get { return IntPtr.Size != 4; }
        }
        private void SetGrpFooter(ListViewGroup lstvwgrp, string footer)
        {
            if (Environment.OSVersion.Version.Major < 6)   //Only Vista and forward allows footer on ListViewGroups
                return;
            if (lstvwgrp == null || lstvwgrp.ListView == null)
                return;
            if (lstvwgrp.ListView.InvokeRequired)
                lstvwgrp.ListView.Invoke(new CallbackSetGroupString(SetGrpFooter), lstvwgrp, footer);
            else
            {
                int grpId = GetGroupID(lstvwgrp);
                int grpIndex = lstvwgrp.ListView.Groups.IndexOf(lstvwgrp);
                LVGROUP group = new LVGROUP();
                group.cbSize = Marshal.SizeOf(group);
                group.pszFooter = footer;
                group.mask = (int)ListViewGroupMask.Footer;
                if (grpId != 0)
                {
                    group.iGroupId = grpId;
                    SendMessage(lstvwgrp.ListView.Handle, (int)LVM.SETGROUPINFO, grpId, group);
                }
                else
                {
                    group.iGroupId = grpIndex;
                    SendMessage(lstvwgrp.ListView.Handle, (int)LVM.SETGROUPINFO, grpIndex, group);
                }
            }
        }
        private int GetGroupID(ListViewGroup lstvwgrp)
        {
            int rtnval = 0;
            Type GrpTp = lstvwgrp.GetType();
            if (GrpTp != null)
            {
                PropertyInfo pi = GrpTp.GetProperty("ID", BindingFlags.NonPublic | BindingFlags.Instance);
                if (pi != null)
                {
                    object tmprtnval = pi.GetValue(lstvwgrp, null);
                    if (tmprtnval != null)
                    {
                        rtnval = (int)tmprtnval;
                    }
                }
            }
            return rtnval;
        }

        #endregion

        #region ComboBox Events

        private void ComboBoxKeyPress(object sender, KeyPressEventArgs e)
        {
            if (sender is ComboBox cb)
            {
                if (e.KeyChar == 13 || e.KeyChar == 27)
                {
                    cb.Hide();
                }
            }
        }

        private void ComboBoxSelected(object sender, EventArgs e)
        {
            if (sender is ComboBox cb)
            {
                var i = cb.SelectedIndex;
                if (i >= 0)
                {
                    var str = cb.Items[i].ToString();
                    _item.SubItems[_selectedSubItem].Text = str;
                }
            }
        }

        private void ComboBoxFocusExit(object sender, EventArgs e)
        {
            if (sender is ComboBox cb)
            {
                cb.Hide();
            }
        }

        #endregion

        #region Win32 Imports

        [DllImport(user32)]
        private static extern int SendMessage
        (
            IntPtr hWnd,
            uint wMsg,
            int wParam,
            ref LV_ITEM item_info
        );

        [DllImport(uxtheme, CharSet = CharSet.Unicode)]
        private static extern int SetWindowTheme
        (
            IntPtr hWnd,
            string pszSubAppName,
            string pszSubIdList
        );

        [DllImport(user32)]
        private static extern IntPtr SendMessage
        (
            IntPtr hWnd,
            int wMsg,
            int wParam,
            int lParam
        );

        [DllImport(user32, EntryPoint = "SendMessage")]
        private static extern IntPtr SendMessage
        (
            IntPtr hWnd,
            int wMsg,
            IntPtr wParam,
            ref HDITEM lParam
        );

        [DllImport(user32)]
        private static extern int SendMessage
        (
            IntPtr hWnd,
            int wMsg,
            int wParam,
            IntPtr lParam
        );

        [DllImport(user32, EntryPoint = "SendMessage")]
        private static extern int SendMessage
         (
            IntPtr hWnd,
            int wMsg,
            int wParam,
            LVGROUP lParam
        );

        [DllImport(user32, EntryPoint = "GetWindowLong", CharSet = CharSet.Auto)]
        private static extern IntPtr GetWindowLong32
        (
            IntPtr hWnd,
            int nIndex
        );

        [DllImport(user32, EntryPoint = "GetWindowLongPtr", CharSet = CharSet.Auto)]
        private static extern IntPtr GetWindowLongPtr64
        (
            IntPtr hWnd,
            int nIndex
        );

        [DllImport(user32, EntryPoint = "SetWindowLong", CharSet = CharSet.Auto)]
        private static extern IntPtr SetWindowLongPtr32
        (
            IntPtr hWnd,
            int nIndex,
            int dwNewLong
        );

        [DllImport(user32, EntryPoint = "SetWindowLongPtr", CharSet = CharSet.Auto)]
        private static extern IntPtr SetWindowLongPtr64
        (
            IntPtr hWnd,
            int nIndex,
            int dwNewLong
        );

        #endregion

        #region Win32 Structures

        [StructLayout(LayoutKind.Sequential)]
        public struct HDITEM
        {
            public int mask;
            public int cxy;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pszText;
            public IntPtr hbm;
            public int cchTextMax;
            public int fmt;
            public IntPtr lParam;
            //public int iImage;
            //public int iOrder;
            //public int type;
            //public IntPtr pvFilter;
            //public int state;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct LVGROUP
        {
            public int cbSize;
            public int mask;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pszHeader;
            public int cchHeader;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pszFooter;
            public int cchFooter;
            public int iGroupId;
            public int stateMask;
            public int state;
            public int uAlign;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct LV_ITEM
        {
            public uint uiMask;
            public int iItem;
            public int iSubItem;
            public uint uiState;
            public uint uiStateMask;
            public string pszText;
            public int cchTextMax;
            public int iImage;
            public IntPtr lParam;
        }

        #endregion
    }

    public sealed class Comparer : IComparer
    {
        #region Variables

        private readonly int _columnIndex;
        private readonly SortOrder _sortOrder;

        #endregion

        #region Constructor

        public Comparer(int columnIndex, SortOrder sortOrder)
        {
            _columnIndex = columnIndex;
            _sortOrder = sortOrder;
        }

        #endregion

        #region Public Methods

        public int Compare(object x, object y)
        {
            if (_columnIndex < 0) return 0;

            int num = 0;
            try
            {
                num = string.Compare(((ListViewItem)x).SubItems[_columnIndex].Text, ((ListViewItem)y).SubItems[_columnIndex].Text, StringComparison.Ordinal);
                if (_sortOrder == SortOrder.Descending)
                {
                    num *= -1;
                }
            }
            catch
            {
                if (num < 0) num = 0;
            }
            return num;
        }

        #endregion
    }

    public sealed class ComparerListViewGroup : IComparer<ListViewGroup>
    {
        public int Compare(ListViewGroup x, ListViewGroup y)
        {
            return string.Compare(x.Header, y.Header, StringComparison.Ordinal);
        }
    }
}