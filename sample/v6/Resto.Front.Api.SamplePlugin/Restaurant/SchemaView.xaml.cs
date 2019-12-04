using System.Collections.ObjectModel;

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
            var sectionSchemas = PluginContext.Operations.GetSectionSchemas();
            foreach (var item in sectionSchemas)
            {
                SectionSchemas.Add(new SectionSchemaModel(item));
            }
        }
    }
}
