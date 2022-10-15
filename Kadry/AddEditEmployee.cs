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
    public partial class AddEditEmployee : Form
    {
        // Dziedziczona klasa po FileHelper wzbogacona o obsługę XML { wykorzystana klasa z kursu Akademia .NET }.
        FileHelperExtended<List<Employee>> _fileHelperExt = new FileHelperExtended<List<Employee>> (Program.dataBasePath);

        // Przygotowanie zdarzenia wywoływanego po dodaniu pracownika do listy.
        public delegate void AddEditEmployeeDelegate();
        public event AddEditEmployeeDelegate EmployeeAdded;

        ActionType _action; // Typ otwieranego okna (Dodawanie/Edycja).
        int? _id;           // Identyfikator pracownika.


        public AddEditEmployee(ActionType action, int? id = null)
        {
            _action = action;
            _id = id;

            InitializeComponent();

            if (id != null)             // Gdy okno formularza zostało otwarte do edycji
            {   
                FillEmployeeFields();   // to wywołaj metodę.
            }
        }


        private void FillEmployeeFields()
        {
            // Wypełnianie pól kontrolek danymi z pliku XML o podanym ID.
            var employees = _fileHelperExt.DeserializeFromFile();
            var employee = employees.FirstOrDefault(x => x.Id == _id);

            tbFirstName.Text        = employee.FirstName.Trim();
            tbLastName.Text         = employee.LastName.Trim();
            tbAddress.Text          = employee.Address.Trim();
            masked.Text             = employee.PostCode.Trim();
            tbCity.Text             = employee.City.Trim();
            tbEmploymentDate.Text   = employee.EmploymentDate.ToString();
            tbDismissalDate.Text    = employee.DismissalDate.ToString();
            tbEmployeeEarning.Text  = employee.EmployeeEarning.ToString().Trim();
            tbComments.Text         = employee.Comments.Trim();
        }


        private void OnEmployeeAdded()
        {
            // Wywołanie zdarzenia powiadamiającego subskrybenta.
            EmployeeAdded?.Invoke();
        }


        private void btnCancel_Click(object sender, EventArgs e)
        {
            // Zamknięcie okna.
            Close();
        }


        private void btnEmploymentDate_Click(object sender, EventArgs e)
        {
            // Wywołanie metody z parametrem, przekazującym pole tekstowe dla daty zatrudnienia.
            ShowCalendar(tbEmploymentDate);
        }


        private void btnDismissalDate_Click(object sender, EventArgs e)
        {
            // Wywołanie metody z parametrem, przekazującym pole tekstowe dla daty zwolnienia.
            ShowCalendar(tbDismissalDate);
        }


        private void btnClearEmploymentDate_Click(object sender, EventArgs e)
        {
            // Wyczyszczenie pola tekstowego daty zatrudnienia.
            tbEmploymentDate.Clear();
        }


        private void btnClearDismissalDate_Click(object sender, EventArgs e)
        {
            // Wyczyszczenie pola tekstowego daty zwolnienia.
            tbDismissalDate.Clear();
        }


        // Metoda otwierająca okno z kalendarzem.
        private void ShowCalendar(TextBox tb)
        {
            // Utworzenie formularza i przekazanie obiektu do konstruktora.
            Calendar form = new Calendar(tb);

            // Otwarcie okienka "Kalendarz".
            form.ShowDialog();
        }


        // Obsługa zdarzenia przechwytująca wciskane klawisze w polu tekstowym "Wynagrodzenie" (walidacja danych).
        private void tbEmployeeEarning_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '.') // Jeśli wciśnięty został klawisz '.' (kropka),
                e.KeyChar = ',';  // to wprowadź znak ',' (przecinek).

            if (e.KeyChar == ',' && tbEmployeeEarning.Text.Contains(",")) // Jeśli w polu tekstowym znajduje się już znak ',' (przecinek)
                e.Handled = true;                                         // to zablokuj dalsze wprowadzenie znaku.

            if (e.KeyChar == (int)8 || e.KeyChar == ',') // Jeśli wciśniętym klawiszem jest "Del" lub "Delete" lub znak "," (przecinek)
                return;                                  // to wyjdź z funkcji bez blokowania tych klawiszy.

            // W pozostałych przypadkach blokuj wszystkie inne znaki nie będące cyframi 0 - 9.
            if ((int)(e.KeyChar) < 48 || (int)(e.KeyChar) > 58)
                e.Handled = true;
        }


        private void AddEditEmploye_Shown(object sender, EventArgs e)
        {
            switch (_action)
            {
                // Dodawanie pracownika.
                case ActionType.AddEmployee:

                    Text = "Dodawanie pracownika";          // Wyświetlenie komunikatu w nagłówku okna.
                    tbDismissalDate.Enabled = false;        //  \
                    btnDismissalDate.Enabled = false;       //   -- Zablokowanie edycji pola "Data zwolnienia".
                    btnClearDismissalDate.Enabled = false;  //  /

                    break;

                // Edycja pracownika
                case ActionType.EditEmployee:

                    Text = "Edycja pracownika";             // Wyświetlenie komunikatu w nagłówku okna.
                    tbEmploymentDate.Enabled = false;       // \
                    btnEmploymentDate.Enabled = false;      //  -- Zablokowanie edycji pola "Data zatrudnienia".
                    btnClearEmploymentDate.Enabled = false; // /

                    break;
            }
        }


        private void btnSave_Click(object sender, EventArgs e)
        {
            // Pobranie danych z pliku XML.
            var employees = _fileHelperExt.DeserializeFromFile();

            switch (_action)
            {
                // Dla dodawanego pracownika wymagane jest uzpełnienie pola z datą zatrudnienia.
                case ActionType.AddEmployee:

                    if (tbEmploymentDate.Text == String.Empty)
                    {
                        MessageBox.Show("Data zatrudnienia jest wymagana.", "Ostrzeżenie", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Pobranie numeru ostatniego Id.
                    int? maxId = employees.Select(x => x.Id).Max();

                    // Jeśli dane nie zwierają rekordów, to przypisz początkowe Id = 1, w przeciwnym razie zwiększ Id o 1.
                    int newId = maxId == null ? 1 : (int)maxId + 1;

                    var employee = new Employee
                    {
                        Id              = newId,
                        FirstName       = tbFirstName.Text.Trim(),
                        LastName        = tbLastName.Text.Trim(),
                        Address         = tbAddress.Text.Trim(),
                        PostCode        = masked.Text.Trim(),
                        City            = tbCity.Text.Trim(),
                        EmploymentDate  = Convert.ToDateTime(tbEmploymentDate.Text),
                        DismissalDate   = Convert.ToDateTime(tbDismissalDate.Text),
                        EmployeeEarning = tbEmployeeEarning.Text == "" ? Convert.ToDecimal(0.ToString("N")) : Convert.ToDecimal(Decimal.Parse(tbEmployeeEarning.Text).ToString("N")),
                        Comments        = tbComments.Text.Trim()
                    };

                    // Dodanie nowego pracownika do listy.
                    employees.Add(employee);

                    // Zapisanie listy pracowników do pliku.
                    _fileHelperExt.SerializeToFile(employees);

                    break;

                // Edycja pracownika.
                case ActionType.EditEmployee:

                    employee = new Employee
                    {
                        Id              = (int)_id,
                        FirstName       = tbFirstName.Text.Trim(),
                        LastName        = tbLastName.Text.Trim(),
                        Address         = tbAddress.Text.Trim(),
                        PostCode        = masked.Text.Trim(),
                        City            = tbCity.Text.Trim(),
                        EmploymentDate  = Convert.ToDateTime(tbEmploymentDate.Text),
                        DismissalDate   = Convert.ToDateTime(tbDismissalDate.Text),
                        EmployeeEarning = tbEmployeeEarning.Text == "" ? Convert.ToDecimal(0.ToString("N")) : Convert.ToDecimal(Decimal.Parse(tbEmployeeEarning.Text).ToString("N")),
                        Comments        = tbComments.Text.Trim()
                    };

                    // Aktualizacja danych w pliku XML.
                    _fileHelperExt.UpdateXML(employee, (int)_id);

                    break;
            }

            // Wywołanie zdarzenia po zmianie zawartości formularza.
            OnEmployeeAdded();

            // Zamknięcie głównego okna po zapisie danych.
            Close();
        }

    }
}
