using Sitecore.HttpStatus.Windows.Commands;
using Sitecore.HttpStatus.Windows.ViewModel;
using System;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace Sitecore.HttpStatus.Windows.ViewModel
{
    public class HomeViewModel : ViewModel
    {
        public HomeViewModel()
        {
            StartCommand = new Command(Start);
            StatusList = new ObservableCollection<Status>();
            m_folders = new List<IFolder>();
            var appCmd = new SitecoreInterface();
            m_folders = appCmd.SitecoreItems();
            CacheRefresherWarmup();
        }

        private IFolder _selectedNode;
        
        private bool _startBtn;

        private Market _selectedMarket;

        private string _selectedProgram;

        public string _viewState;

        public string _eventValidation;

        public bool StartBtn { get { return _startBtn; }
            set {
                _startBtn = value;
                NotifyPropertyChanged("StartBtn");
            }
        }

        public IFolder SelectedNode { get{ return _selectedNode; } set { _selectedNode = value; NotifyPropertyChanged("SelectedNode"); canStartExecute(); } }

        public Market SelectedMarket { get { return _selectedMarket; } set { _selectedMarket = value; NotifyPropertyChanged("SelectedMarket"); canStartExecute(); } }

        public string SelectedProgram { get { return _selectedProgram; } set { _selectedProgram = value; NotifyPropertyChanged("SelectedProgram"); canStartExecute(); } }

        public bool IncludeDestinations {get; set; }

        public bool IncludeCourses { get; set; }

        public bool GenerateScreenshot { get; set; }

        public ObservableCollection<Status> StatusList { get; set; }

        public Command StartCommand { get; set; }

        //public string screenshotFolder = ConfigurationManager.AppSettings["screenshotFolder"];
        public string screenshotFolder = @"C:\SourceTree\Sitecore.HttpStatus\Sitecore.HttpStatus.Windows\screenshots\";

        public List<Market> MarketsList
        {
            get
            {
                var appCmd = new SitecoreInterface();
                return appCmd.GetMarketList();
            }
        }

        public bool canStartExecute()
        {
            if (SelectedMarket == null || (SelectedProgram == null && SelectedNode == null))
            {
                StartBtn = false;
                return false;
            }
            StartBtn = true;
            return true;         
        }

        private List<IFolder> m_folders;

        public List<IFolder> Folders
        {
            get { return m_folders; }
            set
            {
                m_folders = value;
                NotifyPropertyChanged("Folders");
            }
        }

        async private void CacheRefresherWarmup()
        {
            var request = WebRequest.Create("https://staticcacherefresh.ef.com/SuperCacheRefreshPage.aspx");
            ((HttpWebRequest)request).UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/44.0.2403.155 Safari/537.36";
            try
            {
                var response = await request.GetResponseAsync();
                var status = ((HttpWebResponse)response).StatusCode;

                if (status == HttpStatusCode.OK)
                {
                    Stream receiveStream = response.GetResponseStream();
                    StreamReader readStream;

                    readStream = new StreamReader(receiveStream);

                    string data = readStream.ReadToEnd();

                    response.Close();
                    readStream.Close();
                }

            }
            catch (WebException ex)
            {
                var status = ((HttpWebResponse)ex.Response).StatusCode;
            }
            catch (Exception ex)
            {
            }
        }

        async public void Start(object Obj)
        {
            _startBtn = false;
            NotifyPropertyChanged("StartBtn");
            canStartExecute();

            var sitecoreItemsList = new List<string>();
            if (SelectedNode != null)
            {
                sitecoreItemsList.Add(SelectedNode.ContentPath);
            }
            if (SelectedProgram != null)
            {
                var appCmd = new SitecoreInterface();
                sitecoreItemsList.AddRange(appCmd.LanguageItems(SelectedProgram, IncludeDestinations, IncludeCourses));
            }
            StatusList.Clear();

            foreach (var item in sitecoreItemsList)
            {
                var url = "http://" + SelectedMarket.localDomain + "/" + item + "/";
                var noCacheUrl = "http://" + SelectedMarket.localDomain + "/" + item + "/?staticignore=true";

                var request = WebRequest.Create(url);
                ((HttpWebRequest)request).UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/44.0.2403.155 Safari/537.36";

                var secondRequest = WebRequest.Create(noCacheUrl);
                ((HttpWebRequest)secondRequest).UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/44.0.2403.155 Safari/537.36";

                if (GenerateScreenshot)
                {
                    var itemName = item.Replace('/', '.');
                    GenerateBitmapMobile(noCacheUrl, itemName);
                    GenerateBitmapDesktop(noCacheUrl, itemName);
                }
                try
                {
                    var response = await request.GetResponseAsync();
                    var status = ((HttpWebResponse)response).StatusCode;
                    response.Close();

                    var noCacheResponse = await secondRequest.GetResponseAsync();
                    var staticIgnoreStatus = ((HttpWebResponse)noCacheResponse).StatusCode;
                    noCacheResponse.Close();

                    var result = new Status();
                    result.Url = url;
                    result.StatusInt = (int)status;
                    result.StaticIgnoreStatus = (int)staticIgnoreStatus;

                    StatusList.Add(result);
                }
                catch (WebException ex)
                {
                    var status = ((HttpWebResponse)ex.Response).StatusCode;
                }
                catch (Exception ex)
                {
                }
            }
            _startBtn = true;
            NotifyPropertyChanged("StartBtn");
            canStartExecute();

        }

        public void GenerateBitmapDesktop(string url, string name)
        {
            // Load the webpage into a WebBrowser control
            System.Windows.Forms.WebBrowser wb = new System.Windows.Forms.WebBrowser();
            wb.ScrollBarsEnabled = false;
            wb.ScriptErrorsSuppressed = true;
            //wb.Width = wb.Document.Body.ScrollRectangle.Width;
            wb.Width = 1024;
            wb.Height = wb.Document.Body.ScrollRectangle.Height;

            wb.Navigate(url);
            while (wb.ReadyState != WebBrowserReadyState.Complete) { System.Windows.Forms.Application.DoEvents(); }

            Bitmap bitmap = new Bitmap(wb.Width, wb.Height);
            wb.DrawToBitmap(bitmap, new Rectangle(0, 0, wb.Width, wb.Height));
            wb.Dispose();
            var savePath = screenshotFolder + @"\" + name + "-desktop.jpg";
            bitmap.Save(savePath, ImageFormat.Jpeg);
        }

        public void GenerateBitmapMobile(string url, string name)
        {
            System.Windows.Forms.WebBrowser wb = new System.Windows.Forms.WebBrowser();
            wb.ScrollBarsEnabled = false;
            wb.ScriptErrorsSuppressed = true;
            wb.Width = wb.Document.Body.ScrollRectangle.Width;
            wb.Height = wb.Document.Body.ScrollRectangle.Height;

            wb.Navigate(url);
            while (wb.ReadyState != WebBrowserReadyState.Complete) { System.Windows.Forms.Application.DoEvents(); }

            Bitmap bitmap = new Bitmap(wb.Width, wb.Height);
            wb.DrawToBitmap(bitmap, new Rectangle(0, 0, wb.Width, wb.Height));
            wb.Dispose();
            var savePath = screenshotFolder + @"\" + name + "-mobile.jpg";
            bitmap.Save(savePath, ImageFormat.Jpeg);
        }
    }

    public class Status
    {
        public string Url { get; set; }
        public int StatusInt { get; set; }
        public int StaticIgnoreStatus { get; set; }
    }

    public class ExtendedTreeView : System.Windows.Controls.TreeView
    {
        public ExtendedTreeView()
            : base()
        {
            this.SelectedItemChanged += new RoutedPropertyChangedEventHandler<object>(___ICH);
        }

        void ___ICH(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (SelectedItem != null)
            {
                SetValue(SelectedItem_Property, SelectedItem);
            }
        }

        public object SelectedItem_
        {
            get { return (object)GetValue(SelectedItem_Property); }
            set { SetValue(SelectedItem_Property, value); }
        }
        public static readonly DependencyProperty SelectedItem_Property = DependencyProperty.Register("SelectedItem_", typeof(object), typeof(ExtendedTreeView), new UIPropertyMetadata(null));
    }
}
