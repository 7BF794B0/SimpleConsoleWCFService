using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;
using System.Windows;
using Microsoft.Win32;

using Client.Commands;
using PdfCreator;
using Contracts;

using NLog;

namespace Client.ViewModels
{
    class ReportViewModel : ViewModelBase
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private IEnumerable<int> _listOfTerminals;
        private int _cmbSelectedValue;
        private DateTime _initialDateSelected;
        private DateTime _finalDateSelected;
        private ObservableCollection<Telemetry> _telemetryList;

        private readonly JavaScriptSerializer _serializer;

        public DelegateCommand CreateReportCommand { get; set; }

        private string GetResponseBody(HttpWebResponse response)
        {
            string res = string.Empty;
            if (response.CharacterSet != null)
            {
                var encoding = Encoding.GetEncoding(response.CharacterSet);

                using (var responseStream = response.GetResponseStream())
                {
                    if (responseStream != null)
                    {
                        using (var reader = new StreamReader(responseStream, encoding))
                        {
                            res = reader.ReadToEnd();
                            reader.Close();
                        }
                    }
                }
            }
            return res;
        }

        private void GetTerminalsInfo()
        {
            try
            {
                var request = (HttpWebRequest) WebRequest.Create("http://localhost:8084/client/getterminalinfo");
                using (var response = (HttpWebResponse) request.GetResponse())
                {
                    _logger.Info($"Connection to server StatusCode: {response.StatusCode}.");

                    int[] termianlIds = _serializer.Deserialize<int[]>(GetResponseBody(response));

                    if (termianlIds != null)
                        ListOfTerminals = termianlIds;
                    else
                        throw new NullReferenceException();

                    response.Close();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                MessageBox.Show(ex.Message, "Ошибка подключения к серверу", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void GetDataByTerminalId(int terminalId)
        {
            try
            {
                var request = (HttpWebRequest) WebRequest.Create($"http://localhost:8084/client/getdata/{terminalId}");
                using (var response = (HttpWebResponse) request.GetResponse())
                {
                    _logger.Info($"Connection to server StatusCode: {response.StatusCode}.");

                    TelemetryCollection telemetryData =
                        _serializer.Deserialize<TelemetryCollection>(GetResponseBody(response));

                    TelemetryList.Clear();
                    foreach (var data in telemetryData.Collection)
                        TelemetryList.Add(data);

                    InitialDateSelected = telemetryData.Collection.Min(i => i.Time);
                    FinalDateSelected = telemetryData.Collection.Max(f => f.Time);

                    response.Close();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                MessageBox.Show(ex.Message, "Ошибка при получении данных", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public ReportViewModel()
        {
            _telemetryList = new ObservableCollection<Telemetry>();

            _serializer = new JavaScriptSerializer();
            CreateReportCommand = new DelegateCommand(CreateReportExecute);
            GetTerminalsInfo();
        }

        public IEnumerable<int> ListOfTerminals
        {
            get { return _listOfTerminals; }
            set
            {
                _listOfTerminals = value;
                RaisePropertyChanged(nameof(ListOfTerminals));
            }
        }

        public int CmbSelectedValue
        {
            get { return _cmbSelectedValue; }
            set
            {
                _cmbSelectedValue = value;
                RaisePropertyChanged(nameof(CmbSelectedValue));
                GetDataByTerminalId(_cmbSelectedValue);
            }
        }

        public DateTime InitialDateSelected
        {
            get { return _initialDateSelected; }
            set
            {
                _initialDateSelected = value;
                RaisePropertyChanged(nameof(InitialDateSelected));
            }
        }

        public DateTime FinalDateSelected
        {
            get { return _finalDateSelected; }
            set
            {
                _finalDateSelected = value;
                RaisePropertyChanged(nameof(FinalDateSelected));
            }
        }

        public ObservableCollection<Telemetry> TelemetryList
        {
            get { return _telemetryList; }
            set
            {
                _telemetryList = value;
                RaisePropertyChanged(nameof(TelemetryList));
            }
        }

        private void CreateReportExecute(object o)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                FileName = "Report",
                DefaultExt = ".pdf",
                Filter = "Pdf documents (.pdf)|*.pdf"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                PdfCreator.PdfCreator pdf = new PdfCreator.PdfCreator();
                pdf.CreatePdf(saveFileDialog.FileName, CmbSelectedValue,
                    new List<Telemetry>(
                        TelemetryList.Where(h => h.Time >= InitialDateSelected && h.Time <= FinalDateSelected)));
            }
            System.Diagnostics.Process.Start(saveFileDialog.FileName);
        }
    }
}
