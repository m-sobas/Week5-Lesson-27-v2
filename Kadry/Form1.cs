using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Kadry
{
    public partial class Form1 : Form
    {
        Task _task; // Obiekt zadania uruchamianego w osobnym wątku.
        CancellationTokenSource _cancellationTokenSource; // Nasłuchiwanie żądania anulującego zadanie.

        // Dziedziczona klasa po FileHelper wzbogacona o obsługę XML { wykorzystana klasa z kursu "Zostań Programistą .NET" }.
        FileHelperExtended<List<Employee>> _fileHelperExt = new FileHelperExtended<List<Employee>>(Program.dataBasePath);

        public Form1()
        {
            InitializeComponent();

            List<Employee> employees = new List<Employee>();

            if (!File.Exists(Program.dataBasePath))
            {
                MessageBox.Show("Brak pliku");
                _fileHelperExt.SerializeToFile(null); // Jeśli plik z bazą nie istnieje, to go utwórz.
            }
            else
            {
                MessageBox.Show("Jest plik");
                employees = _fileHelperExt.DeserializeFromFile(); // Pobierz dane z pliku XML.
            }

            AddFilter();              // Utworzenie listy filtrów.
            SetDataGrid();            // Ustawienie siatki DataGridView.
            LoadDataAsync(employees); // Wypełnij siatkę danymi.

            // Zasubskrybowanie zdarzenia dla wybieranych opcji w kontrolce ComboBox "filter" ( koniecznie po wywołaniu metody AddFilter() ).
            tsFilterGroup.TextChanged += tsFilterGroup_TextChanged;
        }


        private void AddFilter()
        {
            // Wypełnienie listy danymi.

            tsFilterGroup.Items.Add("Wszyscy pracownicy");
            tsFilterGroup.Items.Add("Pracownicy zatrudnieni");
            tsFilterGroup.Items.Add("Pracownicy zwolnieni");

            tsFilterGroup.SelectedIndex = 0; // Ustawienie zaznaczenia na pierwszą pozycję z listy.
        }


        private void SetDataGrid()
        {
            // Ustawienie właściwości wyglądu siatki.

            dgvEmployees.ColumnHeadersDefaultCellStyle.BackColor = Color.LightGray;

            dgvEmployees.Columns.Add("Number", "Lp.");
            dgvEmployees.Columns.Add("Id", "ID");
            dgvEmployees.Columns.Add("FirstName", "Imię");
            dgvEmployees.Columns.Add("LastName", "Nazwisko");
            dgvEmployees.Columns.Add("Address", "Adres zamieszkania");
            dgvEmployees.Columns.Add("PostCode", "Kod pocztowy");
            dgvEmployees.Columns.Add("City", "Miejscowość");
            dgvEmployees.Columns.Add("EmploymentDate", "Data zatrudnienia");
            dgvEmployees.Columns.Add("DismissalDate", "Data zwolnienia");
            dgvEmployees.Columns.Add("EmployeeSalary", "Wynagrodzenie");
            dgvEmployees.Columns.Add("Comments", "Komentarz");

            dgvEmployees.Columns["Number"].Width = 50;
            dgvEmployees.Columns["Id"].Width = 50;
            dgvEmployees.Columns["FirstName"].Width = 150;
            dgvEmployees.Columns["LastName"].Width = 150;
            dgvEmployees.Columns["Address"].Width = 250;
            dgvEmployees.Columns["PostCode"].Width = 100;
            dgvEmployees.Columns["City"].Width = 200;
            dgvEmployees.Columns["EmploymentDate"].Width = 100;
            dgvEmployees.Columns["DismissalDate"].Width = 100;
            dgvEmployees.Columns["EmployeeSalary"].Width = 120;
            dgvEmployees.Columns["Comments"].Width = 300;
        }


        private async void LoadDataAsync(List<Employee> employees)
        {
            // Wywołanie asynchronicznej metody wypełniającej siatkę danymi.
            await FillDataGridAsync(employees); // W parametrze przekazujemy kolekcję z pracownikami.          
        }


        private async Task FillDataGridAsync(List<Employee> employees)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            CancellationToken ct = _cancellationTokenSource.Token;

            // Przygotowanie zadania do uruchomienia.
            _task = new Task(() =>
            {
                int number = 1; // 

                foreach (var e in employees)
                {
                    if (ct.IsCancellationRequested)         // Jeśli przyszło żądanie anulowania zadania,
                    {
                        ct.ThrowIfCancellationRequested();  // to rzuć wyjątek.
                    }

                    if (dgvEmployees.InvokeRequired) // Sprawdza, czy kontrolka wymaga użycia metody Invoke.
                    {
                        dgvEmployees.Invoke(new Action(() =>
                        {
                            dgvEmployees.Rows.Add(  number,
                                                    e.Id,
                                                    e.FirstName,
                                                    e.LastName,
                                                    e.Address,
                                                    e.PostCode,
                                                    e.City,
                                                    e.EmploymentDate,
                                                    e.DismissalDate,
                                                    e.EmployeeSalary,
                                                    e.Comments
                                                  );
                        }));
                    }
                    else
                    {
                        dgvEmployees.Rows.Add(  number,
                                                e.Id,
                                                e.FirstName,
                                                e.LastName,
                                                e.Address,
                                                e.PostCode,
                                                e.City,
                                                e.EmploymentDate,
                                                e.DismissalDate,
                                                e.EmployeeSalary,
                                                e.Comments
                                              );
                    }

                    number++;
                }
            }, _cancellationTokenSource.Token);

            _task.Start(); // Uruchomienie zadania w osobnym wątku.
        }


        private void tsInfo_Click(object sender, EventArgs e)
        {
            Information form = new Information();

            form.ShowDialog(); // Otwarcie okienka z informacją.
        }


        private void tsAddEditEmployee_Click(object sender, EventArgs e)
        {
            // Wywołanie metody otwierającej formularz umożliwiający dodanie nowego pracownika.
            ShowEmployeeForm(ActionType.AddEmployee);
        }


        private void tsEditEmployee_Click(object sender, EventArgs e)
        {
            // Wywołanie metody otwierającej formularz umożliwiający edycję wskazanego pracownika.
            if (dgvEmployees.SelectedRows.Count > 0)
            {
                // Pobranie numeru zaznaczonego wiersza siatki.
                int selectedRowIndex = dgvEmployees.CurrentCell.RowIndex;
                
                // Pobranie identyfikatora pracownika.
                int id = (int)dgvEmployees["Id", selectedRowIndex].Value;

                // Przekazanie parametrów metodzie otwierającej okno "Dodawanie/Edycja pracownika".
                ShowEmployeeForm(ActionType.EditEmployee, id);
            }
        }


        private void ShowEmployeeForm(ActionType action, int? id = null)
        {
            AddEditEmployee form = null;

            switch (action)
            {
                case ActionType.AddEmployee:    // Dodanie nowego pracownika.

                    form = new AddEditEmployee(action);

                    break;

                case ActionType.EditEmployee:   // Edycja pracownika.

                    form = new AddEditEmployee(action, id);

                    break;
            }

            // Zapisanie subskrybenta do zdarzenia.
            form.EmployeeAdded += AddEditEmployee_EmployeeAdded;

            // Otwarcie okna dodawania/edycji pracownika.
            form.ShowDialog();

            // Usunięcie subskrybenta ze zdarzenia.
            form.EmployeeAdded -= AddEditEmployee_EmployeeAdded;
        }


        // Zdarzenie wywoływane po dodaniu pracownika do listy
        private void AddEditEmployee_EmployeeAdded()
        {
            // Odświeżenie siatki na podstwie wybranej opcji fitru
            tsFilterGroup_TextChanged(this, null);
        }


        private async Task BreakTask()
        {
            if (!_task.IsCompleted)                 // Jeśli zadanie jest w trakcie działania, to
            {
                _cancellationTokenSource.Cancel();  // wywołaj żądanie anulowania zadania.

                await Task.Run(() =>
                {
                    while (!_task.IsCanceled) ;      // Oczekuje na zakończenie zadania.
                });
            }

            // Zwolnienie zasobów z pamięci.
            _cancellationTokenSource.Dispose();
            _task.Dispose();
        }


        private void ClearDataGrid()
        {
            // Czyszczenie siatki.
            dgvEmployees.Rows.Clear();
            dgvEmployees.DataSource = null;
        }


        private async void tsFilterGroup_TextChanged(object sender, EventArgs e)
        {
            var employees = _fileHelperExt.DeserializeFromFile();  // Pobranie danych z pliku XML.

            switch (tsFilterGroup.Text)
            {
                case "Wszyscy pracownicy":

                    await BreakTask();  // Przerwij zadanie.

                    ClearDataGrid();    // Wyczyść siatkę.

                    await FillDataGridAsync(employees); // Wypełnij siatkę danymi.

                    break;

                case "Pracownicy zatrudnieni":

                    await BreakTask(); // Przerwij zadanie.

                    ClearDataGrid(); // Wyczyść siatkę.

                    // Przefiltruj dane za pomocą wyrażenia lambda.
                    var filteredEmployees = employees.Where(x => x.DismissalDate == null).ToList();
                    
                    // Wypełnij siatkę danymi.
                    await FillDataGridAsync(filteredEmployees);

                    break;

                case "Pracownicy zwolnieni":

                    await BreakTask(); // Przerwij zadanie.

                    ClearDataGrid(); // Wyczyść siatkę.

                    // Przefiltruj dane za pomocą wyrażenia lambda.
                    filteredEmployees = employees.Where(x => x.DismissalDate != null).ToList();

                    // Wypełnij siatkę danymi.
                    await FillDataGridAsync(filteredEmployees);

                    break;
            }
        }

    }
}
