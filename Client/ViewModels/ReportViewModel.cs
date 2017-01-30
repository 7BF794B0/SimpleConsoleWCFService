using System;
using System.Collections.Generic;
using System.Net;
using NLog;

namespace Client.ViewModels
{
    class ReportViewModel : ViewModelBase
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private IList<int> _listOfTerminals;

        private void GetTerminalsInfo()
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create("http://localhost:8084/client/getterminalinfo/");
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    _logger.Info($"LogIn StatusCode: {response.StatusCode}.");
                    response.Close();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        public ReportViewModel()
        {
            GetTerminalsInfo();
        }

        public IList<int> ListOfTerminals
        {
            get { return _listOfTerminals; }
            set
            {
                _listOfTerminals = value;
                OnPropertyChanged(nameof(ListOfTerminals));
            }
        }
    }
}
