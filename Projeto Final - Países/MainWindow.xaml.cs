using Librabry;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Projeto_Final___Países
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Country> _listCountries;
        private CountryApiService _countryService;
        private NetWorkService _netWorkService;
        private DataService _dataService;
        private string _currentCountryName;
        public MainWindow()
        {
            InitializeComponent();
            _countryService = new CountryApiService();
            _listCountries = new List<Country>();
            _netWorkService = new NetWorkService();
            _dataService = new DataService();
            searchTextBox.IsEnabled = false;
            LoadData();

        }

        /// <summary>
        /// Carrega os dados dos países, seja da API ou localmente, e atualiza a interface do usuário.
        /// </summary>
        private async void LoadData()
        {
            var progress = new Progress<int>(ReportProgress);

            lblResultado.Content = "A carregar Países...";

            if (_netWorkService.CheckConnection())
            {
                await LoadDataApi(progress);

                lblResultado.Content = "Países carregados da API...";
            }
            else
            {
                LoadLocalData(progress);

                lblResultado.Content = "Países carregados da base de dados...";
            }

            if (_listCountries.Count == 0)
            {
                lblResultado.Content = "Não há ligação há internet" + Environment.NewLine +
                    "e não foram previamente guardados  países na base de dados .";
                return;
            }
            searchTextBox.IsEnabled = true;
        }

        /// <summary>
        /// Carrega os dados da BD e reporta o progresso.
        /// </summary>
        /// <param name="progress"></param>
        private async void LoadLocalData(IProgress<int> progress)
        {
            try
            {
                _listCountries = _dataService.getData();

                var orderedCountries = _listCountries.OrderBy(c => c.Name.Common).ToList();

                int count = _listCountries.Count;

                for (int i = 0; i < count; i++)
                {
                    listBox.Items.Add(orderedCountries[i]);
                    listBox.DisplayMemberPath = "Name.Common";
                    await Task.Delay(5);
                    int percentage = (i + 1) * 100 / count;
                    progress.Report(percentage);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                _dataService.CloseConnection();
            }
        }

        /// <summary>
        /// Carrega os dados da API e reporta o progresso.
        /// </summary>
        /// <param name="progress"></param>
        /// <returns></returns>
        private async Task LoadDataApi(IProgress<int> progress)
        {
            try
            {
                _dataService.DeleteData();

                _listCountries = await _countryService.GetCountriesAsync("https://restcountries.com", "/v3.1/all");

                var orderedCountries = _listCountries.OrderBy(c => c.Name.Common).ToList();

                int count = _listCountries.Count;

                for (int i = 0; i < count; i++)
                {
                    _dataService.SaveData(_listCountries[i]);
                    listBox.Items.Add(orderedCountries[i]);
                    listBox.DisplayMemberPath = "Name.Common";
                    await Task.Delay(5);

                    int percentage = (i + 1) * 100 / count;
                    progress.Report(percentage);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                _dataService.CloseConnection();
            }
        }

        /// <summary>
        /// Atualiza os detalhes do país selecionado no ListBox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (listBox.SelectedItem is Country selectedCountry)
            {
                textBlockCapital.Text = selectedCountry.OutputCapital;
                textBlockRegion.Text = selectedCountry.Region;
                textBlockSubregion.Text = selectedCountry.Subregion;
                textBlockPopulation.Text = selectedCountry.OutputPopulation;
                textBlockGini.Text = selectedCountry.OutputGini;
                textBlockLanguages.Text = selectedCountry.OutputLanguages;
                textBlockCurrencies.Text = selectedCountry.OutputCurrencies;
                textBlockArea.Text = selectedCountry.OutPutArea;
                textBlockUnMember.Text = selectedCountry.OutPutUnMember;
                textBlockStatus.Text = selectedCountry.Status;
                textBlockIndependency.Text = selectedCountry.OutputIndependent;

                if (_netWorkService.CheckConnection())
                {
                    Uri uri = new Uri(selectedCountry.Flags.Png);
                    imageFlag.Source = new BitmapImage(uri);
                }
                else
                {
                    string path = @$"bandeiras\{selectedCountry.Name.Common}.png";

                    if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
                    {
                        Uri uri = new Uri(Path.GetFullPath(path));
                        imageFlag.Source = new BitmapImage(uri);
                    }
                    else
                    {
                        imageFlag.Source = new BitmapImage(new Uri(Path.GetFullPath(@"Bandeiras\imagenotfound.png")));
                    }
                }
                _currentCountryName = selectedCountry.Name.Common;
                mapButton.IsEnabled = !string.IsNullOrEmpty(_currentCountryName);
            }
            else
            {
                _currentCountryName = null;
                mapButton.IsEnabled = false;
            }
        }

        /// <summary>
        /// Atualiza a barra de progresso e o texto do progresso com base no percentual fornecido.
        /// </summary>
        /// <param name="percentage"></param>
        private void ReportProgress(int percentage)
        {
            ProgressBar1.Value = percentage;
            ProgressText.Text = $"{percentage}%";
        }

        /// <summary>
        /// Baixa imagens quando a janela é carregada, se houver conexão com a internet.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_netWorkService.CheckConnection())
            {
                await _dataService.DowloadImagens();
            }
        }

        /// <summary>
        /// Método para procurar os países na lista por nome
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = searchTextBox.Text.ToLower();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                listBox.Items.Clear();

                var countries = _listCountries.OrderBy(p => p.Name.Common).ToList();

                for (int i = 0; i < countries.Count; i++)
                {
                    listBox.Items.Add(countries[i]);
                }
            }
            else
            {
                var filteredList = _listCountries
                    .Where(c => c.Name.Common.ToLower().Contains(searchText))
                    .OrderBy(p => p.Name.Common)
                    .ToList();

                listBox.Items.Clear();


                for (int i = 0; i < filteredList.Count; i++)
                {
                    listBox.Items.Add(filteredList[i]);

                }
            }
        }

        /// <summary>
        /// Método de evento click do botão que abre uma janela no google do país que foi selecionado
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MapButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_currentCountryName))
            {
                string url = $"https://www.google.com/maps/search/?api=1&query={Uri.EscapeDataString(_currentCountryName)}";

                try
                {
                    // Abre o URL no navegador padrão do sistema
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true // Necessário para abrir URLs no navegador padrão
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao abrir o navegador: {ex.Message}");
                }
            }
        }
    }
}