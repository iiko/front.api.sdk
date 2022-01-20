using System.Collections.ObjectModel;
using System.Linq;
using Resto.Front.Api.Data.Organization.Sections;

namespace Resto.Front.Api.SamplePlugin.Restaurant
{
    /// <summary>
    /// Interaction logic for SchemaView.xaml
    /// </summary>
    public partial class SchemaView
    {
        public SchemaView()
        {
            ReloadSchemas();
            InitializeComponent();
        }

        public ObservableCollection<SectionSchemaModel> SectionSchemas { get; set; }

        private void ReloadSchemas()
        {
            SectionSchemas = new ObservableCollection<SectionSchemaModel>();
            var sectionSchemas = PluginContext.Operations.GetTerminalsGroupRestaurantSections(PluginContext.Operations.GetHostTerminalsGroup())
                .Select(section => section.TryGetSectionSchema())
                .Where(schema => schema != null)
                .ToList();

            foreach (var item in sectionSchemas)
            {
                SectionSchemas.Add(new SectionSchemaModel(item));
            }
        }
    }
}
