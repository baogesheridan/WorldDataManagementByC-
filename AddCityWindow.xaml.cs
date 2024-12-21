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
    /// Interaction logic for AddCityWindow.xaml
    /// </summary>
    public partial class AddCityWindow : Window
    {
        public AddCityWindow()
        {
            InitializeComponent();
            RefreshCountryComboBox();
        }

        private void btnAdd_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public void RefreshCountryComboBox()
        {
            List<string> countries = new List<string>();

            try
            {
                using (SqlConnection conn = new SqlConnection(Data.ConnectionString))
                {
                    string query = "SELECT CountryName FROM Country";
                    SqlCommand command = new SqlCommand(query, conn);
                    conn.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            countries.Add(reader["CountryName"].ToString());
                        }
                    }
                }

                AddCity_cmbCountryName.ItemsSource = countries;
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnAdd_Continent_Click(object sender, RoutedEventArgs e)
        {
            if (AddCity_cmbCountryName.SelectedItem == null)
            {
                AddCity_CountryValidation.Content = "This is required";
                return;
            }
            string newCityName = AddCity_txtCityName.Text.Trim();
            if (string.IsNullOrWhiteSpace(newCityName))
            {
                AddCity_CityValidation.Content = "This is required";
                return;
            }

            bool isCapital = AddCity_IsCapital.IsChecked ?? false;

            try
            {
                using (SqlConnection conn = new SqlConnection(Data.ConnectionString))
                {
                    string checkQuery = "SELECT COUNT(*) FROM City c join Country co ON c.CountryId = co.CountryId WHERE CityName = @CityName and CountryName= @CountryName";
                    SqlCommand checkCmd = new SqlCommand(checkQuery, conn);
                    checkCmd.Parameters.AddWithValue("@CountryName", AddCity_cmbCountryName.SelectedItem.ToString());
                    checkCmd.Parameters.AddWithValue("@CityName", newCityName);
                    conn.Open();

                    int count = (int)checkCmd.ExecuteScalar();
                    if (count > 0)
                    {
                        MessageBox.Show($"The City '{newCityName}' already exists in the database.", "Duplicate Entry", MessageBoxButton.OK, MessageBoxImage.Information);
                        AddCity_txtCityName.Text = "";
                        return;
                    }

                    string insertQuery = @"INSERT INTO City (CityName, IsCapital, Population, CountryId)
                    VALUES (@CityName, @IsCapital, @Population, 
                    (SELECT CountryId FROM Country WHERE CountryName = @CountryName))";
                    SqlCommand cmd = new SqlCommand(insertQuery, conn);
                    cmd.Parameters.AddWithValue("@CityName", newCityName);
                    cmd.Parameters.AddWithValue("@CountryName", AddCity_cmbCountryName.SelectedItem.ToString());
                    cmd.Parameters.AddWithValue("@IsCapital", isCapital);
                    cmd.Parameters.AddWithValue("@Population", AddCity_txtPopulation.Text);

                    int result = (int)cmd.ExecuteNonQuery();
                    if (result == 1)
                    {
                        MessageBox.Show($"City {newCityName} has been added successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        AddCity_txtCityName.Text = "";
                        AddCity_IsCapital.IsChecked = false;
                        AddCity_txtPopulation.Text = "";
                        AddCity_cmbCountryName.SelectedItem=null;
                    }
                    else
                    {
                        MessageBox.Show($"Failed to add Country {newCityName}.", "Failure", MessageBoxButton.OK, MessageBoxImage.Error);
                        AddCity_txtCityName.Text = "";
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
