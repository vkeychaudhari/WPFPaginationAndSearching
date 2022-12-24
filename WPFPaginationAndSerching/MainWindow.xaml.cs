using System;
using System.Collections.Generic;
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
using System.Data.SqlClient;
using System.Diagnostics;
using System.Data;
using System.Configuration;

namespace WPFPaginationAndSerching
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly string MockDBConnectionString = ConfigurationManager.ConnectionStrings["MOCK_DBConnectionString"].ConnectionString;
        private static readonly int NumberOfRecords = 50;
        public MainWindow()
        {
            InitializeComponent();
            LoadDropDownList();
            LoadGridView(true);
        }

        private void LoadGridView(bool isInitial = false)
        {
            if (ddlCustomPagination.SelectedValue!=null)
            {
                int offset = (ConvertInt(ddlCustomPagination.SelectedValue.ToString()) - 1) * NumberOfRecords;

                if (isInitial || offset < 0)
                    offset = 0;

                DataTable dtRecords = ExecuteSqlCommand(MockDBConnectionString, GetMockData(offset));
                gridViewMockData.ItemsSource = dtRecords.DefaultView;
                //gridViewMockData.DataBind();

                Debug.WriteLine("---------------------------");
                Debug.WriteLine(string.Format("Page: {0}", ddlCustomPagination.SelectedValue));
                Debug.WriteLine(string.Format("Total Records Loaded: {0}", dtRecords.Rows.Count));
                Debug.WriteLine(string.Format("Query: {0}", GetMockData(offset)));
                Debug.WriteLine("---------------------------"); 
            }
        }

        private string GetMockData(int offset = 0)
        {
            return string.Format("SELECT * FROM MOCK_DATA {0} ORDER BY id OFFSET {1} ROWS FETCH NEXT {2} ROWS ONLY",
                WhereClause(),
                offset,
                NumberOfRecords);
        }

        private void LoadDropDownList()
        {
            DataTable dtTotalRecords = ExecuteSqlCommand(MockDBConnectionString, CountMockData());

            int totalRecords = ConvertInt(dtTotalRecords.Rows[0][0].ToString());
            int totalPages = (int)Math.Ceiling((double)totalRecords / NumberOfRecords);

            lblTotalPages.Text = string.Format(" of {0} Pages", totalPages.ToString());

            if (totalRecords > 0)
            {
                ddlCustomPagination.Items.Clear();
                for (int i = 1; i <= totalPages; i++)
                {
                    ddlCustomPagination.Items.Add(i.ToString());
                }
                ddlCustomPagination.SelectedIndex = 0;
            }
            else
            {
                ddlCustomPagination.Items.Add("0");
            }
        }

        private string CountMockData()
        {
            return string.Format("SELECT COUNT(id) FROM MOCK_DATA {0}",
                WhereClause());
        }

        private string WhereClause()
        {
            SearchInput searchInput = GetSearchInput();

            string whereClause = "WHERE 1=1 ";

            if (!string.IsNullOrEmpty(searchInput.FirstName))
                whereClause += string.Format("AND first_name LIKE '%{0}%' ", searchInput.FirstName);

            if (!string.IsNullOrEmpty(searchInput.LastName))
                whereClause += string.Format("AND last_name LIKE '%{0}%' ", searchInput.LastName);

            if (!string.IsNullOrEmpty(searchInput.Email))
                whereClause += string.Format("AND email LIKE '%{0}%' ", searchInput.Email);

            return whereClause;
        }

        private SearchInput GetSearchInput()
        {
            return new SearchInput()
            {
                FirstName = txtFirstName.Text,
                LastName = txtLastName.Text,
                Email = txtEmail.Text
            };
        }

        private DataTable ExecuteSqlCommand(string connectionString, string queryString)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                DataTable datatable = new DataTable();
                SqlDataAdapter adapter = new SqlDataAdapter();
                adapter.SelectCommand = new SqlCommand(queryString, connection);
                adapter.Fill(datatable);
                return datatable;
            }
        }

        private int ConvertInt(string input)
        {
            int.TryParse(input, out int output);
            return output;
        }

        private void ddlCustomPagination_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadGridView();
        }

        private void btnPrevious_Click(object sender, RoutedEventArgs e)
        {
            if (ddlCustomPagination.SelectedValue.ToString() != "0" && ddlCustomPagination.SelectedValue.ToString() != "1")
                ddlCustomPagination.SelectedValue = (ConvertInt(ddlCustomPagination.SelectedValue.ToString()) - 1).ToString();

            LoadGridView();
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            if (ddlCustomPagination.SelectedValue.ToString() != "0" && ddlCustomPagination.SelectedValue.ToString() != ddlCustomPagination.Items.Count.ToString())
                ddlCustomPagination.SelectedValue = (ConvertInt(ddlCustomPagination.SelectedValue.ToString()) + 1).ToString();

            LoadGridView();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            LoadDropDownList();
            LoadGridView(true);
        }

        private void btnViewAll_Click(object sender, RoutedEventArgs e)
        {
            ClearSearchInput();
            LoadDropDownList();
            LoadGridView(true);
        }

        private void ClearSearchInput()
        {
            txtFirstName.Text = string.Empty;
            txtLastName.Text = string.Empty;
            txtEmail.Text = string.Empty;
        }
    }

    class SearchInput
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
    }
}
