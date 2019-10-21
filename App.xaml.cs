using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Sitecore.Data;

namespace Sitecore.HttpStatus.Windows
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void SitecoreWarmup()
        {
            //implement here a call to sitecore to initialize it
            using (new SecurityModel.SecurityDisabler())
            {
                var db = Database.GetDatabase("master");
            }
        }
    }
}
