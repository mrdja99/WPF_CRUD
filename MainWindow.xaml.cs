using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPF_MoviesCRUD
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SqlConnection sqlConnection;
        public MainWindow()
        {
            InitializeComponent();
            string connection = ConfigurationManager.ConnectionStrings["WPF_MoviesCRUD.Properties.Settings.dbMoviesConnectionString"].ConnectionString;
            sqlConnection = new SqlConnection(connection);
            txtMovie.Text = "";
            txtYear.Text = "";
            showMovies();
        }

        private void showMovies() {
            try
            {
                string query = "SELECT Id, Name + ' (' + CAST(Year AS VARCHAR) + ')' AS DisplayText FROM Movie";
                DataTable dt = new DataTable();
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(query, sqlConnection);

                sqlDataAdapter.Fill(dt);
                listMovies.SelectedValuePath = "Id";
                listMovies.DisplayMemberPath = "DisplayText";
                listMovies.ItemsSource = dt.DefaultView;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        
        }

        private void listMovies_selectionChanged(object sender, SelectionChangedEventArgs e)
        {
            showSelectedMovie();
        }

        private void showSelectedMovie() {
            try
            {
                string query = "SELECT * FROM Movie WHERE Id=@MovieId";
                SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                sqlCommand.CommandType = CommandType.Text;
                sqlCommand.Parameters.AddWithValue("@MovieId", listMovies.SelectedValue);
                

                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);
                DataTable dt = new DataTable();
                sqlDataAdapter.Fill(dt);
                
                txtMovie.Text = dt.Rows[0]["Name"].ToString();
                txtYear.Text = dt.Rows[0]["Year"].ToString();

            }
            catch (Exception ex)
            {

                //MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        
        
        }

        private void AddNewMovie(object sender, RoutedEventArgs e)
        {
            if (txtMovie.Text == "" || txtMovie.Text == null)
            {
                MessageBox.Show("Movie text field is empty", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                txtMovie.Focus();
                return;
            }
            else if (txtYear.Text == "" || txtYear.Text == null) {
                MessageBox.Show("Movie year text field is empty", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                txtYear.Focus();
                return;
            }

            try
            {
                string checkQuery = "SELECT COUNT(*) FROM Movie WHERE Name=@MovieName AND Year=@MovieYear";
                SqlCommand checkCommand = new SqlCommand(checkQuery, sqlConnection);
                sqlConnection.Open();
                checkCommand.Parameters.AddWithValue("@MovieName", txtMovie.Text);
                checkCommand.Parameters.AddWithValue("@MovieYear", txtYear.Text);
                int movieCount = (int) checkCommand.ExecuteScalar();

                if (movieCount > 0) {
                    MessageBox.Show("Movie already exists in database", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                string query = "INSERT INTO Movie VALUES (@Name, @Year)";

                SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                sqlCommand.Parameters.AddWithValue("@Name", txtMovie.Text);
                sqlCommand.Parameters.AddWithValue("@Year", txtYear.Text);
                sqlCommand.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally {
                sqlConnection.Close();
                showMovies();
                ClearTextBoxes();
            }
        }

        private void ClearTextBoxes() { 
            txtMovie.Clear();
            txtYear.Clear();
        }

        private void UpdateMovie(object sender, RoutedEventArgs e)
        {
            if (listMovies.SelectedValue == null)
            {
                MessageBox.Show("Please select a movie to update.", "Selection Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                string query = "UPDATE Movie SET Name=@MovieName, Year=@MovieYear WHERE Id=@MovieId";
                SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                sqlConnection.Open();
                sqlCommand.CommandType = CommandType.Text;
                sqlCommand.Parameters.AddWithValue("@MovieId", listMovies.SelectedValue);
                sqlCommand.Parameters.AddWithValue("@MovieName", txtMovie.Text);
                sqlCommand.Parameters.AddWithValue("@MovieYear", txtYear.Text);
                sqlCommand.ExecuteNonQuery();
                


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Console.WriteLine(ex.Message.ToString());
            }
            finally {
                sqlConnection.Close();
                showMovies();
                ClearTextBoxes();
            }
        }

        private void DeleteMovie(object sender, RoutedEventArgs e)
        {

            if (MessageBox.Show($"Do you want to delete {txtMovie.Text}?", "Information", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes) {

                try
                {
                    string query = "DELETE FROM Movie WHERE Id=@MovieId";
                    SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                    sqlConnection.Open();
                    sqlCommand.Parameters.AddWithValue("@MovieId", listMovies.SelectedValue);
                    sqlCommand.ExecuteNonQuery();
                    MessageBox.Show($"{txtMovie.Text} deleted successfully!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    sqlConnection.Close();
                    ClearTextBoxes();
                    showMovies();
                }

            }

            
        }

        private void SearchMovies(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMovie.Text) && string.IsNullOrWhiteSpace(txtYear.Text))
            {
                showMovies();
                return;
            }

            
            try
            {
                string query = "SELECT Id,Name + ' (' + CAST(Year AS VARCHAR) + ')' AS DisplayText FROM Movie WHERE 1=1 ";

                SqlCommand sqlCommand = new SqlCommand();

                if (!string.IsNullOrWhiteSpace(txtMovie.Text) && !string.IsNullOrWhiteSpace(txtYear.Text))
                {
                    query += " AND Name LIKE @MovieName AND Year=@MovieYear";
                    sqlCommand.Parameters.AddWithValue("@MovieName","%" + txtMovie.Text + "%");
                    sqlCommand.Parameters.AddWithValue("@MovieYear", txtYear.Text);
                }else if (!string.IsNullOrEmpty(txtMovie.Text))
                {
                    query += " AND Name LIKE @MovieName";
                    sqlCommand.Parameters.AddWithValue("@MovieName","%" + txtMovie.Text + "%");
                }else if (!string.IsNullOrWhiteSpace(txtYear.Text))
                {
                    query = query + "AND Year=@MovieYear";
                    sqlCommand.Parameters.AddWithValue("@MovieYear", txtYear.Text);
                }

                sqlCommand.Connection = sqlConnection;
                sqlCommand.CommandText = query;

                sqlConnection.Open();
                sqlCommand.ExecuteNonQuery();
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);
                DataTable dt = new DataTable();
                sqlDataAdapter.Fill(dt);

                listMovies.SelectedValuePath = "Id";
                listMovies.DisplayMemberPath = "DisplayText";
                listMovies.ItemsSource = dt.DefaultView;
                
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally {
                sqlConnection.Close();
            }
            
        }
    }
}
