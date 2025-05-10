using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace Library
{
    public partial class Form2 : Form
    {
        private NpgsqlConnection conn;
        private NpgsqlDataAdapter adapter;
        private DataTable dt;
        private Dictionary<string, Dictionary<int, string>> lookupData = new Dictionary<string, Dictionary<int, string>>();

        public Form2()
        {
            InitializeComponent();
            string connectionString = "Host=localhost;Port=5432;Database=ffff;Username=postgres;Password=1111;";
            conn = new NpgsqlConnection(connectionString);
            comboBox1.Items.AddRange(new object[] { "книги", "читатели", "выдачи", "сотрудники" });
            comboBox1.SelectedIndex = 0; // По умолчанию выбираем первую таблицу
            comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedTable = comboBox1.Text;
            LoadLookupData();

            string query = $"SELECT * FROM {selectedTable}";
            adapter = new NpgsqlDataAdapter(query, conn);
            var builder = new NpgsqlCommandBuilder(adapter);

            dt = new DataTable();
            adapter.Fill(dt);

            // Удаляем ID-столбцы, чтобы не отображались
            RemovePrimaryKeyColumns(selectedTable);

            dataGridView1.Columns.Clear();
            dataGridView1.DataSource = dt;

            ConfigureGridColumns(selectedTable);
        }

        private void LoadLookupData()
        {
            // Загружаем словари для связанных таблиц
            lookupData["книги"] = LoadLookup("книги", "id_книги", "название");
            lookupData["читатели"] = LoadLookup("читатели", "id_читателя", "полное_имя");
            lookupData["сотрудники"] = LoadLookup("сотрудники", "id_сотрудника", "фио");
        }

        private Dictionary<int, string> LoadLookup(string table, string keyCol, string valCol)
        {
            var dict = new Dictionary<int, string>();
            try
            {
                using (var cmd = new NpgsqlCommand($"SELECT {keyCol}, {valCol} FROM {table}", conn))
                {
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            dict[reader.GetInt32(0)] = reader.GetString(1);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
            finally
            {
                conn.Close();
            }
            return dict;
        }

        private void ConfigureGridColumns(string tableName)
        {
            // скрываем первичные ключи, чтобы они не мешали
            HidePrimaryKey(tableName);
            // добавляем ComboBox для внешних ключей
            AddComboBoxColumns(tableName);
        }

        private void RemovePrimaryKeyColumns(string tableName)
        {
            string pkColumn = null;
            switch (tableName)
            {
                case "книги": pkColumn = "id_книги"; break;
                case "читатели": pkColumn = "id_читателя"; break;
                case "выдачи": pkColumn = "id_выдачи"; break;
                case "сотрудники": pkColumn = "id_сотрудника"; break;
            }
            if (pkColumn != null && dt.Columns.Contains(pkColumn))
            {
                dt.Columns.Remove(pkColumn);
            }
        }

        private void HidePrimaryKey(string tableName)
        {
            string pkColumn = null;
            switch (tableName)
            {
                case "книги": pkColumn = "id_книги"; break;
                case "читатели": pkColumn = "id_читателя"; break;
                case "выдачи": pkColumn = "id_выдачи"; break;
                case "сотрудники": pkColumn = "id_сотрудника"; break;
            }

            if (pkColumn != null && dataGridView1.Columns.Contains(pkColumn))
            {
                dataGridView1.Columns[pkColumn].Visible = false;
            }
        }

        private void AddComboBoxColumns(string tableName)
        {
            if (tableName == "выдачи")
            {
                CreateComboBoxColumn("id_читателя", "читатели");
                CreateComboBoxColumn("id_книги", "книги");
            }
        }

        private void CreateComboBoxColumn(string columnName, string lookupTable)
        {
            if (!dt.Columns.Contains(columnName))
                return;

            // Удаляем, если есть
            if (dataGridView1.Columns.Contains(columnName))
                dataGridView1.Columns.Remove(columnName);

            var comboColumn = new DataGridViewComboBoxColumn
            {
                Name = columnName,
                DataPropertyName = columnName,
                HeaderText = columnName.Replace("id_", ""), // например, "id_читателя" -> "читателя"
                DataSource = new BindingSource(lookupData[lookupTable], null),
                DisplayMember = "Value",
                ValueMember = "Key"
            };

            // Вставляем в таблицу
            int insertIndex = dataGridView1.Columns.Count > 0 ? dataGridView1.Columns.Count : 0;
            dataGridView1.Columns.Insert(insertIndex, comboColumn);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                adapter.Update(dt);
                MessageBox.Show("Изменения успешно сохранены");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow != null && !dataGridView1.CurrentRow.IsNewRow)
            {
                dataGridView1.Rows.RemoveAt(dataGridView1.CurrentRow.Index);
                MessageBox.Show("Строка удалена. Не забудьте сохранить изменения.");
            }
        }

        private void dataGridView1_DefaultValuesNeeded(object sender, DataGridViewRowEventArgs e)
        {
            string table = comboBox1.Text;
            if (table == "выдачи")
            {
                e.Row.Cells["дата_выдачи"].Value = DateTime.Today;
                e.Row.Cells["количество"].Value = 1;
                // Можно добавить другие значения по умолчанию
            }
        }
    }
}