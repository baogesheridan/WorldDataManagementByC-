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
    /// <summary>
    /// Interaction logic for AddCountryWindow.xaml
    /// </summary>
    public partial class AddCountryWindow : Window
    {
        public AddCountryWindow()
        {
            InitializeComponent();
            RefreshContinentsComboBox();
        }

        public void RefreshContinentsComboBox()
        {
            List<string> continents = new List<string>();

            try
            {
                using (SqlConnection conn = new SqlConnection(Data.ConnectionString))
                {
                    string query = "SELECT ContinentName FROM Continent";
                    SqlCommand command = new SqlCommand(query, conn);
                    conn.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            continents.Add(reader["ContinentName"].ToString());
                        }
                    }
                }

                cmbAdd_ContinentName.ItemsSource = continents;
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnAdd_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnAdd_Country_Click(object sender, RoutedEventArgs e)
        {
            if (cmbAdd_ContinentName.SelectedItem == null) {
                lblAddCountry_ContinentValidation.Content = "This is required";
                return;
            }
            string newCountryName = txtAdd_CountryName.Text.Trim();
            if (string.IsNullOrWhiteSpace(newCountryName))
            {
                lblAddCountry_CountryValidation.Content = "This is required";
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(Data.ConnectionString))
                {
                    string checkQuery = "SELECT COUNT(*) FROM Country WHERE CountryName = @CountryName";
                    SqlCommand checkCmd = new SqlCommand(checkQuery, conn);
                    checkCmd.Parameters.AddWithValue("@CountryName", newCountryName);
                    conn.Open();

                    int count = (int)checkCmd.ExecuteScalar();
                    if (count > 0)
                    {
                        MessageBox.Show($"The country '{newCountryName}' already exists in the database.", "Duplicate Entry", MessageBoxButton.OK, MessageBoxImage.Information);
                        txtAdd_CountryName.Text = "";
                        return;
                    }
                    conn.Close();

                    string insertQuery = @"INSERT INTO Country (CountryName, Language,Currency, ContinentId)
                    VALUES (@CountryName, @Language, @Currency, 
                    (SELECT ContinentId FROM Continent WHERE ContinentName = @ContinentName))";
                    SqlCommand cmd = new SqlCommand(insertQuery, conn);
                    cmd.Parameters.AddWithValue("@CountryName", newCountryName);
                    cmd.Parameters.AddWithValue("@ContinentName", cmbAdd_ContinentName.SelectedItem.ToString());
                    cmd.Parameters.AddWithValue("@Language", txtAdd_Language.Text);
                    cmd.Parameters.AddWithValue("@Currency", txtAdd_Currency.Text);
                    conn.Open();

                    int result = (int)cmd.ExecuteNonQuery();
                    if (result == 1)
                    {
                        MessageBox.Show($"Country {newCountryName} has been added successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        txtAdd_CountryName.Text = "";
                        cmbAdd_ContinentName.SelectedItem = null;
                        txtAdd_Currency.Text = "";
                        txtAdd_Language.Text = "";
                    }
                    else
                    {
                        MessageBox.Show($"Failed to add Country {newCountryName}.", "Failure", MessageBoxButton.OK, MessageBoxImage.Error);
                        txtAdd_CountryName.Text = "";
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
