using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Sitecore;
using Sitecore.Data;
using Sitecore.Data.Items;
using System.Collections.ObjectModel;
using Sitecore.HttpStatus.Windows.ViewModel;

namespace Sitecore.HttpStatus.Windows.ViewModel
{
    public class SitecoreInterface
    {
        public const string CoursesOverviewTemplate = "Courses Section Page";
        public const string CoursesCategoryTemplate = "Course Category Page";
        public const string CoursesDetailsTemplate = "Course Detail Page";
        public const string DestinationOverviewTemplate = "Destinations Section Page";
        public const string CountryTemplate = "Destination Country Page";
        public const string CityTemplate = "Destination City Page";

        public List<Market> GetMarketList()
        {
            var list = new List<Market>();

            using (new SecurityModel.SecurityDisabler())
            {
                var db = Database.GetDatabase("master");
                var root = db.GetItem(new ID("8DDAF0F4-CBDF-43FE-BC45-B6FA5C520B3C"));

                foreach (Item child in root.GetChildren())
                {
                    if (child.TemplateID.ToString() != "{BA7D4735-4503-444B-93BD-2A25D7959384}") continue;
                    var mc = new Market();
                    mc.localDomain = child["Local Domain"];
                    mc.twoLetterCode = child.Name;
                    mc.programs = child["Available Programs"].Split('|');
                    mc.ProgramCollection = GetLanguageProgramsPerMarket(mc);
                    list.Add(mc);
                }
            }
            return list;
        }

        public List<string> LanguageItems(string SelectedProgram, bool destinations, bool courses)
        {
            var list = new List<string>();
            var partialPath = string.Empty;

            using (new SecurityModel.SecurityDisabler())
            {
                var db = Database.GetDatabase("master");
                var root = db.GetItem(new ID("8D4BC969-D6A1-449E-9B96-53CFA9F0BED4"));

                foreach (Item child in root.GetChildren())
                {
                    if ((child.TemplateID.ToString() != "{21B12BE5-73D9-448E-8853-E6FD7361BE12}") || (child.Name != SelectedProgram)) continue;
                    var name = child.Name;
                    list.Add(name);
                    if (courses)
                    {
                        foreach (Item course in child.GetChildren())
                        {
                            if (course.TemplateName == CoursesOverviewTemplate)
                            {
                                partialPath = course.Paths.ContentPath.Replace("/efcom/", string.Empty);
                                list.Add(partialPath);
                                foreach (Item category in course.GetChildren())
                                {
                                    if (category.TemplateName == CoursesCategoryTemplate)
                                    {
                                        partialPath = category.Paths.ContentPath.Replace("/efcom/", string.Empty);
                                        list.Add(partialPath);
                                        foreach (Item courseDetail in category.GetChildren())
                                        {
                                            if (courseDetail.TemplateName == CoursesDetailsTemplate)
                                            {
                                                partialPath = courseDetail.Paths.ContentPath.Replace("/efcom/", string.Empty);
                                                list.Add(partialPath);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (destinations)
                    {
                        foreach (Item destination in child.GetChildren())
                        {
                            if (destination.TemplateName == DestinationOverviewTemplate)
                            {
                                partialPath = destination.Paths.ContentPath.Replace("/efcom/", string.Empty);
                                list.Add(partialPath);
                                foreach (Item country in destination.GetChildren())
                                {
                                    if (country.TemplateName == CountryTemplate)
                                    {
                                        partialPath = country.Paths.ContentPath.Replace("/efcom/", string.Empty);
                                        list.Add(partialPath);
                                        foreach (Item city in country.GetChildren())
                                        {
                                            if (city.TemplateName == CityTemplate)
                                            {
                                                partialPath = city.Paths.ContentPath.Replace("/efcom/", string.Empty);
                                                list.Add(partialPath);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return list;
        }

        public List<IFolder> SitecoreItems()
        {
            var list = new List<IFolder>();
            var partialPath = string.Empty;

            using (new SecurityModel.SecurityDisabler())
            {
                var db = Database.GetDatabase("master");
                var root = db.GetItem(new ID("8D4BC969-D6A1-449E-9B96-53CFA9F0BED4"));
                var childItemsList = new List<Item>();

                foreach (Item unsortedChild in root.GetChildren())
                {
                    if (string.IsNullOrEmpty(unsortedChild.Fields["__Renderings"].Value)) continue;
                    childItemsList.Add(unsortedChild);
                }

                for (int z = 0; z < childItemsList.Count; z++)
                {
                    Item child = childItemsList[z];
                    list.Add(new Folder { FolderLabel = child.Name, ContentPath = child.Paths.ContentPath.Replace("/efcom/", string.Empty), Folders = new List<IFolder>() });

                    for (int i = 0; i < child.Children.Count; i++)
                    {
                        Item secondChild = child.Children[i];
                        if (string.IsNullOrEmpty(secondChild.Fields["__Renderings"].Value)) continue;
                        list[z].Folders.Add(new Folder { FolderLabel = secondChild.Name, ContentPath = secondChild.Paths.ContentPath.Replace("/efcom/", string.Empty), Folders = new List<IFolder>() });
                    }
                }
            }
            return list;
        }

        public ObservableCollection<string> GetLanguageProgramsPerMarket(Market market)
        {
            var collection = new ObservableCollection<string>();

            using (new SecurityModel.SecurityDisabler())
            {
                var db = Database.GetDatabase("master");
                var root = db.GetItem(new ID("8D4BC969-D6A1-449E-9B96-53CFA9F0BED4"));
                foreach (Item child in root.GetChildren())
                {
                    if (child.TemplateID.ToString() != "{21B12BE5-73D9-448E-8853-E6FD7361BE12}") continue;
                    if (market.programs.Contains(child["Program"]))
                    {
                        collection.Add(child.Name);
                    }
                }
                return collection;
            }
        }
    }

    public class Folder : IFolder
    {
        public string ContentPath { get; set; }
        public string FolderLabel { get; set; }
        public List<IFolder> Folders { get; set; }
    }

    public class Market
    {
        public string localDomain { get; set; }
        public string twoLetterCode { get; set; }
        public string[] programs { get; set; }
        public ObservableCollection<string> ProgramCollection { get; set; }
    }
}
