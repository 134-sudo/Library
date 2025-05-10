using System;
using System.Windows.Forms;
using Npgsql;
using Library;

namespace Library
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string name = textBox1.Text;
            string password = textBox2.Text;

            string connString = "Host=localhost;Port=5432;Database=mr_poul;Username=postgres;Password=1111;";

            try
            {
                using (var conn = new NpgsqlConnection(connString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = conn;
                        // Используем параметры для предотвращения SQL-инъекций
                        cmd.CommandText = "SELECT COUNT(*) FROM users WHERE login = @login AND password = @password";
                        cmd.Parameters.AddWithValue("@login", name);
                        cmd.Parameters.AddWithValue("@password", password);

                        int count = Convert.ToInt32(cmd.ExecuteScalar());

                        if (count > 0)
                        {
                            MessageBox.Show("Вход успешен!");
                            Form form2 = new Form2();
                            this.Hide();
                            form2.Show();
                        }
                        else
                        {
                            MessageBox.Show("Неверное имя пользователя или пароль.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Произошла ошибка при подключении: " + ex.Message);
            }
        }
    }
}