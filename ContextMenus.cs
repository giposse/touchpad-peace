namespace TouchpadPeaceFree
{
    using System;
    using System.Windows.Forms;
    using TouchpadPeaceFree.Properties;

    /// <summary>
    /// 
    /// </summary>
    class ContextMenus : IDisposable
    {
        TPCIcon tpcIcon;
        private ContextMenuStrip menu;

        private static string MENU_OPTION_NAME_EXIT = "exitItem";
        private static string MENU_OPTION_NAME_HELP = "Help";

        public ContextMenus(TPCIcon tpcIcon)
        {
            this.tpcIcon = tpcIcon;
        }

        /// <summary>
        /// Creates this instance.
        /// </summary>
        /// <returns>ContextMenuStrip</returns>
        public ContextMenuStrip Create()
        {
            // Add the default menu options.
            menu = new ContextMenuStrip();
            menu.ShowImageMargin = true;
            menu.Items.Add(new ToolStripMenuItem(Strings.Help, Resources.help_24px,
                OnHelp_Click, MENU_OPTION_NAME_HELP));

            menu.Items.Add(new ToolStripMenuItem(Strings.QuitString, Resources.exit_24px, 
                Exit_Click, MENU_OPTION_NAME_EXIT));

            return menu;
        }

        internal static void OnHelp_Click(object sender, EventArgs e)
        {
            HelpForm helpForm = new HelpForm();
            helpForm.ShowDialog();
        }

        /// <summary>
        /// Processes a menu item.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void Exit_Click(object sender, EventArgs e)
        {
            tpcIcon.RemoveHooks();
            tpcIcon.TrayIcon.Visible = false;
            tpcIcon.TrayIcon.Icon = null;
            Application.Exit();
        }

         public void Dispose()
        {
            foreach (ToolStripItem singleItem in menu.Items)
            {
                singleItem.Dispose();
            }

           menu.Items.Clear();
        }
    }
}