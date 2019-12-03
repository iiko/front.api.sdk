using System.Windows;
using Resto.Front.Api.Data.Brd;
using Resto.Front.Api.Attributes.JetBrains;

namespace Resto.Front.Api.SamplePlugin.Restaurant
{
    public sealed partial class StreetBox
    {
        [NotNull]
        private readonly IOperationService operationService;

        [CanBeNull]
        public IStreet CreatedStreet { get; private set; }

        public StreetBox()
        {

        }

        public StreetBox(IOperationService operationService)
        {
            this.operationService = operationService;
            InitializeComponent();
        }

        private void BtnOkClick(object sender, RoutedEventArgs e)
        {
            var editSession = operationService.CreateEditSession();
            var streetStub = editSession.CreateStreet(txtAdd.Text);
            var createdEntities = operationService
                .SubmitChanges(operationService.GetCredentials(), editSession);
            CreatedStreet = createdEntities.Get(streetStub);

            Close();
        }
    }
}
