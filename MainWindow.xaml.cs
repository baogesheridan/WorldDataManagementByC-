using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.SqlClient;
using System.Data;
using System.Diagnostics.Metrics;
using System.Windows.Controls.Primitives;

namespace A2baoyongzhao
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
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

                cbContinent.ItemsSource = continents;
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnAddContinent_Click(object sender, RoutedEventArgs e)
        {
            AddContinentWindow childWindow= new AddContinentWindow(this);
            childWindow.Owner = this;
            childWindow.ShowDialog();


            RefreshContinentsComboBox();
        }

        public void RefreshCountriesListBox(string continentName)
        {
            List<string> countries = new List<string>();

            try
            {
                using (SqlConnection conn = new SqlConnection(Data.ConnectionString))
                {
                    string query = @"
                        SELECT c.CountryName FROM Country c JOIN Continent co 
                        ON c.ContinentID = co.ContinentID
                        WHERE co.ContinentName = @ContinentName";

                    SqlCommand command = new SqlCommand(query, conn);
                    command.Parameters.AddWithValue("@ContinentName", cbContinent.SelectedItem.ToString());
                    conn.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            countries.Add(reader["CountryName"].ToString());
                        }
                    }
                }

                lbCountry.ItemsSource = countries;
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void cbContinent_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbContinent.SelectedItem == null)
                return;

            string selectedContinent = cbContinent.SelectedItem.ToString();
            RefreshCountriesListBox(selectedContinent);
            lblCurrency.Content = "";
            lblLanguage.Content = "";
            grdCities.ItemsSource = null;
        }

        private void LoadGrid()
        {
            SqlConnection conn = new SqlConnection(Data.ConnectionString);

            string query = "SELECT * FROM City WHERE CityId " +
                "IN(SELECT CityId FROM Country c JOIN Continent co " +
                "ON c.ContinentId = co.ContinentId " +
                "JOIN City ci ON c.CountryId = ci.CountryId " +
                "WHERE c.CountryName = @countryName " +
                "AND co.ContinentName = @continentName)";

            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@ContinentName", cbContinent.SelectedItem.ToString());
            cmd.Parameters.AddWithValue("@CountryName", lbCountry.SelectedItem.ToString());

            conn.Open();

            SqlDataReader reader = cmd.ExecuteReader();
            DataTable tbcities = new DataTable();
            tbcities.Load(reader);

            grdCities.ItemsSource = tbcities.DefaultView;

            conn.Close();

        }

        private void lbCountry_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lbCountry.SelectedItem == null)
                return;

            LoadGrid();
            RefreshLanguageandCurrency();
        }

        private void RefreshLanguageandCurrency()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(Data.ConnectionString))
                {
                    // Retrieve Currency
                    string currencyQuery = "SELECT Currency FROM Country WHERE CountryName = @CountryName";
                    SqlCommand currencyCmd = new SqlCommand(currencyQuery, conn);
                    currencyCmd.Parameters.AddWithValue("@CountryName", lbCountry.SelectedItem.ToString());

                    conn.Open();
                    string currencyResult = (string)currencyCmd.ExecuteScalar();
                    lblCurrency.Content = currencyResult;
                    conn.Close();

                    // Retrieve Language
                    string languageQuery = "SELECT Language FROM Country WHERE CountryName = @CountryName";
                    SqlCommand languageCmd = new SqlCommand(languageQuery, conn);
                    languageCmd.Parameters.AddWithValue("@CountryName", lbCountry.SelectedItem.ToString());

                    conn.Open();
                    string languageResult = (string)languageCmd.ExecuteScalar();
                    lblLanguage.Content = languageResult;
                    conn.Close();
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unexpected error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnAddCountry_Click(object sender, RoutedEventArgs e)
        {
            AddCountryWindow childWindow = new AddCountryWindow();
            childWindow.Owner = this;
            childWindow.ShowDialog();


            RefreshContinentsComboBox();
        }

        private void btnAddCity_Click(object sender, RoutedEventArgs e)
        {
            AddCityWindow childWindow = new AddCityWindow();
            childWindow.Owner = this;
            childWindow.ShowDialog(); 


            RefreshContinentsComboBox();
        }


    }
}