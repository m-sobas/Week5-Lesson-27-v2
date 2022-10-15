using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Kadry
{
    public partial class Calendar : Form
    {
        TextBox _tbSelectedDate;

        public Calendar(TextBox tb)
        {
            // Utworzenie kopii obiektu TextBox
            _tbSelectedDate = tb;

            InitializeComponent();
        }


        private void btnCancel_Click(object sender, EventArgs e)
        {
            // Zamknięcie okna
            Close();
        }


        private void btnSet_Click(object sender, EventArgs e)
        {
            // Pobranie daty z kalendarza, sformatowanie jej na datę krótką ("d") i skopiowanie daty do pola TexBox.
            _tbSelectedDate.Text = monthCalendar.SelectionStart.ToString("d");

            // Zamknięcie okna
            Close();
        }

    }
}
