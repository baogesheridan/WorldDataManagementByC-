using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace A2baoyongzhao
{
    public partial class AddContinentWindow : Window
    {

        private MainWindow _mainWindow;

        public AddContinentWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow; 
        }


        private void btnAdd_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            
        }

        private void btnAdd_Continent_Click(object sender, RoutedEventArgs e)
        {
            string newContinentName = txtAdd_ContinentName.Text.Trim();
            if (string.IsNullOrWhiteSpace(newContinentName))
            {
                lblAdd_ContinentValidation.Content="this field is required";
                return; 
            }
            try
            {
                using (SqlConnection conn = new SqlConnection(Data.ConnectionString))
                {
                    string checkQuery = "SELECT COUNT(*) FROM Continent WHERE ContinentName = @ContinentName";
                    SqlCommand checkCmd = new SqlCommand(checkQuery, conn);
                    checkCmd.Parameters.AddWithValue("@ContinentName", newContinentName);
                    conn.Open();

                    int count = (int)checkCmd.ExecuteScalar();
                    if (count > 0)
                    {
                        MessageBox.Show($"The continent '{newContinentName}' already exists in the database.", "Duplicate Entry", MessageBoxButton.OK, MessageBoxImage.Information);
                        txtAdd_ContinentName.Text = "";
                        return; 
                    }
                    conn.Close();

                    string insertQuery = "INSERT INTO Continent (ContinentName) values (@ContinentName)";
                    SqlCommand cmd = new SqlCommand(insertQuery, conn);
                    cmd.Parameters.AddWithValue("@ContinentName", newContinentName);
                    conn.Open();

                    int result = cmd.ExecuteNonQuery();
                    if (result == 1)
                    {
                        MessageBox.Show($"Continent {newContinentName} has been added successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        txtAdd_ContinentName.Text = "";
                    }
                    else
                    {
                        MessageBox.Show($"Failed to add Continent {newContinentName}.", "Failure", MessageBoxButton.OK, MessageBoxImage.Error);
                        txtAdd_ContinentName.Text = "";
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

